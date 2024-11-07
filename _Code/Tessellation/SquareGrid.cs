using Arcen.AIW2.Core;
using Arcen.Universal;
using System;
using System.Linq;

namespace AhyangyiMaps.Tessellation
{
    public class SquareGrid
    {
        protected static readonly int unit;
        static readonly FakePattern square;
        static readonly FInt percolationThreshold = FInt.Create(593, false);
        static SquareGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 10;
            square = new FakePattern();
            var p0 = square.AddPlanetAt(ArcenPoint.Create(0, 0));
            var p1 = square.AddPlanetAt(ArcenPoint.Create(unit, 0));
            var p2 = square.AddPlanetAt(ArcenPoint.Create(unit, unit));
            var p3 = square.AddPlanetAt(ArcenPoint.Create(0, unit));

            square.AddLink(p0, p1);
            square.AddLink(p1, p2);
            square.AddLink(p2, p3);
            square.AddLink(p3, p0);
        }
        public static (FakeGalaxy, FakeGalaxy) MakeSquareGalaxy(int outerPath, AspectRatio aspectRatioEnum, int galaxyShape, int symmetry, int dissonance, int numPlanets)
        {
            {
                var (gg, pp) = SquareGridTable.MakeSquareTableGalaxy(outerPath, (int)aspectRatioEnum, galaxyShape, symmetry, dissonance, numPlanets);
                if (gg != null && pp != null)
                {
                    return (gg, pp);
                }
            }
            numPlanets = numPlanets * 12 / (12 - dissonance);

            int rows = 9;
            int columns = 16;
            FInt badness = (FInt)1000000;
            FInt aspectRatio = aspectRatioEnum.Value();
            for (int r = 1; r <= 35; ++r)
            {
                for (int c = 1; c <= 35; ++c)
                {
                    if (galaxyShape == 2 && (r + c) % 2 == 1 && symmetry < 300) continue;
                    if (symmetry == 10001 && c % 3 != 0) continue;
                    if (symmetry == 10101 && c % 2 != 0) continue;
                    int planets = r * c;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = (FInt)r / (FInt)c;
                    FInt p1 = currentAspectRatio / aspectRatio;
                    FInt p2 = aspectRatio / currentAspectRatio;
                    FInt aspectRatioBadness = ((p1 > p2 ? p1 : p2) - FInt.One) * (FInt)10;
                    FInt currentBadness = planetBadness + aspectRatioBadness;
                    if (currentBadness < badness)
                    {
                        badness = currentBadness;
                        rows = r;
                        columns = c;
                    }
                }
            }
            FakeGalaxy g;
            if (galaxyShape == 0)
            {
                g = MakeGrid(rows, columns);
            }
            else if (galaxyShape == 1)
            {
                g = MakeGridOctagonal(rows, columns, (Math.Min(rows, columns) / FInt.Create(3414, false)).GetNearestIntPreferringLower());
            }
            else
            {
                g = MakeGridCross(rows, columns, (Math.Min(rows, columns) / 3 + 1) & -1 | rows % 2);
            }

            if (symmetry == 150)
            {
                g.MakeBilateral();
            }
            else if (symmetry == 200)
            {
                g.MakeRotational2();
            }
            else if (symmetry == 250)
            {
                g.MakeRotational2Bilateral();
            }
            else if (symmetry >= 300 && symmetry < 10000)
            {
                FInt newBadness = (FInt)1000000;
                FakeGalaxy fg = MakeGrid(1, 1);
                for (int c = 1; c <= 30; ++c)
                {
                    int r1 = (c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false)).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(r, c);
                        g2.MakeRotationalGeneric(c * unit / 2, r * unit, unit, symmetry / 100, symmetry % 100 == 50, c % 2 == 1);
                        int planets = g2.planets.Count;
                        FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                        FInt currentBadness = planetBadness;
                        if (currentBadness < newBadness)
                        {
                            newBadness = currentBadness;
                            fg = g2;
                        }
                    }
                }
                g = fg;
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2(unit * ((columns + 1) / 2));
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych(unit * (columns / 3));
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy(unit * ((columns + 1) / 2));
            }
            else if (symmetry == 10100)
            {
                int borderWidth = (Math.Min(rows, columns) + 3) / 4;
                g = MakeGridBordered(rows, columns, borderWidth);
                g.MakeDuplexBarrier(unit, unit * borderWidth, unit * borderWidth);
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }
            else if (symmetry == 10200)
            {
                columns = (columns + 3) / 4;
                g = MakeGrid(rows, columns);
                g.MakeY(aspectRatioEnum, unit, ((columns * 4 + 3) / 5) * unit);
            }

            FakeGalaxy p;
            if (outerPath == 0)
            {
                p = new FakeGalaxy();
            }
            else if (outerPath == 1)
            {
                p = g.MarkOutline();
            }
            else
            {
                p = g.MakeBeltWay();
            }

            return (g, p);
        }

        protected static FakeGalaxy MakeGrid(int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));

            return g;
        }

        protected static FakeGalaxy MakeGridOctagonal(int rows, int columns, int octagonalSideLength, int sectionColumns = 0, int sectionOffset = 0)
        {
            FakeGalaxy g = new FakeGalaxy();
            if (sectionColumns == 0)
            {
                sectionColumns = sectionOffset = columns;
            }
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    int k = j % sectionOffset % sectionColumns;
                    if ((i + k) < octagonalSideLength) continue;
                    if ((i + sectionColumns - 1 - k) < octagonalSideLength) continue;
                    if ((rows - 1 - i + k) < octagonalSideLength) continue;
                    if ((rows - 1 - i + sectionColumns - 1 - k) < octagonalSideLength) continue;
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));
                }

            return g;
        }

        protected static FakeGalaxy MakeGridCross(int rows, int columns, int crossWidth, int sectionColumns = 0, int sectionOffset = 0)
        {
            FakeGalaxy g = new FakeGalaxy();
            if (sectionColumns == 0)
            {
                sectionColumns = sectionOffset = columns;
            }
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    int k = j % sectionOffset % sectionColumns;
                    if ((i * 2 < rows - crossWidth || i * 2 > rows + crossWidth - 2) &&
                        (k * 2 < sectionColumns - crossWidth || k * 2 > sectionColumns + crossWidth - 2)) continue;
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));
                }

            return g;
        }

        protected static FakeGalaxy MakeGridBordered(int rows, int columns, int borderWidth)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    if (i >= borderWidth && i < rows - borderWidth && j >= borderWidth && j < columns - borderWidth) continue;
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));
                }

            return g;
        }

        public static void GenerateTable(System.Collections.Generic.List<int> planetNumbers, string gridType)
        {
            var optimalCommands = new System.Collections.Generic.Dictionary<TableGen.TableKey, TableGen.TableValue>();
            var sections = new System.Collections.Generic.Dictionary<TableGen.SectionKey, TableGen.SectionalMetadata>();
            var loopSymmetries = new System.Collections.Generic.List<int> { 100, 150, 200, 250, 10000, 10001, 10002 };
            const int maxKnownBadness = 26;

            var rectangularEpilogue = new System.Collections.Generic.List<string> {
                "if (outerPath == 0)",
                "{",
                "    p = new FakeGalaxy();",
                "}",
                "else if (outerPath == 1)",
                "{",
                "    p = g.MarkOutline();",
                "}",
                "else",
                "{",
                "    p = g.MakeBeltWay();",
                "}",
            };

            // Shape 0 stuff
            var schemaRC = new System.Collections.Generic.List<(string, string)> { ("int", "r"), ("int", "c") };
            var schemaRCD = new System.Collections.Generic.List<(string, string)> { ("int", "r"), ("int", "c"), ("int", "d") };
            sections[new TableGen.SectionKey { GalaxyShape = 0, Symmetry = 100 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRC,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGrid(r, c);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 0, Symmetry = 150 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRC,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGrid(r, c);", "g.MakeBilateral();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 0, Symmetry = 200 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRC,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGrid(r, c);", "g.MakeRotational2();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 0, Symmetry = 250 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRC,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGrid(r, c);", "g.MakeRotational2Bilateral();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 0, Symmetry = 10000 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 2;", "int offset = c - f;", "g = MakeGrid(r, c);", "g.MakeTranslational2(unit * offset);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 0, Symmetry = 10001 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 3;", "int offset = (c - f) / 2;", "g = MakeGrid(r, c);", "g.MakeTriptych(unit * (f + offset) / 2, unit * (f + offset * 3) / 2);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 0, Symmetry = 10002 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 2;", "int offset = c - f;", "g = MakeGrid(r, c);", "g.MakeDualGalaxy(unit * offset);" }.Concat(rectangularEpilogue).ToList()
            };

            // Shape 1 stuff
            var schemaRCO = new System.Collections.Generic.List<(string, string)> { ("int", "r"), ("int", "c"), ("int", "o") };
            var schemaRCOD = new System.Collections.Generic.List<(string, string)> { ("int", "r"), ("int", "c"), ("int", "o"), ("int", "d") };
            sections[new TableGen.SectionKey { GalaxyShape = 1, Symmetry = 100 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCO,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGridOctagonal(r, c, o);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 1, Symmetry = 150 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCO,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGridOctagonal(r, c, o);", "g.MakeBilateral();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 1, Symmetry = 200 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCO,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGridOctagonal(r, c, o);", "g.MakeRotational2();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 1, Symmetry = 250 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCO,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGridOctagonal(r, c, o);", "g.MakeRotational2Bilateral();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 1, Symmetry = 10000 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCOD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 2;", "int offset = c - f;", "g = MakeGridOctagonal(r, c, o, f, offset);", "g.MakeTranslational2(unit * offset);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 1, Symmetry = 10001 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCOD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 3;", "int offset = (c - f) / 2;", "g = MakeGridOctagonal(r, c, o, f, offset);", "g.MakeTriptych(unit * (f + offset) / 2, unit * (f + offset * 3) / 2);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 1, Symmetry = 10002 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCOD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 2;", "int offset = c - f;", "g = MakeGridOctagonal(r, c, o, f, offset);", "g.MakeDualGalaxy(unit * offset);" }.Concat(rectangularEpilogue).ToList()
            };

            // Shape 2 stuff
            var schemaRCX = new System.Collections.Generic.List<(string, string)> { ("int", "r"), ("int", "c"), ("int", "x") };
            var schemaRCXD = new System.Collections.Generic.List<(string, string)> { ("int", "r"), ("int", "c"), ("int", "x"), ("int", "d") };
            sections[new TableGen.SectionKey { GalaxyShape = 2, Symmetry = 100 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCX,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGridCross(r, c, x);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 2, Symmetry = 150 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCX,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGridCross(r, c, x);", "g.MakeBilateral();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 2, Symmetry = 200 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCX,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGridCross(r, c, x);", "g.MakeRotational2();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 2, Symmetry = 250 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCX,
                Epilogue = new System.Collections.Generic.List<string> { "g = MakeGridCross(r, c, x);", "g.MakeRotational2Bilateral();" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 2, Symmetry = 10000 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCXD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 2;", "int offset = c - f;", "g = MakeGridCross(r, c, x, f, offset);", "g.MakeTranslational2(unit * offset);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 2, Symmetry = 10001 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCXD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 3;", "int offset = (c - f) / 2;", "g = MakeGridCross(r, c, x, f, offset);", "g.MakeTriptych(unit * (f + offset) / 2, unit * (f + offset * 3) / 2);" }.Concat(rectangularEpilogue).ToList()
            };
            sections[new TableGen.SectionKey { GalaxyShape = 2, Symmetry = 10002 }] = new TableGen.SectionalMetadata
            {
                Schema = schemaRCXD,
                Epilogue = new System.Collections.Generic.List<string> { "int f = (c + d) / 2;", "int offset = c - f;", "g = MakeGridCross(r, c, x, f, offset);", "g.MakeDualGalaxy(unit * offset);" }.Concat(rectangularEpilogue).ToList()
            };

            // Aspect Ratio respecting symmetries
            foreach (int symmetry in loopSymmetries)
            {
                int parts;
                if (symmetry == 10000 || symmetry == 10002)
                {
                    parts = 2;
                }
                else if (symmetry == 10001)
                {
                    parts = 3;
                }
                else
                {
                    parts = 1;
                }
                for (int galaxyShape = 0; galaxyShape <= 2; ++galaxyShape)
                {
                    var sectionKey = new TableGen.SectionKey { Symmetry = symmetry, GalaxyShape = galaxyShape };
                    int rcMin = (galaxyShape == 0 ? 1 : 3);
                    int rcMax = (symmetry == 2 ? 45 : 35);

                    for (int r = rcMin; r <= rcMax; ++r)
                    {
                        for (int c = rcMin; c <= rcMax; ++c)
                        {
                            int spMin, spMax;
                            if (galaxyShape == 0)
                            {
                                spMin = spMax = 0;
                            }
                            else if (galaxyShape == 1)
                            {
                                spMin = (Math.Min(r, c) + 12) / 15;
                                spMax = (Math.Min(r, c) - 1) / 2;
                            }
                            else
                            {
                                spMin = (Math.Min(r, c) + 12) / 15;
                                spMax = (Math.Min(r, c) * 2 + 2) / 3;
                            }
                            for (int sp = spMin; sp <= spMax; ++sp)
                            {
                                if (c % parts > 1 && c % parts < parts - 1) continue;
                                if (galaxyShape == 1 && r <= sp * 2) continue;
                                if (galaxyShape == 2 && ((r + sp) % 2 != 0 || r < sp + 2)) continue;

                                for (int overlap = -1; overlap <= 1; ++overlap)
                                {
                                    int d = overlap * (parts - 1);
                                    if ((c + d) % parts != 0)
                                    {
                                        continue;
                                    }

                                    int f = (c + d) / parts;
                                    if (galaxyShape == 1 && f <= sp * 2) continue;
                                    if (galaxyShape == 2 && ((f + sp) % 2 != 0 || f < sp + 2)) continue;

                                    int offset = parts == 1 ? c : (c - f) / (parts - 1);
                                    if (offset == 0) continue;

                                    FInt badness = FInt.Zero;
                                    var badnessReasons = new System.Collections.Generic.Dictionary<string, string>();

                                    if (galaxyShape == 1)
                                    {
                                        FInt idealO = Math.Min(r, f) / FInt.Create(3414, false);
                                        badness += (sp > idealO ? sp - idealO : idealO - sp);

                                        badnessReasons["Ideal O"] = idealO.ToString();
                                        badnessReasons["O Badness"] = badness.ToString();
                                    }
                                    else if (galaxyShape == 2)
                                    {
                                        FInt idealX = Math.Min(r, f) / FInt.Create(3000, false);
                                        badness += (sp > idealX ? sp - idealX : idealX - sp);

                                        badnessReasons["Ideal X"] = idealX.ToString();
                                        badnessReasons["X Badness"] = badness.ToString();
                                    }

                                    FakeGalaxy g = null;
                                    string value;

                                    if (galaxyShape == 0)
                                    {
                                        if (parts == 1)
                                        {
                                            value = $"{r}, {c}";
                                        }
                                        else
                                        {
                                            value = $"{r}, {c}, {d}";
                                        }
                                    }
                                    else
                                    {
                                        if (parts == 1)
                                        {
                                            value = $"{r}, {c}, {sp}";
                                        }
                                        else
                                        {
                                            value = $"{r}, {c}, {sp}, {d}";
                                        }
                                    }

                                    if (galaxyShape == 0)
                                    {
                                        g = MakeGrid(r, c);
                                    }
                                    else if (galaxyShape == 1)
                                    {
                                        g = MakeGridOctagonal(r, c, sp, f, offset);
                                    }
                                    else
                                    {
                                        g = MakeGridCross(r, c, sp, f, offset);
                                    }

                                    if (symmetry == 150)
                                    {
                                        g.MakeBilateral();
                                    }
                                    else if (symmetry == 200)
                                    {
                                        g.MakeRotational2();
                                    }
                                    else if (symmetry == 250)
                                    {
                                        g.MakeRotational2Bilateral();
                                    }
                                    else if (symmetry == 10000)
                                    {
                                        g.MakeTranslational2(unit * offset);
                                    }
                                    else if (symmetry == 10001)
                                    {
                                        g.MakeTriptych(unit * (f + offset) / 2, unit * (f + offset * 3) / 2);
                                    }
                                    else if (symmetry == 10002)
                                    {
                                        g.MakeDualGalaxy(unit * offset);
                                    }

                                    if (parts == 2)
                                    {
                                        if (overlap == 0)
                                        {
                                            badness += 5;
                                            badnessReasons["Two-part Galaxies sharing an edge"] = "+5";
                                        }
                                        else if (overlap > 0)
                                        {
                                            badness += 10;
                                            badnessReasons["Two-part Galaxies overlapping"] = "+10";
                                        }
                                    }
                                    else if (parts == 3 && c % 3 != 0)
                                    {
                                        if (overlap < 0)
                                        {
                                            badness += 10;
                                            badnessReasons["Three-part Galaxies not touching in Triptych"] = "+10";
                                        }
                                        else if (overlap > 0)
                                        {
                                            badness += 5;
                                            badnessReasons["Three-part Galaxies overlapping"] = "+5";
                                        }
                                    }

                                    RegisterRespectingAspectRatio(planetNumbers, optimalCommands, maxKnownBadness,
                                        value, g, symmetry, galaxyShape, percolationThreshold, badness, badnessReasons);
                                }
                            }
                        }
                    }
                }
            }

            // Now let's deal with aspectRatio-irrelevant stuff...
            var rotationalSymmetries = new System.Collections.Generic.List<int> { 300, 350, 400, 450, 500, 600, 700, 800 };
            foreach (int symmetry in rotationalSymmetries)
            {
                sections[new TableGen.SectionKey { GalaxyShape = 0, Symmetry = symmetry }] = new TableGen.SectionalMetadata
                {
                    Schema = schemaRC,
                    Epilogue = new System.Collections.Generic.List<string> {
                        "g = MakeGrid(r, c);",
                        $"g.MakeRotationalGeneric(c * unit / 2, r * unit, unit, {symmetry / 100}, {(symmetry % 100 == 50).ToString().ToLower()}, c % 2 == 1);",
                    }.Concat(rectangularEpilogue).ToList()
                };

                for (int c = 1; c <= 35; ++c)
                {
                    FInt idealR = c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false);

                    for (int r = 1; r <= 35; ++r)
                    {
                        FInt rBadness = (r > idealR ? r - idealR : idealR - r);
                        var g = MakeGrid(r, c);
                        g.MakeRotationalGeneric(c * unit / 2, r * unit, unit, symmetry / 100, symmetry % 100 == 50, c % 2 == 1);

                        int planets = g.planetCollection.planets.Count;

                        for (int galaxyShape = 0; galaxyShape <= 2; ++galaxyShape)
                        {
                            RegisterWithoutAspectRatio(planetNumbers, optimalCommands, maxKnownBadness,
                                $"{r}, {c}", planets, symmetry, galaxyShape, percolationThreshold, rBadness,
                                new System.Collections.Generic.Dictionary<string, string>
                                {
                                    { "R Badness", rBadness.ToString() }
                                });
                        }
                    }
                }
            }

            TableGen.WriteTable(gridType, optimalCommands, sections);
        }

        private static void RegisterWithoutAspectRatio(
            System.Collections.Generic.List<int> planetNumbers,
            System.Collections.Generic.Dictionary<TableGen.TableKey, TableGen.TableValue> optimalCommands,
            int maxKnownBadness,
            string value,
            int planets,
            int symmetry,
            int galaxyShape,
            FInt percolationThreshold,
            FInt extraBadness,
            System.Collections.Generic.Dictionary<string, string> extraInfo = null
            )
        {
            foreach (int targetPlanets in planetNumbers)
            {
                for (int dissonance = 0; dissonance <= 4; ++dissonance)
                {
                    FInt desiredPlanets = targetPlanets * 4 / (percolationThreshold * dissonance + (4 - dissonance));
                    FInt planetsBadness = (planets > desiredPlanets ? planets - desiredPlanets : desiredPlanets - planets);
                    if (planetsBadness + extraBadness > maxKnownBadness) continue;

                    FInt currentBadness = planetsBadness + extraBadness;

                    for (int outerPath = 0; outerPath <= 2; ++outerPath)
                    {
                        var key = new TableGen.TableKey
                        {
                            AspectRatioIndex = 0,
                            Dissonance = dissonance,
                            GalaxyShape = galaxyShape,
                            TargetPlanets = targetPlanets,
                            OuterPath = outerPath,
                            Symmetry = symmetry
                        };

                        if (!optimalCommands.ContainsKey(key) || currentBadness < optimalCommands[key].Badness)
                        {
                            var info = new System.Collections.Generic.Dictionary<string, string> {
                                        { "Planets", $"{planets}" },
                                        { "Planets Badness", $"{planetsBadness}" },
                                    };
                            if (extraInfo != null)
                            {
                                foreach (var kvp in extraInfo)
                                {
                                    info[kvp.Key] = kvp.Value;
                                }
                            }
                            optimalCommands[key] = new TableGen.TableValue
                            {
                                Badness = currentBadness,
                                Value = value,
                                Info = info,
                            };
                        }
                    }
                }
            }
        }

        private static void RegisterRespectingAspectRatio(
            System.Collections.Generic.List<int> planetNumbers,
            System.Collections.Generic.Dictionary<TableGen.TableKey, TableGen.TableValue> optimalCommands,
            int maxKnownBadness,
            string value,
            FakeGalaxy g,
            int symmetry,
            int galaxyShape,
            FInt percolationThreshold,
            FInt extraBadness,
            System.Collections.Generic.Dictionary<string, string> extraInfo = null
            )
        {
            FInt aspectRatio = g.AspectRatio();
            int planets = g.planetCollection.planets.Count;

            foreach (int targetPlanets in planetNumbers)
            {
                for (int dissonance = 0; dissonance <= 4; ++dissonance)
                {
                    FInt desiredPlanets = targetPlanets * 4 / (percolationThreshold * dissonance + (4 - dissonance));
                    FInt planetsBadness = (planets > desiredPlanets ? planets - desiredPlanets : desiredPlanets - planets);
                    if (planetsBadness + extraBadness > maxKnownBadness) continue;

                    for (int aspectRatioIndex = 0; aspectRatioIndex <= 2; ++aspectRatioIndex)
                    {
                        FInt targetAspectRatio = ((AspectRatio)aspectRatioIndex).Value();

                        FInt p1 = targetAspectRatio / aspectRatio;
                        FInt p2 = aspectRatio / targetAspectRatio;
                        FInt aspectRatioBadness = ((p1 > p2 ? p1 : p2) - FInt.One) * (FInt)10;
                        FInt currentBadness = planetsBadness + aspectRatioBadness + extraBadness;
                        if (currentBadness > maxKnownBadness) continue;

                        for (int outerPath = 0; outerPath <= 2; ++outerPath)
                        {
                            var key = new TableGen.TableKey
                            {
                                AspectRatioIndex = aspectRatioIndex,
                                Dissonance = dissonance,
                                GalaxyShape = galaxyShape,
                                TargetPlanets = targetPlanets,
                                OuterPath = outerPath,
                                Symmetry = symmetry
                            };

                            if (!optimalCommands.ContainsKey(key) || currentBadness < optimalCommands[key].Badness)
                            {
                                var info = new System.Collections.Generic.Dictionary<string, string> {
                                        { "Planets", $"{planets}" },
                                        { "Planets Badness", $"{planetsBadness}" },
                                        { "Aspect Ratio", $"{aspectRatio}" },
                                        { "Aspect Ratio Badness", $"{aspectRatioBadness}" },
                                    };
                                if (extraInfo != null)
                                {
                                    foreach (var kvp in extraInfo)
                                    {
                                        info[kvp.Key] = kvp.Value;
                                    }
                                }
                                optimalCommands[key] = new TableGen.TableValue
                                {
                                    Badness = currentBadness,
                                    Value = value,
                                    Info = info
                                };
                            }
                        }
                    }
                }
            }
        }
    }
}