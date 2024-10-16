using Arcen.Universal;
using System;

namespace AhyangyiMaps
{
    public static class ArcenPointExtensions
    {
        public static int DotProduct(this ArcenPoint a, ArcenPoint b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        public static int CrossProduct(this ArcenPoint a, ArcenPoint b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static double AccurateAngleInRadian(this ArcenPoint a)
        {
            return Math.Atan2(a.Y, a.X);
        }

        public static double AccurateDistanceTo(this ArcenPoint a, ArcenPoint b)
        {
            var c = a - b;
            return Math.Sqrt(c.X * c.X + c.Y * c.Y);
        }
    }
    public struct Matrix2x2
    {
        public FInt xx, xy, yx, yy;


        public Matrix2x2(FInt xx, FInt xy, FInt yx, FInt yy)
        {
            this.xx = xx;
            this.xy = xy;
            this.yx = yx;
            this.yy = yy;
        }

        public static Matrix2x2 Rotation(FInt xx, FInt xy)
        {
            return new Matrix2x2(xx, xy, -xy, xx);
        }

        public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
        {
            return new Matrix2x2(a.xx * b.xx + a.xy * b.yx, a.xx * b.xy + a.xy * b.yy, a.yx * b.xx + a.yy * b.yx, a.yx * b.xy + a.yy * b.yy);
        }

        public static Matrix2x2 operator *(Matrix2x2 a, FInt b)
        {
            return new Matrix2x2(a.xx * b, a.xy * b, a.yx * b, a.yy * b);
        }

        public (FInt, FInt) Apply(FInt x, FInt y)
        {
            return (this.xx * x + this.yx * y, this.xy * x + this.yy * y);
        }

        public (int, int) Apply(int x, int y)
        {
            return ((this.xx * x + this.yx * y).GetNearestIntPreferringLower(), (this.xy * x + this.yy * y).GetNearestIntPreferringLower());
        }

        public ArcenPoint Apply(ArcenPoint reference, int x, int y)
        {
            (x, y) = Apply(x, y);
            return ArcenPoint.Create(reference.X + x, reference.Y + y);
        }
        public ArcenPoint AbsoluteApply(ArcenPoint reference, ArcenPoint point)
        {
            var (x, y) = Apply(point.X - reference.X, point.Y - reference.Y);
            return ArcenPoint.Create(reference.X + x, reference.Y + y);
        }

        public static Matrix2x2 Identity = Matrix2x2.Rotation(FInt.One, FInt.Zero);
        public static Matrix2x2 Zero = Matrix2x2.Rotation(FInt.Zero, FInt.Zero);
        public static Matrix2x2 FlipX = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 FlipY = new Matrix2x2(FInt.One, FInt.Zero, FInt.Zero, (FInt)(-1));
        public static Matrix2x2 ProjectToX = new Matrix2x2(FInt.One, FInt.Zero, FInt.Zero, FInt.Zero);
        public static Matrix2x2 ProjectToNegX = new Matrix2x2((FInt)(-1), FInt.Zero, FInt.Zero, FInt.Zero);
        public static Matrix2x2 ProjectToY = new Matrix2x2(FInt.Zero, FInt.Zero, FInt.Zero, FInt.One);
        public static Matrix2x2 ProjectToNegY = new Matrix2x2(FInt.Zero, FInt.Zero, FInt.Zero, (FInt)(-1));

        public static Matrix2x2 Rotation2 = Matrix2x2.Rotation((FInt)(-1), FInt.Zero);
        public static Matrix2x2 Rotation3_1 = Matrix2x2.Rotation(FInt.Create(-500, false), FInt.Create(866, false));
        public static Matrix2x2 Rotation3_2 = Matrix2x2.Rotation(FInt.Create(-500, false), FInt.Create(-866, false));
        public static Matrix2x2[] Rotation3 = { Identity, Rotation3_1, Rotation3_2 };
        public static Matrix2x2 Rotation4_1 = Matrix2x2.Rotation(FInt.Zero, FInt.One);
        public static Matrix2x2 Rotation4_3 = Matrix2x2.Rotation(FInt.Zero, (FInt)(-1));
        public static Matrix2x2[] Rotation4 = { Identity, Rotation4_1, Rotation2, Rotation4_3 };
        public static Matrix2x2 Rotation5_1 = Matrix2x2.Rotation(FInt.Create(309, false), FInt.Create(951, false));
        public static Matrix2x2 Rotation5_4 = Matrix2x2.Rotation(FInt.Create(309, false), FInt.Create(-951, false));
        public static Matrix2x2 Rotation5_2 = Matrix2x2.Rotation(FInt.Create(-809, false), FInt.Create(588, false));
        public static Matrix2x2 Rotation5_3 = Matrix2x2.Rotation(FInt.Create(-809, false), FInt.Create(-588, false));
        public static Matrix2x2[] Rotation5 = { Identity, Rotation5_1, Rotation5_2, Rotation5_3, Rotation5_4 };
        public static Matrix2x2 Rotation6_1 = Matrix2x2.Rotation(FInt.Create(500, false), FInt.Create(866, false));
        public static Matrix2x2 Rotation6_5 = Matrix2x2.Rotation(FInt.Create(500, false), FInt.Create(-866, false));
        public static Matrix2x2[] Rotation6 = { Identity, Rotation6_1, Rotation3_1, Rotation2, Rotation3_2, Rotation6_5 };
        public static Matrix2x2 Rotation7_1 = Matrix2x2.Rotation(FInt.Create(623, false), FInt.Create(782, false));
        public static Matrix2x2 Rotation7_6 = Matrix2x2.Rotation(FInt.Create(623, false), FInt.Create(-782, false));
        public static Matrix2x2 Rotation7_2 = Matrix2x2.Rotation(FInt.Create(-223, false), FInt.Create(975, false));
        public static Matrix2x2 Rotation7_5 = Matrix2x2.Rotation(FInt.Create(-223, false), FInt.Create(-975, false));
        public static Matrix2x2 Rotation7_3 = Matrix2x2.Rotation(FInt.Create(-901, false), FInt.Create(434, false));
        public static Matrix2x2 Rotation7_4 = Matrix2x2.Rotation(FInt.Create(-901, false), FInt.Create(-434, false));
        public static Matrix2x2[] Rotation7 = { Identity, Rotation7_1, Rotation7_2, Rotation7_3, Rotation7_4, Rotation7_5, Rotation7_6 };
        public static Matrix2x2 Rotation8_1 = Matrix2x2.Rotation(FInt.Create(707, false), FInt.Create(707, false));
        public static Matrix2x2 Rotation8_7 = Matrix2x2.Rotation(FInt.Create(707, false), FInt.Create(-707, false));
        public static Matrix2x2 Rotation8_3 = Matrix2x2.Rotation(FInt.Create(-707, false), FInt.Create(707, false));
        public static Matrix2x2 Rotation8_5 = Matrix2x2.Rotation(FInt.Create(-707, false), FInt.Create(-707, false));
        public static Matrix2x2[] Rotation8 = { Identity, Rotation8_1, Rotation4_1, Rotation8_3, Rotation2, Rotation8_5, Rotation4_3, Rotation8_7 };
        public static Matrix2x2 Rotation10_1 = Matrix2x2.Rotation(FInt.Create(809, false), FInt.Create(588, false));
        public static Matrix2x2 Rotation12_1 = Matrix2x2.Rotation(FInt.Create(866, false), FInt.Create(500, false));
        public static Matrix2x2[][] Rotations = { null, null, null, Rotation3, Rotation4, Rotation5, Rotation6, Rotation7, Rotation8 };

        public static Matrix2x2[] Rotation3ReflectLeft = { ProjectToY * Rotation3_1, ProjectToY * Rotation3_2, ProjectToY };
        public static Matrix2x2[] Rotation3ReflectCenter = { ProjectToY, ProjectToY * Rotation3_1, ProjectToY * Rotation3_2 };

        public static Matrix2x2 ProjectToXY = new Matrix2x2(FInt.Create(707, false), FInt.Create(707, false), FInt.Create(707, false), FInt.Create(707, false));
        public static Matrix2x2[] Rotation4ReflectLeft = { ProjectToXY, ProjectToXY * Rotation4_1, ProjectToXY * Rotation2, ProjectToXY * Rotation4_3 };
        public static Matrix2x2[] Rotation4ReflectCenter = { ProjectToY, ProjectToY * Rotation4_1, ProjectToY * Rotation2, ProjectToY * Rotation4_3 };

        public static Matrix2x2[][] RotationReflectLeft = { null, null, null, Rotation3ReflectLeft, Rotation4ReflectLeft, null, null, null, null };
        public static Matrix2x2[][] RotationReflectCenter = { null, null, null, Rotation3ReflectCenter, Rotation4ReflectCenter, null, null, null, null };
    }
    public class SymmetryConstants
    {
        // dx = dy * sectorSlope; tan(180 / n)
        public FInt sectorSlope;
        // dx = d * distanceCoefficient; sec(180 / n)
        public FInt distanceCoefficient;

        public SymmetryConstants(FInt sectorSlope, FInt distanceCoefficient)
        {
            this.sectorSlope = sectorSlope;
            this.distanceCoefficient = distanceCoefficient;
        }

        public static SymmetryConstants Rotational3 = new SymmetryConstants(FInt.Create(1732, false), FInt.Create(2000, false));
        public static SymmetryConstants Rotational4 = new SymmetryConstants(FInt.Create(1000, false), FInt.Create(1414, false));
        public static SymmetryConstants Rotational5 = new SymmetryConstants(FInt.Create(727, false), FInt.Create(1236, false));
        public static SymmetryConstants Rotational6 = new SymmetryConstants(FInt.Create(577, false), FInt.Create(1155, false));
        public static SymmetryConstants Rotational7 = new SymmetryConstants(FInt.Create(482, false), FInt.Create(1110, false));
        public static SymmetryConstants Rotational8 = new SymmetryConstants(FInt.Create(414, false), FInt.Create(1082, false));
        public static SymmetryConstants[] Rotational = { null, null, null, Rotational3, Rotational4, Rotational5, Rotational6, Rotational7, Rotational8 };
    }

    public static class Geometry
    {
        public static bool LineSegmentIntersectsLineSegment(ArcenPoint a1, ArcenPoint a2, ArcenPoint b1, ArcenPoint b2, bool strict = false)
        {
            if (strict)
            {
                if (a1 == b1) return false;
                if (a1 == b2) return false;
                if (a2 == b1) return false;
                if (a2 == b2) return false;
            }
            if (Math.Min(a1.X, a2.X) > Math.Max(b1.X, b2.X))
                return false;
            if (Math.Max(a1.X, a2.X) < Math.Min(b1.X, b2.X))
                return false;
            if (Math.Min(a1.Y, a2.Y) > Math.Max(b1.Y, b2.Y))
                return false;
            if (Math.Max(a1.Y, a2.Y) < Math.Min(b1.Y, b2.Y))
                return false;

            int x, y;
            x = (a1 - a2).CrossProduct(b1 - a2);
            y = (a1 - a2).CrossProduct(b2 - a2);
            if (x < 0 && y < 0 || x > 0 && y > 0)
                return false;
            x = (b1 - b2).CrossProduct(a1 - b2);
            y = (b1 - b2).CrossProduct(a2 - b2);
            if (x < 0 && y < 0 || x > 0 && y > 0)
                return false;

            return true;
        }

        // Returns a number between 0 and 3
        // 0: next is on the ray cur->prev
        // 1: next is on the "left-hand" semiplane divided by the cur->prev line
        // 2: next is opposite to cur->prev
        // 3: next is on the "right-hand" semiplane divided by the cur->prev line
        // The purpose of this number is that 0->1->2->3 maintains a counterclockwise order,
        //     and two directions in the same region can be compared by cross product
        public static int RegionNumber(ArcenPoint cur, ArcenPoint prev, ArcenPoint next)
        {
            int cross = (next - cur).CrossProduct(prev - cur);
            if (cross == 0)
            {
                int dot = (next - cur).DotProduct(prev - cur);

                if (dot >= 0)
                    return 0;
                return 2;
            }
            if (cross > 0)
            {
                return 1;
            }
            return 3;
        }
    }
}