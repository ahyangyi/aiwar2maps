using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class DiamondYFlowerGrid
    {
        static readonly int unit, dunit;
        static DiamondYFlowerGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 7071 / 1000;
            dunit = PlanetType.Normal.GetData().InterStellarRadius * 10;
        }
        static readonly int[] dr = { 1, 2, 1, 0, -1, -2, -1, 0 };
        static readonly int[] dc = { 1, 0, -1, -2, -1, 0, 1, 2 };

        public static FakeGalaxy MakeGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 3; r <= 70; r += 2)
            {
                for (int c = 3; c <= 70; c += 2)
                {
                    if (symmetry == 150 && c % 4 == 1) continue;
                    if (symmetry == 200 && ((r + c) % 4 == 0)) continue;
                    if (symmetry == 250 && (r % 4 == 1 || c % 4 == 1)) continue;
                    // FIXME: simple estimation
                    int planets = r * c * 2;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = (FInt)(r + 1) / (FInt)(c + 1);
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
            FakeGalaxy g = MakeGrid( rows, columns);

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
                for (int c = 3; c <= 60; c += 4)
                {
                    int r1 = (((c + 1) / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) - 1) / 2).ToInt();
                    for (int r = Math.Max(r1 * 2 - 1, 1); r <= r1 * 2 + 1; r += 2)
                    {
                        var g2 = MakeGrid(r, c);
                        g2.MakeRotationalGeneric((c + 1) * unit, (r + 1) * 2 * unit, dunit, symmetry / 100, symmetry % 100 == 50, false);
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
            for (int i = 0; i < rows + 2; ++i)
            {
                for (int j = 0; j < columns + 2; ++j)
                {
                    if ((i + j) % 2 == 0 && (i > 0 && i <= rows || j > 0 && j <= columns))
                    {
                        points[(i * 2, j * 2)] = g.AddPlanetAt(ArcenPoint.Create(j * 2 * unit, i * 2 * unit));
                    }
                }
            }
            for (int i = 0; i < rows + 2; ++i)
            {
                for (int j = 0; j < columns + 2; ++j)
                {
                    if (i % 2 == 0 && j % 2 == 0 && points.ContainsKey((i * 2, j * 2)) && (i + j) % 4 == 0)
                    {
                        for (int d = 0; d < 8; ++d)
                        {
                            if (points.ContainsKey((i * 2 + dr[d] * 2, j * 2 + dc[d] * 2))
                                && (points.ContainsKey((i * 2 + dr[(d + 1) % 8] * 2, j * 2 + dc[(d + 1) % 8] * 2))
                                && points.ContainsKey((i * 2 + dr[(d + 7) % 8] * 2, j * 2 + dc[(d + 7) % 8] * 2)) || d % 2 == 0))
                            {
                                points[(i * 2 + dr[d], j * 2 + dc[d])] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + dc[d]) * unit, (i * 2 + dr[d]) * unit));
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < rows + 2; ++i)
            {
                for (int j = 0; j < columns + 2; ++j)
                {
                    if ((i + j) % 2 == 0 && points.ContainsKey((i * 2, j * 2)))
                    {
                        if (points.ContainsKey((i * 2 + 1, j * 2 - 1)))
                        {
                            points[(i * 2, j * 2)].AddLinkTo(points[(i * 2 + 1, j * 2 - 1)]);
                            if (points.ContainsKey((i * 2 + 2, j * 2 - 2)))
                                points[(i * 2 + 1, j * 2 - 1)].AddLinkTo(points[(i * 2 + 2, j * 2 - 2)]);
                        }
                        else if (points.ContainsKey((i * 2 + 2, j * 2 - 2)))
                        {
                            points[(i * 2, j * 2)].AddLinkTo(points[(i * 2 + 2, j * 2 - 2)]);
                        }
                        if (points.ContainsKey((i * 2 + 1, j * 2 + 1)))
                        {
                            points[(i * 2, j * 2)].AddLinkTo(points[(i * 2 + 1, j * 2 + 1)]);
                            if (points.ContainsKey((i * 2 + 2, j * 2 + 2)))
                                points[(i * 2 + 1, j * 2 + 1)].AddLinkTo(points[(i * 2 + 2, j * 2 + 2)]);
                        }
                        else if (points.ContainsKey((i * 2 + 2, j * 2 + 2)))
                        {
                            points[(i * 2, j * 2)].AddLinkTo(points[(i * 2 + 2, j * 2 + 2)]);
                        }

                        if (i % 2 == 0 && j % 2 == 0 && (i + j) % 4 == 0)
                        {
                            for (int d = 0; d < 8; ++d)
                            {
                                if (points.ContainsKey((i * 2 + dr[d], j * 2 + dc[d])))
                                {
                                    if (d % 2 == 1)
                                    {
                                        points[(i * 2 + dr[d], j * 2 + dc[d])].AddLinkTo(points[(i * 2 + dr[d] * 2, j * 2 + dc[d] * 2)]);
                                    }
                                    if (points.ContainsKey((i * 2 + dr[(d + 1) % 8], j * 2 + dc[(d + 1) % 8])))
                                    {
                                        points[(i * 2 + dr[d], j * 2 + dc[d])].AddLinkTo(points[(i * 2 + dr[(d + 1) % 8], j * 2 + dc[(d + 1) % 8])]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return g;
        }
    }
}