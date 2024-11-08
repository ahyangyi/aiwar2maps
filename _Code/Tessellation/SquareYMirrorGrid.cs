using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class SquareYMirrorGrid : GridGenerator
    {
        static readonly int unit;
        static readonly FakePattern squareY, squareYFlipped;
        static SquareYMirrorGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 10;
            squareY = SquareYGrid.squareY;
            squareYFlipped = SquareYGrid.squareYFlipped;
        }
        public (FakeGalaxy, FakeGalaxy) MakeGrid(int outerPath, AspectRatio aspectRatioEnum, int galaxyShape, int symmetry, int dissonance, int numPlanets, ParameterService par)
        {
            numPlanets = numPlanets * 12 / (12 - dissonance);
            FInt aspectRatio = 1 / aspectRatioEnum.Value();
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 70; r += 2)
            {
                for (int c = 2; c <= 70; ++c)
                {
                    if (symmetry == 10001 && c % 3 != 0) continue;
                    if (symmetry == 10002 && c % 2 == 1) continue;
                    int planets = (r + 1) * (c + 1) + r * c + ((r + 1) / 2) * c;
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
                for (int c = 4; c <= 60; ++c)
                {
                    int r1 = (c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false)).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(r, c);
                        g2.MakeRotationalGeneric((c - 1) * unit, (c % 2 == 0 ? ((r - 1) * 2 - 1) * unit : (r - 1) * 2 * unit), unit, symmetry / 100, symmetry % 100 == 50);
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
                g.MakeTranslational2(unit * 2 * ((columns + 1) / 2));
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych(unit * 2 * (columns / 3));
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy(unit * 2 * ((columns + 1) / 2));
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
                    (i % 2 == 0 ? squareYFlipped : squareY).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
    }
}