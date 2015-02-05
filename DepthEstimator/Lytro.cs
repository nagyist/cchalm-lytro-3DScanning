using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace Scanning
{
    // A static wrapper class for Lytro constants and utilities
    public class Lytro
    {
        public class Constants
        {
            public const int ImgHeight = 3420;
            public const int ImgWidth = 3420;
            public const int ArrayHeight = 9;
            public const int ArrayWidth = 9;
            public const int NumSubImgs = ArrayHeight * ArrayWidth;
            public const int SubImgHeight = ImgHeight / ArrayHeight;
            public const int SubImgWidth = ImgWidth / ArrayWidth;
            public const int ImgChannels = 3;
            // CHECKME assumes same spacing vertically and horizontally
            public const double SubImgSpacing = 1;

            public const int usedArrayHeight = ArrayHeight - 2;
            public const int usedArrayWidth = ArrayWidth - 2;
            public const int usedArrayStartY = 1;
            public const int usedArrayStartX = 1;
            public const int usedNumSubImgs = usedArrayHeight * usedArrayWidth;

            // CHECKME this is an estimate
            public const double FieldOfView = 60.0 / 180 * Math.PI;

            public readonly static Point3Df Orientation = new Point3Df(0, 0, 1);
        }

        public static void CheckConstants_DEBUG()
        {
            Debug.Assert(Constants.ImgHeight % Constants.ArrayHeight == 0,
               "Image height is not divisible by the height of the sub-image array.");
            Debug.Assert(Constants.ImgWidth % Constants.ArrayWidth == 0,
               "Image width is not divisible by the width of the sub-image array.");
        }

        public static SceneView[][] CreateEmptyViewArray()
        {
            // FIXME hacky
            return CreateViewArrayFromBitmap(null);
        }

        public static SceneView[][] CreateViewArrayFromBitmap(Bitmap lytroBmp)
        {
            Debug.Assert(lytroBmp == null ||
                (lytroBmp.Height == Constants.ImgHeight && lytroBmp.Width == Constants.ImgWidth),
                "Image dimensions don't match Lytro constants");

            SceneView[][] sceneArr =
                new SceneView[Constants.usedArrayHeight][];

            for (int y = Constants.usedArrayStartY; y < Constants.usedArrayHeight + 1; y++)
            {
                SceneView[] sceneArrRow = new SceneView[Constants.usedArrayWidth];

                double yOffset = Constants.SubImgSpacing *
                    ((double)(Constants.usedArrayHeight - 1) / 2 - (y - Constants.usedArrayStartY));

                for (int x = Constants.usedArrayStartX; x < Constants.usedArrayWidth + 1; x++)
                {
                    Rectangle subImgROI =
                        new Rectangle(x * Constants.SubImgWidth, y * Constants.SubImgHeight,
                            Constants.SubImgWidth, Constants.SubImgHeight);

                    Image subImage;
                    if (lytroBmp == null)
                    {
                        subImage = new Image(Constants.SubImgWidth, Constants.SubImgHeight, 
                            Constants.ImgChannels);
                    }
                    else
                    {
                        byte[] subImageData = Image.BitmapToBytes(lytroBmp, subImgROI);
                        subImage = new Image(subImageData, Constants.SubImgWidth, 
                            Constants.SubImgHeight, Constants.ImgChannels);
                    }

                    double xOffset = Constants.SubImgSpacing *
                        ((x - Constants.usedArrayStartX) - (double)(Constants.usedArrayWidth - 1) / 2);

                    Point3Df cop = new Point3Df(xOffset, yOffset, 0);

                    SceneView scene = new SceneView(subImage, cop, Constants.Orientation, 
                        Constants.FieldOfView, Constants.FieldOfView);

                    sceneArrRow[x - Constants.usedArrayStartX] = scene;
                }

                sceneArr[y - Constants.usedArrayStartY] = sceneArrRow;
            }

            return sceneArr;
        }
    }
}
