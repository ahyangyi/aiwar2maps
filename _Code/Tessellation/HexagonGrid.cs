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

        private void MakeRotationalGrid(int outerPath, int galaxyShape, int symmetry, ParameterService par)
        {
            // parse the symmetry value
            int n = symmetry / 100;
            bool dihedral = symmetry % 100 == 50;
            FInt sectorSlope = SymmetryConstants.Rotational[n].sectorSlope;

            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 1, 45, 7);
            int columns = par.AddParameter("columns", 1, 45, 10);

            if (columns % 2 == 0)
            {
                return;
            }

            bool advance = true;
            int connectThreshold = 0;

            int actualColumns;
            if (advance)
            {
                actualColumns = columns - 1;
            }
            else
            {
                actualColumns = columns;
            }

            FakeGalaxy g;
            if (n == 3)
            {
                if (galaxyShape == 0)
                {
                    // FIXME
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
                else if (galaxyShape == 1)
                {
                    // FIXME
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
                else
                {
                    // Pointy 3-fold symmetry: a equilateral triangle
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
            }
            else if (n == 4)
            {
                if (galaxyShape == 0)
                {
                    // Normal 4 fold symmetry ==> always a square shape
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
                else if (galaxyShape == 1)
                {
                    // FIXME
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
                else
                {
                    // FIXME
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
            }
            else
            {
                if (galaxyShape == 0)
                {
                    // FIXME
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
                else if (galaxyShape == 1)
                {
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
                else
                {
                    // FIXME
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
            }

            if (g == null)
            {
                return;
            }
            g.MakeRotationalGeneric((columns + 1) * xunit / 2, (rows * 3 + 1) * yunit, dunit, n, dihedral, advance, connectThreshold);

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

        private static FakeGalaxy PolygonStyle(FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            FInt idealR = actualColumns / sectorSlope / 2;
            if (rows > idealR)
            {
                return null;
            }
            return MakeGridRectangular(rows, columns, 0);
        }

        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            if (symmetry >= 300 && symmetry < 10000)
            {
                MakeRotationalGrid(outerPath, galaxyShape, symmetry, par);
                return;
            }

            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 2, 35, 7);
            int columns = par.AddParameter("columns", 2, 60, 10);
            int oddity = 0;
            if (galaxyShape == 0)
            {
                oddity = par.AddParameter("oddity", 0, 1, 0);
            }

            if (galaxyShape >= 1)
            {
                if (rows % 2 == 0 || columns % 2 == 0)
                {
                    return;
                }
                int columnsThreshold = rows * 2 + 1 - 2 * galaxyShape;
                if (aspectRatioIndex == 0 && columns <= columnsThreshold)
                {
                    return;
                }
                if (aspectRatioIndex == 1 && columns != columnsThreshold)
                {
                    return;
                }
                if (aspectRatioIndex == 2 && (columns >= columnsThreshold || columns % 4 != 1))
                {
                    return;
                }
            }

            if (symmetry == 150 && columns % 2 == 0) return;
            if (symmetry == 200 && (rows + columns) % 2 == 1) return;
            if (symmetry == 250 && (rows % 2 == 0 || columns % 2 == 0)) return;
            if (symmetry == 10000 && (columns % 4 == 1 || columns % 4 == 2)) return;
            if (symmetry == 10001 && columns % 3 != 2) return;
            if (symmetry == 10002 && ((rows + columns) % 2 == 1 || columns % 4 == 1 || columns % 4 == 2)) return;
            if (symmetry == 10101 && columns % 2 != 1) return;

            FakeGalaxy g;

            if (galaxyShape == 0)
            {
                g = MakeGridRectangular(rows, columns, oddity);
            }
            else if (galaxyShape == 1)
            {
                if (aspectRatioIndex <= 1)
                {
                    g = MakeGridOctagonal(rows, columns, rows / 2);
                }
                else
                {
                    g = MakeGridOctagonal(rows, columns, columns / 4);
                }
            }
            else
            {
                if (aspectRatioIndex <= 1)
                {
                    g = MakeGridConcave(rows, columns, rows / 2);
                }
                else
                {
                    g = MakeGridConcave(rows, columns, columns / 4);
                }
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
                g = MakeGridRectangular(rows, columns, oddity);
                g.MakeY((AspectRatio)aspectRatioIndex, dunit, (columns + 1) * xunit);
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

        private static FakeGalaxy MakeGridRectangular(int rows, int columns, int oddity)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    if ((i + j) % 2 == oddity)
                        hexagon.Imprint(g, ArcenPoint.Create(j * xunit, i * yunit * 3));

            return g;
        }

        private static FakeGalaxy MakeGridOctagonal(int rows, int columns, int bevel)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    if ((i + j) % 2 == bevel % 2)
                    {
                        if ((i + j) < bevel) continue;
                        if ((i + columns - 1 - j) < bevel) continue;
                        if ((rows - 1 - i + j) < bevel) continue;
                        if ((rows - 1 - i + columns - 1 - j) < bevel) continue;
                        hexagon.Imprint(g, ArcenPoint.Create(j * xunit, i * yunit * 3));
                    }

            return g;
        }

        private static FakeGalaxy MakeGridConcave(int rows, int columns, int bevel)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    if ((i + j) % 2 == 0)
                    {
                        if (j < i && j < rows - 1 - i && j < bevel) continue;
                        int k = columns - 1 - j;
                        if (k < i && k < rows - 1 - i && k < bevel) continue;
                        hexagon.Imprint(g, ArcenPoint.Create(j * xunit, i * yunit * 3));
                    }

            return g;
        }
    }
}