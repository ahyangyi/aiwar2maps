using AhyangyiMaps.Tessellation;
using Arcen.AIW2.External;
using Arcen.Universal;
using System;
using System.IO;
using System.Linq;
using UnityEngine.Assertions;

namespace AhyangyiMaps
{
    public enum TableGenMode
    {
        USE = 0,
        HEURISTIC = 1,
        OPTIMIZE = 2,
        GEN_TABLE = 3,
    }

    public class ParameterService
    {
        public const int MAX_PARAMETERS = 6;
        static System.Collections.Generic.List<int> planetNumbers;
        static ParameterService()
        {
            planetNumbers = new System.Collections.Generic.List<int> { 40, 42, 44, 46, 48 };
            for (int i = 50; i <= TessellationTypeGenerator.MAX_PLANETS; i += 5) planetNumbers.Add(i);
        }

        TableGenMode mode;
        System.Collections.Generic.List<ParameterRange> history;
        int historyRevisited;
        System.Collections.Generic.Dictionary<string, string> info;
        FInt badness;
        public FakeGalaxy g, p;
        int numPlanets, dissonance, aspectRatioIndex, outerPath;

        class ParameterRange
        {
            public int Low, High, Current;
        }

        public ParameterService(TableGenMode mode, int numPlanets, int dissonance, int aspectRatioIndex, int outerPath)
        {
            this.mode = mode;
            this.numPlanets = numPlanets;
            this.dissonance = dissonance;
            this.aspectRatioIndex = aspectRatioIndex;
            this.outerPath = outerPath;

            badness = FInt.Zero;
            history = new System.Collections.Generic.List<ParameterRange>();
            historyRevisited = 0;
            info = new System.Collections.Generic.Dictionary<string, string>();
        }

        public void SetTable(System.Collections.Generic.List<int> values)
        {
            foreach (int value in values)
            {
                history.Add(new ParameterRange { Low=0, High=0, Current=value });
            }
        }

        public int AddParameter(int low, int high, int heuristicValue)
        {
            if (mode == TableGenMode.HEURISTIC)
            {
                return heuristicValue;
            }

            if (mode == TableGenMode.USE)
            {
                return history[historyRevisited++].Current;
            }

            if (historyRevisited < history.Count)
            {
                if (low != history[historyRevisited].Low || high != history[historyRevisited].High)
                {
                    ArcenDebugging.ArcenDebugLogSingleLine(
                        $"Table replay history inconsistent: was {history[historyRevisited].Low} {history[historyRevisited].High}, got {low} {high}",
                        Verbosity.ShowAsError);
                }
                return history[historyRevisited++].Current;
            }

            history.Add(new ParameterRange { Low = low, High = high, Current = low });
            return low;
        }

        internal void AddInfo(string key, string value)
        {
            info[key] = value;
        }

        internal bool AddBadness(string key, FInt badness)
        {
            this.badness += badness;
            return false;
        }

        internal void Commit(FakeGalaxy g, FakeGalaxy p)
        {
            this.g = g;
            this.p = p;

            if (mode == TableGenMode.GEN_TABLE || mode == TableGenMode.OPTIMIZE)
            {
                int planets = g.planets.Count;
                int irremovablePlanets = p.planets.Count;
                FInt aspectRatio = g.AspectRatio();

                FInt percolationThreshold = FInt.Create(593, false);
 
                if (mode == TableGenMode.GEN_TABLE)
                {
                    for (int targetPlanetIndex = 0; targetPlanetIndex < planetNumbers.Count; ++targetPlanetIndex)
                    {
                        int targetPlanets = planetNumbers[targetPlanetIndex];
                        for (int dissonance = 0; dissonance < TessellationTypeGenerator.DISSONANCE_TYPES; ++dissonance)
                        {
                            FInt dissonanceRatio = (percolationThreshold * dissonance + (4 - dissonance)) / 4;
                            FInt postDissonancePlanets = irremovablePlanets + (planets * irremovablePlanets) * dissonanceRatio;
                            FInt planetDifference = (postDissonancePlanets - targetPlanets).Abs();
                            AddInfo("Equivalent Planets", postDissonancePlanets.ToString());
                            AddBadness("Planets Difference", planetDifference / dissonanceRatio);
                        }
                    }
                }
            }
        }
        public bool Step()
        {
            // General clean-up
            badness = FInt.Zero;
            historyRevisited = 0;
            info.Clear();

            // Rewind history...
            while (history.Count > 0)
            {
                int i = history.Count - 1;
                if (history[i].Current < history[i].High)
                {
                    history[i].Current += 1;
                    break;
                }
                history.PopLast();
            }

            return history.Count > 0;
        }
    }

