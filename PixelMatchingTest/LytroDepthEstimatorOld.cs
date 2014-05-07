using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PixelMatchingTest
{
    // A DepthEstimator extension that uses knowledge of the Lytro to perform preprocessing
    // of necessary point/vector transformations.
    class LytroDepthEstimatorOld : DepthEstimator
    {        
        // source view x index => target view x index => pixel x coord => endpoint pair
        private static Tuple<int, int>[][][] xLookup;
        private static Tuple<int, int>[][][] yLookup;

        // CHECKME this is a pretty hacky solution to finding array indices within GetSearchEndpoints
        private static Dictionary<SceneView, Point2Di> viewIndices;

        static LytroDepthEstimatorOld()
        {
            viewIndices = new Dictionary<SceneView, Point2Di>();

            SceneView[][] viewArr = Lytro.CreateEmptyViewArray();

            xLookup = new Tuple<int, int>[viewArr.Length][][];
            yLookup = new Tuple<int, int>[viewArr.Length][][];

            Debug.Assert(viewArr.Length == viewArr[0].Length &&
                         viewArr[0][0].Image.Width == viewArr[0][0].Image.Height,
                         "This algorithm relies on the Lytro image array and sub-images being square");

            int imgDim = viewArr[0][0].Image.Width;

            for (int sourceXY = 0; sourceXY < viewArr.Length; sourceXY++)
            {
                Tuple<int, int>[][] xSourceRow = new Tuple<int, int>[viewArr.Length][];
                Tuple<int, int>[][] ySourceRow = new Tuple<int, int>[viewArr.Length][];

                for (int targetXY = 0; targetXY < viewArr.Length; targetXY++)
                {
                    Tuple<int, int>[] xTargetRow = new Tuple<int, int>[imgDim];
                    Tuple<int, int>[] yTargetRow = new Tuple<int, int>[imgDim];

                    for (int pixelXY = 0; pixelXY < viewArr[0][0].Image.Width; pixelXY++)
                    {
                        Point2Df sourcePt = new Point2Df(pixelXY, pixelXY);

                        SceneView source = viewArr[sourceXY][sourceXY];
                        SceneView target = viewArr[targetXY][targetXY];

                        Point2Di endpoint1 =
                            source.FindInOtherView((Point2Df)sourcePt, target, MIN_DEPTH).Round();
                        Point2Di endpoint2 =
                            source.FindInOtherView((Point2Df)sourcePt, target, MAX_DEPTH).Round();

                        Tuple<int, int> xEndpointCoords =
                            new Tuple<int, int>(endpoint1.X, endpoint2.X);
                        Tuple<int, int> yEndpointCoords =
                            new Tuple<int, int>(endpoint1.Y, endpoint2.Y);

                        xTargetRow[pixelXY] = xEndpointCoords;
                        yTargetRow[pixelXY] = yEndpointCoords;
                    }

                    xSourceRow[targetXY] = xTargetRow;
                    ySourceRow[targetXY] = yTargetRow;
                }

                xLookup[sourceXY] = xSourceRow;
                yLookup[sourceXY] = ySourceRow;
            }

            Debug.WriteLine("Done preprocessing");
        }

        new public static double[][] EstimateDepth(SceneView source, SceneView target)
        {
            // Assume both images have the same dimensions
            int imgHeight = source.Image.Height;
            int imgWidth = source.Image.Width;

            double[][] depthBuffer = new double[imgHeight][];

            Parallel.For(0, imgHeight, y =>//for(int y = 0; y < imgHeight; y++)
            {
                double[] depthBufferRow = new double[imgWidth];

                /*Parallel.For(0, imgWidth, x =>*/
                for (int x = 0; x < imgWidth; x++)
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

                    // FIXME ugly tuples
                    Tuple<Point2Di, Point2Di> endpoints = GetSearchEndpoints(source, target,
                        sourcePt);

                    // The pixel that best matches along the line from endpoint1 to endpoint2
                    Point2Di matchingPixel = ChooseBestPixelMatch(source.Image, target.Image,
                        sourcePt, endpoints.Item1, endpoints.Item2);

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
                            Math.Sqrt(Math.Pow((endpoints.Item2.X - endpoints.Item1.X), 2) +
                                        Math.Pow((endpoints.Item2.Y - endpoints.Item1.Y), 2));

                        // The distance between the first endpoint and the best-matching pixel
                        double partialLength =
                            Math.Sqrt(Math.Pow((matchingPixel.X - endpoints.Item1.X), 2) +
                                        Math.Pow((matchingPixel.Y - endpoints.Item1.Y), 2));

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

        new public static double[][] EstimateDepth(SceneView[][] sceneArr)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int arrHeight = sceneArr.Length;
            int arrWidth = sceneArr[0].Length;

            int sourceY = 0;
            int sourceX = 0;
            SceneView source = sceneArr[sourceY][sourceX];
            viewIndices.Add(source, new Point2Di(sourceX, sourceY));

            int imgHeight = source.Image.Height;
            int imgWidth = source.Image.Width;

            /*for (int y = 0; y < sceneArr.Length; y++)
            {
                for(int x = 0; x < sceneArr[y].Length; x++)
                {
                    Debug.Assert(scenes[y][x].Image.Width == imgWidth);
                    Debug.Assert(scenes[y][x].Image.Height == imgHeight);
                }
            }*/

            // An array of 2D arrays of depth buffers, one for each pair of images we're
            // considering
            int numVotes = sceneArr.Length * sceneArr[0].Length - 1;
            double[][][] depthBufferVotes = new double[numVotes][][];

            object viewIndicesLock = new object();

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

                        lock (viewIndicesLock)
                            viewIndices.Add(target, new Point2Di(targetX, targetY));

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

        private static Tuple<Point2Di, Point2Di> GetSearchEndpoints(SceneView source, 
            SceneView target, Point2Di pixel)
        {
            // FIXME this is sooo hacky... should be able to pass these points in somehow
            Point2Di sourcePos = viewIndices[source];
            Point2Di targetPos = viewIndices[target];

            Tuple<int, int> xCoords = xLookup[sourcePos.X][targetPos.X][pixel.X];
            Tuple<int, int> yCoords = yLookup[sourcePos.Y][targetPos.Y][pixel.Y];

            Point2Di nearPt = new Point2Di(xCoords.Item1, yCoords.Item1);
            Point2Di farPt = new Point2Di(xCoords.Item2, yCoords.Item2);

            return new Tuple<Point2Di, Point2Di>(nearPt, farPt);
        }
    }
}
