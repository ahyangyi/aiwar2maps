using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class TriangleGrid
    {
        static readonly int[] dr = { 1, 2, 1, -1, -2, -1 };
        static readonly int[] dc = { -1, 0, 1, 1, 0, -1 };
        static readonly int xunit, yunit;
        static TriangleGrid()
        {
            xunit = PlanetType.Normal.GetData().InterStellarRadius * 866 / 100;
            yunit = PlanetType.Normal.GetData().InterStellarRadius * 5;
        }
        public static FakeGalaxy MakeGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 70; ++r)
            {
                for (int c = 2; c <= 120; ++c)
                {
                    if (symmetry == 150 && c % 2 == 0) continue;
                    if (symmetry == 200 && (r + c) % 2 == 1) continue;
                    if (symmetry == 250 && (r % 2 == 0 || c % 2 == 0)) continue;
                    // FIXME: rough estimation
                    int planets = (r * c) / 2;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = ((FInt)r - 1) * yunit / (((FInt)c - 1) * xunit);
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
            FakeGalaxy g = MakeGrid(rows, columns);

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
                FakeGalaxy fg = MakeGrid(1, 1);
                for (int c = 1; c <= 100; c += 2)
                {
                    int r1 = ((c - 1) * xunit / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) / yunit + 1).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(r, c);
                        g2.MakeRotationalGeneric((c - 1) * xunit / 2, (r - 1) * yunit, yunit * 2, symmetry / 100, symmetry % 100 == 50, true);
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

        private static FakeGalaxy MakeGrid(int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> points = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();

            for (int i = 0; i < rows; ++i)
            {
                for (int j = (i + 1) % 2; j < columns; j += 2)
                {
                    points[(i, j)] = g.AddPlanetAt(ArcenPoint.Create(j * xunit, i * yunit));
                }
            }

            for (int i = 0; i < rows; ++i)
            {
                for (int j = (i + 1) % 2; j < columns; j += 2)
                {
                    for (int d = 0; d < 6; ++d)
                    {
                        if (i + dr[d] >= 0 && i + dr[d] < rows && j + dc[d] >= 0 && j + dc[d] < columns)
                        {
                            points[(i, j)].AddLinkTo(points[(i + dr[d], j + dc[d])]);
                        }
                    }
                }
            }

            return g;
        }
    }
}