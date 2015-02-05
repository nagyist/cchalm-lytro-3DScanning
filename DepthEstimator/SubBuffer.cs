using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelMatchingTest
{
    // A sub-buffer represents a rectangular section of a larger 2D buffer. It is immutable. It
    // has a constructor and a method that returns an index into the whole buffer given offsets
    // from the center of this SubBuffer
    // TODO remove this class and add ROI logic to the Image class. Comments to follow
    class SubBuffer
    {
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public int Channels { get { return channels; } }
        public int Top { get { return top; } }
        public int Left { get { return left; } }
        public int Bottom { get { return bottom; } }
        public int Right { get { return right; } }

        public int RowJump { get { return rowJump; } }

        private readonly int width;
        private readonly int height;
        private readonly int channels;

        private readonly int top;
        private readonly int left;
        private readonly int bottom;
        private readonly int right;

        private readonly Point2Di center;
        private readonly int origWidth;

        private readonly int rowJump;

        public SubBuffer(Image img, Point2Di center, int size)
        {
            int halfSize = size / 2;

            int xMin = Math.Max(center.X - halfSize, 0);
            int yMin = Math.Max(center.Y - halfSize, 0);
            int xMax = Math.Min(center.X + halfSize, img.Width - 1);
            int yMax = Math.Min(center.Y + halfSize, img.Height - 1);

            width = xMax - xMin + 1;
            height = yMax - yMin + 1;
            rowJump = (img.Width - width) * 3;
            channels = img.BytesPerPixel;

            top = yMin - center.Y;
            left = xMin - center.X;
            bottom = yMax - center.Y;
            right = xMax - center.X;

            this.center = center;
            origWidth = img.Width;
        }

        public int GetIndexFromOffsets(int left, int top)
        {
            return ((top + center.Y) * origWidth + (left + center.X)) * 3;
        }
    }
}
