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
using System.Text.RegularExpressions;

namespace Scanning
{
    class LaunchPad
    {
        // Get a depth map from 1080p post-processed images
        public static double[][] GetDepthMap_1080()
        {
            int imgSize = 1080;
            Point3Df orientation = new Point3Df(0, 0, 1);
            double fov = 60.0 / 180.0 * Math.PI;

            List<SceneView> views = new List<SceneView>();

            foreach (string filename in Directory.GetFiles("Images/Rect/"))
            {
                if (!filename.EndsWith(".bmp"))
                    continue;

                Bitmap bmp = new Bitmap(filename);
                Image img = new Image(bmp);

                Debug.Assert(img.Width == imgSize && img.Height == imgSize);

                string xy = String.Concat(filename.SkipWhile(ch => ch != 'X'));
                string[] x_y = String.Concat(xy.Take(xy.Length - ".bmp".Length)).
                                      Split(new string[] { ", " }, StringSplitOptions.None);

                double x = double.Parse(String.Concat(x_y[0].Skip(4)));
                double y = double.Parse(String.Concat(x_y[1].Skip(4)));

                Point3Df focalPt = new Point3Df(x * 10, y * 10, 0);

                views.Add(new SceneView(img, focalPt, orientation, fov, fov));
            }

            return DepthEstimator.CalculateDepthMap(views.First(), views.Skip(1).ToArray());
        }

        // Get a depth map from a single sub-aperture image grid
        public static double[][] GetDepthMap_Grid()
        {
            Lytro.CheckConstants_DEBUG();

            SceneView[][] views = 
                Lytro.CreateViewArrayFromBitmap(new Bitmap("Images/sample2.bmp"));

            SceneView[] flattenedViews = views.SelectMany(inner => inner).ToArray();

            double[][] depthBuffer = DepthEstimator.CalculateDepthMap(flattenedViews[0],
                flattenedViews.Skip(1).ToArray());
            /*
            // Take the log of every depth. This helps smooth out noise
            for (int y = 0; y < depthBuffer.Length; y++)
            {
                for (int x = 0; x < depthBuffer[0].Length; x++)
                {
                    double depth = depthBuffer[y][x];

                    if (depth != DepthEstimator.Constants.InvalidDepth &&
                        !double.IsInfinity(depth))
                        depthBuffer[y][x] = Math.Log(depth);
                }
            }
            */
            return depthBuffer;
        }

        // Get a depth map from a pair of binocular test images
        public static double[][] GetDepthMap_Binocular()
        {
            Bitmap leftBmp = new Bitmap("Images/im2.bmp");
            Bitmap rightBmp = new Bitmap("Images/im6.bmp");

            Image leftImg = new Image(leftBmp);
            Image rightImg = new Image(rightBmp);

            Debug.Assert(leftImg.Width == rightImg.Width && leftImg.Height == rightImg.Height);

            Point3Df leftFocalPt = new Point3Df(-100, 0, 0);
            Point3Df rightFocalPt = -leftFocalPt;
            Point3Df orientation = new Point3Df(0, 0, 1);
            double fov = 70.0 / 180.0 * Math.PI;

            SceneView leftView = new SceneView(leftImg, leftFocalPt, orientation, fov, fov);
            SceneView rightView = new SceneView(rightImg, rightFocalPt, orientation, fov, fov);

            double[][] depthBuffer = DepthEstimator.CalculateDepthMap(leftView, rightView);

            // Take the log of every depth. This helps smooth out noise
            for (int y = 0; y < depthBuffer.Length; y++)
            {
                for (int x = 0; x < depthBuffer[0].Length; x++)
                {
                    double depth = depthBuffer[y][x];

                    if (depth != DepthEstimator.Constants.InvalidDepth &&
                        !double.IsInfinity(depth))
                        depthBuffer[y][x] = Math.Log(depth);
                }
            }

            return depthBuffer;
        }
    }
}
