using Arcen.AIW2.Core;
using Arcen.Universal;

namespace AhyangyiMaps.Tessellation
{
    public class SquareYMirrorGrid : IGridGenerator
    {
        static readonly int unit;
        static readonly FakePattern squareY, squareYFlipped;
        static SquareYMirrorGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 10;
            squareY = SquareYGrid.squareY;
            squareYFlipped = SquareYGrid.squareYFlipped;
        }
        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 2, 34, 6);
            int columns = par.AddParameter("columns", 1, 35, 10);

            if (rows % 2 == 1) return;
            if (symmetry >= 300 && symmetry < 10000)
            {
                FInt idealR = columns / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false);
                par.AddInfo("Ideal R", idealR.ToString());
                if (par.AddBadness("Rotational Shape", (rows - idealR).Abs())) return;
            }
            if (symmetry == 10001 && columns % 3 != 0) return;
            if (symmetry == 10002 && columns % 2 == 1) return;

            FakeGalaxy g = MakeGrid(rows, columns);

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
                g.MakeRotationalGeneric((columns - 1) * unit, (columns % 2 == 0 ? ((rows - 1) * 2 - 1) * unit : (rows - 1) * 2 * unit),
                    unit, symmetry / 100, symmetry % 100 == 50);
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2(unit * 2 * ((columns + 1) / 2));
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych(unit * 2 * (columns / 3));
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy(unit * 2 * ((columns + 1) / 2));
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }

            FakeGalaxy p;
            var outline = new Outline(g.FindOutline());
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

            par.Commit(g, p, outline);
        }
        private static FakeGalaxy MakeGrid(int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();

            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    (i % 2 == 0 ? squareYFlipped : squareY).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
    }
}