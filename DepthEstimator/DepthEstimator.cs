using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace Scanning
{
    class DepthEstimator
    {
        // Public constants
        public static class Constants
        {
            public const double InvalidDepth = double.NaN;
        }

        // Private constants
        private static class constants
        {
            public const double MinDepth = 300;

            // QUADRATIC FACTOR: KERNEL_SIZE
            public const int KernelSize = 1;
            // A constant used in calculating the gaussian distribution
            public const double GaussianSpread = KernelSize;

            // Maximum region difference to consider successful
            public const int MaxDiff = 50;
            // Maximum number of equivalent matched pixels to consider successful
            public const int MaxEquivMatches = 2;
            // Minimum number of scan points to consider successful
            public const int MinScanPoints = 2;

            // The step size to use when iterating through a scanline. 1.0 corresponds to one row or
            // column per step.
            public const double ScanStepSize = 1.0;
        }

        private static readonly double[][] GAUSSIAN_KERNEL;

        static DepthEstimator()
        {
            int halfSize = constants.KernelSize/ 2;
            GAUSSIAN_KERNEL = new double[constants.KernelSize][];

            for (int y = -halfSize; y <= halfSize; y++)
            {
                double[] kernelRow = new double[constants.KernelSize];

                for (int x = -halfSize; x <= halfSize; x++)
                {
                    kernelRow[x + halfSize] = Math.Pow(Math.E,
                            -(Math.Pow(x, 2) / (2 * constants.GaussianSpread) +
                              Math.Pow(y, 2) / (2 * constants.GaussianSpread)));
                }

                GAUSSIAN_KERNEL[y + halfSize] = kernelRow;
            }
        }

        public static
        double[][] CalculateDepthMap(SceneView source, SceneView[] targets)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int imgHeight = source.Image.Height;
            int imgWidth = source.Image.Width;

            // A depth map for each target
            double[][][] depthBufferVotes = new double[targets.Length][][];

            Parallel.For(0, targets.Length, targetIndex =>
            {
                SceneView target = targets[targetIndex];

                double[][] d = CalculateDepthMap(source, target);

                depthBufferVotes[targetIndex] = d;
            });//targets

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
                    List<double> votes = new List<double>(targets.Length);
                    for (int vote = 0; vote < targets.Length; vote++)
                    {
                        double depth = depthBufferVotes[vote][y][x];
                        if (depth != Constants.InvalidDepth)
                            votes.Add(depthBufferVotes[vote][y][x]);
                    }

                    votes.Sort();

                    int medianIndex = (votes.Count - 1) / 2;

                    if (votes.Count == 0)
                        finalDepthBufferRow[x] = Constants.InvalidDepth;
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

        public static
        double[][] CalculateDepthMap(SceneView source, SceneView target)
        {
            // Assume both images have the same dimensions
            int imgHeight = source.Image.Height;
            int imgWidth = source.Image.Width;

            double[][] depthBuffer = new double[imgHeight][];

            Parallel.For(0, imgHeight, y =>
            //for(int y = 0; y < imgHeight; y++)
            {
                double[] depthBufferRow = new double[imgWidth];

                //Parallel.For(0, imgWidth, x =>
                for (int x = 0; x < imgWidth; x++)
                {
                    depthBufferRow[x] = EstimatePixelDepth(source, target, new Point2Di(x, y));
                }//); // x

                // We do NOT need to lock access to this shared structure because each buffer
                // location is accessed by exactly one thread
                depthBuffer[y] = depthBufferRow;
            }); // y

            return depthBuffer;
        }

        private static
        double EstimatePixelDepth(SceneView source, SceneView target, Point2Di sourcePt)
        {
            Scanline scanline =
                new Scanline(source, target, (Point2Df)sourcePt, constants.MinDepth);

            Dictionary<Scanline.Section, double> errors = 
                new Dictionary<Scanline.Section, double>();
            foreach (Scanline.Section sect in scanline.GetSteps(constants.ScanStepSize))
            {
                double error = Image.Compare(source.Image, sourcePt, 
                                             target.Image, sect.Point.Round());
                errors.Add(sect, error);
            }

            // Assign to null to make the compiler happy
            List<Scanline.Section> bestSections = null;
            double lowest_error = double.PositiveInfinity;

            foreach (var kvPair in errors)
            {
                Scanline.Section section = kvPair.Key;
                double error = kvPair.Value;

                if (error < lowest_error)
                {
                    lowest_error = kvPair.Value;
                    bestSections = new List<Scanline.Section> { section };
                }
                else if (error == lowest_error)
                {
                    bestSections.Add(section);
                }
            }

            if (lowest_error > constants.MaxDiff || 
                bestSections.Count > constants.MaxEquivMatches || 
                errors.Count < constants.MinScanPoints)
                return Constants.InvalidDepth;
            else
                return bestSections.First().Depth;
        }
    }
}
