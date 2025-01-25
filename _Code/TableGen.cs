using Arcen.AIW2.Core;
using Arcen.AIW2.External;
using Arcen.Universal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        private static readonly int MAX_BADNESS = 1000;

        public const int MAX_PARAMETERS = 6;
        public static System.Collections.Generic.List<int> PlanetNumbers;
        public static System.Collections.Generic.Dictionary<int, int> PlanetIndex;
        static readonly int TABLE_SIZE;
        static readonly int TABLE_SIZE_NO_ASPECT_RATIO;
        static ParameterService()
        {
            PlanetNumbers = new System.Collections.Generic.List<int> { 40, 42, 44, 46, 48 };
            for (int i = 50; i <= TessellationTypeGenerator.MAX_PLANETS; i += 5) PlanetNumbers.Add(i);

            PlanetIndex = new System.Collections.Generic.Dictionary<int, int>();
            for (int i = 0; i < PlanetNumbers.Count; ++i)
            {
                PlanetIndex[PlanetNumbers[i]] = i;
            }

            TABLE_SIZE_NO_ASPECT_RATIO = PlanetNumbers.Count
                    * TessellationTypeGenerator.DISSONANCE_TYPES
                    * TessellationTypeGenerator.OUTER_PATH_TYPES;
            TABLE_SIZE = TABLE_SIZE_NO_ASPECT_RATIO * TessellationTypeGenerator.ASPECT_RATIO_TYPES;
        }
        public struct TableValue
        {
            public FInt Badness;
            public bool HasWarning;
            public System.Collections.Generic.Dictionary<string, string> Info;
            public System.Collections.Generic.Dictionary<string, FInt> BadnessBreakdown;
            public System.Collections.Generic.List<ParameterRange> Parameters;
        }

        readonly TableGenMode mode;
        System.Collections.Generic.List<ParameterRange> history;
        int historyRevisited;
        System.Collections.Generic.Dictionary<string, string> info;
        System.Collections.Generic.Dictionary<string, (FInt, bool)> badnessInfo;
        System.Collections.Generic.List<TableValue> table;
        FInt CurrentBadness;
        public FakeGalaxy g, p;
        public Outline o;
        public readonly int Tessellation, Symmetry, GalaxyShape;
        public int NumPlanets, Dissonance, AspectRatioIndex, OuterPath;
        public SymmetryConstants.AspectRatioMode aspectRatioMode;

        public class ParameterRange
        {
            public string Name;
            public int Low, High, Current;
        }

        public ParameterService(TableGenMode mode,
            int tessellation, int symmetry, int galaxyShape, int numPlanets, int dissonance, int aspectRatioIndex, int outerPath)
        {
            this.mode = mode;
            Tessellation = tessellation;
            Symmetry = symmetry;
            GalaxyShape = galaxyShape;
            NumPlanets = numPlanets;
            Dissonance = dissonance;
            AspectRatioIndex = aspectRatioIndex;
            OuterPath = outerPath;
            aspectRatioMode = SymmetryConstants.GetAspectRatioMode(symmetry, tessellation);

            CurrentBadness = FInt.Zero;
            history = new System.Collections.Generic.List<ParameterRange>();
            historyRevisited = 0;
            info = new System.Collections.Generic.Dictionary<string, string>();
            badnessInfo = new System.Collections.Generic.Dictionary<string, (FInt, bool)>();

            if (mode == TableGenMode.USE)
            {
                var entryName = $"custom_AhyangyiTessellation_{tessellation}_{symmetry}_{galaxyShape}";
                string table;

                if (existingTable != null)
                {
                    table = existingTable[entryName];
                }
                else
                {
                    table = ExternalConstants.Instance.GetCustomString_Slow(entryName);
                }

                int index = CalculateIndex(symmetry, PlanetIndex[numPlanets], aspectRatioIndex, dissonance, outerPath);

                for (int i = 0; i < MAX_PARAMETERS; ++i)
                {
                    char c = table[index * MAX_PARAMETERS + i];
                    int value;
                    if (c == '-')
                    {
                        value = -1;
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        value = c - '0';
                    }
                    else if (c >= 'A' && c <= 'Z')
                    {
                        value = c - 'A' + 10;
                    }
                    else if (c >= 'a' && c <= 'z')
                    {
                        value = c - 'a' + 36;
                    }
                    else
                    {
                        break;
                    }

                    history.Add(new ParameterRange { Low = 0, High = 0, Current = value });
                }
            }

            if (mode == TableGenMode.GEN_TABLE || mode == TableGenMode.OPTIMIZE)
            {
                table = new System.Collections.Generic.List<TableValue>();
                int tableSize;
                if (mode == TableGenMode.GEN_TABLE)
                {
                    if (aspectRatioMode == SymmetryConstants.AspectRatioMode.IGNORE)
                        tableSize = TABLE_SIZE_NO_ASPECT_RATIO;
                    else
                        tableSize = TABLE_SIZE;
                }
                else
                {
                    tableSize = 1;
                }
                for (int i = 0; i < tableSize; ++i)
                {
                    table.Add(new TableValue { Badness = (FInt)999, Info = null, BadnessBreakdown = null, Parameters = null });
                }
            }
        }

        public int AddParameter(string name, int low, int high, int heuristicValue)
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
                var old = history[historyRevisited];
                if (low != old.Low || high != old.High)
                {
                    ArcenDebugging.ArcenDebugLogSingleLine(
                        $"Table replay history step #{historyRevisited} inconsistent: was {old.Low} {old.High}, got {low} {high}",
                        Verbosity.ShowAsError);
                }
                return history[historyRevisited++].Current;
            }

            history.Add(new ParameterRange { Name = name, Low = low, High = high, Current = low });
            historyRevisited++;
            return low;
        }

        internal void AddInfo(string key, string value)
        {
            info[key] = value;
        }

        internal bool AddBadness(string key, FInt badness, bool isWarning = false)
        {
            if (badnessInfo.ContainsKey(key))
            {
                CurrentBadness -= badnessInfo[key].Item1;
            }
            badnessInfo[key] = (badness, isWarning);
            CurrentBadness += badness;
            return CurrentBadness >= MAX_BADNESS;
        }

        internal void Commit(FakeGalaxy g, FakeGalaxy p, Outline o)
        {
            this.g = g;
            this.p = p;
            this.o = o;

            if (mode == TableGenMode.GEN_TABLE || mode == TableGenMode.OPTIMIZE)
            {
                if (aspectRatioMode == SymmetryConstants.AspectRatioMode.NORMAL || aspectRatioMode == SymmetryConstants.AspectRatioMode.BOTH)
                {
                    FInt aspectRatio = g.AspectRatio();
                    AddInfo("AspectRatio", aspectRatio.ToString());

                    if (mode == TableGenMode.GEN_TABLE && aspectRatioMode == SymmetryConstants.AspectRatioMode.NORMAL)
                    {
                        for (int aspectRatioIndex = 0; aspectRatioIndex < TessellationTypeGenerator.ASPECT_RATIO_TYPES; ++aspectRatioIndex)
                        {
                            AddBadness("Aspect Ratio Difference", ((AspectRatio)aspectRatioIndex).Value().RatioDeviance(aspectRatio) * 10);
                            EvaluateStep1(g, p, aspectRatioIndex);
                        }
                    }
                    else
                    {
                        AddBadness("Aspect Ratio Difference", ((AspectRatio)AspectRatioIndex).Value().RatioDeviance(aspectRatio) * 10);
                        EvaluateStep1(g, p, AspectRatioIndex);
                    }
                }
                else
                {
                    EvaluateStep1(g, p, AspectRatioIndex);
                }
            }
        }

        private void EvaluateStep1(FakeGalaxy g, FakeGalaxy p, int aspectRatioIndex)
        {
            int planets = g.planets.Count;
            int irremovablePlanets = g.FindPreservedGroups(p).Sum(group => group.planets.Count);

            FInt percolationThreshold = FInt.Create(593, false);

            if (mode == TableGenMode.GEN_TABLE)
            {
                for (int targetPlanetIndex = 0; targetPlanetIndex < PlanetNumbers.Count; ++targetPlanetIndex)
                {
                    int targetPlanets = PlanetNumbers[targetPlanetIndex];
                    EvaluateStep2(planets, irremovablePlanets, percolationThreshold, targetPlanetIndex, targetPlanets, aspectRatioIndex);
                }
            }
            else
            {
                EvaluateStep2(planets, irremovablePlanets, percolationThreshold, 0, NumPlanets, aspectRatioIndex);
            }
        }

        private void EvaluateStep2(int planets, int irremovablePlanets, FInt percolationThreshold, int targetPlanetIndex, int targetPlanets, int aspectRatioIndex)
        {
            for (int dissonanceType = 0; dissonanceType < (mode == TableGenMode.GEN_TABLE ? TessellationTypeGenerator.DISSONANCE_TYPES : 1); ++dissonanceType)
            {
                int dissonance = (mode == TableGenMode.GEN_TABLE ? dissonanceType : this.Dissonance);

                FInt dissonanceRatio = (percolationThreshold * dissonance + (4 - dissonance)) / 4;
                FInt postDissonancePlanets = irremovablePlanets + (planets - irremovablePlanets) * dissonanceRatio;
                FInt planetDifference = (postDissonancePlanets - targetPlanets).Abs();
                AddInfo("Planets", planets.ToString());
                AddInfo("Irremoveable Planets", irremovablePlanets.ToString());
                AddInfo("Equivalent Planets", postDissonancePlanets.ToString());
                AddBadness("Planets Difference", planetDifference);

                int index;
                if (mode == TableGenMode.GEN_TABLE)
                {
                    index = CalculateIndex(Symmetry, targetPlanetIndex, aspectRatioIndex, dissonanceType, OuterPath);
                }
                else
                {
                    index = 0;
                }

                if (CurrentBadness < table[index].Badness)
                {
                    table[index] = new TableValue
                    {
                        Badness = CurrentBadness,
                        Info = info.ToDictionary(x => x.Key, x => x.Value),
                        BadnessBreakdown = badnessInfo.ToDictionary(x => x.Key, x => x.Value.Item1),
                        Parameters = history.Select(x => new ParameterRange { Current = x.Current, Name = x.Name }).ToList(),
                        HasWarning = badnessInfo.Any(x => x.Value.Item2)
                    };
                }
            }
        }

        public int CalculateIndex(int symmetry, int targetPlanetIndex, int aspectRatioIndex, int dissonanceType, int outerPath)
        {
            int index;
            if (SymmetryConstants.GetAspectRatioMode(symmetry, Tessellation) == SymmetryConstants.AspectRatioMode.IGNORE)
            {
                index = (targetPlanetIndex * TessellationTypeGenerator.DISSONANCE_TYPES + dissonanceType)
                    * TessellationTypeGenerator.OUTER_PATH_TYPES + outerPath;
            }
            else
            {
                index = ((targetPlanetIndex * TessellationTypeGenerator.DISSONANCE_TYPES + dissonanceType)
                    * TessellationTypeGenerator.ASPECT_RATIO_TYPES + aspectRatioIndex)
                    * TessellationTypeGenerator.OUTER_PATH_TYPES + outerPath;
            }

            return index;
        }

        public bool Next()
        {
            // General clean-up
            CurrentBadness = FInt.Zero;
            info.Clear();
            badnessInfo.Clear();

            // Rewind history...
            while (history.Count > historyRevisited)
            {
                history.PopLast();
            }
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

            historyRevisited = 0;
            return history.Count > 0;
        }

        static System.Collections.Generic.Dictionary<string, string> existingTable = null;
        public static HashSet<string> generatedThisSession = null;

        internal void LoadTable()
        {
            existingTable = new System.Collections.Generic.Dictionary<string, string>();
            generatedThisSession = new HashSet<string>();
            using (var sw = File.OpenText($"XMLMods\\AhyangyiMaps\\ExternalConstants\\TessellationLookup.xml"))
            {
                string line;
                while ((line = sw.ReadLine()) != null)
                {
                    if (line.StartsWith("    custom"))
                    {
                        var s = line.Split('=');
                        var key = s[0].Substring(4);
                        var value = s[1].Substring(1, s[1].Length - 2);

                        existingTable[key] = value;
                    }
                }
            }
        }

        internal void GenerateTable()
        {
            if (existingTable == null)
            {
                LoadTable();
            }

            string key = $"custom_AhyangyiTessellation_{Tessellation}_{Symmetry}_{GalaxyShape}";

            StringBuilder s = new StringBuilder();
            for (int i = 0; i < table.Count; ++i)
            {
                for (int j = 0; j < MAX_PARAMETERS; ++j)
                {
                    if (j < table[i].Parameters.Count)
                    {
                        int value = table[i].Parameters[j].Current;
                        if (value == -1)
                        {
                            s.Append('-');
                        }
                        else if (value < 10)
                        {
                            s.Append((char)(value + '0'));
                        }
                        else if (value < 36)
                        {
                            s.Append((char)(value - 10 + 'A'));
                        }
                        else
                        {
                            s.Append((char)(value - 36 + 'a'));
                        }
                    }
                    else
                    {
                        s.Append('_');
                    }
                }
            }

            existingTable[key] = s.ToString();
            generatedThisSession.Add(key);

            using (StreamWriter sw = File.CreateText("XMLMods\\AhyangyiMaps\\ExternalConstants\\TessellationLookup.xml"))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sw.WriteLine("<root");
                sw.WriteLine("    is_partial_record=\"true\"");
                foreach (var kvp in existingTable.OrderBy(x => x.Key))
                {
                    sw.WriteLine($"    {kvp.Key}=\"{kvp.Value}\"");
                }
                sw.WriteLine(">");
                sw.WriteLine("</root>");
            }

            using (StreamWriter sw = File.CreateText($"XMLMods\\AhyangyiMaps\\Debug\\Debug_{Tessellation}_{Symmetry}_{GalaxyShape}.txt"))
            {
                for (int i = 0; i < table.Count; ++i)
                {
                    if (table[i].Badness >= 20 || table[i].HasWarning)
                    {
                        sw.WriteLine("-------------------------------------------------------------------------------");
                        sw.WriteLine($"Warning for case {IndexToString(i)}");
                        sw.WriteLine($"Badness: {table[i].Badness}");

                        sw.WriteLine();
                        string parameterString = "";
                        bool first = true;
                        foreach (var par in table[i].Parameters)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                parameterString += ", ";
                            }
                            parameterString += $"{par.Name}: {par.Current}";
                        }
                        sw.WriteLine($"Parameters: {parameterString}");

                        sw.WriteLine();
                        sw.WriteLine("Badness Breakdown");
                        foreach (var badnessReason in table[i].BadnessBreakdown)
                        {
                            sw.WriteLine($"{badnessReason.Key} : {badnessReason.Value}");
                        }

                        sw.WriteLine();
                        sw.WriteLine("Extra Information");
                        foreach (var info in table[i].Info)
                        {
                            sw.WriteLine($"{info.Key} : {info.Value}");
                        }
                    }
                }
            }
        }

        private string IndexToString(int i)
        {
            int outerPath = i % TessellationTypeGenerator.OUTER_PATH_TYPES;
            i /= TessellationTypeGenerator.OUTER_PATH_TYPES;

            int aspectRatioIndex = 0;
            if (aspectRatioMode != SymmetryConstants.AspectRatioMode.IGNORE)
            {
                aspectRatioIndex = i % TessellationTypeGenerator.ASPECT_RATIO_TYPES;
                i /= TessellationTypeGenerator.ASPECT_RATIO_TYPES;
            }

            int dissonance = i % TessellationTypeGenerator.DISSONANCE_TYPES;
            i /= TessellationTypeGenerator.DISSONANCE_TYPES;

            int planets = i;

            if (aspectRatioMode != SymmetryConstants.AspectRatioMode.IGNORE)
            {
                return $"{{Planets: {PlanetNumbers[planets]}, Dissonance: {dissonance}, aspectRatio: {aspectRatioIndex}, outerPath: {outerPath}}}";
            }
            return $"{{Planets: {PlanetNumbers[planets]}, Dissonance: {dissonance}, outerPath: {outerPath}}}";
        }

        internal bool alreadyDone()
        {
            string key = $"custom_AhyangyiTessellation_{Tessellation}_{Symmetry}_{GalaxyShape}";

            return generatedThisSession != null && generatedThisSession.Contains(key);
        }
    }
}