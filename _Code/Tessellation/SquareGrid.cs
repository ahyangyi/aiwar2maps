using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class SquareGrid : IGridGenerator
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
        enum RotationalStyle
        {
            POLYGON,
            EXTENDED,
        };

        private void MakeRotationalGrid(int outerPath, int galaxyShape, int symmetry, ParameterService par)
        {
            // parse the symmetry value
            int n = symmetry / 100;
            bool dihedral = symmetry % 50 == 50;
            FInt sectorSlope = SymmetryConstants.Rotational[n].sectorSlope;

            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 1, 45, 7);
            int columns = par.AddParameter("columns", 1, 45, 10);
            bool advance = columns % 2 == 1;

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
                    FInt leastR = actualColumns / sectorSlope * FInt.Create(750, false);
                    FInt idealR = actualColumns / sectorSlope * FInt.Create(750, false);
                    if (rows <= leastR)
                    {
                        return;
                    }
                    par.AddInfo("Ideal rows", idealR.ToString());
                    if (par.AddBadness("Rows Difference", (rows - idealR).Abs() * 5)) return;
                    g = MakeGrid(rows, columns);
                }
                else if (galaxyShape == 1)
                {
                    // Legacy case here
                    FInt idealR = actualColumns / sectorSlope * FInt.Create(750, false);
                    par.AddInfo("Ideal R", idealR.ToString());
                    if (par.AddBadness("Rotational Shape", (rows - idealR).Abs())) return;
                    g = MakeGrid(rows, columns);
                }
                else
                {
                    // Pointy 3-fold symmetry: a equilateral triangle
                    FInt idealR = actualColumns / sectorSlope / 2;
                    if (rows > idealR)
                    {
                        return;
                    }
                    g = MakeGrid(rows, columns);
                }
            }
            else if (n == 4)
            {
                if (galaxyShape == 0)
                {
                    // Normal 4 fold symmetry ==> always a square shape
                    FInt idealC = rows * sectorSlope * 2;
                    if (actualColumns < idealC || actualColumns > idealC + 2)
                    {
                        return;
                    }
                    g = MakeGrid(rows, columns);
                }
                else
                {
                    // FIXME: explain what I'm doing here
                    FInt idealR = actualColumns / sectorSlope * FInt.Create(500, false);
                    if (rows > idealR)
                    {
                        return;
                    }
                    g = MakeGrid(rows, columns);
                }
            }
            else
            {
                if (galaxyShape == 0)
                {
                    FInt leastR = actualColumns / sectorSlope * FInt.Create(750, false);
                    FInt idealR = actualColumns / sectorSlope * FInt.Create(750, false);
                    if (rows <= leastR)
                    {
                        return;
                    }
                    par.AddInfo("Ideal rows", idealR.ToString());
                    if (par.AddBadness("Rows Difference", (rows - idealR).Abs() * 5)) return;
                    g = MakeGrid(rows, columns);
                }
                else if (galaxyShape == 1)
                {
                    FInt idealR = actualColumns / sectorSlope / 2;
                    if (rows > idealR)
                    {
                        return;
                    }
                    g = MakeGrid(rows, columns);
                }
                else
                {
                    // Legacy case here
                    FInt idealR = actualColumns / sectorSlope * FInt.Create(750, false);
                    par.AddInfo("Ideal R", idealR.ToString());
                    if (par.AddBadness("Rotational Shape", (rows - idealR).Abs())) return;
                    g = MakeGrid(rows, columns);
                }
            }

            g.MakeRotationalGeneric(unit * columns / 2, unit * rows, unit, n, dihedral, advance);

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

        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            if (symmetry >= 300 && symmetry < 10000)
            {
                MakeRotationalGrid(outerPath, galaxyShape, symmetry, par);
                return;
            }

            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 1, 45, 7);
            int columns = par.AddParameter("columns", 1, 45, 10);

            if (galaxyShape != 2 && (rows > 35 || columns > 35)) return;
            if (galaxyShape != 0 && (rows < 3 || columns < 3)) return;
            if (symmetry == 10100)
            {
                // FIXME, should support all combinations
                galaxyShape = 0;
                if (rows < 3 || columns < 3) return;
            }
            else if (symmetry == 10200)
            {
                if (rows < columns) return;
                if (columns < 2) return;

                FInt aspectRatio = (FInt)columns / (FInt)rows;
                FInt idealAspectRatio = ((AspectRatio)aspectRatioIndex).Value() * FInt.Create(500, false);
                if (par.AddBadness("Y-Trunk Aspect Ratio", aspectRatio.RatioDeviance(idealAspectRatio) * 10)) return;
            }

            // `parts`: We divide the columns into this many parts.
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
            if (columns % parts > 1 && columns % parts < parts - 1) return;

            // `sp`: extra shape parameter for shape 1 & 2
            // For shape 1, it is the "bevel" size for the octagon
            // For shape 2, it is the cross width
            int sp = 0;
            if (galaxyShape == 1)
            {
                sp = par.AddParameter("bevel",
                    (Math.Min(rows, columns) + 12) / 15,
                    (Math.Min(rows, columns) * 2 + 2) / 3,
                    (Math.Min(rows, columns / parts) + 3) / 4);
            }
            else if (galaxyShape == 2)
            {
                sp = par.AddParameter("cross_width",
                    (Math.Min(rows, columns) + 12) / 15,
                    (Math.Min(rows, columns) - 1) / 2,
                    (Math.Min(rows, columns / parts) + 2) / 3 | (rows % 2));
            }
            if (galaxyShape == 1 && rows <= sp * 2) return;
            if (galaxyShape == 2 && ((rows + sp) % 2 != 0 || rows < sp + 2)) return;

            // `overlap`
            int overlap;
            if (parts == 1)
                overlap = 0;
            else
                overlap = par.AddParameter("overlap", -1, 1, parts == 2 ? 1 : 0);
            int d = overlap * (parts - 1);
            if ((columns + d) % parts != 0) return;

            if (parts == 2)
            {
                if (overlap == 0)
                {
                    if (par.AddBadness("Two-part Galaxies sharing an edge", (FInt)7, true)) return;
                }
                else if (overlap > 0)
                {
                    if (par.AddBadness("Two-part Galaxies overlapping", (FInt)12, true)) return;
                }
            }
            else if (parts == 3 && columns % 3 != 0)
            {
                if (overlap < 0)
                {
                    if (par.AddBadness("Three-part Galaxies not touching", (FInt)12, true)) return;
                }
                else if (overlap > 0)
                {
                    if (par.AddBadness("Three-part Galaxies overlapping", (FInt)7, true)) return;
                }
            }

            // `f`
            int f = (columns + d) / parts;
            if (galaxyShape == 1 && f <= sp * 2) return;
            if (galaxyShape == 2 && ((f + sp) % 2 != 0 || f < sp + 2)) return;

            if (galaxyShape == 1)
            {
                FInt idealO = Math.Min(rows, f) / FInt.Create(3414, false);
                par.AddInfo("Ideal O", idealO.ToString());
                if (par.AddBadness("Octagon Shape", (sp - idealO).Abs())) return;
            }
            else if (galaxyShape == 2)
            {
                FInt idealX = Math.Min(rows, f) / FInt.Create(3000, false);
                par.AddInfo("Ideal X", idealX.ToString());
                if (par.AddBadness("Cross Shape", (sp - idealX).Abs())) return;
            }

            // `offset`
            int offset = parts == 1 ? columns : (columns - f) / (parts - 1);
            if (offset == 0) return;

            // `borderThickness`
            // Only used for symmetry 10100 Duplex Barrier, but affects MakeGrid so has to be put here
            int borderThickness = 0;
            if (symmetry == 10100)
            {
                borderThickness = par.AddParameter("border_thickness", 1, (Math.Min(rows, columns) - 1) / 2, 1);
                FInt idealThickness = Math.Min(rows, columns) / FInt.Create(3000, false);
                if (par.AddBadness("Border Thickness", (rows - idealThickness).Abs())) return;
            }

            FakeGalaxy g = null;

            if (galaxyShape == 0)
            {
                g = MakeGrid(rows, columns, borderThickness);
            }
            else if (galaxyShape == 1)
            {
                g = MakeGridOctagonal(rows, columns, sp, f, offset);
            }
            else
            {
                g = MakeGridCross(rows, columns, sp, f, offset);
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
            else if (symmetry == 10100)
            {
                g.MakeDuplexBarrier(unit, unit * borderThickness, unit * borderThickness);
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }
            else if (symmetry == 10200)
            {
                int branchWidth = par.AddParameter("branch_width", (columns + 1) / 2, columns - 1, (columns * 4 + 3) / 5);
                g.MakeY((AspectRatio)aspectRatioIndex, unit, unit * branchWidth);
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

        protected static FakeGalaxy MakeGrid(int rows, int columns, int borderWidth = 0)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                {
                    if (borderWidth > 0 &&
                        i >= borderWidth && i < rows - borderWidth && j >= borderWidth && j < columns - borderWidth) continue;
                    square.Imprint(g, ArcenPoint.Create(j * unit, i * unit));
                }

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
    }
}