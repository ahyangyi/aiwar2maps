using Arcen.AIW2.Core;
using Arcen.Universal;

namespace AhyangyiMaps.Tessellation
{
    public class HexagonGrid : IGridGenerator
    {
        static readonly int xunit, yunit, dunit;
        static readonly FakePattern hexagon;
        static HexagonGrid()
        {
            xunit = PlanetType.Normal.GetData().InterStellarRadius * 866 / 100;
            yunit = PlanetType.Normal.GetData().InterStellarRadius * 5;
            dunit = PlanetType.Normal.GetData().InterStellarRadius * 10;

            hexagon = new FakePattern();
            var p0 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit, 0));
            var p1 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit * 2, yunit));
            var p2 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit * 2, yunit * 3));
            var p3 = hexagon.AddPlanetAt(ArcenPoint.Create(xunit, yunit * 4));
            var p4 = hexagon.AddPlanetAt(ArcenPoint.Create(0, yunit * 3));
            var p5 = hexagon.AddPlanetAt(ArcenPoint.Create(0, yunit));

            hexagon.AddLink(p0, p1);
            hexagon.AddLink(p1, p2);
            hexagon.AddLink(p2, p3);
            hexagon.AddLink(p3, p4);
            hexagon.AddLink(p4, p5);
            hexagon.AddLink(p5, p0);
        }
        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 1, 35, 7);
            int columns = par.AddParameter("columns", 1, 35, 10);

            if (symmetry == 150 && columns % 2 == 0) return;
            if (symmetry == 200 && (rows + columns) % 2 == 1) return;
            if (symmetry == 250 && (rows % 2 == 0 || columns % 2 == 0)) return;
            if (symmetry == 10000 && (columns % 4 == 1 || columns % 4 == 2)) return;
            if (symmetry == 10001 && columns % 3 != 2) return;
            if (symmetry == 10002 && ((rows + columns) % 2 == 1 || columns % 4 == 1 || columns % 4 == 2)) return;
            if (symmetry == 10101 && columns % 2 != 1) return;

            if (symmetry >= 300 && symmetry < 10000)
            {
                FInt idealR = ((columns + 1) * xunit / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) / yunit - 1) / 3;
                par.AddInfo("Ideal R", idealR.ToString());
                if (par.AddBadness("Rotational Shape", (rows - idealR).Abs())) return;
            }

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
                g.MakeRotationalGeneric((columns + 1) * xunit / 2, (rows * 3 + 1) * yunit, dunit, symmetry / 100, symmetry % 100 == 50, true);
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2((columns + 3) / 4 * 2 * xunit);
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych((columns + 1) / 3 * xunit);
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy((columns + 3) / 4 * 2 * xunit);
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }
            else if (symmetry == 10200)
            {
                columns = (columns + 3) / 4;
                rows = (rows * 4 / 3) / 2 * 2 + columns % 2;
                g = MakeGrid(rows, columns);
                g.MakeY((AspectRatio)aspectRatioIndex, dunit, (columns + 1) * xunit);
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

        private static FakeGalaxy MakeGrid(int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    if ((i + j) % 2 == 0)
                        hexagon.Imprint(g, ArcenPoint.Create(j * xunit, i * yunit * 3));

            return g;
        }
    }
}