using Arcen.AIW2.Core;
using Arcen.Universal;
using System;
using System.Linq;
using System.Net;

namespace AhyangyiMaps.Tessellation
{
    public class SquareGrid : GridGenerator
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
        public static void MakeSquareGalaxyNew(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, int dissonance, int numPlanets, ParameterService par)
        {
            // `r` & `c`: The base grid size
            int r = par.AddParameter(1, 45, 9);
            int c = par.AddParameter(1, 45, 9);

            if (galaxyShape != 2 && (r > 35 || c > 35)) return;
            if (galaxyShape != 0 && (r < 3 || c < 3)) return;
            if (symmetry == 10100)
            {
                // FIXME, should support all combinations
                galaxyShape = 0;
                if (r < 3 || c < 3) return;
            }
            if (symmetry >= 300 && symmetry < 10000)
            {
                FInt idealR = c / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false);
                par.AddInfo("Ideal R", idealR.ToString());
                if (par.AddBadness("R Badness", (r - idealR).Abs())) return;
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
            if (c % parts > 1 && c % parts < parts - 1) return;

            // `sp`: extra shape parameter for shape 1 & 2
            // For shape 1, it is the corner size for the octagon
            // For shape 2, it is the cross width
            int sp = 0;
            if (galaxyShape == 1)
            {
                sp = par.AddParameter((Math.Min(r, c) + 12) / 15, (Math.Min(r, c) * 2 + 2) / 3, (Math.Min(r, c / parts) + 3) / 4);
            }
            else if (galaxyShape == 2)
            {
                sp = par.AddParameter((Math.Min(r, c) + 12) / 15, (Math.Min(r, c) - 1) / 2, (Math.Min(r, c / parts) + 2) / 3 | (r % 2));
            }
            if (galaxyShape == 1 && r <= sp * 2) return;
            if (galaxyShape == 2 && ((r + sp) % 2 != 0 || r < sp + 2)) return;

            // `overlap`
            int overlap;
            if (parts == 1)
                overlap = 0;
            else
                overlap = par.AddParameter(-1, 1, parts == 2? 1 : 0);
            int d = overlap * (parts - 1);
            if ((c + d) % parts != 0) return;

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
            else if (parts == 3 && c % 3 != 0)
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
            int f = (c + d) / parts;
            if (galaxyShape == 1 && f <= sp * 2) return;
            if (galaxyShape == 2 && ((f + sp) % 2 != 0 || f < sp + 2)) return;

            if (galaxyShape == 1)
            {
                FInt idealO = Math.Min(r, f) / FInt.Create(3414, false);
                par.AddInfo("Ideal O", idealO.ToString());
                if (par.AddBadness("Octagon Shape", (sp - idealO).Abs())) return;
            }
            else if (galaxyShape == 2)
            {
                FInt idealX = Math.Min(r, f) / FInt.Create(3000, false);
                par.AddInfo("Ideal X", idealX.ToString());
                if (par.AddBadness("Cross Shape", (sp - idealX).Abs())) return;
            }

            // `offset`
            int offset = parts == 1 ? c : (c - f) / (parts - 1);
            if (offset == 0) return;

            // `borderThickness`
            // Only used for symmetry 10100 Duplex Barrier
            int borderThickness = 0;
            if (symmetry == 10100)
            {
                borderThickness = par.AddParameter(1, (Math.Min(r, c) - 1) / 2, 1);
            }

            int branchWidth = 0;
            if (symmetry == 10200)
            {
                branchWidth = par.AddParameter((c + 1) / 2, c - 1, (c * 4 + 3) / 5);
            }

            FakeGalaxy g = null;

            if (galaxyShape == 0)
            {
                g = MakeGrid(r, c, borderThickness);
            }
            else if (galaxyShape == 1)
            {
                g = MakeGridOctagonal(r, c, sp, f, offset);
            }
            else
            {
                g = MakeGridCross(r, c, sp, f, offset);
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
                g.MakeRotationalGeneric(unit * c / 2, unit * r, unit, symmetry / 100, symmetry % 100 == 50, c % 2 == 1);
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
                g.MakeY((AspectRatio)aspectRatioIndex, unit, unit * branchWidth);
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

        public (FakeGalaxy, FakeGalaxy) MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, int dissonance, int numPlanets, ParameterService par)
        {
            MakeSquareGalaxyNew(outerPath, aspectRatioIndex, galaxyShape, symmetry, dissonance, numPlanets, par);
            return (par.g, par.p);
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