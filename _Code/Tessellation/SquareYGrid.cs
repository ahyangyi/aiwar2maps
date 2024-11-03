using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class SquareYGrid
    {
        static readonly int unit;
        public static readonly FakePattern squareY, squareYFlipped;
        static SquareYGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 10;
            squareY = new FakePattern();
            var p0 = squareY.AddPlanetAt(ArcenPoint.Create(0, 0));
            var p1 = squareY.AddPlanetAt(ArcenPoint.Create(unit * 2, 0));
            var p2 = squareY.AddPlanetAt(ArcenPoint.Create(unit * 2, unit * 2));
            var p3 = squareY.AddPlanetAt(ArcenPoint.Create(0, unit * 2));
            var center = squareY.AddPlanetAt(ArcenPoint.Create(unit, unit));
            var root = squareY.AddPlanetAt(ArcenPoint.Create(unit, 0));

            squareY.AddLink(p0, root);
            squareY.AddLink(root, p1);
            squareY.AddLink(p1, p2);
            squareY.AddLink(p2, p3);
            squareY.AddLink(p3, p0);
            squareY.AddLink(center, p2);
            squareY.AddLink(center, p3);
            squareY.AddLink(center, root);

            squareY.breakpoints.Add((p2.Location, p3.Location), new System.Collections.Generic.List<ArcenPoint> { ArcenPoint.Create(unit, unit * 2) });
            squareY.connectionsToBreak.Add((p0.Location, p1.Location));

            squareYFlipped = squareY.FlipY();
        }
        public static (FakeGalaxy, FakeGalaxy) MakeGalaxy(int outerPath, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 1; r <= 60; ++r)
            {
                for (int c = 1; c <= 60; ++c)
                {
                    if (symmetry == 10001 && c % 3 != 0) continue;
                    int planets = (r + 1) * (c + 1) + r * c * 2;
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
            FakeGalaxy g = MakeGrid(rows, columns, false);

            if (symmetry == 150)
            {
                g.MakeBilateral();
            }
            else if (symmetry >= 300 && symmetry < 10000)
            {
                FInt newBadness = (FInt)1000000;
                FakeGalaxy fg = MakeGrid(1, 1, true);
                for (int c = 4; c <= 30; ++c)
                {
                    int r1 = (c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false)).ToInt();
                    for (int r = r1; r <= r1 + 1; ++r)
                    {
                        var g2 = MakeGrid(r, c, true);
                        g2.MakeRotationalGeneric(c * unit, r * 2 * unit, unit, symmetry / 100, symmetry % 100 == 50, c % 2 == 0);
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
                return (fg, new FakeGalaxy(fg.planetCollection));
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2(unit * 2 * ((columns + 1) / 2));
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych(unit * 2 * (columns / 3));
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }

            return (g, new FakeGalaxy(g.planetCollection));
        }

        private static FakeGalaxy MakeGrid(int rows, int columns, bool flip)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    (flip ? squareYFlipped : squareY).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
        public static void GenerateTable(System.Collections.Generic.List<int> planetNumbers, string gridType)
        {

        }
    }
}