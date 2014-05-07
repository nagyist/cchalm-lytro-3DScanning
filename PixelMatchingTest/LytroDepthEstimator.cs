using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;

namespace PixelMatchingTest
{
    class LytroDepthEstimator
    {
        public static class Constants
        {
            public const double InvalidDepth = 0;

            public const double MinDepth = 10;

            // Maximum region difference to consider successful
            public const int MaxError = 100;
            // Minimum number of depth guesses on a scanline to have confidence in minimum error value
            public const int MinGuesses = 10;

            public const double RoundingPoint = 1.0;

            public const int MaxAmbiguity = 2;

            public const bool UseKernel = false;
            public const int KernelSize = 3;
            // A constant used in calculating the gaussian distribution
            public const int GaussianSpread = KernelSize;

            public static readonly double[][] GaussianKernel;

            public const bool UseSubset = true;
            public static bool[][] SubsetFilter = new bool[][] { 
                new bool[] {false, false, false, false, false, false, false},
                new bool[] {false, false, false, false, false, true, false},
                new bool[] {false, false, false, false, false, false, false},
                new bool[] {false, false, false, false, false, false, false},
                new bool[] {false, false, false, false, false, false, false},
                new bool[] {false, false, false, false, false, false, false},
                new bool[] {false, false, false, false, false, false, false}};

            public const int SourceX = 1;
            public const int SourceY = 1;

            static Constants()
            {
                int halfSize = KernelSize / 2;
                GaussianKernel = new double[KernelSize][];

                for (int y = -halfSize; y <= halfSize; y++)
                {
                    double[] kernelRow = new double[KernelSize];

                    for (int x = -halfSize; x <= halfSize; x++)
                    {
                        kernelRow[x + halfSize] = Math.Pow(Math.E,
                                -(Math.Pow(x, 2) / (2 * GaussianSpread) + 
                                  Math.Pow(y, 2) / (2 * GaussianSpread)));
                    }

                    GaussianKernel[y + halfSize] = kernelRow;
                }
            }
        }

        public static double[][] EstimateDepth(SceneView[][] viewArr)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            SceneView source = viewArr[Constants.SourceY][Constants.SourceX];

            double[][] depthMap = new double[source.Image.Height][];

            int imgHeight = source.Image.Height;
            int imgWidth = source.Image.Width;

            for (int pixY = 0; pixY < imgHeight; pixY++ )
            //Parallel.For(0, imgHeight, pixY =>
            {
                double[] depthMapRow = new double[source.Image.Width];

                for (int pixX = 0; pixX < imgHeight; pixX++)
                //Parallel.For(0, imgWidth, pixX =>
                {
                    List<DepthErrorPair> depthErrors = GetDepthErrorsForPixel(source,
                        viewArr, new Point2Di(pixX, pixY));

                    DepthErrorPair bestEstimate = GetBestDepthEstimate(depthErrors);

                    depthMapRow[pixX] = bestEstimate.Depth;
                }//);//pixX

                depthMap[pixY] = depthMapRow;
            }//);//pixY

            sw.Stop();
            Debug.WriteLine("Elapsed: " + sw.Elapsed);

            return depthMap;
        }

        private static DepthErrorPair GetBestDepthEstimate(List<DepthErrorPair> depthErrors)
        {
            if (depthErrors.Count < Constants.MinGuesses)
                return new DepthErrorPair(Constants.InvalidDepth, double.PositiveInfinity);

            // Round
            /*Dictionary<double, List<double>> roundedDepthErrors = 
                new Dictionary<double, List<double>>();

            foreach (DepthErrorPair dePair in depthErrors)
            {
                double depth = dePair.Depth;

                if (depth != Constants.InvalidDepth)
                {
                    double roundedDepth = Math.Round(depth / Constants.RoundingPoint) * 
                        Constants.RoundingPoint;

                    if (!roundedDepthErrors.ContainsKey(roundedDepth))
                        roundedDepthErrors.Add(roundedDepth, new List<double>());

                    roundedDepthErrors[roundedDepth].Add(dePair.Error);
                }
            }*/

            // Sort
            depthErrors.Sort((dePair1, dePair2) => dePair1.Depth.CompareTo(dePair2.Depth));

            // Smooth
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < depthErrors.Count; i++)
                {
                    DepthErrorPair dePair = depthErrors[i];

                    double avgError = 0;

                    int avgAreaSize = Math.Min(depthErrors.Count - 1 - i, Math.Min(i, 2));

                    for (int j = -avgAreaSize; j <= avgAreaSize; j++)
                        avgError += depthErrors[i + j].Error;

                    avgError /= avgAreaSize * 2 + 1;

                    depthErrors[i] = new DepthErrorPair(dePair.Depth, avgError);
                }

            // Aggregate
            double bestDepth = Constants.InvalidDepth;
            double minError = Constants.MaxError + 1;

            foreach (var dePair in depthErrors)
            {
                if (dePair.Error < minError && !double.IsInfinity(dePair.Depth))
                {
                    bestDepth = dePair.Depth;
                    minError = dePair.Error;
                }
            }

