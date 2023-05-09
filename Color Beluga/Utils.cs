using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Color_Beluga
{
    internal static class Utils
    {
        public static Bitmap ApplyGaussianBlur(Bitmap source, int kernelSize = 20, double sigma = 10)
        {
            GaussianBlur gaussianBlur = new GaussianBlur(sigma, kernelSize);
            Bitmap blurredBitmap = gaussianBlur.Apply(source);

            return blurredBitmap;
        }

        public static Color GetMedianColorOfBitmap(Bitmap bitmap)
        {
            List<int> rList = new List<int>();
            List<int> gList = new List<int>();
            List<int> bList = new List<int>();

            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    rList.Add(pixelColor.R);
                    gList.Add(pixelColor.G);
                    bList.Add(pixelColor.B);
                }
            }

            rList.Sort();
            gList.Sort();
            bList.Sort();

            int medianIndex = rList.Count / 2;

            return Color.FromArgb(rList[medianIndex], gList[medianIndex], bList[medianIndex]);
        }

        public static System.Drawing.Color GetAverageColorOfBitmap(Bitmap bitmap)
        {
            long sumR = 0;
            long sumG = 0;
            long sumB = 0;
            long pixelCount = bitmap.Width * bitmap.Height;
            System.Drawing.Color pixelColor = System.Drawing.Color.White;

            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    pixelColor = bitmap.GetPixel(x, y);
                    sumR += pixelColor.R;
                    sumG += pixelColor.G;
                    sumB += pixelColor.B;
                }
            }

            return System.Drawing.Color.FromArgb(
                (int)(sumR / pixelCount),
                (int)(sumG / pixelCount),
                (int)(sumB / pixelCount)
            );
        }

        public static int GetMaxScreenDimension()
        {
            int maxDimension = 0;

            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.Bounds.Width > maxDimension)
                {
                    maxDimension = screen.Bounds.Width;
                }

                if (screen.Bounds.Height > maxDimension)
                {
                    maxDimension = screen.Bounds.Height;
                }
            }

            return maxDimension;
        }
    }
}
