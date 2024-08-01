using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class DiamondYGrid
    {
        public static FakeGalaxy MakeDiamondYGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 7071 / 1000;
            int dunit = planetType.GetData().InterStellarRadius * 10;
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 60; r++)
            {
                for (int c = 2; c <= 60; c++)
                {
                    if (symmetry == 150 && c % 2 == 0) continue;
                    if (symmetry >= 300 && (r * 2 + c) % 4 != 3) continue;
                    // FIXME: only works when both are odd
                    int planets = 2 * r * c + r + c + 3;
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
            FakeGalaxy g = new FakeGalaxy();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> corners = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> centers = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> lwings = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> rwings = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();
            for (int i = 0; i < rows + 2; ++i)
            {
                for (int j = 0; j < columns + 2; ++j)
                {
                    if ((i + j) % 2 == 1 && (i > 0 && i <= rows || j > 0 && j <= columns))
                        corners[(i, j)] = g.AddPlanetAt(ArcenPoint.Create(j * 2 * unit, i * 2 * unit));
                    if ((i + j) % 2 == 0 && i > 0 && i <= rows && j > 0 && j <= columns)
                    {
                        centers[(i, j)] = g.AddPlanetAt(ArcenPoint.Create(j * 2 * unit, i * 2 * unit));
                        lwings[(i, j)] = g.AddPlanetAt(ArcenPoint.Create((j * 2 - 1) * unit, (i * 2 + 1) * unit));
                        rwings[(i, j)] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + 1) * unit, (i * 2 + 1) * unit));
                    }
                }
            }

            for (int i = 0; i < rows + 2; ++i)
            {
                for (int j = 0; j < columns + 2; ++j)
                {
                    if (corners.ContainsKey((i, j)))
                    {
                        if (rwings.ContainsKey((i, j - 1)))
                        {
                            corners[(i, j)].AddLinkTo(rwings[(i, j - 1)]);
                            corners[(i + 1, j - 1)].AddLinkTo(rwings[(i, j - 1)]);
                        }
                        else if (corners.ContainsKey((i + 1, j - 1)))
                        {
                            corners[(i, j)].AddLinkTo(corners[(i + 1, j - 1)]);
                        }
                        if (lwings.ContainsKey((i, j + 1)))
                        {
                            corners[(i, j)].AddLinkTo(lwings[(i, j + 1)]);
                            corners[(i + 1, j + 1)].AddLinkTo(lwings[(i, j + 1)]);
                        }
                        else if (corners.ContainsKey((i + 1, j + 1)))
                        {
                            corners[(i, j)].AddLinkTo(corners[(i + 1, j + 1)]);
                        }
                    }
                    if (centers.ContainsKey((i, j)))
                    {
                        centers[(i, j)].AddLinkTo(lwings[(i, j)]);
                        centers[(i, j)].AddLinkTo(rwings[(i, j)]);
                        centers[(i, j)].AddLinkTo(corners[(i - 1, j)]);
                    }
                }
            }

            if (symmetry == 150)
            {
                g.MakeBilateral();
            }
            else if (symmetry >= 300 && symmetry < 10000)
            {
                g.MakeRotationalGeneric((columns + 1) * unit, (rows + 1) * 2 * unit, dunit, symmetry / 100, symmetry % 100 == 50);
            }

            return g;
        }
    }
}