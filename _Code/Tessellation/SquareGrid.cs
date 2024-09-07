using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class SquareGrid
    {
        static readonly int unit;
        static readonly FakePattern square;
        static SquareGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 10;
            square = new FakePattern();
            var p0 = square.AddPlanetAt(ArcenPoint.Create(0, 0));
            var p1 = square.AddPlanetAt(ArcenPoint.Create(unit, 0));
            var p2 = square.AddPlanetAt(ArcenPoint.Create(unit, unit));
            var p3 = square.AddPlanetAt(ArcenPoint.Create(0, unit));
            p0.AddLinkTo(p1);
            p1.AddLinkTo(p2);
            p2.AddLinkTo(p3);
            p3.AddLinkTo(p0);
        }
        public static FakeGalaxy MakeSquareGalaxy(PlanetType planetType, AspectRatio aspectRatioEnum, int galaxyShape, int symmetry, int numPlanets)
        {
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            FInt aspectRatio = aspectRatioEnum.Value();
            for (int r = 1; r <= 35; ++r)
            {
                for (int c = 1; c <= 35; ++c)
                {
                    if (galaxyShape == 2 && (r + c) % 2 == 1 && symmetry < 300) continue;
                    if (symmetry == 10001 && c % 3 != 0) continue;
                    if (symmetry == 10101 && c % 2 != 0) continue;
                    int planets = r * c;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = (FInt)r / (FInt)c;
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
            FakeGalaxy g;
            if (galaxyShape == 0)
            {
                g = MakeGrid(rows, columns);
            }
            else if (galaxyShape == 1)
            {
                g = MakeGridOctagonal(rows, columns, (Math.Min(rows, columns) / FInt.Create(3414, false)).GetNearestIntPreferringLower());
            }
            else
            {
                g = MakeGridCross(rows, columns, (Math.Min(rows, columns) / 3 + 1) & -1 | rows % 2);
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
                FInt newBadness = (FInt)1000000;
                FakeGalaxy fg = MakeGrid(1, 1);
                for (int c = 1; c <= 30; ++c)
                {
                    int r1 = (c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false)).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(r, c);
                        g2.MakeRotationalGeneric(c * unit / 2, r * unit, unit, symmetry / 100, symmetry % 100 == 50, c % 2 == 1);
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
                return fg;
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2(unit * ((columns + 1) / 2));
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych(unit * (columns / 3));
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy(unit * ((columns + 1) / 2));
            }
            else if (symmetry == 10100)
            {
                g = MakeGridBordered(rows, columns, (Math.Min(rows, columns) + 3) / 4);
                g.MakeDuplexBarrier((FInt)2);
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }
            else if (symmetry == 10200)
            {
                columns = (columns + 3) / 4;
                g = MakeGrid(rows, columns);
                g.MakeY(aspectRatioEnum, unit, ((columns * 4 + 3) / 5) * unit);
            }

            return g;
        }

        protected static FakeGalaxy MakeGrid(int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));

            return g;
        }

        protected static FakeGalaxy MakeGridOctagonal(int rows, int columns, int octagonalSideLength)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    if ((i + j) < octagonalSideLength) continue;
                    if ((i + columns - 1 - j) < octagonalSideLength) continue;
                    if ((rows - 1 - i + j) < octagonalSideLength) continue;
                    if ((rows - 1 - i + columns - 1 - j) < octagonalSideLength) continue;
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));
                }

            return g;
        }

        protected static FakeGalaxy MakeGridCross(int rows, int columns, int crossWidth)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    if ((i * 2 < rows - crossWidth || i * 2 > rows + crossWidth - 2) && (j * 2 < columns - crossWidth || j * 2 > columns + crossWidth - 2)) continue;
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));
                }

            return g;
        }

        protected static FakeGalaxy MakeGridBordered(int rows, int columns, int borderWidth)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    if (i >= borderWidth && i < rows - borderWidth && j >= borderWidth && j < columns - borderWidth) continue;
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));
                }

            return g;
        }

        public static void TableGen()
        {
            var planetNumbers = new System.Collections.Generic.List<int> { 40, 42, 44, 46, 48 };
            for (int i = 50; i <= 300; i += 5) planetNumbers.Add(i);
            foreach (int desiredPlanets in planetNumbers)
            {
            }
        }
    }
}