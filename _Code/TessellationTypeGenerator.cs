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
            int additionalConnections = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "AdditionalConnections").RelatedIntValue;
            int traversability = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Traversability").RelatedIntValue;
            int connectivity = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Connectivity").RelatedIntValue;
            int wobble = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Wobble").RelatedIntValue;

            int numPlanetsToMake = numPlanets * 12 / (12 - dissonance);
            var randomNumberGenerator = Context.RandomToUse;

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

            // STEP 2 - MARK OUTER PATH FOR PERSERVATION
            // Mark outer path.
            // The marked planets would be prevented from any consideration in STEP 3.
            // And the links would be always included in STEP 6.
            var outline = new Outline(g.FindOutline());
            if (outerPath == 1)
            {
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

            // STEP 3 - DISSONANCE
            // Remove planets randomly, respecting symmetry and stick bits.
            if (dissonance > 0)
            {
                int retry = 0;
                while (g.planets.Count > numPlanets)
                {
                    SymmetricGroup s = g.symmetricGroups[randomNumberGenerator.Next(0, g.symmetricGroups.Count - 1)];
                    if (s.stick)
                    {
                        if (++retry == 1000) break;
                        continue;
                    }
                    g.RemoveSymmetricGroup(s);
                    retry = 0;
                }
            }

            // STEP 4 - CONNNECT
            // In case we cut off the graph, connect it back
            g.EnsureConnectivity();

            // STEP 5 - EXTRA LINKS
            // Make extra links available
            if (additionalConnections == 1)
            {
                g.AddExtraLinks(33, 0, randomNumberGenerator, outline);
            }
            else if (additionalConnections == 2)
            {
                g.AddExtraLinks(67, 0, randomNumberGenerator, outline);
            }
            else if (additionalConnections == 3)
            {
                g.AddExtraLinks(200, 0, randomNumberGenerator, outline);
            }
            else if (additionalConnections == 4)
            {
                g.AddExtraLinks(133, 1, randomNumberGenerator, outline);
            }
            else if (additionalConnections == 5)
            {
                g.AddExtraLinks(400, 1, randomNumberGenerator, outline);
            }
            else if (additionalConnections == 6)
            {
                g.AddExtraLinks(2000, 5, randomNumberGenerator, outline);
            }

            // STEP 6 - SKELETON
            // Select a subset of edges that'll be in the game
            g.MakeSpanningTree(traversability);

            // STEP 7 - FILL
            // Add edges until the desired density is reached
            g.AddEdges(connectivity, traversability);

            // STEP 8 - WOBBLE
            // Add random offsets to each planet, respecting symmetry
            g.Wobble(planetType, wobble, randomNumberGenerator);

            // STEP 9 - POPULATE
            // Translate our information into Arcenverse
            g.Populate(galaxy, planetType, randomNumberGenerator);
        }
    }
}