    class TableGen
    {

        public struct TableKey
        {
            public int AspectRatioIndex;
            public int GalaxyShape;
            public int Symmetry;
            public int Dissonance;
            public int OuterPath;
            public int TargetPlanets;
        }

        public struct TableValue
        {
            public FInt Badness;
            public string Value;
            public System.Collections.Generic.Dictionary<string, string> Info;
        }

        public struct SectionKey
        {
            public int Symmetry;
            public int GalaxyShape;
        }
        public struct SectionalMetadata
        {
            public System.Collections.Generic.List<(string, string)> Schema;
            public System.Collections.Generic.List<string> Epilogue;
        }

        public static void WriteTable(
            string gridType,
            System.Collections.Generic.Dictionary<TableKey, TableValue> optimalCommands,
            System.Collections.Generic.Dictionary<SectionKey, SectionalMetadata> sections
            )
        {
            using (StreamWriter sw = File.CreateText($"XMLMods\\AhyangyiMaps\\_Code\\Tessellation\\Generated\\{gridType}GridTable.cs"))
            {
                sw.WriteLine("namespace AhyangyiMaps.Tessellation");
                sw.WriteLine("{");
                sw.WriteLine($"    public class {gridType}GridTable : {gridType}Grid");
                sw.WriteLine("    {");
                sw.WriteLine($"        public static (FakeGalaxy, FakeGalaxy) Make{gridType}TableGalaxy(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, int dissonance, int numPlanets)");
                sw.WriteLine("        {");
                sw.WriteLine("            FakeGalaxy g = null, p = null;");

                foreach (int symmetry in sections.Keys.Select(x => x.Symmetry).Distinct().OrderBy(x => x).ToList())
                {
                    var symmetrySections = sections.Where(x => x.Key.Symmetry == symmetry).ToDictionary(x => x.Key, x => x.Value);
                    var symmetryCommands = optimalCommands.Where(x => x.Key.Symmetry == symmetry).ToDictionary(x => x.Key, x => x.Value);
                    sw.WriteLine($"            if (symmetry == {symmetry})");
                    sw.WriteLine("            {");
                    foreach (int galaxyShape in symmetrySections.Keys.Select(x => x.GalaxyShape).Distinct().OrderBy(x => x).ToList())
                    {
                        var section = sections[new SectionKey { Symmetry = symmetry, GalaxyShape = galaxyShape }];
                        var galaxyShapeCommands = symmetryCommands.Where(x => x.Key.GalaxyShape == galaxyShape)
                            .OrderBy(x => (x.Key.TargetPlanets, x.Key.Dissonance, x.Key.OuterPath, x.Key.AspectRatioIndex))
                            .ToDictionary(x => x.Key, x => x.Value);

                        sw.WriteLine($"                if (galaxyShape == {galaxyShape})");
                        sw.WriteLine("                {");
                        var namesTuple = "(" + string.Join(", ", section.Schema.Select(x => x.Item2)) + ")";
                        sw.WriteLine($"                    var {namesTuple} = {gridType}GridTable{symmetry}.lookupTable{galaxyShape}[(numPlanets, dissonance, outerPath, aspectRatioIndex)];");
                        foreach (var line in section.Epilogue)
                        {
                            sw.WriteLine($"                    {line}");
                        }
                        sw.WriteLine("                }");
                    }
                    sw.WriteLine("            }");
                }

                sw.WriteLine("            return (g, p);");
                sw.WriteLine("        }");
                sw.WriteLine("    }");
                sw.WriteLine("}");
                sw.WriteLine("");

                sw.WriteLine($"// Summary: max overall badness {optimalCommands.Values.Select(x => x.Badness).Max()}");
            }

            foreach (int symmetry in sections.Keys.Select(x => x.Symmetry).Distinct().OrderBy(x => x).ToList())
            {
                var symmetrySections = sections.Where(x => x.Key.Symmetry == symmetry).ToDictionary(x => x.Key, x => x.Value);
                var symmetryCommands = optimalCommands.Where(x => x.Key.Symmetry == symmetry).ToDictionary(x => x.Key, x => x.Value);

                using (StreamWriter sw = File.CreateText($"XMLMods\\AhyangyiMaps\\_Code\\Tessellation\\Generated\\{gridType}GridTable{symmetry}.cs"))
                {
                    sw.WriteLine("namespace AhyangyiMaps.Tessellation");
                    sw.WriteLine("{");
                    sw.WriteLine($"    public class {gridType}GridTable{symmetry}");
                    sw.WriteLine("    {");
                    foreach (int galaxyShape in symmetrySections.Keys.Select(x => x.GalaxyShape).Distinct().OrderBy(x => x).ToList())
                    {
                        var section = sections[new SectionKey { Symmetry = symmetry, GalaxyShape = galaxyShape }];
                        var galaxyShapeCommands = symmetryCommands.Where(x => x.Key.GalaxyShape == galaxyShape)
                            .OrderBy(x => (x.Key.TargetPlanets, x.Key.Dissonance, x.Key.OuterPath, x.Key.AspectRatioIndex))
                            .ToDictionary(x => x.Key, x => x.Value);

                        var valueTuple = "(" + string.Join(", ", section.Schema.Select(x => x.Item1)) + ")";
                        var typeStr = $"System.Collections.Generic.Dictionary <(int, int, int, int), {valueTuple}>";
                        sw.WriteLine($"        public static {typeStr} lookupTable{galaxyShape} = new {typeStr} {{");

                        foreach (var kvp in galaxyShapeCommands.OrderBy(x => (x.Key.TargetPlanets, x.Key.Dissonance, x.Key.OuterPath, x.Key.AspectRatioIndex)))
                        {
                            sw.WriteLine($"            // Total badness: {kvp.Value.Badness}");
                            foreach (var infoKey in kvp.Value.Info.Keys)
                            {
                                sw.WriteLine($"            // {infoKey}: {kvp.Value.Info[infoKey]}");
                            }
                            sw.WriteLine($"            {{({kvp.Key.TargetPlanets}, {kvp.Key.Dissonance}, {kvp.Key.OuterPath}, {kvp.Key.AspectRatioIndex}), ({kvp.Value.Value})}},");
                        }

                        sw.WriteLine($"        }};");
                    }
                    sw.WriteLine("    }");
                    sw.WriteLine("}");
                    sw.WriteLine("");
                    sw.WriteLine($"// Summary: max overall badness in this file: {symmetryCommands.Values.Select(x => x.Badness).Max()}");
                }
            }
        }

        public static void Main(string[] args)
        {
            var planetNumbers = new System.Collections.Generic.List<int> { 40, 42, 44, 46, 48 };
            for (int i = 50; i <= 300; i += 5) planetNumbers.Add(i);

            SquareGrid.GenerateTable(planetNumbers, "Square");
            SquareYGrid.GenerateTable(planetNumbers, "SquareY");
        }
    }
}