using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class SquareYMirrorGrid
    {
        static readonly int unit;
        static readonly FakePattern squareY, squareYFlipped;
        static SquareYMirrorGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 10;
            squareY = SquareYGrid.squareY;
            squareYFlipped = SquareYGrid.squareYFlipped;
        }
        public static FakeGalaxy MakeGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 3; r <= 70; r += 2)
            {
                for (int c = 2; c <= 70; ++c)
                {
                    int planets = r * c + (r - 1) * (c - 1) + (r / 2) * (c - 1);
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = (FInt)(r - 1) / (FInt)(c - 1);
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
                for (int c = 4; c <= 60; ++c)
                {
                    int r1 = (c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false)).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(r, c);
                        g2.MakeRotationalGeneric((c - 1) * unit, (c % 2 == 0 ? ((r - 1) * 2 - 1) * unit : (r - 1) * 2 * unit), unit, symmetry / 100, symmetry % 100 == 50);
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

            for (int i = 0; i < rows - 1; ++i)
                for (int j = 0; j < columns - 1; ++j)
                    (i % 2 == 0 ? squareYFlipped : squareY).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
    }
}