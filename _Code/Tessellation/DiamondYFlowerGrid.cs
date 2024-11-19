using Arcen.AIW2.Core;
using Arcen.Universal;

namespace AhyangyiMaps.Tessellation
{
    public class DiamondYFlowerGrid : IGridGenerator
    {
        static readonly int unit, dunit;
        public static readonly FakePattern diamondY, diamondYFlipped, diamondYLeft, diamondYRight;
        static DiamondYFlowerGrid()
        {
            unit = PlanetType.Normal.GetData().InterStellarRadius * 7071 / 1000;
            dunit = PlanetType.Normal.GetData().InterStellarRadius * 10;

            diamondY = DiamondYGrid.diamondY;
            diamondYFlipped = DiamondYGrid.diamondYFlipped;
            diamondYLeft = diamondY.RotateLeft();
            diamondYRight = diamondYFlipped.RotateLeft();
        }

        public void MakeGrid(int outerPath, int aspectRatioIndex, int galaxyShape, int symmetry, ParameterService par)
        {
            // `rows` & `columns`: The base grid size
            int rows = par.AddParameter("rows", 3, 35, 5);
            int columns = par.AddParameter("columns", 3, 35, 7);

            if (rows % 2 == 0 || columns % 2 == 0) return;
            if (symmetry == 150 && columns % 4 == 1) return;
            if (symmetry == 200 && ((rows + columns) % 4 == 0)) return;
            if (symmetry == 250 && (rows % 4 == 1 || columns % 4 == 1)) return;
            if (symmetry == 10000 && columns % 8 != 7) return;
            if (symmetry == 10101 && columns % 4 != 3) return;

            if (symmetry >= 300 && symmetry < 10000)
            {
                if (columns % 2 == 0) return;
                FInt idealR = ((columns + 1) / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) - 1) / 2;
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
                g.MakeRotationalGeneric((columns + 1) * unit, (rows + 1) * 2 * unit, dunit, symmetry / 100, symmetry % 100 == 50, false);
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2((columns + 1) * unit);
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
                    if ((i + j) % 2 == 1)
                        (i % 2 == 0 ? ((i + j) % 4 == 1 ? diamondY : diamondYFlipped) : ((i + j) % 4 == 1 ? diamondYRight : diamondYLeft)).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
    }
}