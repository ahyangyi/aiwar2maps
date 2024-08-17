using AhyangyiMaps.Tessellation;
using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;

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

        private readonly FInt[] aspectRatios = { FInt.Create(625, false), FInt.Create(1000, false), FInt.Create(1778, false) };

        protected void InnerGenerate(Galaxy galaxy, ArcenHostOnlySimContext Context, MapConfiguration mapConfig, PlanetType planetType, MapTypeData mapType)
        {
            int numPlanets = mapConfig.GetClampedNumberOfPlanetsForMapType(mapType);
            int tessellation = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Tessellation").RelatedIntValue;
            int aspectRatioEnum = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "AspectRatio").RelatedIntValue;
            int galaxyShape = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "GalaxyShape").RelatedIntValue;
            int dissonance = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue;
            int symmetry = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Symmetry").RelatedIntValue;

            int numPlanetsToMake = numPlanets * 12 / (12 - dissonance);

            FInt aspectRatio = aspectRatios[aspectRatioEnum];
            FakeGalaxy g;
            if (tessellation == 0)
            {
                g = SquareGrid.MakeSquareGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 1)
            {
                g = HexagonGrid.MakeGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 2)
            {
                g = TriangleGrid.MakeGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 100)
            {
                g = SquareYGrid.MakeGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 101)
            {
                g = SquareYMirrorGrid.MakeGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 102)
            {
                g = DiamondYGrid.MakeGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }
            else
            {
                g = DiamondYFlowerGrid.MakeGalaxy(planetType, aspectRatio, galaxyShape, symmetry, numPlanetsToMake);
            }

            g.MakeSymmetricGroups();

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
            g.EnsureConnectivity();
            g.Populate(galaxy, planetType, Context.RandomToUse);
        }
    }
}