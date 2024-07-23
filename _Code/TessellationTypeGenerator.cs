using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;
using System;

namespace AhyangyiMaps
{
    public class TessellationTypeGenerator : IMapGenerator
    {
        public TessellationTypeGenerator()
        {
        }

        public void ClearAllMyDataForQuitToMainMenuOrBeforeNewMap()
        {

        }

        public void GenerateMapStructureOnly(Galaxy galaxy, ArcenHostOnlySimContext context, MapConfiguration mapConfig, MapTypeData mapType)
        {
            this.InnerGenerate(galaxy, context, mapConfig, PlanetType.Normal, mapType);
        }

        protected void InnerGenerate(Galaxy galaxy, ArcenHostOnlySimContext Context, MapConfiguration mapConfig, PlanetType planetType, MapTypeData mapType)
        {
            int numPlanets = mapConfig.GetClampedNumberOfPlanetsForMapType(mapType);
            int tessellation = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Tessellation").RelatedIntValue;
            int aspectRatioEnum = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "AspectRatio").RelatedIntValue;
            int galaxyShape = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "GalaxyShape").RelatedIntValue;
            int dissonance = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue;
            int symmetry = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Symmetry").RelatedIntValue;

            int numPlanetsToMake = numPlanets * 12 / (12 - dissonance);

            FInt aspectRatio;
            if (aspectRatioEnum == 0)
            {
                aspectRatio = FInt.FromParts(0, 625);
            }
            else if (aspectRatioEnum == 1)
            {
                aspectRatio = FInt.FromParts(1, 0);
            }
            else
            {
                aspectRatio = FInt.FromParts(1, 778);
            }
            FakeGalaxy g;
            if (tessellation == 0)
            {
                g = MakeSquareGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 1)
            {
                g = MakeHexagonGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 2)
            {
                g = MakeTriangleGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 100)
            {
                g = MakeSquareYGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 101)
            {
                g = MakeSquareYMirrorGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 102)
            {
                g = MakeDiamondYGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else
            {
                g = MakeDiamondYFlowerGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }

            int wobble = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Wobble").RelatedIntValue;
            g.Wobble(planetType, wobble, Context.RandomToUse);

            if (dissonance > 0)
            {
                while (g.planets.Count > numPlanets)
                {
                    SymmetricGroup s = g.symmetricGroups[Context.RandomToUse.Next(0, g.symmetricGroups.Count - 1)];
                    g.RemoveSymmetricGroup(s);
                }
            }

            g.Populate(galaxy, planetType, Context.RandomToUse);

            BadgerUtilityMethods.makeSureGalaxyIsFullyConnected(true, galaxy);
        }

