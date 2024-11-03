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
            public int aspectRatioIndex;
            public int galaxyShape;
            public int symmetry;
            public int dissonance;
            public int outerPath;
            public int targetPlanets;
        }

        public static void WriteTable(string path, System.Collections.Generic.Dictionary<TableKey, (FInt, string, string)> optimalCommands)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("namespace AhyangyiMaps.Tessellation");
                sw.WriteLine("{");
                sw.WriteLine("    public class SquareGridTable");
                sw.WriteLine("    {");
                sw.WriteLine("        public static (FakeGalaxy, FakeGalaxy) MakeSquareGalaxy(int outerPath, AspectRatio aspectRatioEnum, int galaxyShape, int symmetry, int dissonance, int numPlanets)");
                sw.WriteLine("        {");

                foreach (int symmetry in optimalCommands.Keys.Select(x => x.symmetry).Distinct().OrderBy(x => x).ToList())
                {
                    var symmetryCommands = optimalCommands.Where(x => x.Key.symmetry == symmetry).ToDictionary(x => x.Key, x => x.Value);
                    sw.WriteLine($"            if (symmetry == {symmetry})");
                    sw.WriteLine("            {");
                    foreach (int galaxyShape in symmetryCommands.Keys.Select(x => x.galaxyShape).Distinct().OrderBy(x => x).ToList())
                    {
                        var galaxyShapeCommands = symmetryCommands.Where(x => x.Key.galaxyShape == galaxyShape).ToDictionary(x => x.Key, x => x.Value);
                        sw.WriteLine($"                if (galaxyShape == {galaxyShape})");
                        sw.WriteLine("                {");
                        foreach (int outerPath in galaxyShapeCommands.Keys.Select(x => x.outerPath).Distinct().OrderBy(x => x).ToList())
                        {
                            var outerPathCommands = galaxyShapeCommands.Where(x => x.Key.outerPath == outerPath).ToDictionary(x => x.Key, x => x.Value);
                            sw.WriteLine($"                    if (outerPath == {outerPath})");
                            sw.WriteLine("                    {");

                            var aspectRatios = outerPathCommands.Keys.Select(x => x.aspectRatioIndex).Distinct().OrderBy(x => x).ToList();
                            if (aspectRatios.Count == 1)
                            {
                                WriteInnermostSwitch(sw, outerPathCommands, "                    ");
                            }
                            else
                            {
                                foreach (int aspectRatioIndex in aspectRatios)
                                {
                                    var aspectRatioIndexCommands = outerPathCommands.Where(x => x.Key.aspectRatioIndex == aspectRatioIndex).ToDictionary(x => x.Key, x => x.Value);
                                    var prefix = "                        ";
                                    sw.WriteLine($"{prefix}if (aspectRatioIndex == {aspectRatioIndex})");
                                    sw.WriteLine($"{prefix}{{");

                                    WriteInnermostSwitch(sw, aspectRatioIndexCommands, prefix);
                                }
                                sw.WriteLine("                    }");
                            }
                        }
                        sw.WriteLine("                }");
                    }
                    sw.WriteLine("            }");
                }
                sw.WriteLine("        }");
                sw.WriteLine("    }");
                sw.WriteLine("}");
                sw.WriteLine("");

                sw.WriteLine($"// Summary: max overall badness {optimalCommands.Values.Select(x => x.Item1).Max()}");
            }
        }

        private static void WriteInnermostSwitch(StreamWriter sw, System.Collections.Generic.Dictionary<TableKey, (FInt, string, string)> aspectRatioIndexCommands, string prefix)
        {
            bool firstLine = true;
            foreach (var key in aspectRatioIndexCommands.Keys)
            {
                if (firstLine)
                {
                    firstLine = false;
                }
                else
                {
                    sw.WriteLine();
                }
                sw.WriteLine($"{prefix}    // Total badness: {aspectRatioIndexCommands[key].Item1}");
                sw.WriteLine($"{prefix}    // Explanation: {{{aspectRatioIndexCommands[key].Item3}}}");
                sw.WriteLine($"{prefix}    {key.dissonance}, {key.targetPlanets}: {aspectRatioIndexCommands[key].Item2}");
            }
            sw.WriteLine($"{prefix}}}");
        }

        public static void Main(string[] args)
        {
            var planetNumbers = new System.Collections.Generic.List<int> { 40, 42, 44, 46, 48 };
            for (int i = 50; i <= 300; i += 5) planetNumbers.Add(i);

            SquareGrid.GenerateTable(planetNumbers, "XMLMods\\AhyangyiMaps\\_Code\\Tessellation\\SquareGridTable.cs");
            SquareYGrid.GenerateTable(planetNumbers, "XMLMods\\AhyangyiMaps\\_Code\\Tessellation\\SquareYGridTable.cs");
        }
    }
}