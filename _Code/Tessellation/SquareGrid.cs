using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class SquareGrid
    {
        public static FakeGalaxy MakeSquareGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 10;
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 100; ++r)
            {
                for (int c = 2; c <= 100; ++c)
                {
                    int planets = r * c;
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
            var g = MakeSquareGrid(unit, rows, columns);

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
                FakeGalaxy fg = MakeSquareGrid(unit, 1, 1);
                for (int c = 1; c <= 100; ++c)
                {
                    int r1 = (c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false)).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeSquareGrid(unit, r, c);
                        g2.MakeRotationalGeneric((c - 1) * unit / 2, (r - 1) * unit, unit, symmetry / 100, symmetry % 100 == 50, c % 2 == 0);
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

        protected static FakeGalaxy MakeSquareGrid(int unit, int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();

            FakePlanet[][] pointRows = new FakePlanet[rows][];
            for (int i = 0; i < rows; ++i)
            {
                pointRows[i] = new FakePlanet[columns];
                for (int j = 0; j < columns; ++j)
                {
                    pointRows[i][j] = g.AddPlanetAt(ArcenPoint.Create(j * unit, i * unit));
                }
            }

            for (int i = 0; i < pointRows.Length; i++)
            {
                for (int j = 0; j < pointRows[i].Length; j++)
                {
                    if (i - 1 >= 0)
                    {
                        pointRows[i][j].AddLinkTo(pointRows[i - 1][j]);
                    }
                    if (j - 1 >= 0)
                    {
                        pointRows[i][j].AddLinkTo(pointRows[i][j - 1]);
                    }
                }
            }

            return g;
        }
    }
}