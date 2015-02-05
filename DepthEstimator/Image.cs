using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Scanning
{
    // A simple representation of an image. Stores image data as an array of bytes as well as the
    // image's width, height, and number of bytes per pixel. Images are immutable by policy but
    // mechanically mutable through the PixelData property - pixel values must not be modified
    // through this property.
    public class Image
    {
        // Image bytes. Byte 0 is at the top-left. Values must not be modified
        public byte[] PixelData { get { return pixelData; } }

        // The width of the image
        public int Width { get { return region.Width; } }
        // the height of the image
        public int Height { get { return region.Height; } }

        public Rectangle Region { get { return region; } }

        // The number of bytes per image pixel (24bpp RGB = 3)
        public int BytesPerPixel { get { return bytesPerPixel; } }

        // Private backing fields for the above
        private readonly byte[] pixelData;
        private readonly Rectangle region;
        private readonly int bytesPerPixel;

        public Image(int width, int height, int channels)
            : this(new byte[width * height * channels], width, height, channels)
        { }

        // Constructs an Image with the given data and properties
        public Image(byte[] pixelData, int width, int height, int channels)
        {
            if (pixelData.Length != width * height * channels)
                throw new ArgumentException("Image data length does not match other parameters");

            this.pixelData = pixelData;
            this.region = new Rectangle(0, 0, width, height);
            this.bytesPerPixel = channels;
        }
        
        // Constructs an Image from a Bitmap. Row padding is removed
        public Image(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
                throw new NotSupportedException("Only 24bpp RGB bitmaps are supported");

            this.pixelData = BitmapToBytes(bmp);
            this.region = new Rectangle(0, 0, bmp.Width, bmp.Height);
            this.bytesPerPixel = 3; //System.Drawing.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
        }

        // FIXME use something other than Point3Di for pixel values?
        public Point3Di GetPixelValue(Point2Di pixel)
        {
            byte[] rgb = new byte[3];

            int index = (pixel.Y * Width + pixel.X) * 3;
            for (int ch = 0; ch < BytesPerPixel; ch++)
            {
                rgb[ch] = pixelData[index];
                index++;
            }

            return new Point3Di((byte)rgb[0], (byte)rgb[1], (byte)rgb[2]);
        }

        public Point3Di GetPixelValue(int x, int y)
        {
            byte[] rgb = new byte[3];

            int index = (y * Width + x) * 3;
            for (int ch = 0; ch < BytesPerPixel; ch++)
            {
                rgb[ch] = pixelData[index];
                index++;
            }

            return new Point3Di((byte)rgb[0], (byte)rgb[1], (byte)rgb[2]);
        }

        public Point3Df GetSubPixelValue(Point2Df pt)
        {
            int loX = (int)Math.Floor(pt.X);
            int loY = (int)Math.Floor(pt.Y);
            int hiX = loX + 1;
            int hiY = loY + 1;

            double hiXWeight = pt.X - loX;
            double hiYWeight = pt.Y - loY;
            double loXWeight = 1 - hiXWeight;
            double loYWeight = 1 - hiYWeight;

            Point3Df weightedPixelVal;

            // loXWeight and loYWeight will always be non-zero because of the relationship between
            // pt.X/Y and loX/Y. We check to make sure hiXWeight and hiYWeight are not zero to make
            // sure we don't wander out of the bounds of the image doing calculations

            weightedPixelVal = GetPixelValue(loX, loY) * loXWeight * loYWeight;

            if (hiXWeight != 0) 
                weightedPixelVal += GetPixelValue(hiX, loY) * hiXWeight * loYWeight;

            if (hiYWeight != 0)
                weightedPixelVal += GetPixelValue(loX, hiY) * loXWeight * hiYWeight;

            if (hiXWeight != 0 && hiYWeight != 0)
                weightedPixelVal += GetPixelValue(hiX, hiY) * hiXWeight * hiYWeight;

            return weightedPixelVal;
        }

        public bool Contains(Point2Df pt)
        {
            return (pt.X >= 0) && (pt.Y >= 0) &&
                   (pt.X < this.Width - 1) && (pt.Y < this.Height - 1);
        }

        public bool ContainsLoose(Point2Df pt)
        {
            return (pt.X > -0.5) && (pt.Y > -0.5) && 
                   (pt.X < this.Width - 0.5) && (pt.Y < this.Height - 0.5);
        }

        public static byte[] BitmapToBytes(Bitmap bmp)
        {
            return BitmapToBytes(bmp, new Rectangle(new Point(), bmp.Size));
        }

        public static unsafe byte[] BitmapToBytes(Bitmap bmp, Rectangle roi)
        {
            BitmapData bData = bmp.LockBits(roi, ImageLockMode.ReadOnly, 
                PixelFormat.Format24bppRgb);

            // Number of bytes in the bitmap ROI
            int byteCount = bData.Width * bData.Height * 3;
            byte[] bmpBytes = new byte[byteCount];

            // Modified from original: http://stackoverflow.com/a/16777061
            for (int y = 0; y < bData.Height; ++y)
            {
                IntPtr mem = (IntPtr)((long)bData.Scan0 + y * bData.Stride);
                Marshal.Copy(mem, bmpBytes, y * bData.Width * 3, bData.Width * 3);
            }

            bmp.UnlockBits(bData);

            return bmpBytes;
        }

        // Returns the absolute difference between the pixels at pt1 and pt2 in images img1 and
        // img2 respectively. pt1 must be within img1, and pt2 must be within img2.
        public static int Compare(Image img1, Point2Di pt1, Image img2, Point2Di pt2)
        {
            Point3Di img1PixVal = img1.GetPixelValue(pt1);
            Point3Di img2PixVal = img2.GetPixelValue(pt2);

            return Math.Abs(img1PixVal.X - img2PixVal.X) +
                   Math.Abs(img1PixVal.Y - img2PixVal.Y) +
                   Math.Abs(img1PixVal.Z - img2PixVal.Z);
        }

        // CHECKME calling compare from an instance could be more efficient - data cursors
        // Returns the absolute difference between the pixels within the kernel centered around pt1
        // and pt2 in img1 and img2 respectively. pt1 must be within img1, and pt2 must be within
        // img2. The kernel must have odd dimensions. None of these conditions are checked, for
        // efficiency.
        public static double Compare(Image img1, Point2Di pt1, Image img2, Point2Di pt2, 
            double[][] kernel)
        {
            double errorSum = 0;
            double weightsSum = 0;

            int kernelHalfHeight = kernel.Length / 2;
            int kernelHalfWidth = kernel[0].Length / 2;

            // CHECKME enforce square kernel?
            // Stores half of the actual width and height being used, rounded down. Each dimension
            // is shrunk symmetrically until the kernel fits within the bounds of both images. 
            // Symmetry is to prevent skewing of the amount of information coming from each side of
            // the center point.
            int halfWidth = Util.Min(kernelHalfWidth, pt1.X, pt2.X, 
                img1.Width - pt1.X - 1, img2.Width - pt2.X - 1);
            int halfHeight = Util.Min(kernelHalfHeight, pt1.Y, pt2.Y,
                img1.Height - pt1.Y - 1, img2.Height - pt2.Y - 1);

            int startX1 = pt1.X - halfWidth;
            int startX2 = pt2.X - halfWidth;

            int y1 = pt1.Y - halfHeight;
            int y2 = pt2.Y - halfHeight;

            for (int yOff = -halfHeight; yOff <= halfHeight; yOff++)
            {
                int x1 = startX1;
                int x2 = startX2;

                for (int xOff = -halfWidth; xOff <= halfWidth; xOff++)
                {
                    int xKernel = kernelHalfWidth + xOff;
                    int yKernel = kernelHalfHeight + yOff;

                    double weight = kernel[yKernel][xKernel];
                    weightsSum += weight;

                    Point3Di pix1 = img1.GetPixelValue(x1, y1);
                    Point3Di pix2 = img2.GetPixelValue(x2, y2);

                    int error = Math.Abs(pix1.X - pix2.X) +
                                Math.Abs(pix1.Y - pix2.Y) +
                                Math.Abs(pix1.Z - pix2.Z);
                    errorSum += error * weight;

                    x1++;
                    x2++;
                }

                y1++;
                y2++;
            }

            return errorSum / weightsSum;
        }

        // Returns the absolute difference between the passed images at the passed sub-pixel
        // locations
        public static double Compare(Image img1, Point2Df pt1, Image img2, Point2Df pt2)
        {
            Point3Df img1PixVal = img1.GetSubPixelValue(pt1);
            Point3Df img2PixVal = img2.GetSubPixelValue(pt2);

            return Math.Abs(img1PixVal.X - img2PixVal.X) +
                   Math.Abs(img1PixVal.Y - img2PixVal.Y) +
                   Math.Abs(img1PixVal.Z - img2PixVal.Z);
        }
    }
}
