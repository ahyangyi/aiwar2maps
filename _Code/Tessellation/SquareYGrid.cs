using Arcen.AIW2.Core;
using Arcen.Universal;

namespace AhyangyiMaps.Tessellation
{
    public class SquareYGrid : IGridGenerator
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
        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 1, 21, 7);
            int columns = par.AddParameter("columns", 1, 21, symmetry == 10001 ? 9 : 10);

            if (symmetry >= 300 && symmetry < 10000)
            {
                FInt idealR = columns / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false);
                par.AddInfo("Ideal R", idealR.ToString());
                if (par.AddBadness("Rotational Shape", (rows - idealR).Abs())) return;
            }
            if (symmetry == 10001 && columns == 1) return;

            FakeGalaxy g = MakeGrid(rows, columns, symmetry >= 300 && symmetry < 10000);

            if (symmetry == 150)
            {
                g.MakeBilateral();
            }
            else if (symmetry >= 300 && symmetry < 10000)
            {
                g.MakeRotationalGeneric(columns * unit, rows * 2 * unit, unit, symmetry / 100, symmetry % 100 == 50, columns % 2 == 0);
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2(unit * 2 * ((columns + 1) / 2));
            }
            else if (symmetry == 10001)
            {
                if (columns % 3 == 0)
                {
                    g.MakeTriptych(unit * 2 * (columns / 3));
                }
                else if (columns % 3 == 1)
                {
                    g.MakeTriptych((columns * 2 + 1) * unit / 3, (columns * 4 - 1) * unit / 3);
                }
                else
                {
                    g.MakeTriptych((columns * 2 - 1) * unit / 3, (columns * 4 + 1) * unit / 3);
                }
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
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

            par.Commit(g, p);
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