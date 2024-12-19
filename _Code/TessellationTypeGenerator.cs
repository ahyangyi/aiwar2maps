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

    interface IGridGenerator
    {
        void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par);
    }

    public class TessellationTypeGenerator : IMapGenerator
    {
        public const int MAX_PLANETS = 300;
        public const int DISSONANCE_TYPES = 5;
        public const int OUTER_PATH_TYPES = 3;
        public const int ASPECT_RATIO_TYPES = (int)AspectRatio.COUNT;
        public const int GALAXY_SHAPE_TYPES = 3;
        static readonly System.Collections.Generic.Dictionary<int, IGridGenerator> GridGenerators;
        static TessellationTypeGenerator()
        {
            GridGenerators = new System.Collections.Generic.Dictionary<int, IGridGenerator> {
                { 0, new SquareGrid() },
                { 1, new HexagonGrid() },
                { 2, new TriangleGrid() },
                { 3, new CairoGrid() },
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
            int tableGen = 0;
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
            // FIXME: making separated rngs doesn't work for some reason
            /*
            var siteRNG = new MersenneTwister(randomNumberGenerator.NextInclus(0, int.MaxValue));
            var linkRNG = new MersenneTwister(randomNumberGenerator.NextInclus(0, int.MaxValue));
            var wobbleRNG = new MersenneTwister(randomNumberGenerator.NextInclus(0, int.MaxValue));
            */
            var siteRNG = randomNumberGenerator;
            var linkRNG = randomNumberGenerator;
            var wobbleRNG = randomNumberGenerator;

            // STEP 1 - TESSELLATION
            // Generate a base grid
            // Some outerPath values or grid types might demand certain planets and links be preserved,
            //   this information is represented as the FakeGalaxy p
            FakeGalaxy g, p;
            GenerateGrid(tableGen, numPlanets, tessellation, aspectRatioIndex, galaxyShape, dissonance, symmetry, outerPath,
                out g, out p, out Outline outline);
            ArcenDebugging.ArcenDebugLogSingleLine("Tessellation step 1 finished", Verbosity.DoNotShow);

            // STEP 2 - MARK OUTER PATH FOR PERSERVATION
            // Mark outer path.
            // The marked planets would be prevented from any consideration in STEP 3.
            // And the links would be always included in STEP 6.
            var keptGroups = g.FindPreservedGroups(p);

            // STEP 3 - DISSONANCE
            // Remove planets randomly, respecting symmetry and stick bits.
            // Also, preserve outline for Step 5
            if (dissonance > 0)
            {
                int retry = 0;
                while (g.planets.Count > numPlanets)
                {
                    SymmetricGroup s = g.symmetricGroups[siteRNG.NextInclus(0, g.symmetricGroups.Count - 1)];
                    if (keptGroups.Contains(s))
                    {
                        if (++retry == 1000) break;
                        continue;
                    }
                    if ((g.planets.Count - numPlanets) * 2 > s.planets.Count)
                    {
                        g.RemoveSymmetricGroup(s);
                    }
                    else
                    {
                        break;
                    }
                    retry = 0;
                }
            }
            ArcenDebugging.ArcenDebugLogSingleLine("Tessellation step 3 finished", Verbosity.DoNotShow);

            // STEP 4 - CONNNECT
            // In case we cut off the graph, connect it back
            g.EnsureConnectivity();
            ArcenDebugging.ArcenDebugLogSingleLine("Tessellation step 4 finished", Verbosity.DoNotShow);

            // STEP 5 - EXTRA LINKS
            // Make extra links available
            if (additionalConnections == 1)
            {
                g.AddExtraLinks(33, 0, 0, linkRNG, outline);
            }
            else if (additionalConnections == 2)
            {
                g.AddExtraLinks(67, 0, 0, linkRNG, outline);
            }
            else if (additionalConnections == 3)
            {
                g.AddExtraLinks(200, 0, 0, linkRNG, outline);
            }
            else if (additionalConnections == 4)
            {
                g.AddExtraLinks(133, 1, 0, linkRNG, outline);
            }
            else if (additionalConnections == 5)
            {
                g.AddExtraLinks(400, 1, 0, linkRNG, outline);
            }
            else if (additionalConnections == 6)
            {
                g.AddExtraLinks(2000, 5, 1, linkRNG, outline);
            }
            ArcenDebugging.ArcenDebugLogSingleLine("Tessellation step 5 finished", Verbosity.DoNotShow);

            // STEP 6 - SKELETON
            // Select a subset of edges that'll be in the game
            var spanningGraph = g.MakeSpanningGraph(traversability, linkRNG, p);
            ArcenDebugging.ArcenDebugLogSingleLine("Tessellation step 6 finished", Verbosity.DoNotShow);

            // STEP 7 - FILL
            // Add edges until the desired density is reached
            g.AddEdges(spanningGraph, connectivity, traversability, linkRNG);
            g = spanningGraph;
            ArcenDebugging.ArcenDebugLogSingleLine("Tessellation step 7 finished", Verbosity.DoNotShow);

            // STEP 8 - WOBBLE
            // Add random offsets to each planet, respecting symmetry
            g.Wobble(planetType, wobble, wobbleRNG);
            ArcenDebugging.ArcenDebugLogSingleLine("Tessellation step 8 finished", Verbosity.DoNotShow);

            // STEP 9 - POPULATE
            // Translate our information into Arcenverse
            g.Populate(galaxy, planetType, randomNumberGenerator);
        }

        private static void GenerateGrid(int tableGen, int numPlanets, int tessellation, int aspectRatioIndex, int galaxyShape, int dissonance, int symmetry, int outerPath,
            out FakeGalaxy g, out FakeGalaxy p, out Outline o)
        {
            if (tableGen == 3)
            {
                RunTableGen(tessellation, galaxyShape, symmetry);
            }
            else if (tableGen == 4)
            {
                RunTableGenGrande(tessellation, symmetry);
            }
            else if (tableGen == 5)
            {
                RunTableGenVenti(tessellation);
            }

            ParameterService par = new ParameterService((TableGenMode)(tableGen >= 3 ? 0 : tableGen),
                tessellation, symmetry, galaxyShape, numPlanets, dissonance, aspectRatioIndex, outerPath);

            if (tableGen == 2)
            {
                // FIXME Not implemented
                // RunTableGen2(numPlanets, tessellation, aspectRatioIndex, galaxyShape, dissonance, symmetry, par, outerPath);
            }

            GridGenerators[tessellation].MakeGrid(outerPath, aspectRatioIndex, galaxyShape, symmetry, par);

            g = par.g;
            p = par.p;
            o = par.o;

            g.MakeSymmetricGroups();
        }
        private static void RunTableGenVenti(int tessellation)
        {
            foreach (int symmetry in SymmetryConstants.AspectRatioModeLookup.Keys)
            {
                RunTableGenGrande(tessellation, symmetry);
            }
        }
        private static void RunTableGenGrande(int tessellation, int symmetry)
        {
            for (int i = 0; i < GALAXY_SHAPE_TYPES; ++i)
            {
                RunTableGen(tessellation, i, symmetry);
            }
        }

        private static void RunTableGen(int tessellation, int galaxyShape, int symmetry)
        {
            ParameterService par = new ParameterService((TableGenMode)3,
                tessellation, symmetry, galaxyShape, 80, 0, 0, 0);
            if (par.alreadyDone())
            {
                return;
            }
            for (int curOuterPath = 0; curOuterPath < OUTER_PATH_TYPES; ++curOuterPath)
            {
                par.OuterPath = curOuterPath;

                if (SymmetryConstants.AspectRatioModeLookup[symmetry] == SymmetryConstants.AspectRatioMode.SPECIAL)
                {
                    for (int aspectRatio = 0; aspectRatio < ASPECT_RATIO_TYPES; ++aspectRatio)
                    {
                        par.AspectRatioIndex = aspectRatio;
                        RunTableGen2(tessellation, aspectRatio, galaxyShape, symmetry, par, curOuterPath);
                    }
                }
                else
                {
                    RunTableGen2(tessellation, 0, galaxyShape, symmetry, par, curOuterPath);
                }
            }
            par.GenerateTable();
        }

        private static void RunTableGen2(int tessellation, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par, int curOuterPath)
        {
            do
            {
                GridGenerators[tessellation].MakeGrid(curOuterPath, aspectRatioIndex, galaxyShape, symmetry, par);
            } while (par.Next());
        }
    }
}