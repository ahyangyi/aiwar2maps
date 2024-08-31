using AhyangyiMaps.Tessellation;
using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;

namespace AhyangyiMaps
{
    public enum AspectRatio
    {
        SIXTEEN_TO_NINE = 0,
        SQUARE = 1,
        NINE_TO_SIXTEEN = 2,
    }
    public static class AspectRatioExtensions
    {
        private static FInt[] values = { FInt.Create(625, false), FInt.Create(1000, false), FInt.Create(1778, false) };
        public static FInt Value(this AspectRatio aspectRatio)
        {
            return values[(int)aspectRatio];
        }
    }

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
            AspectRatio aspectRatioEnum = (AspectRatio)BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "AspectRatio").RelatedIntValue;
            int galaxyShape = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "GalaxyShape").RelatedIntValue;
            int dissonance = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue;
            int symmetry = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Symmetry").RelatedIntValue;

            int numPlanetsToMake = numPlanets * 12 / (12 - dissonance);

            FakeGalaxy g;
            if (tessellation == 0)
            {
                g = SquareGrid.MakeSquareGalaxy(planetType, aspectRatioEnum, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 1)
            {
                g = HexagonGrid.MakeGalaxy(planetType, aspectRatioEnum, galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 2)
            {
                g = TriangleGrid.MakeGalaxy(planetType, aspectRatioEnum.Value(), galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 100)
            {
                g = SquareYGrid.MakeGalaxy(planetType, aspectRatioEnum.Value(), galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 101)
            {
                g = SquareYMirrorGrid.MakeGalaxy(planetType, aspectRatioEnum.Value(), galaxyShape, symmetry, numPlanetsToMake);
            }
            else if (tessellation == 102)
            {
                g = DiamondYGrid.MakeGalaxy(planetType, aspectRatioEnum, galaxyShape, symmetry, numPlanetsToMake);
            }
            else
            {
                g = DiamondYFlowerGrid.MakeGalaxy(planetType, aspectRatioEnum.Value(), galaxyShape, symmetry, numPlanetsToMake);
            }

            g.MakeSymmetricGroups();
            if (dissonance > 0)
            {
                while (g.planets.Count > numPlanets)
                {
                    SymmetricGroup s = g.symmetricGroups[Context.RandomToUse.Next(0, g.symmetricGroups.Count - 1)];
                    g.RemoveSymmetricGroup(s);
                }
            }
            g.EnsureConnectivity();

            int wobble = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Wobble").RelatedIntValue;
            g.Wobble(planetType, wobble, Context.RandomToUse);

            g.Populate(galaxy, planetType, Context.RandomToUse);
        }
    }
}