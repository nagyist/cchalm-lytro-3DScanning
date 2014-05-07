using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace PixelMatchingTest
{
    public static class Geometry
    {
        // Returns the endpoints of a line segment which is the portion of the ray inside the 
        // rectangle, or null if the ray is never inside the rectangle. Endpoints are returned as 
        // step counts from the ray position. Note that the entry point need not be on the edge of 
        // the rectangle if rayPos is inside the rectangle. rayDir must have length > 0. Note that
        // this method deals in PIXELS, so points on opposite sides of a rectangle with width 10
        // are only 9 units apart.
        public static RayIntersection RayRectangleIntersect(Point2Df rayPos, 
            Point2Df rayStep, Rectangle rect)
        {
            // CHECKME don't treat starting inside the rectangle as a special case?
            // Keep in mind the y-axis increases down

            Debug.Assert(rayStep.X != 0 || rayStep.Y != 0,
                "Ray direction is (0, 0)");

            // Inclusive rectangle bounds
            int left = rect.Left;
            int right = rect.Right - 1;
            int top = rect.Top;
            int bottom = rect.Bottom - 1;

            // By policy, infinite enter distances are positive, and infinite exit distances are 
            // negative
            EnterLeavePair intersectX = RayLineIntersect(rayPos.X, rayStep.X, left, right);
            EnterLeavePair intersectY = RayLineIntersect(rayPos.Y, rayStep.Y, top, bottom);

            // We should take the number of steps that gets us to the furthest away entry point
            double maxStepsToEnter = Math.Max(intersectX.Enter, intersectY.Enter);
            double minStepsToLeave = Math.Min(intersectX.Leave, intersectY.Leave);

            // If either:
            //  (a) either step count to enter is negative (this includes infinites, by policy)
            //  (b) the furthest entry point happens before the closest exit point (infinite 
            //      exit points are positive by policy)
            // Then the ray does not intersect the rectangle. Otherwise the entry point is the
            // furthest entry point
            if (intersectX.Enter < 0 || intersectY.Enter < 0 ||
                maxStepsToEnter > minStepsToLeave)
                return null;
            else
                return new RayIntersection(maxStepsToEnter, minStepsToLeave);
        }

        public class RayIntersection
        {
            public double StepsToEntry { get { return stepsToEntry; } }
            public double StepsToExit { get { return stepsToExit; } }

            private double stepsToEntry;
            private double stepsToExit;

            public RayIntersection(double stepsToEntry, double stepsToExit)
            {
                this.stepsToEntry = stepsToEntry;
                this.stepsToExit = stepsToExit;
            }
        }

        // FIXME better comment, public?
        // one-dimensional ray intersecting with a line segment, entry and exit distances weighted
        // by step length
        private static EnterLeavePair RayLineIntersect(double rayPos, double rayStep, 
            int low, int high)
        {
            double distToLow = (low - rayPos) / rayStep;
            double distToHigh = (high - rayPos) / rayStep;

            double distToEnter;
            double distToLeave;

            if (rayPos < low)
            {
                distToEnter = distToLow;
                distToLeave = distToHigh;
            }
            else if (rayPos > high)
            {
                distToEnter = distToHigh;
                distToLeave = distToLow;
            }
            else
            {
                distToEnter = 0;

                if (rayStep > 0)
                    distToLeave = distToHigh;
                else if (rayStep < 0)
                    distToLeave = distToLow;
                else // rayStep == 0
                    distToLeave = double.PositiveInfinity;
            }

            if (double.IsPositiveInfinity(distToEnter))
                distToEnter = double.NegativeInfinity;
            if (double.IsNegativeInfinity(distToLeave))
                distToLeave = double.PositiveInfinity;

            return new EnterLeavePair(distToEnter, distToLeave);
        }

        private struct EnterLeavePair
        {
            public double Enter { get; set; }
            public double Leave { get; set; }

            public EnterLeavePair(double enter, double leave) : this()
            {
                this.Enter = enter;
                this.Leave = leave;
            }
        }
    }

    public class Point2Di
    {
        public int X { get { return x; } }
        public int Y { get { return y; } }

        // readonly for internal enforcement
        private readonly int x;
        private readonly int y;

        public Point2Di(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Point2Di operator +(Point2Di p1, Point2Di p2)
        {
            return new Point2Di(p1.x + p2.x, p1.y + p2.y);
        }

        public static Point2Di operator -(Point2Di pt)
        {
            return new Point2Di(-pt.x, -pt.y);
        }

        public static Point2Di operator -(Point2Di p1, Point2Di p2)
        {
            return p1 + -p2;
        }

        public static Point2Di operator *(Point2Di pt, int scalar)
        {
            return new Point2Di(pt.x * scalar, pt.y * scalar);
        }

        public static Point2Di operator *(int scalar, Point2Di pt)
        {
            return pt * scalar;
        }

        public static Point2Df operator *(Point2Di pt, double scalar)
        {
            return new Point2Df(pt.x * scalar, pt.y * scalar);
        }

        public static Point2Df operator *(double scalar, Point2Di pt)
        {
            return pt * scalar;
        }

        public static Point2Df operator /(Point2Di pt, double scalar)
        {
            return pt * (1 / scalar);
        }

        public static Point2Df operator /(double scalar, Point2Di pt)
        {
            return pt / scalar;
        }

        public static Point2Di operator /(Point2Di pt, int scalar)
        {
            return new Point2Di(pt.X / scalar, pt.Y / scalar);
        }

        public static Point2Di operator /(int scalar, Point2Di pt)
        {
            return pt / scalar;
        }

        public static explicit operator Point2Df(Point2Di pt2Df)
        {
            return new Point2Df((double)pt2Df.X, (double)pt2Df.Y);
        }

        public double Length()
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Point2Di other = obj as Point2Di;
            if ((object)other == null)
                return false;

            return (this.X == other.X) && (this.Y == other.Y);
        }

        // http://stackoverflow.com/a/9135980
        public override int GetHashCode()
        {
            int hash = 7;
            hash = 71 * hash + this.x;
            hash = 71 * hash + this.y;
            return hash;
        }
    }

    public class Point3Di
    {
        public int X { get { return xy.X; } }
        public int Y { get { return xy.Y; } }
        public int Z { get { return z; } }

        private readonly Point2Di xy;
        private readonly int z;

        public Point3Di(Point2Di xy, int z) : this(xy.X, xy.Y, z) { }

        public Point3Di(int x, int y, int z)
        {
            this.xy = new Point2Di(x, y);
            this.z = z;
        }

        public static Point3Di operator +(Point3Di p1, Point3Di p2)
        {
            return new Point3Di(p1.xy + p2.xy, p1.z + p2.z);
        }

        public static Point3Di operator -(Point3Di pt)
        {
            return new Point3Di(-pt.xy, -pt.z);
        }

        public static Point3Di operator -(Point3Di p1, Point3Di p2)
        {
            return p1 + -p2;
        }

        public static Point3Di operator *(Point3Di pt, int scalar)
        {
            return new Point3Di(pt.xy * scalar, pt.z * scalar);
        }

        public static Point3Di operator *(int scalar, Point3Di pt)
        {
            return pt * scalar;
        }

        public static Point3Df operator *(Point3Di pt, double scalar)
        {
            return new Point3Df(pt.xy * scalar, pt.z * scalar);
        }

        public static Point3Df operator *(double scalar, Point3Di pt)
        {
            return pt * scalar;
        }

        public static Point3Di operator /(Point3Di pt, int scalar)
        {
            return new Point3Di(pt.xy / scalar, pt.Z / scalar);
        }

        public static Point3Di operator /(int scalar, Point3Di pt)
        {
            return pt / scalar;
        }
        public static Point3Df operator /(Point3Di pt, double scalar)
        {
            return pt * (1 / scalar);
        }

        public static Point3Df operator /(double scalar, Point3Di pt)
        {
            return pt / scalar;
        }

        public static explicit operator Point2Df(Point3Di pt3Di)
        {
            return (Point2Df)(Point2Di)pt3Di;
        }

        public static explicit operator Point3Df(Point3Di pt3Df)
        {
            return new Point3Df((Point2Df)pt3Df.xy, (double)pt3Df.z);
        }

        public static explicit operator Point2Di(Point3Di pt3Di)
        {
            return pt3Di.xy;
        }

        public Point3Di Cross(Point3Di other)
        {
            int rx = this.Y * other.Z - this.Z * other.Y;
            int ry = this.Z * other.X - this.X * other.Z;
            int rz = this.X * other.Y - this.Y * other.X;

            return new Point3Di(rx, ry, rz);
        }

        public int Dot(Point3Di other)
        {
            int t1 = this.X * other.X;
            int t2 = this.Y * other.Y;
            int t3 = this.Z * other.Z;

            return t1 + t2 + t3;
        }

        public Point3Df Normalize()
        {
            double len = this.Length();
            if (len == 0)
                throw new InvalidOperationException("Cannot normalize a zero-length vector");

            return new Point3Df(xy / len, z / len);
        }

        public double Length()
        {
            return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Point3Di other = obj as Point3Di;
            if ((object)other == null)
                return false;

            return (this.X == other.X) && (this.Y == other.Y) && (this.Z == other.Z);
        }

        // http://stackoverflow.com/a/9135980
        public override int GetHashCode()
        {
            int hash = 7;
            hash = 71 * hash + this.X;
            hash = 71 * hash + this.Y;
            hash = 71 * hash + this.Z;
            return hash;
        }
    }

    public class Point2Df
    {
        public double X { get { return x; } }
        public double Y { get { return y; } }

        // readonly for internal enforcement
        private readonly double x;
        private readonly double y;

        public Point2Df(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Point2Df operator +(Point2Df p1, Point2Df p2)
        {
            return new Point2Df(p1.x + p2.x, p1.y + p2.y);
        }

        public static Point2Df operator -(Point2Df pt)
        {
            return new Point2Df(-pt.x, -pt.y);
        }

        public static Point2Df operator -(Point2Df p1, Point2Df p2)
        {
            return p1 + -p2;
        }

        public static Point2Df operator *(Point2Df pt, double scalar)
        {
            return new Point2Df(pt.x * scalar, pt.y * scalar);
        }

        public static Point2Df operator *(double scalar, Point2Df pt)
        {
            return pt * scalar;
        }

        public static Point2Df operator /(Point2Df pt, double scalar)
        {
            return pt * (1 / scalar);
        }

        public static Point2Df operator /(double scalar, Point2Df pt)
        {
            return pt / scalar;
        }

        public static explicit operator Point2Di(Point2Df pt2Df)
        {
            return new Point2Di((int)pt2Df.X, (int)pt2Df.Y);
        }

        public Point2Di Round()
        {
            return new Point2Di((int)Math.Round(x), (int)Math.Round(y));
        }

        public Point2Df Round(int digits)
        {
            return new Point2Df(Math.Round(x, digits), Math.Round(y, digits));
        }

        public Point2Df Normalize()
        {
            double len = this.Length();
            return new Point2Df(x / len, y / len);
        }

        public double Length()
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        public double Distance(Point2Df other)
        {
            return Math.Sqrt(Math.Pow(this.x - other.x, 2) + Math.Pow(this.y - other.y, 2));
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Point2Df other = obj as Point2Df;
            if ((object)other == null)
                return false;

            return (this.X == other.X) && (this.Y == other.Y);
        }

        // http://stackoverflow.com/a/9135980
        public override int GetHashCode()
        {
            int hash = 7;
            hash = 71 * hash + this.X.GetHashCode();
            hash = 71 * hash + this.Y.GetHashCode();
            return hash;
        }
    }

    public class Point3Df
    {
        public double X { get { return xy.X; } }
        public double Y { get { return xy.Y; } }
        public double Z { get { return z; } }

        private readonly Point2Df xy;
        private readonly double z;

        public Point3Df(Point2Df xy, double z) : this(xy.X, xy.Y, z) { }

        public Point3Df(double x, double y, double z)
        {
            this.xy = new Point2Df(x, y);
            this.z = z;
        }

        public static Point3Df operator +(Point3Df p1, Point3Df p2)
        {
            return new Point3Df(p1.xy + p2.xy, p1.z + p2.z);
        }

        public static Point3Df operator -(Point3Df pt)
        {
            return new Point3Df(-pt.xy, -pt.z);
        }

        public static Point3Df operator -(Point3Df p1, Point3Df p2)
        {
            return p1 + -p2;
        }

        public static Point3Df operator *(Point3Df pt, double scalar)
        {
            return new Point3Df(pt.xy * scalar, pt.z * scalar);
        }

        public static Point3Df operator *(double scalar, Point3Df pt)
        {
            return pt * scalar;
        }

        public static Point3Df operator /(Point3Df pt, double scalar)
        {
            return pt * (1 / scalar);
        }

        public static Point3Df operator /(double scalar, Point3Df pt)
        {
            return pt / scalar;
        }

        public static explicit operator Point2Di(Point3Df pt3Df)
        {
            return (Point2Di)(Point2Df)pt3Df;
        }

        public static explicit operator Point3Di(Point3Df pt3Df)
        {
            return new Point3Di((Point2Di)pt3Df.xy, (int)pt3Df.z);
        }

        public static explicit operator Point2Df(Point3Df pt3Df)
        {
            return pt3Df.xy;
        }

        public Point3Di Round()
        {
            return new Point3Di(xy.Round(), (int)Math.Round(z));
        }

        public Point3Df Round(int digits)
        {
            return new Point3Df(xy.Round(digits), Math.Round(z, digits));
        }

        public Point3Df Cross(Point3Df other)
        {
            double rx = this.Y * other.Z - this.Z * other.Y;
            double ry = this.Z * other.X - this.X * other.Z;
            double rz = this.X * other.Y - this.Y * other.X;

            return new Point3Df(rx, ry, rz);
        }

        public double Dot(Point3Df other)
        {
            double t1 = this.X * other.X;
            double t2 = this.Y * other.Y;
            double t3 = this.Z * other.Z;

            return t1 + t2 + t3;
        }

        public Point3Df Normalize()
        {
            double len = this.Length();
            return new Point3Df(xy / len, z / len);
        }

        public double Length()
        {
            // Could use xy.Length() intermediately, but this is more efficient
            return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        }

        public double Distance(Point3Df other)
        {
            return Math.Sqrt(Math.Pow(this.X - other.X, 2) + Math.Pow(this.Y - other.Y, 2) 
                + Math.Pow(this.Z - other.Z, 2));
        }

        public Point3Df Rotate(Point3Df axis, double angle)
        {
            Point3Df axisUnit = axis.Normalize();

            Point3Df t1 = this * Math.Cos(angle);
            Point3Df t2 = axisUnit.Cross(this) * Math.Sin(angle);
            Point3Df t3 = axisUnit * (axisUnit.Dot(this)) * (1 - Math.Cos(angle));

            return t1 + t2 + t3;
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Point3Df other = obj as Point3Df;
            if ((object)other == null)
                return false;

            return (this.X == other.X) && (this.Y == other.Y) && (this.Z == other.Z);
        }

        // http://stackoverflow.com/a/9135980
        public override int GetHashCode()
        {
            int hash = 7;
            hash = 71 * hash + this.X.GetHashCode();
            hash = 71 * hash + this.Y.GetHashCode();
            hash = 71 * hash + this.Z.GetHashCode();
            return hash;
        }
    }

    // This is a stub of a generic numeric multi-dimensional point. The above code, rife with
    // redundancy, is used instead for efficiency, simplicity, and portability.
    /*
    private class Point
    {
        private readonly dynamic[] coords;
        private readonly int dimensions;

        public static Point(params dynamic[] coords)
        {
            this.coords = coords;
            this.dimensions = coords.Length;
        }

        public T Add<T>(T otherPt) where T : Point
        {
            return (T)Activator.CreateInstance(typeof(T), new object[] {
                this.coords.Zip(otherPt.coords, (coord1, coord2) => coord1 + coord2)
            });
        }

        public T Neg<T>() where T : Point
        {
            return (T)Activator.CreateInstance(typeof(T), new object[] { 
                this.coords.Select(coord => -coord) 
            });
        }

        public T Sub<T>(T otherPt) where T : Point
        {
            return this.Add(otherPt.Neg<T>());
        }
    }
    */
}
