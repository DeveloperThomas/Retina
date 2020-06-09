using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Retina
{
    /// <summary>
    /// Singleton
    /// </summary>
    class Binarization
    {
        private static Binarization instance = new Binarization();

        private Binarization()
        {

        }

        public static Binarization GetInstance()
        {
            return instance;
        }

        public Bitmap OtsuMethod(Bitmap Image, bool isTopThereshold)
        {
            int[] histogramArray = new int[256];
            histogramArray = Histogram.GetInstance().CreateHistogram(Image, 0);
            int N = Image.Width * Image.Height;
            double Wb = 0.0;
            double Wf = 0.0;
            double VarianceB = 0.0;
            double VarianceF = 0.0;
            double GammaB = 0.0;
            double GammaF = 0.0;
            double[] minimumValues = new double[256];

            for (int T = 0; T < 256; T++)
            {
                Wb = 0.0;
                Wf = 0.0;
                VarianceB = 0.0;
                VarianceF = 0.0;
                GammaB = 0.0;
                GammaF = 0.0;
                for (int i = 0; i <= (T - 1); i++)
                {
                    Wb += histogramArray[i] / (double)N;
                }

                for (int i = 0; i <= (T - 1); i++)
                {
                    GammaB += i * (histogramArray[i] / (double)N) / Wb;
                }

                for (int i = T; i <= 255; i++)
                {
                    Wf += histogramArray[i] / (double)N;
                }

                for (int i = T; i <= 255; i++)
                {
                    GammaF += i * (histogramArray[i] / (double)N) / Wf;
                }

                for (int i = 0; i <= (T - 1); i++)
                {
                    VarianceB += (histogramArray[i] * (double)N) * (Math.Pow((i - GammaB), 2) / Wb);
                }

                for (int i = T; i <= 255; i++)
                {
                    VarianceF += (histogramArray[i] * (double)N) * (Math.Pow((i - GammaF), 2) / Wf);
                }

                double currentValue = (Wf * Math.Pow(VarianceF, 2)) + (Wb * Math.Pow(VarianceB, 2));

                minimumValues[T] = currentValue;
            }

            double minimumValue = minimumValues[0];
            int threshold = 0;

            for (int i = 1; i < 256; i++)
            {
                if (minimumValues[i] < minimumValue)
                {
                    minimumValue = minimumValues[i];
                    threshold = i;
                }
            }

            int point = threshold;

            return BinarizeThereshol(Image, point, isTopThereshold);
        }

        private int ProbabilityK(int begin, int end, int[] hist)
        {
            int sum = 0;

            for (int i = begin; i <= end; i++)
            {
                sum += hist[i];
            }
            return sum;
        }

        private int MeanIntensitiesK(int begin, int end, int[] hist)
        {
            int sum = 0;
            for (int i = begin; i < end; i++)
            {
                sum += i * hist[i];
            }
            return sum;
        }


        private int FindMaximum(double[] list)
        {
            double max = 0;
            int idx = 0;

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] > max)
                {
                    max = list[i];
                    idx = i;
                }
            }

            return idx;
        }

        public Bitmap BinarizeThereshol(Bitmap Image, int point, Boolean isTopThereshold)
        {
            System.Drawing.Rectangle rect = new Rectangle(0, 0, Image.Width, Image.Height);
            BitmapData bitmapData = Image.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bitmapData.Scan0;// wskaźnik na pierwszą linię obrazka
            int size = bitmapData.Width * bitmapData.Height * 3;

            byte[] data = new byte[size];// tablica z wartościami RGB
            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(mem, data, y * bitmapData.Width * 3, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            for (int i = 0; i < size; i += 3)// kazda komórka zawiera jedną z trzech wartości- więc przesuwa się co 3 pixele
            {
                if (data[i] >= point)
                {
                    data[i] = (isTopThereshold) ? (byte)255 : (byte)0;
                    data[i + 1] = (isTopThereshold) ? (byte)255 : (byte)0;
                    data[i + 2] = (isTopThereshold) ? (byte)255 : (byte)0;
                }
                else
                {
                    data[i] = (isTopThereshold) ? (byte)0 : (byte)255;
                    data[i + 1] = (isTopThereshold) ? (byte)0 : (byte)255;
                    data[i + 2] = (isTopThereshold) ? (byte)0 : (byte)255;
                }
            }

            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(data, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Image.UnlockBits(bitmapData);

            MemoryStream stream = new MemoryStream();

            Image.Save(stream, ImageFormat.Bmp);
            return Image;
        }




    }
}
