using AhyangyiMaps.Tessellation;
using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;
using System.Collections.Generic;
using System.Linq;

namespace AhyangyiMaps
{
    public enum AspectRatio
    {
        SIXTEEN_TO_NINE = 0,
        SQUARE = 1,
        NINE_TO_SIXTEEN = 2,
        COUNT = 3,
    }
    public static class AspectRatioExtensions
    {
        private static readonly FInt[] values = { FInt.Create(1778, false), FInt.Create(1000, false), FInt.Create(563, false) };
        public static FInt Value(this AspectRatio aspectRatio)
        {
            return values[(int)aspectRatio];
        }
    }

    interface GridGenerator
    {
        (FakeGalaxy, FakeGalaxy) MakeGrid(
            int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, int dissonance, int numPlanets, ParameterService par);
    }

    public class TessellationTypeGenerator : IMapGenerator
    {
        public const int MAX_PLANETS = 300;
        public const int DISSONANCE_TYPES = 5;
        public const int OUTER_PATH_TYPES = 3;
        public const int ASPECT_RATIO_TYPES = (int)AspectRatio.COUNT;
        static System.Collections.Generic.Dictionary<int, GridGenerator> GridGenerators;
        static TessellationTypeGenerator()
        {
            GridGenerators = new System.Collections.Generic.Dictionary<int, GridGenerator> {
                { 0, new SquareGrid() },
                { 1, new HexagonGrid() },
                { 2, new TriangleGrid() },
                { 100, new SquareYGrid() },
                { 101, new SquareYMirrorGrid() },
                { 102, new DiamondYGrid() },
                { 103, new DiamondYFlowerGrid() },
            };
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
            int tableGen = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "TableGen").RelatedIntValue;
            int numPlanets = mapConfig.GetClampedNumberOfPlanetsForMapType(mapType);
            int tessellation = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Tessellation").RelatedIntValue;
            int aspectRatioIndex = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "AspectRatio").RelatedIntValue;
            int galaxyShape = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "GalaxyShape").RelatedIntValue;
            int dissonance = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Dissonance").RelatedIntValue;
            int symmetry = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Symmetry").RelatedIntValue;
            int outerPath = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "OuterPath").RelatedIntValue;
            int additionalConnections = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "AdditionalConnections").RelatedIntValue;
            int traversability = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Traversability").RelatedIntValue;
            int connectivity = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Connectivity").RelatedIntValue;
            int wobble = BadgerUtilityMethods.getSettingValueMapSettingOptionChoice_Expensive(mapConfig, "Wobble").RelatedIntValue;

            var randomNumberGenerator = Context.RandomToUse;

            // STEP 1 - TESSELLATION
            // Generate a base grid
            // Some outerPath values or grid types might demand certain planets and links be preserved,
            //   this information is represented as the FakeGalaxy p
            FakeGalaxy g, p;
            GenerateGrid(tableGen, numPlanets, tessellation, aspectRatioIndex, galaxyShape, dissonance, symmetry, outerPath, out g, out p);

            // STEP 2 - MARK OUTER PATH FOR PERSERVATION
            // Mark outer path.
            // The marked planets would be prevented from any consideration in STEP 3.
            // And the links would be always included in STEP 6.
            var keptGroups = new HashSet<SymmetricGroup>(g.symmetricGroups.Where(x => x.planets.Any(planet => p.planetCollection.planets.Contains(planet))).ToList());

            // STEP 3 - DISSONANCE
            // Remove planets randomly, respecting symmetry and stick bits.
            // Also, preserve outline for Step 5
            var outline = new Outline(g.FindOutline());
            if (dissonance > 0)
            {
                int retry = 0;
                while (g.planets.Count > numPlanets)
                {
                    SymmetricGroup s = g.symmetricGroups[randomNumberGenerator.NextInclus(0, g.symmetricGroups.Count - 1)];
                    if (keptGroups.Contains(s))
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
            var spanningGraph = g.MakeSpanningGraph(traversability, randomNumberGenerator, p);

            // STEP 7 - FILL
            // Add edges until the desired density is reached
            g.AddEdges(spanningGraph, connectivity, traversability, randomNumberGenerator);
            g = spanningGraph;

            // STEP 8 - WOBBLE
            // Add random offsets to each planet, respecting symmetry
            g.Wobble(planetType, wobble, randomNumberGenerator);

            // STEP 9 - POPULATE
            // Translate our information into Arcenverse
            g.Populate(galaxy, planetType, randomNumberGenerator);
        }

        private static void GenerateGrid(int tableGen, int numPlanets, int tessellation, int aspectRatioIndex, int galaxyShape, int dissonance, int symmetry, int outerPath, out FakeGalaxy g, out FakeGalaxy p)
        {
            if (tableGen == 0)
            {
                var tableName = $"custom_AhyangyiTessellation_{tessellation}_{symmetry}_{galaxyShape}";
                string table = ExternalConstants.Instance.GetCustomString_Slow(tableName);

                // FIXME
                tableGen = 1;
            }

            ParameterService par = new ParameterService((TableGenMode)tableGen, tessellation, symmetry, galaxyShape, numPlanets, dissonance, aspectRatioIndex, outerPath, AspectRatioMode.NORMAL);

            if (tableGen == 2)
            {
                do
                {
                    GridGenerators[tessellation].MakeGrid(outerPath, aspectRatioIndex, galaxyShape, symmetry, dissonance, numPlanets, par);
                } while (par.Next());
            }
            else if (tableGen == 3)
            {
                for (int curOuterPath = 0; curOuterPath < OUTER_PATH_TYPES; ++curOuterPath)
                {
                    par.OuterPath = curOuterPath;
                    do
                    {
                        GridGenerators[tessellation].MakeGrid(curOuterPath, aspectRatioIndex, galaxyShape, symmetry, dissonance, numPlanets, par);
                    } while (par.Next());
                }
            }

            if (tableGen == 3)
            {
                par.GenerateTable();
            }

            (g, p) = GridGenerators[tessellation].MakeGrid(outerPath, aspectRatioIndex, galaxyShape, symmetry, dissonance, numPlanets, par);
            g.MakeSymmetricGroups();
        }
    }
}