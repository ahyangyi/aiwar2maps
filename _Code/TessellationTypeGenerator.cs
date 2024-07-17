using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;
using System;
using static Arcen.AIW2.External.TextVarMap;
using System.IO;

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
            int numberToSeed = mapConfig.GetClampedNumberOfPlanetsForMapType(mapType);
            int numberOfRows = 9;
            int numberOfColumns = 16;
            int distanceBetweenPoints = planetType.GetData().InterStellarRadius * 4;
            FakeGalaxy g = new FakeGalaxy();

            FakePlanet[][] pointRows = new FakePlanet[numberOfRows][];
            for (int i = 0; i < numberOfRows; ++i)
            {
                pointRows[i] = new FakePlanet[numberOfColumns];
                for (int j = 0; j < numberOfColumns; ++j)
                {
                    pointRows[i][j] = g.AddPlanetAt(ArcenPoint.Create(j * distanceBetweenPoints, i * distanceBetweenPoints));
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

            g.Populate(galaxy, planetType, Context.RandomToUse);

            // FIXME
            int randomExtraConnections = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue * 5;
            BadgerUtilityMethods.RandomlyConnectXPlanetsWithoutIntersectingOthers(galaxy, randomExtraConnections, 40, 20, Context);

            BadgerUtilityMethods.makeSureGalaxyIsFullyConnected(true, galaxy);
        }
    }
}