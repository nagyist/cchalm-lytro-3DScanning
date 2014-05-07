using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace PixelMatchingTest
{
    class DepthEstimator
    {
        public const double INVALID_DEPTH = 0;

        // LINEAR FACTOR: MAX_DEPTH - MIN_DEPTH
        protected const double MAX_DEPTH = 1000000;
        protected const double MIN_DEPTH = 10;

        // QUADRATIC FACTOR: KERNEL_SIZE
        private const int KERNEL_SIZE = 1;
        // A constant used in calculating the gaussian distribution
        private const int GAUSSIAN_SPREAD = KERNEL_SIZE;

        // Maximum region difference to consider successful
        private const int MAX_DIFF = 50;
        // Maximum number of equivalent matched pixels to consider successful
        private const int MAX_EQUIVALENT_MATCHES = 2;
        private const int MIN_SCAN_POINTS = 20;

        private static readonly double[][] GAUSSIAN_KERNEL;

        static DepthEstimator()
        {
            int halfSize = KERNEL_SIZE / 2;
            GAUSSIAN_KERNEL = new double[KERNEL_SIZE][];

            for (int y = -halfSize; y <= halfSize; y++)
            {
                double[] kernelRow = new double[KERNEL_SIZE];

                for (int x = -halfSize; x <= halfSize; x++)
                {
                    kernelRow[x + halfSize] = Math.Pow(Math.E,
                            -(Math.Pow(x, 2) / (2 * GAUSSIAN_SPREAD) + 
                              Math.Pow(y, 2) / (2 * GAUSSIAN_SPREAD)));
                }

                GAUSSIAN_KERNEL[y + halfSize] = kernelRow;
            }
        }

        public static double[][] EstimateDepth(SceneView[][] sceneArr)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int arrHeight = sceneArr.Length;
            int arrWidth = sceneArr[0].Length;

            int sourceY = 0;
            int sourceX = 0;
            SceneView source = sceneArr[sourceY][sourceX];

            int imgHeight = source.Image.Height;
            int imgWidth = source.Image.Width;

            // An array of 2D arrays of depth buffers, one for each pair of images we're
            // considering
            int numVotes = sceneArr.Length * sceneArr[0].Length - 1;
            double[][][] depthBufferVotes = new double[numVotes][][];

            int voteIndex = 0;
            object voteIndexLock = new object();

            Parallel.For(0, arrHeight, targetY =>
            {
                Parallel.For(0, arrWidth, targetX =>
                {
                    if (targetY == sourceY && targetX == sourceX)
                    {
                        /*noop: skip the source image*/
                    }
                    else
                    {
                        SceneView target = sceneArr[targetY][targetX];

                        double[][] d = EstimateDepth(source, target);

                        depthBufferVotes[voteIndex] = d;

                        lock (voteIndexLock)
                            voteIndex++;
                    }
                });//x
            });//y

            #region average
            /*double[][] finalDepthBuffer = depthBufferVotes[0];
            int[][] numValidVotes = new int[finalDepthBuffer.Length][];

            for (int i = 0; i < finalDepthBuffer.Length; i++)
            {
                numValidVotes[i] = new int[finalDepthBuffer[0].Length];
            }

            for (int vote = 0; vote < numVotes; vote++)
            {
                for (int y = 0; y < finalDepthBuffer.Length; y++)
                {
                    for (int x = 0; x < finalDepthBuffer[0].Length; x++)
                    {
                        double depth = depthBufferVotes[vote][y][x];
                        finalDepthBuffer[y][x] += depth;

                        if (depth >= MIN_DEPTH)
                            numValidVotes[y][x]++;
                    }
                }
            }

            for (int y = 0; y < finalDepthBuffer.Length; y++)
            {
                for (int x = 0; x < finalDepthBuffer[0].Length; x++)
                {
                    if (numValidVotes[y][x] == numVotes)
                        finalDepthBuffer[y][x] /= numValidVotes[y][x];
                    else
                        finalDepthBuffer[y][x] = 0;
                }
            }*/
            #endregion

            #region median
            double[][] finalDepthBuffer = new double[imgHeight][];

            Parallel.For(0, imgHeight, y =>
            {
                double[] finalDepthBufferRow = new double[imgWidth];

                Parallel.For(0, imgWidth, x =>
                {
                    List<double> votes = new List<double>(numVotes);
                    for (int vote = 0; vote < numVotes; vote++)
                    {
                        double depth = depthBufferVotes[vote][y][x];
                        if (depth != INVALID_DEPTH)
                            votes.Add(depthBufferVotes[vote][y][x]);
                    }

                    votes.Sort();

                    int medianIndex = (votes.Count - 1) / 2;

                    if (votes.Count == 0)
                        finalDepthBufferRow[x] = INVALID_DEPTH;
                    else if ((votes.Count - 1) % 2 == 0)
                        finalDepthBufferRow[x] = votes[medianIndex];
                    else
                        finalDepthBufferRow[x] = (votes[medianIndex] + votes[medianIndex + 1]) / 2;
                });//x

                finalDepthBuffer[y] = finalDepthBufferRow;
            });//y
            #endregion

            sw.Stop();
            Debug.WriteLine("Elapsed: " + sw.Elapsed);

            return finalDepthBuffer;
        }

        public static double[][] EstimateDepth(SceneView source, SceneView target)
        {
            // Assume both images have the same dimensions
            int imgHeight = source.Image.Height;
            int imgWidth = source.Image.Width;

            double[][] depthBuffer = new double[imgHeight][];

            Parallel.For(0, imgHeight, y =>//for(int y = 0; y < imgHeight; y++)
            {
                double[] depthBufferRow = new double[imgWidth];

                /*Parallel.For(0, imgWidth, x =>*/for (int x = 0; x < imgWidth; x++)
                {
                    #region diff test
                    /*
                        Point2Di pt = new Point2Di(x, y);

                        SubBuffer sub1 = new SubBuffer(scenes[0].Image, pt, 1);
                        SubBuffer sub2 = new SubBuffer(scenes[1].Image, pt, 1);

                        int diffSum = 0;
                        int startIndex = sub1.GetIndexFromOffsets(sub1.Left, sub1.Top);

                        unsafe
                        {
                            fixed (byte* ptr1 = &scenes[0].Image.PixelData[startIndex],
                                         ptr2 = &scenes[1].Image.PixelData[startIndex])
                            {
                                byte* cursor1 = ptr1;
                                byte* cursor2 = ptr2;

                                for (int subX = sub1.Left; subX <= sub1.Right; subX++)
                                {
                                    for (int subY = sub1.Top; subY <= sub1.Bottom; subY++)
                                    {
                                        for (int channel = 0; channel < 3; channel++)
                                        {
                                            byte pix1 = *cursor1;
                                            byte pix2 = *cursor2;

                                            if (pix1 > pix2)
                                                diffSum += pix1 - pix2;
                                            else
                                                diffSum += pix2 - pix1;

                                            cursor1++;
                                            cursor2++;
                                        }
                                    }

                                    cursor1 += sub1.RowJump;
                                    cursor2 += sub2.RowJump;
                                }
                            }
                        }

                        depthBufferRow[x] = diffSum;
                        */
                    #endregion

                    Point2Di sourcePt = new Point2Di(x, y);

                    Point2Di endpoint1 =
                        source.FindInOtherView((Point2Df)sourcePt, target, MIN_DEPTH).Round();
                    Point2Di endpoint2 =
                        source.FindInOtherView((Point2Df)sourcePt, target, MAX_DEPTH).Round();

                    // The pixel that best matches along the line from endpoint1 to endpoint2
                    Point2Di matchingPixel = ChooseBestPixelMatch(source.Image, target.Image,
                        sourcePt, endpoint1, endpoint2);

                    double depth;

                    if (matchingPixel == null)
                    {
                        // A depth of zero indicates failure
                        depth = INVALID_DEPTH;
                    }
                    else
                    {
                        // The distance between the two endpoints
                        double segmentLength =
                            Math.Sqrt(Math.Pow((endpoint2.X - endpoint1.X), 2) +
                                        Math.Pow((endpoint2.Y - endpoint1.Y), 2));

                        // The distance between the first endpoint and the best-matching pixel
                        double partialLength =
                            Math.Sqrt(Math.Pow((matchingPixel.X - endpoint1.X), 2) +
                                        Math.Pow((matchingPixel.Y - endpoint1.Y), 2));

                        double distanceRatio = partialLength / segmentLength;

                        depth = MIN_DEPTH + distanceRatio * (MAX_DEPTH - MIN_DEPTH);
                    }

                    depthBufferRow[x] = depth;
                }//); // x

                // We do NOT need to lock access to this shared structure because each buffer
                // location is accessed by exactly one thread
                depthBuffer[y] = depthBufferRow;
            }); // y

            return depthBuffer;
        }

        protected static Point2Di ChooseBestPixelMatch(Image sourceImage, Image targetImage,
            Point2Di sourcePt, Point2Di targetEndpoint1, Point2Di targetEndpoint2)
        {
            Dictionary<double, List<Point2Di>> diffs = new Dictionary<double, List<Point2Di>>();

            List<Point2Di> pts = PixelsOnLineBounded(targetEndpoint1, targetEndpoint2, 0, 0,
                targetImage.Height - 1, targetImage.Width - 1);

            if (pts.Count < MIN_SCAN_POINTS)
                return null;

            foreach (Point2Di targetPt in pts)
            {
                double diff = Image.Compare(sourceImage, sourcePt, targetImage, targetPt, GAUSSIAN_KERNEL);
                if (!diffs.ContainsKey(diff))
                    diffs.Add(diff, new List<Point2Di>());
                diffs[diff].Add(targetPt);
            }

            // picks the point that the minimum key maps to
            KeyValuePair<double, List<Point2Di>> res = diffs.Aggregate((entry1, entry2) =>
                entry1.Key > entry2.Key ? entry2 : entry1);

            if (res.Key > MAX_DIFF || res.Value.Count > MAX_EQUIVALENT_MATCHES)
                return null;
            else
                return res.Value.First();
        }

        // TODO restrict line algorithm to within the bounds of the image
        private static List<Point2Di> PixelsOnLineBounded(Point2Di start, Point2Di end,
            int top, int left, int bot, int right)
        {
            List<Point2Di> res = new List<Point2Di>();

            // Encapsulates actions we have to perform when we process each point found by the
            // line algorithm
            Action<int, int> AddPoint = new Action<int, int>((x, y) =>
            {
                if (x >= left && x <= right && y >= top && y <= bot)
                    res.Add(new Point2Di(x, y));
            });

            #region Bresenham's
            // Bresenham's line algorithm
            // http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm

            // Reassign for mutability (and brevity)
            int x0 = start.X;
            int x1 = end.X;
            int y0 = start.Y;
            int y1 = end.Y;

            // diff x, diff y
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            // step x, step y
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            // TODO do away with Bresenham's algorithm

            /*=================================Algorithm Meat=================================*/
            // TODO stop iterating once we leave the image, but not before we get into it     //
            //
            while (true/*x0 >= 0 && x0 < sourceImg.Width &&                                   //
                   y0 >= 0 && y0 < sourceImg.Height*/)                                        //
            {                                                                                 //
                AddPoint(x0, y0);                                                           //
                //
                if (x0 == x1 && y0 == y1)                                                     //
                    break;                                                                    //
                //
                int e2 = 2 * err;                                                             //
                //
                if (e2 > -dy)                                                                 //
                {                                                                             //
                    err -= dy;                                                                //
                    x0 += sx;                                                                 //
                }                                                                             //
                //
                if (x0 == x1 && y0 == y1)                                                     //
                {                                                                             //
                    AddPoint(x0, y0);                                            //
                    break;                                                                    //
                }                                                                             //
                //
                if (e2 < dx)                                                                  //
                {                                                                             //
                    err += dx;                                                                //
                    y0 += sy;                                                                 //
                }                                                                             //
            }                                                                                 //
            /*================================================================================*/
            #endregion

            return res;
        }
    }
}
