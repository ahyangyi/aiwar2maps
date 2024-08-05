using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class SquareYMirrorGrid
    {
        public static FakeGalaxy MakeGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 10;
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
            FakeGalaxy g = MakeGrid(unit, rows, columns);

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
                FakeGalaxy fg = MakeGrid(unit, 1, 1);
                for (int c = 4; c <= 60; ++c)
                {
                    int r1 = (c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false)).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(unit, r, c);
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
        private static FakeGalaxy MakeGrid(int unit, int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();

            FakePlanet[][] corners = new FakePlanet[rows][];
            FakePlanet[][] centers = new FakePlanet[rows - 1][];
            FakePlanet[][] bottoms = new FakePlanet[rows / 2][];
            for (int i = 0; i < rows; ++i)
            {
                corners[i] = new FakePlanet[columns];
                if (i + 1 < rows)
                {
                    centers[i] = new FakePlanet[columns - 1];
                }
                for (int j = 0; j < columns; ++j)
                {
                    corners[i][j] = g.AddPlanetAt(ArcenPoint.Create(j * 2 * unit, (rows - i - 1) * 2 * unit));
                    if (i + 1 < rows && j + 1 < columns)
                    {
                        centers[i][j] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + 1) * unit, ((rows - i - 1) * 2 - 1) * unit));
                    }
                }
            }
            for (int i = 0; i < rows / 2; ++i)
            {
                bottoms[i] = new FakePlanet[columns - 1];
                for (int j = 0; j < columns - 1; ++j)
                {
                    bottoms[i][j] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + 1) * unit, ((rows - i * 2 - 1) * 2 - 2) * unit));
                }
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (i + 1 < rows)
                    {
                        corners[i][j].AddLinkTo(corners[i + 1][j]);
                    }
                    if (i % 2 == 0 && j + 1 < columns)
                    {
                        corners[i][j].AddLinkTo(corners[i][j + 1]);
                    }
                    if (i + 1 < rows && j + 1 < columns)
                    {
                        corners[i + i % 2][j].AddLinkTo(centers[i][j]);
                        corners[i + i % 2][j + 1].AddLinkTo(centers[i][j]);
                        centers[i][j].AddLinkTo(bottoms[i / 2][j]);
                    }
                    if (i % 2 == 1 && j + 1 < columns)
                    {
                        corners[i][j].AddLinkTo(bottoms[i / 2][j]);
                        corners[i][j + 1].AddLinkTo(bottoms[i / 2][j]);
                    }
                }
            }

            return g;
        }
    }
}