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
            public string Commands;
            public System.Collections.Generic.Dictionary<string, string> Info;
        }

        public static void WriteTable(string gridType, System.Collections.Generic.Dictionary<TableKey, TableValue> optimalCommands)
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

                foreach (int symmetry in optimalCommands.Keys.Select(x => x.Symmetry).Distinct().OrderBy(x => x).ToList())
                {
                    var symmetryCommands = optimalCommands.Where(x => x.Key.Symmetry == symmetry).ToDictionary(x => x.Key, x => x.Value);
                    sw.WriteLine($"            if (symmetry == {symmetry})");
                    sw.WriteLine("            {");
                    foreach (int galaxyShape in symmetryCommands.Keys.Select(x => x.GalaxyShape).Distinct().OrderBy(x => x).ToList())
                    {
                        var galaxyShapeCommands = symmetryCommands.Where(x => x.Key.GalaxyShape == galaxyShape).ToDictionary(x => x.Key, x => x.Value);
                        sw.WriteLine($"                if (galaxyShape == {galaxyShape})");
                        sw.WriteLine("                {");
                        foreach (int outerPath in galaxyShapeCommands.Keys.Select(x => x.OuterPath).Distinct().OrderBy(x => x).ToList())
                        {
                            var outerPathCommands = galaxyShapeCommands.Where(x => x.Key.OuterPath == outerPath).ToDictionary(x => x.Key, x => x.Value);
                            sw.WriteLine($"                    if (outerPath == {outerPath})");
                            sw.WriteLine("                    {");

                            var aspectRatios = outerPathCommands.Keys.Select(x => x.AspectRatioIndex).Distinct().OrderBy(x => x).ToList();
                            if (aspectRatios.Count == 1)
                            {
                                WriteInnermostSwitch(sw, outerPathCommands, "                        ");
                            }
                            else
                            {
                                foreach (int aspectRatioIndex in aspectRatios)
                                {
                                    var aspectRatioIndexCommands = outerPathCommands.Where(x => x.Key.AspectRatioIndex == aspectRatioIndex).ToDictionary(x => x.Key, x => x.Value);
                                    var prefix = "                        ";
                                    sw.WriteLine($"{prefix}if (aspectRatioIndex == {aspectRatioIndex})");
                                    sw.WriteLine($"{prefix}{{");

                                    WriteInnermostSwitch(sw, aspectRatioIndexCommands, prefix + "    ");

                                    sw.WriteLine($"{prefix}}}");
                                }
                            }
                            sw.WriteLine("                    }");
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
        }

        private static void WriteInnermostSwitch(StreamWriter sw, System.Collections.Generic.Dictionary<TableKey, TableValue> aspectRatioIndexCommands, string prefix)
        {
            foreach (var key in aspectRatioIndexCommands.Keys.OrderBy(x => (x.TargetPlanets, x.Dissonance)))
            {
                sw.WriteLine($"{prefix}if (dissonance == {key.Dissonance} && numPlanets == {key.TargetPlanets})");
                sw.WriteLine($"{prefix}{{");
                sw.WriteLine($"{prefix}    // Total badness: {aspectRatioIndexCommands[key].Badness}");
                foreach (var infoKey in aspectRatioIndexCommands[key].Info.Keys)
                {
                    sw.WriteLine($"{prefix}    // {infoKey}: {aspectRatioIndexCommands[key].Info[infoKey]}");
                }
                sw.WriteLine($"{prefix}    {aspectRatioIndexCommands[key].Commands}");
                sw.WriteLine($"{prefix}}}");
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