/*
 * This file is a scratchpad for development of a multi-image pixel-matching and depth
 * calculation algorithm.
 * 
 * Inspiration for parts of this code came from:
 * http://www.slideshare.net/GuillaumeGales/pixel-matching-from-stereo-images-callan-seminar
 *  
 * Chris Chalmers, Feb2014
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PixelMatchingTest
{
    class LaunchPad
    {
        double[][] depthBuffer;

        public LaunchPad()
        {
            //string leftImgFile = "Images/smallDotLeft.bmp";
            //string rightImgFile = "Images/smallDotRight.bmp";
            /*string leftImgFile = "Images/im2.bmp";
            string rightImgFile = "Images/im6.bmp";

            Bitmap leftBmp = new Bitmap(leftImgFile);
            Bitmap rightBmp = new Bitmap(rightImgFile);

            if (leftBmp.Width != rightBmp.Width || leftBmp.Height != rightBmp.Height ||
                leftBmp.PixelFormat != rightBmp.PixelFormat)
                throw new Exception("Images must have the same dimensions and pixel formats");

            Image leftImg = new Image(leftBmp);
            Image rightImg = new Image(rightBmp);

            // left and right images have the same dimensions, declare these variables to avoid
            // confusion later on
            int imgWidth = leftBmp.Width;
            int imgHeight = leftBmp.Height;

            Point3Df focalPtLeft = new Point3Df(-100, 0, 0);
            Point3Df focalPtRight = new Point3Df(100, 0, 0);
            Point3Df orientation = new Point3Df(0, 0, 1);

            double verticalFOV = 60.0 / 180.0 * Math.PI;
            double horizontalFOV = 60.0 / 180.0 * Math.PI;

            SceneView camera1 = new SceneView(leftImg, focalPtLeft, orientation,
                verticalFOV, horizontalFOV);
            SceneView camera2 = new SceneView(rightImg, focalPtRight, orientation,
                verticalFOV, horizontalFOV);

            depthBuffer = DepthEstimator.EstimateDepth(camera1, camera2);

            SceneView[][] views = new SceneView[][] { new SceneView[] { camera1, camera2 } };*/

            Lytro.CheckConstants_DEBUG();

            SceneView[][] views = 
                Lytro.CreateViewArrayFromBitmap(new Bitmap("Images/sample2.bmp"));

            depthBuffer = LytroDepthEstimator.EstimateDepth(views);
            //depthBuffer = DepthEstimator.EstimateDepth(new SceneView[][] { new SceneView[] { views[1][1], views[1][5] } });
            
            // Take the log of every depth. This helps smooth out noise
            /*for (int y = 0; y < depthBuffer.Length; y++)
            {
                for (int x = 0; x < depthBuffer[0].Length; x++)
                {
                    double depth = depthBuffer[y][x];

                    if (depth != LytroDepthEstimator.Constants.InvalidDepth && 
                        !double.IsInfinity(depth))
                        depthBuffer[y][x] = Math.Log(depth);
                }
            }*/

            List<double> depths = new List<double>();

            for (int y = 0; y < depthBuffer.Length; y++)
            {
                for (int x = 0; x < depthBuffer[0].Length; x++)
                {
                    double depth = depthBuffer[y][x];

                    if ((depth != LytroDepthEstimator.Constants.InvalidDepth) && 
                        !double.IsInfinity(depth) && !depths.Contains(depth))
                        depths.Add(depth);
                }
            }

            depths.Sort();

            Dictionary<double, int> depthIndices = new Dictionary<double, int>();

            for (int i = 0; i < depths.Count; i++)
                depthIndices.Add(depths[i], i + 1);

            for (int y = 0; y < depthBuffer.Length; y++)
            {
                for (int x = 0; x < depthBuffer[0].Length; x++)
                {
                    double depth = depthBuffer[y][x];

                    if ((depth != LytroDepthEstimator.Constants.InvalidDepth) && 
                        !double.IsInfinity(depth))
                        depthBuffer[y][x] = depthIndices[depth];
                }
            }
        }

        public double[][] GetDepths()
        {
            return depthBuffer;
        }

        
    }
}
