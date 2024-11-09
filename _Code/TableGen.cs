using AhyangyiMaps.Tessellation;
using Arcen.AIW2.External;
using Arcen.Universal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    public enum AspectRatioMode
    {
        NORMAL = 0,
        IGNORE = 1,
        SPECIAL = 2,
    }

    public class ParameterService
    {
        public const int MAX_PARAMETERS = 6;
        static System.Collections.Generic.List<int> PlanetNumbers;
        static int TABLE_SIZE;
        static ParameterService()
        {
            PlanetNumbers = new System.Collections.Generic.List<int> { 40, 42, 44, 46, 48 };
            for (int i = 50; i <= TessellationTypeGenerator.MAX_PLANETS; i += 5) PlanetNumbers.Add(i);

            TABLE_SIZE = PlanetNumbers.Count
                    * TessellationTypeGenerator.DISSONANCE_TYPES
                    * TessellationTypeGenerator.ASPECT_RATIO_TYPES
                    * TessellationTypeGenerator.OUTER_PATH_TYPES;
        }
        public struct TableValue
        {
            public FInt Badness;
            public System.Collections.Generic.Dictionary<string, string> Info;
            public System.Collections.Generic.Dictionary<string, FInt> BadnessBreakdown;
            public System.Collections.Generic.List<int> Parameters;
        }

        readonly TableGenMode mode;
        System.Collections.Generic.List<ParameterRange> history;
        int historyRevisited;
        System.Collections.Generic.Dictionary<string, string> info;
        System.Collections.Generic.Dictionary<string, FInt> badnessInfo;
        System.Collections.Generic.List<TableValue> table;
        FInt CurrentBadness;
        public FakeGalaxy g, p;
        public readonly int Tessellation, Symmetry, GalaxyShape;
        public int NumPlanets, Dissonance, AspectRatioIndex, OuterPath;
        public AspectRatioMode aspectRatioMode;

        class ParameterRange
        {
            public int Low, High, Current;
        }

        public ParameterService(TableGenMode mode,
            int tessellation, int symmetry, int galaxyShape, int numPlanets, int dissonance, int aspectRatioIndex, int outerPath, AspectRatioMode aspectRatioMode)
        {
            this.mode = mode;
            Tessellation = tessellation;
            Symmetry = symmetry;
            GalaxyShape = galaxyShape;
            NumPlanets = numPlanets;
            Dissonance = dissonance;
            AspectRatioIndex = aspectRatioIndex;
            OuterPath = outerPath;
            this.aspectRatioMode = aspectRatioMode;

            CurrentBadness = FInt.Zero;
            history = new System.Collections.Generic.List<ParameterRange>();
            historyRevisited = 0;
            info = new System.Collections.Generic.Dictionary<string, string>();
            badnessInfo = new System.Collections.Generic.Dictionary<string, FInt>();

            if (mode == TableGenMode.GEN_TABLE || mode == TableGenMode.OPTIMIZE)
            {
                table = new System.Collections.Generic.List<TableValue>();
                for (int i = 0; i < (mode == TableGenMode.GEN_TABLE? TABLE_SIZE: 1); ++i)
                {
                    table.Add(new TableValue { Badness = (FInt)999, Info = null, BadnessBreakdown = null, Parameters = null });
                }
            }
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
                        $"Table replay history step #{historyRevisited} inconsistent: was {history[historyRevisited].Low} {history[historyRevisited].High}, got {low} {high}",
                        Verbosity.ShowAsError);
                }
                return history[historyRevisited++].Current;
            }

            history.Add(new ParameterRange { Low = low, High = high, Current = low });
            historyRevisited++;
            return low;
        }

        internal void AddInfo(string key, string value)
        {
            info[key] = value;
        }

        internal bool AddBadness(string key, FInt badness)
        {
            if (badnessInfo.ContainsKey(key))
            {
                CurrentBadness -= badnessInfo[key];
            }
            badnessInfo[key] = badness;
            CurrentBadness += badness;
            return CurrentBadness >= 25;
        }

        internal void Commit(FakeGalaxy g, FakeGalaxy p)
        {
            this.g = g;
            this.p = p;

            if (mode == TableGenMode.GEN_TABLE || mode == TableGenMode.OPTIMIZE)
            {
                if (aspectRatioMode == AspectRatioMode.NORMAL)
                {
                    FInt aspectRatio = g.AspectRatio();
                    AddInfo("AspectRatio", aspectRatio.ToString());

                    if (mode == TableGenMode.GEN_TABLE)
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
            int irremovablePlanets = p.planets.Count;

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
                AddInfo("Equivalent Planets", postDissonancePlanets.ToString());
                AddBadness("Planets Difference", planetDifference / dissonanceRatio);

                int index;
                if (mode == TableGenMode.GEN_TABLE)
                {
                    index = ((targetPlanetIndex * TessellationTypeGenerator.DISSONANCE_TYPES + dissonanceType)
                        * TessellationTypeGenerator.ASPECT_RATIO_TYPES + aspectRatioIndex)
                        * TessellationTypeGenerator.OUTER_PATH_TYPES + OuterPath;
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
                        BadnessBreakdown = badnessInfo.ToDictionary(x=>x.Key, x=>x.Value),
                        Parameters = history.Select(x => x.Current).ToList(),
                    };
                }
            }
        }

        public bool Next()
        {
            // General clean-up
            CurrentBadness = FInt.Zero;
            historyRevisited = 0;
            info.Clear();
            badnessInfo.Clear();

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

        static System.Collections.Generic.Dictionary<string, string> existingTable = null;
        static HashSet<string> generatedThisSession = null;

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

            if (generatedThisSession.Contains(key))
            {
                return;
            }

            StringBuilder s = new StringBuilder();
            for (int i = 0; i < TABLE_SIZE; ++i)
            {
                for (int j = 0; j < MAX_PARAMETERS; ++j)
                {
                    if (j < table[i].Parameters.Count)
                    {
                        int value = table[i].Parameters[j];
                        if (value == -1)
                        {
                            s.Append('?');
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
                for (int i = 0; i < TABLE_SIZE; ++i)
                {
                    if (table[i].Badness >= 25)
                    {
                        sw.WriteLine("-------------------------------------------------------------------------------");
                        sw.WriteLine($"Warning for case {i}");
                        sw.WriteLine($"Badness: {table[i].Badness}");

                        sw.WriteLine();
                        sw.WriteLine("Parameters");
                        foreach (var par in table[i].Parameters)
                        {
                            sw.WriteLine($"{par}");
                        }

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
    }
}