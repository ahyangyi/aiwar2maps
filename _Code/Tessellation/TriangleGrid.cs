using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class TriangleGrid
    {
        static readonly int xunit, yunit;
        public static readonly FakePattern leftTriangle, rightTriangle;
        static TriangleGrid()
        {
            xunit = PlanetType.Normal.GetData().InterStellarRadius * 866 / 100;
            yunit = PlanetType.Normal.GetData().InterStellarRadius * 5;

            rightTriangle = new FakePattern();
            var p0 = rightTriangle.AddPlanetAt(ArcenPoint.Create(0, 0));
            var p1 = rightTriangle.AddPlanetAt(ArcenPoint.Create(xunit, yunit));
            var p2 = rightTriangle.AddPlanetAt(ArcenPoint.Create(0, yunit * 2));

            rightTriangle.AddLink(p0, p1);
            rightTriangle.AddLink(p1, p2);
            rightTriangle.AddLink(p2, p0);

            leftTriangle = rightTriangle.FlipX();
        }
        public static (FakeGalaxy, FakeGalaxy) MakeGalaxy(int outerPath, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 70; ++r)
            {
                for (int c = 2; c <= 120; ++c)
                {
                    if (symmetry == 150 && c % 2 == 1) continue;
                    if (symmetry == 200 && (r + c) % 2 == 0) continue;
                    if (symmetry == 250 && (r % 2 == 0 || c % 2 == 1)) continue;
                    if (symmetry == 10000 && (c % 4 == 1 || c % 4 == 2)) continue;
                    if (symmetry == 10001 && c % 3 != 0) continue;
                    if (symmetry == 10002 && ((r + c) % 2 == 0 || c % 4 == 1 || c % 4 == 2)) continue;
                    if (symmetry == 10101 && c % 2 != 0) continue;
                    // FIXME: rough estimation
                    int planets = (r * c) / 2;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = ((FInt)(r + 1)) * yunit / ((FInt)c * xunit);
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
                for (int c = 2; c <= 100; c += 2)
                {
                    int r1 = (c * xunit / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) / yunit - 1).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(r, c);
                        g2.MakeRotationalGeneric(c * xunit / 2, (c % 4 == 0 ? r + 1 : r) * yunit, yunit * 2, symmetry / 100, symmetry % 100 == 50, false);
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
                g.MakeTriptych(columns / 3 * xunit);
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy((columns + 3) / 4 * 2 * xunit);
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }

            return (g, new FakeGalaxy(g.planetCollection));
        }

        private static FakeGalaxy MakeGrid(int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    ((i + j) % 2 == 0 ? rightTriangle : leftTriangle).Imprint(g, ArcenPoint.Create(j * xunit, i * yunit));

            return g;
        }
    }
}