        protected FakeGalaxy MakeSquareGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
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
            else if (symmetry == 300)
            {
                g.MakeRotational3((columns - 1) * unit / 2, (rows - 1) * unit);
            }
            return g;
        }

        protected FakeGalaxy MakeHexagonGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int xunit = planetType.GetData().InterStellarRadius * 8660 / 1000;
            int yunit = planetType.GetData().InterStellarRadius * 5;
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
            FakeGalaxy g = new FakeGalaxy();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> uppy = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> downy = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();

            for (int i = 0; i <= rows; ++i)
            {
                for (int j = -i % 2; j <= columns + 1; j += 2)
                {
                    if (i >= 0 && i < rows && j >= 0 && j < columns ||
                        i >= 0 && i < rows && j - 2 >= 0 && j - 2 < columns ||
                        i - 1 >= 0 && i - 1 < rows && j - 1 >= 0 && j - 1 < columns)
                        uppy[(i, j)] = g.AddPlanetAt(ArcenPoint.Create(j * xunit, (i * 3 + 1) * yunit));
                    if (i >= 0 && i < rows && j >= 0 && j < columns ||
                        i - 1 >= 0 && i - 1 < rows && j - 1 >= 0 && j - 1 < columns ||
                        i - 1 >= 0 && i - 1 < rows && j + 1 >= 0 && j + 1 < columns)
                        downy[(i, j)] = g.AddPlanetAt(ArcenPoint.Create((j + 1) * xunit, (i * 3) * yunit));
                }
            }

            for (int i = 0; i <= rows; ++i)
            {
                for (int j = -i % 2; j <= columns + 1; j += 2)
                {
                    if (uppy.ContainsKey((i, j)))
                    {
                        if (downy.ContainsKey((i, j)))
                        {
                            uppy[(i, j)].AddLinkTo(downy[(i, j)]);
                        }
                        if (downy.ContainsKey((i, j - 2)))
                        {
                            uppy[(i, j)].AddLinkTo(downy[(i, j - 2)]);
                        }
                        if (downy.ContainsKey((i + 1, j - 1)))
                        {
                            uppy[(i, j)].AddLinkTo(downy[(i + 1, j - 1)]);
                        }
                    }
                }
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
            return g;
        }

        protected FakeGalaxy MakeTriangleGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int[] dr = { 1, 2, 1, -1, -2, -1 };
            int[] dc = { -1, 0, 1, 1, 0, -1 };
            int xunit = planetType.GetData().InterStellarRadius * 8660 / 1000;
            int yunit = planetType.GetData().InterStellarRadius * 5;
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 70; ++r)
            {
                for (int c = 2; c <= 120; ++c)
                {
                    if (symmetry == 150 && c % 2 == 0) continue;
                    if (symmetry == 200 && (r + c) % 2 == 1) continue;
                    if (symmetry == 250 && (r % 2 == 0 || c % 2 == 0)) continue;
                    // FIXME: rough estimation
                    int planets = (r * c) / 2;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = ((FInt)r - 1) * yunit / (((FInt)c - 1) * xunit);
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
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> points = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();

            for (int i = 0; i < rows; ++i)
            {
                for (int j = (i + 1) % 2; j < columns; j += 2)
                {
                    points[(i, j)] = g.AddPlanetAt(ArcenPoint.Create(j * xunit, i * yunit));
                }
            }

            for (int i = 0; i < rows; ++i)
            {
                for (int j = (i + 1) % 2; j < columns; j += 2)
                {
                    for (int d = 0; d < 6; ++d)
                    {
                        if (i + dr[d] >= 0 && i + dr[d] < rows && j + dc[d] >= 0 && j + dc[d] < columns)
                        {
                            points[(i, j)].AddLinkTo(points[(i + dr[d], j + dc[d])]);
                        }
                    }
                }
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
            return g;
        }

        protected FakeGalaxy MakeSquareYGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 10;
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 60; ++r)
            {
                for (int c = 2; c <= 60; ++c)
                {
                    int planets = r * c + (r - 1) * (c - 1) * 2;
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
            FakeGalaxy g = new FakeGalaxy();

            FakePlanet[][] corners = new FakePlanet[rows][];
            FakePlanet[][] centers = new FakePlanet[rows - 1][];
            FakePlanet[][] bases = new FakePlanet[rows - 1][];
            for (int i = 0; i < rows; ++i)
            {
                corners[i] = new FakePlanet[columns];
                if (i + 1 < rows)
                {
                    centers[i] = new FakePlanet[columns - 1];
                    bases[i] = new FakePlanet[columns - 1];
                }
                for (int j = 0; j < columns; ++j)
                {
                    corners[i][j] = g.AddPlanetAt(ArcenPoint.Create(j * 2 * unit, (rows - i - 1) * 2 * unit));
                    if (i + 1 < rows && j + 1 < columns)
                    {
                        centers[i][j] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + 1) * unit, ((rows - i - 1) * 2 - 1) * unit));
                        bases[i][j] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + 1) * unit, ((rows - i - 1) * 2 - 2) * unit));
                    }
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
                    if (i == 0 && j + 1 < columns)
                    {
                        corners[i][j].AddLinkTo(corners[i][j + 1]);
                    }
                    if (i + 1 < rows && j + 1 < columns)
                    {
                        corners[i][j].AddLinkTo(centers[i][j]);
                        corners[i][j + 1].AddLinkTo(centers[i][j]);
                        centers[i][j].AddLinkTo(bases[i][j]);
                        corners[i + 1][j].AddLinkTo(bases[i][j]);
                        corners[i + 1][j + 1].AddLinkTo(bases[i][j]);
                    }
                }
            }

            if (symmetry == 150)
            {
                g.MakeBilateral();
            }
            else if (symmetry == 300)
            {
                g.MakeRotational3(columns * unit, rows * unit * 2);
            }

            return g;
        }
        protected FakeGalaxy MakeSquareYMirrorGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
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
            else if (symmetry == 300)
            {
                g.MakeRotational3(columns * unit, rows * unit * 2);
            }

            return g;
        }
        protected FakeGalaxy MakeDiamondYGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 7071 / 1000;
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 60; r++)
            {
                for (int c = 2; c <= 60; c++)
                {
                    if (symmetry == 150 && c % 2 == 0) continue;
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

            return g;
        }
        protected FakeGalaxy MakeDiamondYFlowerGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            // FIXME move to seperate namespace/class
            int[] dr = { 1, 2, 1, 0, -1, -2, -1, 0 };
            int[] dc = { 1, 0, -1, -2, -1, 0, 1, 2 };

            int unit = planetType.GetData().InterStellarRadius * 7071 / 1000;
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
                    // FIXME: simple estimation
                    int planets = r * c * 2;
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
            System.Collections.Generic.Dictionary<(int, int), FakePlanet> points = new System.Collections.Generic.Dictionary<(int, int), FakePlanet>();
            for (int i = 0; i < rows + 2; ++i)
            {
                for (int j = 0; j < columns + 2; ++j)
                {
                    if ((i + j) % 2 == 0 && (i > 0 && i <= rows || j > 0 && j <= columns))
                    {
                        points[(i * 2, j * 2)] = g.AddPlanetAt(ArcenPoint.Create(j * 2 * unit, i * 2 * unit));
                    }
                }
            }
            for (int i = 0; i < rows + 2; ++i)
            {
                for (int j = 0; j < columns + 2; ++j)
                {
                    if (i % 2 == 0 && j % 2 == 0 && points.ContainsKey((i * 2, j * 2)) && (i + j) % 4 == 0)
                    {
                        for (int d = 0; d < 8; ++d)
                        {
                            if (points.ContainsKey((i * 2 + dr[d] * 2, j * 2 + dc[d] * 2))
                                && (points.ContainsKey((i * 2 + dr[(d + 1) % 8] * 2, j * 2 + dc[(d + 1) % 8] * 2))
                                && points.ContainsKey((i * 2 + dr[(d + 7) % 8] * 2, j * 2 + dc[(d + 7) % 8] * 2)) || d % 2 == 0))
                            {
                                points[(i * 2 + dr[d], j * 2 + dc[d])] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + dc[d]) * unit, (i * 2 + dr[d]) * unit));
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < rows + 2; ++i)
            {
                for (int j = 0; j < columns + 2; ++j)
                {
                    if ((i + j) % 2 == 0 && points.ContainsKey((i * 2, j * 2)))
                    {
                        if (points.ContainsKey((i * 2 + 1, j * 2 - 1)))
                        {
                            points[(i * 2, j * 2)].AddLinkTo(points[(i * 2 + 1, j * 2 - 1)]);
                            if (points.ContainsKey((i * 2 + 2, j * 2 - 2)))
                                points[(i * 2 + 1, j * 2 - 1)].AddLinkTo(points[(i * 2 + 2, j * 2 - 2)]);
                        }
                        else if (points.ContainsKey((i * 2 + 2, j * 2 - 2)))
                        {
                            points[(i * 2, j * 2)].AddLinkTo(points[(i * 2 + 2, j * 2 - 2)]);
                        }
                        if (points.ContainsKey((i * 2 + 1, j * 2 + 1)))
                        {
                            points[(i * 2, j * 2)].AddLinkTo(points[(i * 2 + 1, j * 2 + 1)]);
                            if (points.ContainsKey((i * 2 + 2, j * 2 + 2)))
                                points[(i * 2 + 1, j * 2 + 1)].AddLinkTo(points[(i * 2 + 2, j * 2 + 2)]);
                        }
                        else if (points.ContainsKey((i * 2 + 2, j * 2 + 2)))
                        {
                            points[(i * 2, j * 2)].AddLinkTo(points[(i * 2 + 2, j * 2 + 2)]);
                        }

                        if (i % 2 == 0 && j % 2 == 0 && (i + j) % 4 == 0)
                        {
                            for (int d = 0; d < 8; ++d)
                            {
                                if (points.ContainsKey((i * 2 + dr[d], j * 2 + dc[d])))
                                {
                                    if (d % 2 == 1)
                                    {
                                        points[(i * 2 + dr[d], j * 2 + dc[d])].AddLinkTo(points[(i * 2 + dr[d] * 2, j * 2 + dc[d] * 2)]);
                                    }
                                    if (points.ContainsKey((i * 2 + dr[(d + 1) % 8], j * 2 + dc[(d + 1) % 8])))
                                    {
                                        points[(i * 2 + dr[d], j * 2 + dc[d])].AddLinkTo(points[(i * 2 + dr[(d + 1) % 8], j * 2 + dc[(d + 1) % 8])]);
                                    }
                                }
                            }
                        }
                    }
                }
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

            return g;
        }
    }
}