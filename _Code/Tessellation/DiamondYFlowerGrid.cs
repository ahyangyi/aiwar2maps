using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class DiamondYFlowerGrid : GridGenerator
    {
        static readonly int unit, dunit;
        public static readonly FakePattern diamondY, diamondYFlipped, diamondYLeft, diamondYRight;
        static DiamondYFlowerGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 7071 / 1000;
            dunit = PlanetType.Normal.GetData().InterStellarRadius * 10;

            diamondY = DiamondYGrid.diamondY;
            diamondYFlipped = DiamondYGrid.diamondYFlipped;
            diamondYLeft = diamondY.RotateLeft();
            diamondYRight = diamondYFlipped.RotateLeft();
        }

        public (FakeGalaxy, FakeGalaxy) MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, int dissonance, int numPlanets, ParameterService par)
        {
            var aspectRatioEnum = (AspectRatio)aspectRatioIndex;
            numPlanets = numPlanets * 12 / (12 - dissonance);
            FInt aspectRatio = 1 / aspectRatioEnum.Value();
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
                    if (symmetry == 10000 && c % 8 != 7) continue;
                    if (symmetry == 10101 && c % 4 != 3) continue;
                    // FIXME: simple estimation
                    int planets = r * c * 2;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = (FInt)(r + 1) / (FInt)(c + 1);
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
                for (int c = 3; c <= 60; c += 4)
                {
                    int r1 = (((c + 1) / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) - 1) / 2).ToInt();
                    for (int r = Math.Max(r1 * 2 - 1, 1); r <= r1 * 2 + 1; r += 2)
                    {
                        var g2 = MakeGrid(r, c);
                        g2.MakeRotationalGeneric((c + 1) * unit, (r + 1) * 2 * unit, dunit, symmetry / 100, symmetry % 100 == 50, false);
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
                g.MakeTranslational2((columns + 1) * unit);
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
                    if ((i + j) % 2 == 1)
                        (i % 2 == 0 ? ((i + j) % 4 == 1 ? diamondY : diamondYFlipped) : ((i + j) % 4 == 1 ? diamondYRight : diamondYLeft)).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
    }
}