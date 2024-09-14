using AhyangyiMaps.Tessellation;
using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;
using System.Collections.Generic;

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
        private static readonly FInt[] values = { FInt.Create(625, false), FInt.Create(1000, false), FInt.Create(1778, false) };
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
            int outerPath = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "OuterPath").RelatedIntValue;
            int wobble = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Wobble").RelatedIntValue;

            int numPlanetsToMake = numPlanets * 12 / (12 - dissonance);

            // STEP 1 - TESSELLATION
            // Generate a base grid
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

            if (outerPath == 1)
            {
                var outline = new HashSet<FakePlanet>(g.FindOutline());
                foreach (var sg in g.symmetricGroups)
                {
                    bool x = false;
                    foreach (var planet in sg.planets)
                        if (outline.Contains(planet))
                            x = true;
                    if (x)
                        sg.stick = true;
                }
            }
            else if (outerPath == 2)
            {
                g.MakeBeltWay();
            }

            // STEP 2 - DISSONANCE
            // Remove planets randomly, respecting symmetry and stick bits.
            if (dissonance > 0)
            {
                int retry = 0;
                while (g.planets.Count > numPlanets)
                {
                    SymmetricGroup s = g.symmetricGroups[Context.RandomToUse.Next(0, g.symmetricGroups.Count - 1)];
                    if (s.stick)
                    {
                        if (++retry == 1000) break;
                        continue;
                    }
                    g.RemoveSymmetricGroup(s);
                    retry = 0;
                }
            }

            // STEP 3 - CONNNECT
            // In case we cut off the graph, connect it back
            g.EnsureConnectivity();

            // STEP 4 - EXTRA LINKS
            // TODO
            // Make extra links available

            // STEP 5 - SKELETON
            // TODO
            // Select a subset of edges that'll be in the game

            // STEP 6 - FILL
            // TODO
            // Add edges until the desired density is reached

            // STEP 7 - WOBBLE
            // Add random offsets to each planet, respecting symmetry
            g.Wobble(planetType, wobble, Context.RandomToUse);

            // STEP 8 - POPULATE
            // Translate our information into Arcenverse
            g.Populate(galaxy, planetType, Context.RandomToUse);
        }
    }
}