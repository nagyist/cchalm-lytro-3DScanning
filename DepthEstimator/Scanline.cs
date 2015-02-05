using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Scanning
{
    // A Scanline represents a line segment on a an image. It is constructed by projecting a 3D
    // line through a source point in a source image and projecting that line onto a target image.
    // Scanline instances allow clients to get a list of points on that line, and their associated
    // depths, with arbitrary granularity.
    class Scanline
    {
        // The start point of this scanline. There is no corresponding endpoint; it is determined
        // either by the bounds of the target image or the maxSteps variable.
        private readonly Point2Df start;

        // The unit step that defines this scanline. It is a unit step in the sense that either x
        // or y is 1; the length of the vector is not necessarily 1.
        private readonly Point2Df step;
        private readonly double maxSteps;

        // d * copDiff stored for efficiency
        private readonly Point2Df d_x_copDiff;

        // Stores the target scene for world/image transformations in GetSteps
        private readonly SceneView target;

        // Stores the min depth of the last scanline point to reduce redundant calculations in 
        // GetSteps
        private double prevMinDepth;

        public Scanline(SceneView source, SceneView target, Point2Df sourcePixel, 
                        double minDepth)
        {
            this.target = target;

            // Center of projection of the two scene views. The subtraction is in reverse order
            // because points translate in the direction opposite the COP shift
            Point2Df copDiff = (Point2Df)(source.CenterOfProjection - target.CenterOfProjection);  

            // Distance to image plane
            double d = source.DistanceToImagePlane;
            Debug.Assert(d == target.DistanceToImagePlane);

            // Store d * copDiff for efficiency
            this.d_x_copDiff = d * copDiff;

            // The start point represents the maximum depth, which is infinity. Pixels at infinity
            // are at the same point in every perspective.
            this.start = sourcePixel;
            Point2Df end = new Point2Df(sourcePixel.X + d_x_copDiff.X / minDepth,
                                        sourcePixel.Y - d_x_copDiff.Y / minDepth);

            // Get the entire step from start to end
            this.step = end - start;

            // Scale the step so that it steps by one unit at a time either horizontally
            // or vertically, whichever results in the fewest steps. This limits the
            // resolution of the step to pixels. We don't have to worry about step being (0, 0)
            // because end and start will never be colocated. Store the divisor so we can check
            // against it while we're stepping.
            this.maxSteps = Math.Max(Math.Abs(step.X), Math.Abs(step.Y));
            this.step /= maxSteps;

            prevMinDepth = DepthEstimator.Constants.InvalidDepth;
        }

        // Get all of the points on this Scaline, each in the form of a tuple where the first
        // element is the 2D point on the target image, and the second element is the depth the 
        // point represents for the source pixel in the source image. The points are returned with
        // a resolution determined by the passed scale. The passed scale will be the length of the
        // primary axis of the step between each scanline point. For example, if the passed scale
        // is 1.0, the step between each point could be (1.0, 0.5), (1.0, 1.0), (0.3, 1.0), etc 
        // depending on the start and end points used to construct this scanline.
        public IEnumerable<Section> GetSteps(double scale)
        {
            int stepCount = 0;
            // CHECKME performance: points are immutable; these create new objects every time 
            // they're changed
            Point2Df cursor = start;
            Point2Df scaledStep = step * scale;

            do
            {
                Section sect = new Section();

                sect.Point = cursor;
                sect.Depth = GetDepth(cursor);

                // CHECKME precompute step / 2?
                // Set min and max depth. maxDepth can be taken from the minDepth of the previous
                // iteration.
                sect.MinDepth = GetDepth(cursor - (scaledStep / 2));
                if (prevMinDepth == DepthEstimator.Constants.InvalidDepth)
                    sect.MaxDepth = GetDepth(cursor + (scaledStep / 2));
                else
                    sect.MaxDepth = prevMinDepth;

                // This section's minDepth will be the next section's maxDepth
                prevMinDepth = sect.MinDepth;

                sect.Freeze();
                yield return sect;

                stepCount++;
                cursor += scaledStep;
            } while (target.Image.Contains(cursor) && stepCount < maxSteps);

            prevMinDepth = DepthEstimator.Constants.InvalidDepth;
        }

        private double GetDepth(Point2Df targetPt)
        {
            if (d_x_copDiff.X == 0)
                return -d_x_copDiff.Y / (targetPt.Y - start.Y);
            else
                return d_x_copDiff.X / (targetPt.X - start.X);
        }

        // A Section is a portion of a scanline. It stores a point on an image and its associated 
        // depth. It also includes a minimum depth and a maximum depth to account for the fact that
        // pixels are discrete areas of an image with dimensions. All of the depths between 
        // MinDepth and MaxDepth are associated with Point. Sections are freezable. 
        public class Section : Freezable
        {
            public Point2Df Point
            {
                get { return point; }
                set { Modify(); point = value; }
            }

            public double Depth
            {
                get { return depth; }
                set { Modify(); depth = value; }
            }

            public double MinDepth
            {
                get { return minDepth; }
                set { Modify(); minDepth = value; }
            }

            public double MaxDepth
            {
                get { return maxDepth; }
                set { Modify(); maxDepth = value; }
            }

            private Point2Df point;
            private double depth;
            private double minDepth;
            private double maxDepth;
        }
    }
}