            return new DepthErrorPair(bestDepth, minError);
        }

        private static List<DepthErrorPair> GetDepthErrorsForPixel(
            SceneView source, SceneView[][] targetArr, Point2Di pixel)
        {
            // CHECKME does reinitializing these for each pixel have a performance impact?
            int arrHeight = targetArr.Length;
            int arrWidth = targetArr[0].Length;

            List<DepthErrorPair> depthEstimates = new List<DepthErrorPair>();

            //Parallel.For(0, arrHeight, targetY =>
            for (int targetY = 0; targetY < arrHeight; targetY++)
            {
                //Parallel.For(0, arrWidth, targetX =>
                for (int targetX = 0; targetX < arrWidth; targetX++)
                {
                    SceneView target = targetArr[targetY][targetX];

                    if (target != source && 
                        (!Constants.UseSubset || Constants.SubsetFilter[targetY][targetX]))
                    {
                        List<DepthErrorPair> estimatesForImagePair =
                            GetDepthEstimatesForPixel(source, target, pixel);
                        depthEstimates.AddRange(estimatesForImagePair);
                    }
                }//);//arrX
            }//);//arrY

            return depthEstimates;
        }

        private static List<DepthErrorPair> GetDepthEstimatesForPixel(
            SceneView source, SceneView target, Point2Di pixel)
        {
            List<DepthErrorPair> depthErrors = new List<DepthErrorPair>();

            Scanline scanline = new Scanline(source, target, (Point2Df)pixel);

            if (Constants.UseKernel)
            {
                foreach (Tuple<Point2Df, double> targetPt in scanline.GetSteps())
                {
                    double depth = targetPt.Item2;

                    double error = 
                        Image.Compare(source.Image, pixel, target.Image, targetPt.Item1.Round(), 
                            Constants.GaussianKernel);

                    depthErrors.Add(new DepthErrorPair(depth, error));
                }
            }
            else
            {
                // Store the source pixel value instead of using Image.Compare. As of 3/29/14, this 
                // improves performance by about 45%
                Point3Di sourcePixelVal = source.Image.GetPixelValue(pixel);

                foreach (Tuple<Point2Df, double> targetPt in scanline.GetSteps())
                {
                    double depth = targetPt.Item2;

                    //Point3Df targetPixelVal = target.Image.GetSubPixelValue(targetPt.Item1);
                    Point3Di targetPixelVal = target.Image.GetPixelValue(targetPt.Item1.Round());

                    double error = Math.Abs(sourcePixelVal.X - targetPixelVal.X) +
                                   Math.Abs(sourcePixelVal.Y - targetPixelVal.Y) +
                                   Math.Abs(sourcePixelVal.Z - targetPixelVal.Z);

                    depthErrors.Add(new DepthErrorPair(depth, error));
                }
            }

            if (pixel.X == 205 && pixel.Y == 309)
                Debug.WriteLine("Hello");

            return depthErrors;
        }

        private class Scanline
        {
            private readonly Point2Df start;

            private readonly Point2Df step;
            private readonly double maxSteps;

            // d * copDiff stored for efficiency
            private readonly Point2Df d_x_copDiff;

            // Stores the target scene for world/image transformations in the GetSteps method
            private readonly SceneView target;

            public Scanline(SceneView source, SceneView target, Point2Df pixel)
            {
                this.target = target;

                // Center of projection of the two scene views. The subtraction is in reverse order
                // because points translate in the direction opposite the COP shift
                Point2Df copDiff = 
                    (Point2Df)(source.CenterOfProjection - target.CenterOfProjection);

                // Distance to image plane
                double d = source.DistanceToImagePlane;
                Debug.Assert(d == target.DistanceToImagePlane);

                this.d_x_copDiff = d * copDiff;

                // Start at the deepest point. For Lytro sub-views, pixels at infinity are in the 
                // same place in every image
                this.start = pixel;
                Point2Df end = new Point2Df(pixel.X + d_x_copDiff.X / Constants.MinDepth,
                                            pixel.Y - d_x_copDiff.Y / Constants.MinDepth);

                // Get the entire step from start to end
                this.step = end - start;

                // Scale the step so that it steps by one unit at a time either horizontally
                // or vertically, whichever results in the fewest steps. This limits the
                // resolution of the step to pixels. We don't have to worry about step being (0, 0)
                // because end and start will never be colocated. Store the divisor so we can check
                // against it while we're stepping.
                this.maxSteps = Math.Max(Math.Abs(step.X), Math.Abs(step.Y));
                this.step /= maxSteps;
            }

            public IEnumerable<Tuple<Point2Df, double>> GetSteps()
            {
                int stepCount = 0;
                // CHECKME points are immutable; these create new objects every time they're changed
                Point2Df cursor = start;
                
                do 
                {
                    double depth = d_x_copDiff.X != 0 ?
                         d_x_copDiff.X / (cursor.X - start.X) :
                        -d_x_copDiff.Y / (cursor.Y - start.Y);

                    yield return new Tuple<Point2Df, double>(cursor, depth);

                    stepCount++;
                    cursor += step;
                } while (target.Image.Contains(cursor) && stepCount < maxSteps) ;
            }
        }

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

        // An immutable pair with properties Depth and Error
        private class DepthErrorPair
        {
            public double Depth { get { return depth; } }
            public double Error { get { return error; } }

            private readonly double depth;
            private readonly double error;

            public DepthErrorPair(double depth, double error)
            {
                this.depth = depth;
                this.error = error;
            }

            public override string ToString()
            {
                return "(" + depth + ", " + error + ")";
            }
        }
    }
}
