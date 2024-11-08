using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class HexagonGrid : GridGenerator
    {
        static readonly int xunit, yunit, dunit;
        static readonly FakePattern hexagon;
        static HexagonGrid()
        {
            xunit = PlanetType.Normal.GetData().InterStellarRadius * 866 / 100;
            yunit = PlanetType.Normal.GetData().InterStellarRadius * 5;
            dunit = PlanetType.Normal.GetData().InterStellarRadius * 10;

            hexagon = new FakePattern();
            var p0 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit, 0));
            var p1 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit * 2, yunit));
            var p2 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit * 2, yunit * 3));
            var p3 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit, yunit * 4));
            var p4 = hexagon.AddPlanetAt(ArcenPoint.Create(0, yunit * 3));
            var p5 = hexagon.AddPlanetAt(ArcenPoint.Create(0, yunit));

            hexagon.AddLink(p0, p1);
            hexagon.AddLink(p1, p2);
            hexagon.AddLink(p2, p3);
            hexagon.AddLink(p3, p4);
            hexagon.AddLink(p4, p5);
            hexagon.AddLink(p5, p0);
        }
        public (FakeGalaxy, FakeGalaxy) MakeGrid(int outerPath, AspectRatio aspectRatioEnum, int galaxyShape, int symmetry, int dissonance, int numPlanets, ParameterService par)
        {
            numPlanets = numPlanets * 12 / (12 - dissonance);
            FInt aspectRatio = 1 / aspectRatioEnum.Value();
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
                    if (symmetry == 10101 && c % 2 != 1) continue;
                    // FIXME: rough estimation
                    int planets = (r + 1) * (c + 1);
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = ((FInt)r * 3 + 1) * yunit / (((FInt)c + 1) * xunit);
                    FInt p1 = currentAspectRatio / aspectRatio;
                    FInt p2 = aspectRatio / currentAspectRatio;
                    FInt aspectRatioBadness = ((p1 > p2 ? p1 : p2) - FInt.One) * (FInt)10;
                    FInt currentBadness = planetBadness + aspectRatioBadness;
                    if (currentBadness < badness)
                    {
                        badness = currentBadness;
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
                        g2.MakeRotationalGeneric((c + 1) * xunit / 2, (r * 3 + 1) * yunit, dunit, symmetry / 100, symmetry % 100 == 50, true);
                        int planets = g2.planets.Count;
                        FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                        FInt currentBadness = planetBadness;
                        if (currentBadness < newBadness)
                        {
                            newBadness = currentBadness;
                            fg = g2;
                        }
                    }
                }
                return (fg, new FakeGalaxy(fg.planetCollection));
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
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }
            else if (symmetry == 10200)
            {
                columns = (columns + 3) / 4;
                rows = (rows * 4 / 3) / 2 * 2 + columns % 2;
                g = MakeGrid(rows, columns);
                g.MakeY(aspectRatioEnum, dunit, (columns + 1) * xunit);
            }

            return (g, new FakeGalaxy(g.planetCollection));
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