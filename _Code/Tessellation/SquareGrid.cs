using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class SquareGrid
    {
        static readonly int unit;
        static readonly FakePattern square;
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

        protected static FakeGalaxy MakeGridOctagonal(int rows, int columns, int octagonalSideLength)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    if ((i + j) < octagonalSideLength) continue;
                    if ((i + columns - 1 - j) < octagonalSideLength) continue;
                    if ((rows - 1 - i + j) < octagonalSideLength) continue;
                    if ((rows - 1 - i + columns - 1 - j) < octagonalSideLength) continue;
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));
                }

            return g;
        }

        protected static FakeGalaxy MakeGridCross(int rows, int columns, int crossWidth)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    if ((i * 2 < rows - crossWidth || i * 2 > rows + crossWidth - 2) && (j * 2 < columns - crossWidth || j * 2 > columns + crossWidth - 2)) continue;
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

        public static void GenerateTable(System.Collections.Generic.List<int> planetNumbers, string path)
        {
            var optimalCommands = new System.Collections.Generic.Dictionary<TableGen.TableKey, (FInt, string, string)>();
            var simpleSymmetries = new System.Collections.Generic.List<int> { 100, 150, 200, 250 };
            var loopSymmetries = new System.Collections.Generic.List<int> { 100, 150, 200, 250, 10000 };
            const int maxKnownBadness = 10;

            for (int r = 1; r <= 35; ++r)
            {
                for (int c = 1; c <= 35; ++c)
                {
                    // shape 0
                    {
                        var g = MakeGrid(r, c);
                        var cmd = $"g = MakeGrid({r}, {c});";

                        FInt aspectRatio = g.AspectRatio();
                        int planets = g.planetCollection.planets.Count;

                        foreach (int symmetry in loopSymmetries)
                        {
                            if (symmetry == 10000)
                            {
                                g = MakeGrid(r, c);
                                g.MakeTranslational2(unit * ((c + 1) / 2));

                                cmd = $"g = MakeGrid({r}, {c}); g.MakeTranslational2(unit * {(c + 1) / 2});";
                            }
                            foreach (int targetPlanets in planetNumbers)
                            {
                                for (int dissonance = 0; dissonance <= 4; ++dissonance)
                                {
                                    FInt planetBadness = (FInt)Math.Abs(planets - targetPlanets * 12 / (12 - dissonance));
                                    if (planetBadness > maxKnownBadness) continue;

                                    for (int aspectRatioIndex = 0; aspectRatioIndex <= 2; ++aspectRatioIndex)
                                    {
                                        FInt targetAspectRatio = ((AspectRatio)aspectRatioIndex).Value();

                                        FInt p1 = targetAspectRatio / aspectRatio;
                                        FInt p2 = aspectRatio / targetAspectRatio;
                                        FInt aspectRatioBadness = ((p1 > p2 ? p1 : p2) - FInt.One) * (FInt)10;
                                        FInt currentBadness = planetBadness + aspectRatioBadness;

                                        for (int outerPath = 0; outerPath <= 2; ++outerPath)
                                        {
                                            var key = new TableGen.TableKey
                                            {
                                                aspectRatioIndex = aspectRatioIndex,
                                                dissonance = dissonance,
                                                galaxyShape = 0,
                                                targetPlanets = targetPlanets,
                                                outerPath = outerPath,
                                                symmetry = symmetry
                                            };

                                            if (!optimalCommands.ContainsKey(key) || currentBadness < optimalCommands[key].Item1)
                                            {
                                                optimalCommands[key] = (currentBadness, cmd, $"planets: {planets}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // shape 1
                    if (r >= 3 && c >= 3)
                    {
                        for (int o = (Math.Min(r, c) + 2) / 5; o <= (Math.Min(r, c) - 1) / 2; ++o)
                        {
                            var g = MakeGridOctagonal(r, c, o);

                            FInt idealO = Math.Min(r, c) / FInt.Create(3414, false);
                            FInt octagonalBadness = (idealO < o ? o - idealO : idealO - o) * 2;

                            FInt aspectRatio = g.AspectRatio();
                            int planets = g.planetCollection.planets.Count;

                            foreach (int targetPlanets in planetNumbers)
                            {
                                for (int dissonance = 0; dissonance <= 4; ++dissonance)
                                {
                                    FInt planetBadness = (FInt)Math.Abs(planets - targetPlanets * 12 / (12 - dissonance));
                                    if (planetBadness + octagonalBadness > maxKnownBadness) continue;

                                    for (int aspectRatioIndex = 0; aspectRatioIndex <= 2; ++aspectRatioIndex)
                                    {
                                        FInt targetAspectRatio = ((AspectRatio)aspectRatioIndex).Value();

                                        FInt p1 = targetAspectRatio / aspectRatio;
                                        FInt p2 = aspectRatio / targetAspectRatio;
                                        FInt aspectRatioBadness = ((p1 > p2 ? p1 : p2) - FInt.One) * (FInt)10;
                                        FInt currentBadness = planetBadness + aspectRatioBadness + octagonalBadness;

                                        foreach (int symmetry in simpleSymmetries)
                                            for (int outerPath = 0; outerPath <= 2; ++outerPath)
                                            {
                                                var key = new TableGen.TableKey
                                                {
                                                    aspectRatioIndex = aspectRatioIndex,
                                                    dissonance = dissonance,
                                                    galaxyShape = 1,
                                                    targetPlanets = targetPlanets,
                                                    outerPath = outerPath,
                                                    symmetry = symmetry
                                                };

                                                if (!optimalCommands.ContainsKey(key) || currentBadness < optimalCommands[key].Item1)
                                                {
                                                    optimalCommands[key] = (currentBadness, $"g = MakeGridOctagonal({r}, {c}, {o});", $"planets: {planets}");
                                                }
                                            }
                                    }
                                }
                            }
                        }
                    }
                    // shape 2
                    if (r >= 3 && c >= 3 && (r + c) % 2 == 0)
                    {
                        for (int x = (Math.Min(r, c) + 2) / 5; x <= (Math.Min(r, c) - 1) / 2 + 1; ++x)
                        {
                            if ((r + x) % 2 == 0)
                            {
                                var g = MakeGridCross(r, c, x);

                                FInt idealX = Math.Min(r, c) / FInt.Create(3000, false);
                                FInt crossBadness = (idealX < x ? x - idealX : idealX - x) * 2;

                                FInt aspectRatio = g.AspectRatio();
                                int planets = g.planetCollection.planets.Count;

                                foreach (int targetPlanets in planetNumbers)
                                {
                                    for (int dissonance = 0; dissonance <= 4; ++dissonance)
                                    {
                                        FInt planetBadness = (FInt)Math.Abs(planets - targetPlanets * 12 / (12 - dissonance));
                                        if (planetBadness + crossBadness > maxKnownBadness) continue;

                                        for (int aspectRatioIndex = 0; aspectRatioIndex <= 2; ++aspectRatioIndex)
                                        {
                                            FInt targetAspectRatio = ((AspectRatio)aspectRatioIndex).Value();

                                            FInt p1 = targetAspectRatio / aspectRatio;
                                            FInt p2 = aspectRatio / targetAspectRatio;
                                            FInt aspectRatioBadness = ((p1 > p2 ? p1 : p2) - FInt.One) * (FInt)10;
                                            FInt currentBadness = planetBadness + aspectRatioBadness + crossBadness;

                                            foreach (int symmetry in simpleSymmetries)
                                                for (int outerPath = 0; outerPath <= 2; ++outerPath)
                                                {
                                                    var key = new TableGen.TableKey
                                                    {
                                                        aspectRatioIndex = aspectRatioIndex,
                                                        dissonance = dissonance,
                                                        galaxyShape = 2,
                                                        targetPlanets = targetPlanets,
                                                        outerPath = outerPath,
                                                        symmetry = symmetry
                                                    };

                                                    if (!optimalCommands.ContainsKey(key) || currentBadness < optimalCommands[key].Item1)
                                                    {
                                                        optimalCommands[key] = (currentBadness, $"g = MakeGridCross({r}, {c}, {x});", $"planets: {planets}");
                                                    }
                                                }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Now let's deal with aspectRatio-irrelevant stuff...
            var rotationalSymmetries = new System.Collections.Generic.List<int> { 300, 350, 400, 450, 500, 600, 700, 800 };
            for (int r = 1; r <= 35; ++r)
            {
                for (int c = 1; c <= 35; ++c)
                {
                    foreach (int symmetry in rotationalSymmetries)
                    {
                        var g = MakeGrid(r, c);
                        g.MakeRotationalGeneric(c * unit / 2, r * unit, unit, symmetry / 100, symmetry % 100 == 50, c % 2 == 1);

                        var cmd = $"g = MakeGrid({r}, {c}); g.MakeRotationalGeneric({c} * unit / 2, {r} * unit, unit, {symmetry / 100}, {symmetry % 100 == 50}, {c % 2 == 1});";

                        FInt aspectRatio = g.AspectRatio();
                        int planets = g.planetCollection.planets.Count;

                        foreach (int targetPlanets in planetNumbers)
                        {
                            for (int dissonance = 0; dissonance <= 4; ++dissonance)
                            {
                                FInt planetBadness = (FInt)Math.Abs(planets - targetPlanets * 12 / (12 - dissonance));
                                if (planetBadness > maxKnownBadness) continue;

                                FInt currentBadness = planetBadness;

                                for (int outerPath = 0; outerPath <= 2; ++outerPath)
                                {
                                    var key = new TableGen.TableKey
                                    {
                                        aspectRatioIndex = 0,
                                        dissonance = dissonance,
                                        galaxyShape = 0,
                                        targetPlanets = targetPlanets,
                                        outerPath = outerPath,
                                        symmetry = symmetry
                                    };

                                    if (!optimalCommands.ContainsKey(key) || currentBadness < optimalCommands[key].Item1)
                                    {
                                        optimalCommands[key] = (currentBadness, cmd, $"planets: {planets}");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            TableGen.WriteTable(path, optimalCommands);
        }
    }
}