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

        public void GenerateMapStructureOnly(Galaxy galaxy, ArcenHostOnlySimContext Context, MapConfiguration mapConfig, MapTypeData mapType)
        {
            if (MapgenLogger.IsActive)
                MapgenLogger.Log("GenerateMapStructureOnly: Mapgen_Honeycomb : " + galaxy.GetTotalPlanetCount() + " planets at start.");

            this.InnerGenerate(galaxy, Context, mapConfig, PlanetType.Normal, mapType);
        }

        protected void InnerGenerate(Galaxy galaxy, ArcenHostOnlySimContext Context, MapConfiguration mapConfig, PlanetType planetType, MapTypeData mapType)
        {
            int numPlanets = mapConfig.GetClampedNumberOfPlanetsForMapType(mapType);
            int tessellation = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Tessellation").RelatedIntValue;
            int mapShapeEnum = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "AspectRatio").RelatedIntValue;
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
                g = MakeSquareGalaxy(planetType, aspectRatio, numPlanets);
            }
            else if (tessellation == 1)
            {
                g = MakeHexagonGalaxy(planetType, aspectRatio, numPlanets);
            }
            else
            {
                g = MakeSquareYGalaxy(planetType, aspectRatio, numPlanets);
            }
            g.Populate(galaxy, planetType, Context.RandomToUse);

            // FIXME
            int randomExtraConnections = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue * 5;
            BadgerUtilityMethods.RandomlyConnectXPlanetsWithoutIntersectingOthers(galaxy, randomExtraConnections, 40, 20, Context);

            BadgerUtilityMethods.makeSureGalaxyIsFullyConnected(true, galaxy);
        }

        protected FakeGalaxy MakeSquareGalaxy(PlanetType planetType, FInt aspectRatio, int numPlanets)
        {
            int unit = planetType.GetData().InterStellarRadius * 10;
            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            for (int r = 1; r <= 30; ++r)
            {
                for (int c = 1; c <= 30; ++c)
                {
                    int planets = r * c;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = (FInt)r / (FInt)c;
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
            System.Collections.Generic.Dictionary<Pair<int, int>, FakePlanet> uppy = new System.Collections.Generic.Dictionary<Pair<int, int>, FakePlanet>();
            System.Collections.Generic.Dictionary<Pair<int, int>, FakePlanet> downy = new System.Collections.Generic.Dictionary<Pair<int, int>, FakePlanet>();

            for (int i = 0; i <= rows; ++i)
            {
                for (int j = -i % 2; j <= columns + 1; j += 2)
                {
                    if (i >= 0 && i < rows && j >= 0 && j < columns ||
                        i >= 0 && i < rows && j - 2 >= 0 && j - 2 < columns ||
                        i - 1 >= 0 && i - 1 < rows && j - 1 >= 0 && j - 1 < columns)
                        uppy[Pair<int, int>.Create(i, j)] = g.AddPlanetAt(ArcenPoint.Create(j * xunit, (i * 3 + 1) * yunit));
                    if (i >= 0 && i < rows && j >= 0 && j < columns ||
                        i - 1 >= 0 && i - 1 < rows && j - 1 >= 0 && j - 1 < columns ||
                        i - 1 >= 0 && i - 1 < rows && j + 1 >= 0 && j + 1 < columns)
                        downy[Pair<int, int>.Create(i, j)] = g.AddPlanetAt(ArcenPoint.Create((j + 1) * xunit, (i * 3) * yunit));
                }
            }

            for (int i = 0; i <= rows; ++i)
            {
                for (int j = -i % 2; j <= columns + 1; j += 2)
                {
                    if (uppy.ContainsKey(Pair<int, int>.Create(i, j)))
                    {
                        if (downy.ContainsKey(Pair<int, int>.Create(i, j)))
                        {
                            uppy[Pair<int, int>.Create(i, j)].AddLinkTo(downy[Pair<int, int>.Create(i, j)]);
                        }
                        if (downy.ContainsKey(Pair<int, int>.Create(i, j - 2)))
                        {
                            uppy[Pair<int, int>.Create(i, j)].AddLinkTo(downy[Pair<int, int>.Create(i, j - 2)]);
                        }
                        if (downy.ContainsKey(Pair<int, int>.Create(i + 1, j - 1)))
                        {
                            uppy[Pair<int, int>.Create(i, j)].AddLinkTo(downy[Pair<int, int>.Create(i + 1, j - 1)]);
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
            for (int r = 1; r <= 20; ++r)
            {
                for (int c = 1; c <= 20; ++c)
                {
                    int planets = r * c + (r - 1) * (c - 1) * 2;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = (FInt)r / (FInt)c;
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
    }
}