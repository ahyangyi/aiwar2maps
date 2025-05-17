using Arcen.AIW2.Core;
using Arcen.Universal;

namespace AhyangyiMaps.Tessellation
{
    public class TriangleGrid : IGridGenerator
    {
        static readonly int xunit, yunit;
        public static readonly FakePattern leftTriangle, rightTriangle;
        static TriangleGrid()
        {
            xunit = PlanetType.Normal.GetData().InterStellarRadius * 866 / 100;
            yunit = PlanetType.Normal.GetData().InterStellarRadius * 5;

            rightTriangle = new FakePattern();
            var p0 = rightTriangle.AddPlanetAt(ArcenPoint.Create(0, 0));
            var p1 = rightTriangle.AddPlanetAt(ArcenPoint.Create(xunit, yunit));
            var p2 = rightTriangle.AddPlanetAt(ArcenPoint.Create(0, yunit * 2));

            rightTriangle.AddLink(p0, p1);
            rightTriangle.AddLink(p1, p2);
            rightTriangle.AddLink(p2, p0);

            leftTriangle = rightTriangle.FlipX();
        }
        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 1, 35, 7);
            int columns = par.AddParameter("columns", 1, 35, 10);

            if (symmetry == 150 && columns % 2 == 1) return;
            if (symmetry == 200 && (rows + columns) % 2 == 0) return;
            if (symmetry == 250 && (rows % 2 == 0 || columns % 2 == 1)) return;
            if (symmetry >= 300 && symmetry < 10000)
            {
                if (columns % 2 == 1) return;

                FInt idealR = columns * xunit / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) / yunit - 1;
                par.AddInfo("Ideal R", idealR.ToString());
                if (par.AddBadness("Rotational Shape", (rows - idealR).Abs())) return;
            }
            if (symmetry == 10000 && (columns % 4 == 1 || columns % 4 == 2)) return;
            if (symmetry == 10001 && columns % 3 != 0) return;
            if (symmetry == 10002 && ((rows + columns) % 2 == 0 || columns % 4 == 1 || columns % 4 == 2)) return;
            if (symmetry == 10101 && columns % 2 != 0) return;

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
                // FIXME: wait for refactor
                //g.MakeRotationalGeneric(columns * xunit / 2, (columns % 4 == 0 ? rows + 1 : rows) * yunit,
                //    yunit * 2, symmetry / 100, symmetry % 100 == 50, false);
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2((columns + 3) / 4 * 2 * xunit);
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych(columns / 3 * xunit);
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy((columns + 3) / 4 * 2 * xunit);
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
                    ((i + j) % 2 == 0 ? rightTriangle : leftTriangle).Imprint(g, ArcenPoint.Create(j * xunit, i * yunit));

            return g;
        }
    }
}