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
                    g = SemifloretStyle(sectorSlope, rows, columns, actualColumns);
                }
                else if (galaxyShape == 1)
                {
                    g = FloretStyle(sectorSlope, rows, columns, actualColumns);
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
                    g = FloretStyle(sectorSlope, rows, columns, actualColumns);
                }
                else
                {
                    g = StarStyle(sectorSlope, rows, columns, actualColumns);
                }
            }
            else
            {
                if (galaxyShape == 0)
                {
                    g = FloretStyle(sectorSlope, rows, columns, actualColumns);
                }
                else if (galaxyShape == 1)
                {
                    g = PolygonStyle(sectorSlope, rows, columns, actualColumns);
                }
                else
                {
                    g = StarStyle(sectorSlope, rows, columns, actualColumns);
                }
            }

            if (g == null)
            {
                return;
            }
            g.MakeRotationalGeneric((columns + 1) * xunit / 2, (rows * 3 + 1) * yunit, dunit, n, dihedral,
                outerPath, out FakeGalaxy p, out Outline outline, advance, connectThreshold);

            par.Commit(g, p, outline);
        }

        private static FakeGalaxy PolygonStyle(FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            // yunit * (3r + 1) = xunit * (c + 1) / 2 / sectorSlope
            FInt idealR = ((actualColumns + 1) * xunit / yunit / sectorSlope / 2 - 1) / 3;
            if (rows > idealR)
            {
                return null;
            }
            return MakeGridRectangular(rows, columns, 0);
        }
        private static FakeGalaxy SemifloretStyle(FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            FInt idealR = ((actualColumns + 1) * xunit / yunit / sectorSlope / 2 - 1) / 3 + columns / 8;
            if (rows <= idealR - 1 || rows > idealR || columns < 9 || columns % 8 != 1)
            {
                return null;
            }
            return MakeGridOctagonal(rows, columns, columns / 8, 0, true);
        }

        private static FakeGalaxy FloretStyle(FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            FInt idealR = ((actualColumns + 1) * xunit / yunit / sectorSlope / 2 - 1) / 3 + columns / 6;
            if (rows <= idealR - 1 || rows > idealR || columns < 7 || columns % 6 != 1)
            {
                return null;
            }
            return MakeGridOctagonal(rows, columns, columns / 6, 0, true);
        }

        private static FakeGalaxy StarStyle(FInt sectorSlope, int rows, int columns, int actualColumns)
        {
            FInt idealR = ((actualColumns + 1) * xunit / yunit / sectorSlope / 2 - 1) / 3 + columns / 2;
            if (rows <= idealR - 1 || rows > idealR || columns < 3)
            {
                return null;
            }
            return MakeGridOctagonal(rows, columns, columns / 2, 0, true);
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

            // `overlap`, fine control how multi-part galaxies look like
            int overlap;
            if (parts == 1)
                overlap = 0;
            else
                overlap = par.AddParameter("overlap", 0, 8, parts == 2 ? 1 : 0);
            int d = overlap * (parts - 1);
            if ((columns + d) % parts != 0) return;

            if (parts > 1 && overlap < 0)
            {
                return;
            }

            if (parts == 2)
            {
                if (galaxyShape == 0)
                {
                    if (overlap < -2)
                    {
                        if (par.AddBadness("Two-part Galaxies too faraway", (FInt)(-1 - overlap * 2), true)) return;
                    }
                    else if (overlap >= -1)
                    {
                        if (par.AddBadness("Two-part Galaxies too close", (FInt)9 + overlap * 2, true)) return;
                    }
                }
                else
                {
                    if (overlap < -1)
                    {
                        if (par.AddBadness("Two-part Galaxies too faraway", (FInt)(10 - overlap * 2), true)) return;
                    }
                    else if (overlap >= 2)
                    {
                        if (par.AddBadness("Two-part Galaxies too close", (FInt)overlap * 2, true)) return;
                    }
                }
            }
            else if (parts == 3)
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

            // `f`: the actual number of columns per part
            int f = (columns + d) / parts;
            if (f == 0)
            {
                return;
            }

            // `offset`
            int offset = parts == 1 ? columns : (columns - f) / (parts - 1);
            if (offset <= 0)
            {
                return;
            }

            // oddity, and other galaxyShape-specific limitations
            int oddity = 0;

            if (galaxyShape == 0)
            {
                oddity = par.AddParameter("oddity", 0, 1, 0);
            }
            else if (galaxyShape == 1)
            {
                if (rows % 2 == 0 || f % 2 == 0)
                {
                    return;
                }
                int style = parts == 1 ? aspectRatioIndex : 2;
                int columnsThreshold = rows * 2 + 1 - 2 * galaxyShape;
                if (style == 0 && f <= columnsThreshold)
                {
                    return;
                }
                if (style == 1 && f != columnsThreshold)
                {
                    return;
                }
                if (style == 2 && (f >= columnsThreshold || f % 4 != 1))
                {
                    return;
                }
            }
            else if (galaxyShape == 2)
            {
                if (rows % 2 == 0 || columns % 2 == 0 || rows < 3)
                {
                    return;
                }
                if (aspectRatioIndex <= 1 && columns <= rows)
                {
                    return;
                }
                if (aspectRatioIndex == 2 && columns != rows)
                {
                    return;
                }
            }

            // symmetry-specific conditions
            if ((symmetry == 10000 || symmetry == 10001) && offset % 2 == 1) return;

            if (symmetry == 150 && columns % 2 == 0) return;
            if (symmetry == 200 && (rows + columns) % 2 == 1) return;
            if (symmetry == 250 && (rows % 2 == 0 || columns % 2 == 0)) return;
            if (symmetry == 10101 && columns % 2 != 1) return;

            FakeGalaxy g;

            if (galaxyShape == 0)
            {
                g = MakeGridRectangular(rows, columns, oddity);
            }
            else if (galaxyShape == 1)
            {
                if (aspectRatioIndex <= 1 && parts == 1)
                {
                    g = MakeGridOctagonal(rows, columns, rows / 2, 0);
                }
                else
                {
                    g = MakeGridOctagonal(rows, columns, f / 4, 0, false, f, offset);
                }
            }
            else
            {
                if (aspectRatioIndex <= 1)
                {
                    g = MakeGridOctagonal(rows, columns, rows / 2, (rows + 1) / 3);
                }
                else
                {
                    g = MakeGridOctagonal(rows, columns, columns / 2, 0);
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
                g.MakeTranslational2(offset * xunit);
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych(offset * xunit);
            }
            else if (symmetry == 10002)
            {
                g.MakeDualGalaxy(offset * xunit);
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

        private static FakeGalaxy MakeGridOctagonal(int rows, int columns, int bevel, int batmanness, bool bottomHalf = false, int sectionColumns = 0, int sectionOffset = 0)
        {
            FakeGalaxy g = new FakeGalaxy();
            if (sectionColumns == 0)
            {
                sectionColumns = sectionOffset = columns;
            }
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    if ((i + j) % 2 == bevel % 2)
                    {
                        int k = j;
                        if (j >= columns - sectionOffset)
                        {
                            while (k >= sectionColumns) k -= sectionOffset;
                        }
                        else if (j >= sectionOffset)
                        {
                            while (k >= (sectionColumns + sectionOffset) / 2) k -= sectionOffset;
                        }
                        if ((i + k) < bevel) continue;
                        if ((i + sectionColumns - 1 - k) < bevel) continue;
                        if (!bottomHalf)
                        {
                            if ((rows - 1 - i + k) < bevel) continue;
                            if ((rows - 1 - i + sectionColumns - 1 - k) < bevel) continue;
                        }
                        if (k > bevel && k < sectionColumns - 1 - bevel)
                        {
                            if (i + k > rows - 1 + bevel && i + sectionColumns - 1 - k > rows - 1 + bevel && rows - 1 - i < batmanness) continue;
                            if (rows - 1 - i + k > rows - 1 + bevel && rows - 1 - i + sectionColumns - 1 - k > rows - 1 + bevel && i < batmanness) continue;
                        }
                        hexagon.Imprint(g, ArcenPoint.Create(j * xunit, i * yunit * 3));
                    }

            return g;
        }
    }
}