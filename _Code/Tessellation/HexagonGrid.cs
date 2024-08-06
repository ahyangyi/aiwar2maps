using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class HexagonGrid
    {
        public static FakeGalaxy MakeGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int xunit = planetType.GetData().InterStellarRadius * 8660 / 1000;
            int yunit = planetType.GetData().InterStellarRadius * 5;
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 1; r <= 70; ++r)
            {
                for (int c = 1; c <= 120; ++c)
                {
                    if (symmetry == 150 && c % 2 == 0) continue;
                    if (symmetry == 200 && (r + c) % 2 == 1) continue;
                    if (symmetry == 250 && (r % 2 == 0 || c % 2 == 0)) continue;
                    // FIXME: rough estimation
                    int planets = (r + 1) * (c + 1);
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = ((FInt)r * 3 + 1) * yunit / (((FInt)c + 1) * xunit);
                    FInt p1 = currentAspectRatio / aspectRatio;
                    FInt p2 = aspectRatio / currentAspectRatio;
                    FInt aspectRatioBadness = ((p1 > p2 ? p1 : p2) - FInt.One) * (FInt)10;
                    FInt current_badness = planetBadness + aspectRatioBadness;
                    if (current_badness < badness)
                    {
                        badness = current_badness;
                        rows = r;
                        columns = c;
                    }
                }
            }
            FakeGalaxy g = MakeGrid(xunit, yunit, rows, columns);
            if (symmetry == 150)
            {
                g.MakeBilateral();
            }
            else if (symmetry == 200)
            {
                g.MakeRotational2();
            }
            else if (symmetry == 250)
            {
                g.MakeRotational2Bilateral();
            }
            else if (symmetry >= 300 && symmetry < 10000)
            {
                FInt newBadness = (FInt)1000000;
                FakeGalaxy fg = MakeGrid(xunit, yunit, 1, 1);
                for (int c = 1; c <= 100; c += 2)
                {
                    int r1 = (((c + 1) * xunit / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) / yunit - 1) / 3).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(xunit, yunit, r, c);
                        g2.MakeRotationalGeneric((c + 1) * xunit / 2, (r * 3 + 1) * yunit, yunit * 2, symmetry / 100, symmetry % 100 == 50, true);
                        int planets = g2.planets.Count;
                        FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                        FInt current_badness = planetBadness;
                        if (current_badness < newBadness)
                        {
                            newBadness = current_badness;
                            fg = g2;
                        }
                    }
                }
                return fg;
            }
            return g;
        }

        private static FakeGalaxy MakeGrid(int xunit, int yunit, int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> uppy = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> downy = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();

            for (int i = 0; i <= rows; ++i)
            {
                for (int j = -i % 2; j <= columns + 1; j += 2)
                {
                    if (i >= 0 && i < rows && j >= 0 && j < columns ||
                        i >= 0 && i < rows && j - 2 >= 0 && j - 2 < columns ||
                        i - 1 >= 0 && i - 1 < rows && j - 1 >= 0 && j - 1 < columns)
                        uppy[(i, j)] = g.AddPlanetAt(ArcenPoint.Create(j * xunit, (i * 3 + 1) * yunit));
                    if (i >= 0 && i < rows && j >= 0 && j < columns ||
                        i - 1 >= 0 && i - 1 < rows && j - 1 >= 0 && j - 1 < columns ||
                        i - 1 >= 0 && i - 1 < rows && j + 1 >= 0 && j + 1 < columns)
                        downy[(i, j)] = g.AddPlanetAt(ArcenPoint.Create((j + 1) * xunit, (i * 3) * yunit));
                }
            }

            for (int i = 0; i <= rows; ++i)
            {
                for (int j = -i % 2; j <= columns + 1; j += 2)
                {
                    if (uppy.ContainsKey((i, j)))
                    {
                        if (downy.ContainsKey((i, j)))
                        {
                            uppy[(i, j)].AddLinkTo(downy[(i, j)]);
                        }
                        if (downy.ContainsKey((i, j - 2)))
                        {
                            uppy[(i, j)].AddLinkTo(downy[(i, j - 2)]);
                        }
                        if (downy.ContainsKey((i + 1, j - 1)))
                        {
                            uppy[(i, j)].AddLinkTo(downy[(i + 1, j - 1)]);
                        }
                    }
                }
            }

            return g;
        }
    }
}