// Old implementation of Scanline that uses scanline bounding, which we may need to use later.
/*
private class Scanline
{
    public Point2Df Start { get { return start; } }
    public Point2Df End { get { return end; } }
    public double Length { get { return length; } }

    public bool IsEmpty { get { return isEmpty; } }

    private readonly Point2Df start;
    private readonly Point2Df end;

    private readonly double length;

    private readonly Point2Df step;
    private readonly int numSteps;

    private readonly double startDepth;

    private readonly bool isEmpty;

    public Scanline(SceneView source, SceneView target, Point2Df pixel)
    {
        // Project pixel out to the minimum depth and the maximum depth
        Point3Df nearPt = source.ProjectToWorld(pixel, MIN_DEPTH);
        Point3Df farPt = source.ProjectToWorld(pixel, MAX_DEPTH);

        // Transform the world coordinates into the target view's coordinate system
        Point3Df transformedNearPt = source.TransformToView(nearPt, target);
        Point3Df transformedFarPt = source.TransformToView(farPt, target);

        // Project the world coordinates in the target view's coordinate system onto to the
        // target view's image plane
        start = target.ProjectFromWorld(transformedNearPt);
        end = target.ProjectFromWorld(transformedFarPt);

        numSteps = NUM_DEPTH_STEPS;
        step = (end - start) / numSteps;

        isEmpty = !BoundScanline(
            new Rectangle(0, 0, target.Image.Width, target.Image.Height),
            ref start, ref end, step, ref numSteps, out startDepth);

        if (!isEmpty)
            length = start.Distance(end);
    }

    public IEnumerable<Tuple<Point2Df, double>> GetSteps()
    {
        if (isEmpty)
            yield break;

        double depth = startDepth;
        // CHECKME points are immutable, this creates a new object every time it's changed
        Point2Df cursor = start;

        yield return new Tuple<Point2Df, double>(cursor, depth);

        // We haven't taken any steps yet, we've just handled the start point
        for (int stepCounter = 0; stepCounter < numSteps; stepCounter++)
        {
            cursor += step;
            depth += DEPTH_STEP_SIZE;
            yield return new Tuple<Point2Df, double>(cursor, depth);
        }
    }

    // CHECKME we calculaste the step count in this method, can we somehow store that so we
    // don't have to recalculate it in DepthErrorMap?
    // Bounds the passed line segment within the passed rectangle. If no part of the line
    // segment is ever inside the rectangle, returns -1. Otherwise, MODIFIES THE PASSED 
    // LINE SEGMENT to be entirely contained within the rectangle and returns the number of 
    // steps between the new start and end points.
    private bool BoundScanline(Rectangle rect, ref Point2Df start, ref Point2Df end,
        Point2Df step, ref int numSteps, out double startDepth)
    {
        startDepth = -1;

        Geometry.RayIntersection intersectFromStart =
            Geometry.RayRectangleIntersect(start, step, rect);
        if (intersectFromStart == null)
            return false;

        Geometry.RayIntersection intersectFromEnd =
            Geometry.RayRectangleIntersect(end, -step, rect);
        if (intersectFromEnd == null)
            return false;

        int numStepsStartToRect = (int)Math.Ceiling(intersectFromStart.StepsToEntry);
        int numStepsEndToRect = (int)Math.Ceiling(intersectFromEnd.StepsToEntry);

        start += step * numStepsStartToRect;
        end -= step * numStepsEndToRect;

        int stepsFromStartToEnd = numSteps - numStepsStartToRect - numStepsEndToRect;

        numSteps = stepsFromStartToEnd;
        startDepth = numStepsStartToRect * DEPTH_STEP_SIZE;

        return true;
    }
}
*/
