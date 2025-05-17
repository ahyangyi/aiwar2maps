using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

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

        private void MakeRotationalGrid(int outerPath, int galaxyShape, int symmetry, ParameterService par)
        {
            // parse the symmetry value
            int n = symmetry / 100;
            bool dihedral = symmetry % 100 == 50;
            FInt sectorSlope = SymmetryConstants.Rotational[n].sectorSlope;

            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 1, 25, 7);
            int columns = par.AddParameter("columns", 1, 25, 10);

            bool advance = columns % 2 == 0;
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
                    g = ExtendedStyle(par, sectorSlope, rows, columns, actualColumns);
                }
                else if (galaxyShape == 1)
                {
                    g = NonagonalStyle(par, sectorSlope, rows, columns, actualColumns);
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
                    g = OctagonalStyle(par, sectorSlope, rows, columns, actualColumns);
                }
                else
                {
                    // A cross, where each part is just a 3:2 rectangle
                    g = AsteriskStyle(par, sectorSlope, rows, columns, ref connectThreshold, actualColumns);
                }
            }
            else
            {
                if (galaxyShape == 0)
                {
                    g = ExtendedStyle(par, sectorSlope, rows, columns, actualColumns);
                }
                else if (galaxyShape == 1)
                {
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
                else
                {
                    g = AsteriskStyle(par, sectorSlope, rows, columns, ref connectThreshold, actualColumns);
                }
            }

            if (g == null)
            {
                return;
            }
            g.MakeRotationalGeneric(columns * unit, rows * 2 * unit, unit, n, dihedral,
                outerPath, out FakeGalaxy p, out Outline outline, advance, connectThreshold);

            par.Commit(g, p, outline);
        }
        private static FakeGalaxy OctagonalStyle(ParameterService par, FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            if (columns < 3)
            {
                return null;
            }
            int bevel = par.AddParameter("bevel",
                Math.Max(columns / 6, 1),
                Math.Max(columns / 4, 1),
                Math.Max(columns / 5, 1));
            FInt idealBevel = columns / FInt.Create(4828, false);
            FInt idealR = actualColumns / (sectorSlope * 2) + bevel;
            if (rows <= idealR - 1 || rows >= idealR + 1)
            {
                return null;
            }
            par.AddInfo("Ideal rows", idealR.ToString());
            if (par.AddBadness("Rows Difference", (rows - idealR).Abs() * 5)) return null;
            par.AddInfo("Ideal bevel", idealBevel.ToString());
            if (par.AddBadness("Bevel Difference", (bevel - idealBevel).Abs() * 10)) return null;
            return MakeGridSemioctagonal(rows, columns, bevel);
        }

        private static FakeGalaxy NonagonalStyle(ParameterService par, FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            if (columns < 3)
            {
                return null;
            }
            int bevel = par.AddParameter("bevel",
                Math.Max(columns / 5, 1),
                Math.Max(columns / 3, 1),
                Math.Max(columns / 4, 1));
            FInt idealBevel = columns / FInt.Create(4000, false);
            FInt idealR = actualColumns / (sectorSlope * 2) + bevel;
            if (rows <= idealR - 1 || rows >= idealR + 1)
            {
                return null;
            }
            par.AddInfo("Ideal rows", idealR.ToString());
            if (par.AddBadness("Rows Difference", (rows - idealR).Abs() * 5)) return null;
            par.AddInfo("Ideal bevel", idealBevel.ToString());
            if (par.AddBadness("Bevel Difference", (bevel - idealBevel).Abs() * 10)) return null;
            return MakeGridSemioctagonal(rows, columns, bevel);
        }

        private static FakeGalaxy AsteriskStyle(ParameterService par, FInt sectorSlope, int rows, int columns, ref int connectThreshold, int actualColumns)
        {
            // We reuse the formula for the cross case, but for an asterisk
            FInt idealR = columns + actualColumns / (sectorSlope * 2);
            if (rows <= idealR - 1 || rows >= idealR + 1)
            {
                return null;
            }
            par.AddInfo("Ideal rows", idealR.ToString());
            if (par.AddBadness("Rows Difference", (rows - idealR).Abs() * 5)) return null;
            connectThreshold = unit * columns;
            return MakeGridRectangular(rows, columns);
        }

        private static FakeGalaxy PolygonStyle(FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            FInt idealR = actualColumns / sectorSlope / 2;
            if (rows > idealR)
            {
                return null;
            }
            return MakeGridRectangular(rows, columns);
        }
        private static FakeGalaxy ExtendedStyle(ParameterService par, FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            FInt leastR = actualColumns / (sectorSlope * 4 / 3);
            FInt idealR = actualColumns / (sectorSlope * 4 / 3);
            if (rows < leastR)
            {
                return null;
            }
            par.AddInfo("Ideal rows", idealR.ToString());
            if (par.AddBadness("Rows Difference", (rows - idealR).Abs() * 5)) return null;
            return MakeGridRectangular(rows, columns);
        }

        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            if (symmetry >= 300 && symmetry < 10000)
            {
                MakeRotationalGrid(outerPath, galaxyShape, symmetry, par);
                return;
            }

            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 2, 34, 6);
            int columns = par.AddParameter("columns", 1, 35, 10);

            if (rows % 2 == 1) return;
            if (symmetry == 10001 && columns % 3 != 0) return;
            if (symmetry == 10002 && columns % 2 == 1) return;

            FakeGalaxy g = MakeGridRectangular(rows, columns);

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
        private static FakeGalaxy MakeGridRectangular(int rows, int columns)
        {
            FakeGalaxy g = new FakeGalaxy();

            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    (i % 2 == 0 ? squareYFlipped : squareY).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
        private static FakeGalaxy MakeGridSemioctagonal(int rows, int columns, int octagonalSideLength)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    int k = j % columns;
                    if ((i + k) < octagonalSideLength) continue;
                    if ((i + columns - 1 - k) < octagonalSideLength) continue;
                    (i % 2 == 0 ? squareYFlipped : squareY).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));
                }

            return g;
        }
    }
}