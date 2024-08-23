using Arcen.AIW2.Core;
using Arcen.Universal;
using System;

namespace AhyangyiMaps.Tessellation
{
    public class DiamondYGrid
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
            bottom.AddLinkTo(right);
            right.AddLinkTo(topRight);
            topRight.AddLinkTo(top);
            top.AddLinkTo(topLeft);
            topLeft.AddLinkTo(left);
            left.AddLinkTo(bottom);
            center.AddLinkTo(topLeft);
            center.AddLinkTo(topRight);
            center.AddLinkTo(bottom);

            diamondY.breakpoints.Add((bottom.Location, left.Location), new System.Collections.Generic.List<ArcenPoint> { ArcenPoint.Create(unit, unit) });
            diamondY.breakpoints.Add((bottom.Location, right.Location), new System.Collections.Generic.List<ArcenPoint> { ArcenPoint.Create(unit * 3, unit) });
            diamondY.connectionsToBreak.Add((top.Location, left.Location));
            diamondY.connectionsToBreak.Add((top.Location, right.Location));

            diamondYFlipped = diamondY.FlipY();
        }
        public static FakeGalaxy MakeGalaxy(PlanetType planetType, FInt aspectRatio, int galaxyShape, int symmetry, int numPlanets)
        {
            int rows = 5;
            int columns = 8;
            FInt badness = (FInt)1000000;
            for (int r = 2; r <= 60; r++)
            {
                for (int c = 2; c <= 60; c++)
                {
                    if (symmetry == 150 && c % 2 == 0) continue;
                    if (symmetry == 10000 && (c % 4 == 1 || c % 4 == 2)) continue;
                    if (symmetry == 10001 && c % 3 != 2) continue;
                    // FIXME: only works when both are odd
                    int planets = 2 * r * c + r + c + 3;
                    FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                    FInt currentAspectRatio = (FInt)(r + 1) / (FInt)(c + 1);
                    FInt p1 = currentAspectRatio / aspectRatio;
                    FInt p2 = aspectRatio / currentAspectRatio;
                    FInt aspectRatioBadness = ((p1 > p2 ? p1 : p2) - FInt.One) * (FInt)10;
                    FInt current_badness = planetBadness + aspectRatioBadness;
                    if (current_badness < badness)
                    {
                        badness = current_badness;
                        rows = r;
                        columns = c;
                    }
                }
            }
            FakeGalaxy g = MakeGrid(rows, columns, false);

            if (symmetry == 150)
            {
                g.MakeBilateral();
            }
            else if (symmetry >= 300 && symmetry < 10000)
            {
                FInt newBadness = (FInt)1000000;
                FakeGalaxy fg = MakeGrid(1, 1, true);
                for (int c = 1; c <= 60; ++c)
                {
                    int r1 = ((c + 1) / SymmetryConstants.Rotational[symmetry / 100].sectorSlope * FInt.Create(750, false) - 1).ToInt();
                    for (int r = Math.Max(r1 - 1, 1); r <= r1 + 3; ++r)
                    {
                        if ((r * 2 + c) % 4 != 3)
                            continue;
                        var g2 = MakeGrid(r, c, true);
                        g2.MakeRotationalGeneric((c + 1) * unit, (r + 1) * 2 * unit, dunit, symmetry / 100, symmetry % 100 == 50, false);
                        int planets = g2.planets.Count;
                        FInt planetBadness = (FInt)Math.Abs(planets - numPlanets);
                        FInt current_badness = planetBadness;
                        if (current_badness < newBadness)
                        {
                            newBadness = current_badness;
                            fg = g2;
                        }
                    }
                }
                return fg;
            }
            else if (symmetry == 10000)
            {
                g.MakeTranslational2((columns + 3) / 4 * 2 * unit);
            }
            else if (symmetry == 10001)
            {
                g.MakeTriptych((columns + 1) / 3 * unit);
            }

            return g;
        }

        private static FakeGalaxy MakeGrid(int rows, int columns, bool flip)
        {
            FakeGalaxy g = new FakeGalaxy();
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < columns; ++j)
                    if ((i + j) % 2 == 0)
                        (flip ? diamondYFlipped : diamondY).Imprint(g, ArcenPoint.Create(j * unit * 2, i * unit * 2));

            return g;
        }
    }
}