using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class HexagonGrid
    {
        static readonly int xunit, yunit;
        static readonly FakePattern hexagon;
        static HexagonGrid()
        {
            xunit = PlanetType.Normal.GetData().InterStellarRadius * 866 / 100;
            yunit = PlanetType.Normal.GetData().InterStellarRadius * 5;

            hexagon = new FakePattern();
            var p0 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit, 0));
            var p1 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit * 2, yunit));
            var p2 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit * 2, yunit * 3));
            var p3 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit, yunit * 4));
            var p4 = hexagon.AddPlanetAt(ArcenPoint.Create(0, yunit * 3));
            var p5 = hexagon.AddPlanetAt(ArcenPoint.Create(0, yunit));

            p0.AddLinkTo(p1);
            p1.AddLinkTo(p2);
            p2.AddLinkTo(p3);
            p3.AddLinkTo(p4);
            p4.AddLinkTo(p5);
            p5.AddLinkTo(p0);
        }
        public static FakeGalaxy MakeGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
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
                    if (symmetry == 10000 && (c % 4 == 1 || c % 4 == 2)) continue;
                    if (symmetry == 10001 && c % 3 != 2) continue;
                    if (symmetry == 10002 && ((r + c) % 2 == 1 || c % 4 == 1 || c % 4 == 2)) continue;
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
                    int r1 = (((c + 1) * xunit / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) / yunit - 1) / 3).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(r, c);
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
            else if (symmetry == 10000)
            {
                g.MakeTranslational2((columns + 3) / 4 * 2 * xunit);
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych((columns + 1) / 3 * xunit);
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy((columns + 3) / 4 * 2 * xunit);
            }

            return g;
        }

        private static FakeGalaxy MakeGrid(int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    if ((i + j) % 2 == 0)
                        hexagon.Imprint(g, ArcenPoint.Create(j * xunit, i * yunit * 3));

            return g;
        }
    }
}