using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace PixelMatchingTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private unsafe void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LaunchPad test = new LaunchPad();
            double[][] depthMap = test.GetDepths();

            int width = depthMap[0].Length;
            int height = depthMap.Length;

            double maxDepth = 0;
            double minDepth = double.PositiveInfinity;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double depth = depthMap[y][x];

                    if (depth != DepthEstimator.INVALID_DEPTH && !double.IsInfinity(depth))
                    {
                        if (depth > maxDepth)
                            maxDepth = depth;
                        if (depth < minDepth)
                            minDepth = depth;
                    }
                }
            }

            int rowPadding = width % 4;

            byte[] depthBuffer = new byte[(width + rowPadding) * height];
            int index = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double depth = depthMap[y][x];

                    if (depth == LytroDepthEstimator.Constants.InvalidDepth)
                        depthBuffer[index] = 0;
                    else if (double.IsInfinity(depth))
                        depthBuffer[index] = byte.MaxValue;
                    else
                        depthBuffer[index] = 
                            (byte)Math.Round((depth - minDepth) / (maxDepth - minDepth) * 
                            (byte.MaxValue - 2) + 1);

                    index++;
                }

                for (int x = 0; x < rowPadding; x++)
                {
                    depthBuffer[index] = 0;
                    index++;
                }
            }

            Bitmap bitmap = new Bitmap(width, height, 
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            ColorPalette pal = bitmap.Palette;

            // Red for invalid
            pal.Entries[0] = System.Drawing.Color.FromArgb(255, 255, 0, 0);
            for (int i = 1; i < 255; i++)
            {
                // Grayscale, darker is farther away
                pal.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
            }
            // Green for infinite
            pal.Entries[255] = System.Drawing.Color.FromArgb(255, 0, 255, 0);

            bitmap.Palette = pal;

            BitmapData bmData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                ImageLockMode.ReadWrite, bitmap.PixelFormat);
            IntPtr pNative = bmData.Scan0;
            Marshal.Copy(depthBuffer, 0, pNative, (width + rowPadding) * height);
            bitmap.UnlockBits(bmData);

            ImageDisplayBox.Width = width;
            ImageDisplayBox.Height = height;
            ImageDisplayBox.Source = LoadBitmap(bitmap);

            // http://www.dotnetperls.com/filename-datetime
            bitmap.Save("Results/" + string.Format("{0:yyyy-MM-dd_hh.mm.ss-tt}", 
                DateTime.Now) + ".bmp");
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource LoadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }
    }
}
