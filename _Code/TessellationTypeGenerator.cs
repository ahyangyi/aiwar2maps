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
            int mapShapeEnum = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "AspectRatio").RelatedIntValue;
            int dissonance = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue;

            int numPlanetsToMake = numPlanets * (11 + dissonance) / 11;

            FInt aspectRatio;
            if (mapShapeEnum == 0)
            {
                aspectRatio = FInt.FromParts(0, 625);
            }
            else if (mapShapeEnum == 1)
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
                g = MakeSquareGalaxy(planetType, aspectRatio, numPlanetsToMake);
            }
            else if (tessellation == 1)
            {
                g = MakeHexagonGalaxy(planetType, aspectRatio, numPlanetsToMake);
            }
            else if (tessellation == 2)
            {
                g = MakeTriangleGalaxy(planetType, aspectRatio, numPlanetsToMake);
            }
            else if (tessellation == 100)
            {
                g = MakeSquareYGalaxy(planetType, aspectRatio, numPlanetsToMake);
            }
            else if (tessellation == 101)
            {
                g = MakeSquareYMirrorGalaxy(planetType, aspectRatio, numPlanetsToMake);
            }
            else
            {
                g = MakeDiamondYGalaxy(planetType, aspectRatio, numPlanetsToMake);
            }

            int wobble = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Wobble").RelatedIntValue;
            g.Wobble(planetType, wobble, Context.RandomToUse);

            int planetsToRemove = dissonance == 0? 0 : g.planets.Count - numPlanets;
            for (int i = 0; i < planetsToRemove; ++i)
            {
                FakePlanet j = g.planets[Context.RandomToUse.Next(0, g.planets.Count - 1)];
                g.RemovePlanet(j);
            }

            g.Populate(galaxy, planetType, Context.RandomToUse);

            BadgerUtilityMethods.makeSureGalaxyIsFullyConnected(true, galaxy);
        }

        protected FakeGalaxy MakeSquareGalaxy(PlanetType planetType, FInt aspectRatio, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 10;
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 30; ++r)
            {
                for (int c = 2; c <= 30; ++c)
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
            return g;
        }

        protected FakeGalaxy MakeHexagonGalaxy(PlanetType planetType, FInt aspectRatio, int numPlanets)
        {
            int xunit = planetType.GetData().InterStellarRadius * 8660 / 1000;
            int yunit = planetType.GetData().InterStellarRadius * 5;
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 1; r <= 25; ++r)
            {
                for (int c = 1; c <= 40; ++c)
                {
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
            return g;
        }

        // FIXME move to seperate namespace/class
        readonly int[] dr = { 1, 2, 1, -1, -2, -1 };
        readonly int[] dc = { -1, 0, 1, 1, 0, -1 };
        protected FakeGalaxy MakeTriangleGalaxy(PlanetType planetType, FInt aspectRatio, int numPlanets)
        {
            int xunit = planetType.GetData().InterStellarRadius * 8660 / 1000;
            int yunit = planetType.GetData().InterStellarRadius * 5;
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 25; ++r)
            {
                for (int c = 2; c <= 40; ++c)
                {
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
            return g;
        }

        protected FakeGalaxy MakeSquareYGalaxy(PlanetType planetType, FInt aspectRatio, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 10;
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 20; ++r)
            {
                for (int c = 2; c <= 20; ++c)
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
            FakePlanet[][] bottoms = new FakePlanet[rows - 1][];
            for (int i = 0; i < rows; ++i)
            {
                corners[i] = new FakePlanet[columns];
                if (i + 1 < rows)
                {
                    centers[i] = new FakePlanet[columns - 1];
                    bottoms[i] = new FakePlanet[columns - 1];
                }
                for (int j = 0; j < columns; ++j)
                {
                    corners[i][j] = g.AddPlanetAt(ArcenPoint.Create(j * 2 * unit, (rows - i - 1) * 2 * unit));
                    if (i + 1 < rows && j + 1 < columns)
                    {
                        centers[i][j] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + 1) * unit, ((rows - i - 1) * 2 - 1) * unit));
                        bottoms[i][j] = g.AddPlanetAt(ArcenPoint.Create((j * 2 + 1) * unit, ((rows - i - 1) * 2 - 2) * unit));
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
                        centers[i][j].AddLinkTo(bottoms[i][j]);
                        corners[i + 1][j].AddLinkTo(bottoms[i][j]);
                        corners[i + 1][j + 1].AddLinkTo(bottoms[i][j]);
                    }
                }
            }
            return g;
        }
        protected FakeGalaxy MakeSquareYMirrorGalaxy(PlanetType planetType, FInt aspectRatio, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 10;
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 3; r <= 20; r += 2)
            {
                for (int c = 2; c <= 20; ++c)
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
            return g;
        }
        protected FakeGalaxy MakeDiamondYGalaxy(PlanetType planetType, FInt aspectRatio, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 7071 / 1000;
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 20; r++)
            {
                for (int c = 2; c <= 20; c++)
                {
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
            return g;
        }
    }
}