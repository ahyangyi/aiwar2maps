using Arcen.AIW2.Core;
using Arcen.Universal;

namespace AhyangyiMaps.Tessellation
{
    public class DiamondYGrid : IGridGenerator
    {
        static readonly int unit, dunit;
        public static readonly FakePattern diamondY, diamondYFlipped;
        static DiamondYGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 7071 / 1000;
            dunit = PlanetType.Normal.GetData().InterStellarRadius * 10;

            diamondY = new FakePattern();
            var bottom = diamondY.AddPlanetAt(ArcenPoint.Create(unit * 2, 0));
            var right = diamondY.AddPlanetAt(ArcenPoint.Create(unit * 4, unit * 2));
            var top = diamondY.AddPlanetAt(ArcenPoint.Create(unit * 2, unit * 4));
            var left = diamondY.AddPlanetAt(ArcenPoint.Create(0, unit * 2));
            var center = diamondY.AddPlanetAt(ArcenPoint.Create(unit * 2, unit * 2));
            var topLeft = diamondY.AddPlanetAt(ArcenPoint.Create(unit, unit * 3));
            var topRight = diamondY.AddPlanetAt(ArcenPoint.Create(unit * 3, unit * 3));

            diamondY.AddLink(bottom, right);
            diamondY.AddLink(right, topRight);
            diamondY.AddLink(topRight, top);
            diamondY.AddLink(top, topLeft);
            diamondY.AddLink(topLeft, left);
            diamondY.AddLink(left, bottom);
            diamondY.AddLink(center, topLeft);
            diamondY.AddLink(center, topRight);
            diamondY.AddLink(center, bottom);

            diamondY.breakpoints.Add((bottom.Location, left.Location), new System.Collections.Generic.List<ArcenPoint> { ArcenPoint.Create(unit, unit) });
            diamondY.breakpoints.Add((bottom.Location, right.Location), new System.Collections.Generic.List<ArcenPoint> { ArcenPoint.Create(unit * 3, unit) });
            diamondY.connectionsToBreak.Add((top.Location, left.Location));
            diamondY.connectionsToBreak.Add((top.Location, right.Location));

            diamondYFlipped = diamondY.FlipY();
        }
        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 2, 35, 7);
            int columns = par.AddParameter("columns", 2, 35, symmetry == 10001 ? 9 : 10);
            int oddity = par.AddParameter("oddity", 0, 1, 0);

            if (symmetry == 150 && columns % 2 == 0) return;
            if (symmetry >= 300 && symmetry < 10000)
            {
                if (columns % 2 == 0) return;
                FInt idealR = ((columns + 1) / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) - 1) / 2;
                par.AddInfo("Ideal R", idealR.ToString());
                if (par.AddBadness("Rotational Shape", (rows - idealR).Abs())) return;
            }
            if (symmetry == 10000 && (columns % 4 == 1 || columns % 4 == 2)) return;
            if (symmetry == 10001 && columns % 3 != 2) return;
            if (symmetry == 10101 && columns % 2 != 1) return;

            FakeGalaxy g = MakeGrid(rows, columns, oddity, false);

            if (symmetry == 150)
            {
                g.MakeBilateral();
            }
            else if (symmetry >= 300 && symmetry < 10000)
            {
                g.MakeRotationalGeneric((columns + 1) * unit, (rows + 1) * 2 * unit, dunit, symmetry / 100, symmetry % 100 == 50, false);
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2((columns + 3) / 4 * 2 * unit);
            }
            else if (symmetry == 10001)
            {
                if (columns % 3 == 0)
                {
                    g.MakeTriptych(columns / 3 * unit);
                }
                else
                {
                    g.MakeTriptych((columns * 2 + 1) * unit / 6, (columns * 4 - 1) * unit / 6);
                }
            }
            else if (symmetry == 10101)
            {
                g.MakeDoubleSpark();
            }
            else if (symmetry == 10200)
            {
                columns = (columns + 3) / 4;
                rows = (rows * 4 / 3) / 2 * 2 + columns % 2;
                g = MakeGrid(rows, columns, oddity, false);
                g.MakeY((AspectRatio)aspectRatioIndex, dunit, columns * 2 * unit);
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

        private static FakeGalaxy MakeGrid(int rows, int columns, int oddity, bool flip)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    if ((i + j) % 2 == oddity)
                        (flip ? diamondYFlipped : diamondY).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
    }
}