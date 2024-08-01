using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class TriangleGrid
    {
        public static FakeGalaxy MakeTriangleGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int[] dr = { 1, 2, 1, -1, -2, -1 };
            int[] dc = { -1, 0, 1, 1, 0, -1 };
            int xunit = planetType.GetData().InterStellarRadius * 8660 / 1000;
            int yunit = planetType.GetData().InterStellarRadius * 5;
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
                    if (symmetry >= 300 && c % 2 == 0) continue;
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
                g.MakeRotationalGeneric((columns - 1) * xunit / 2, (rows - 1) * yunit, yunit * 2, symmetry / 100, symmetry % 100 == 50, true);
            }
            return g;
        }
    }
}