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
            int numberToSeed = mapConfig.GetClampedNumberOfPlanetsForMapType(mapType);
            int numberOfRows = 9;
            int numberOfColumns = 16;
            int distanceBetweenPoints = planetType.GetData().InterStellarRadius * 4;

            ArcenPoint[][] pointRows = new ArcenPoint[numberOfRows][];
            for (int i = 0; i < numberOfRows; ++i)
            {
                pointRows[i] = new ArcenPoint[numberOfColumns];
                for (int j = 0; j < numberOfColumns; ++j)
                {
                    pointRows[i][j] = ArcenPoint.Create(j * distanceBetweenPoints, i * distanceBetweenPoints);
                }
            }

            Planet[][] planetRows = new Planet[numberOfRows][];
            for (int i = 0; i < pointRows.Length; i++)
            {
                planetRows[i] = new Planet[pointRows[i].Length];
                for (int j = 0; j < pointRows[i].Length; j++)
                {
                    ArcenPoint point = pointRows[i][j];
                    planetRows[i][j] = galaxy.AddPlanet(planetType, point,
                        World_AIW2.Instance.GetPlanetGravWellSizeForPlanetType(Context.RandomToUse, PlanetPopulationType.None));
                    if (i - 1 >= 0)
                    {
                        planetRows[i][j].AddLinkTo(planetRows[i - 1][j]);
                    }
                    if (j - 1 >= 0)
                    {
                        planetRows[i][j].AddLinkTo(planetRows[i][j - 1]);
                    }
                }
            }

            // FIXME
            int randomExtraConnections = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue * 5;
            BadgerUtilityMethods.RandomlyConnectXPlanetsWithoutIntersectingOthers(galaxy, randomExtraConnections, 40, 20, Context);

            BadgerUtilityMethods.makeSureGalaxyIsFullyConnected(true, galaxy);
        }
    }
}