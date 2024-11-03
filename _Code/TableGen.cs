using AhyangyiMaps.Tessellation;
using Arcen.Universal;
using System.IO;
using System.Linq;

namespace AhyangyiMaps
{

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

                        foreach (var kvp in galaxyShapeCommands)
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