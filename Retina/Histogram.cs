using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Retina
{
    class Histogram
    {
        private static Histogram instance = new Histogram();

        private Histogram()
        {

        }

        public static Histogram GetInstance()
        {
            return instance;
        }


        /// <summary>
        /// Tworzy histogram, w którym indeks komówki określa odcień koloru a wartośc - ile takich pikseli jest w obrazie
        /// </summary>
        /// <param name="image">Image in Bitmap type</param>
        /// <param name="color">R-0, G-1, B-2, Average -3, Luminance-4. Other values will throw exception</param>
        /// <returns></returns>
        public int[] CreateHistogram(Bitmap image, int color)
        {
            int[] Histogram = new int[256];
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bitmapData = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr ptr = bitmapData.Scan0;// wskaźnik na pierwszą linię obrazka
            int BitsPerPixel = 24;// na jeden pixel składają się trzy 8-bitowe wartości RGB
            int size = bitmapData.Stride * bitmapData.Height;

            size = size - size % 3;


            byte[] data = new byte[size];// tablica z wartościami RGB

            Marshal.Copy(ptr, data, 0, size);// skopiuj całą bitmapę do tablicy


            switch (color)
            {
                case 0:
                case 1:
                case 2:
                    {
                        for (int i = 0; i < size; i += BitsPerPixel / 8)// kazda komórka zawiera jedną z trzech wartości- więc przesuwa się co 3 pixele
                        {
                            Histogram[data[i + color]]++;
                        }
                        break;
                    }
                case 3:
                    {// for average histogram

                        for (int i = 0; i < size; i += BitsPerPixel / 8)// kazda komórka zawiera jedną z trzech wartości- więc przesuwa się co 3 pixele
                        {
                            Histogram[(Int16)((data[i] + data[i + 1] + data[i + 2]) / 3)]++;
                        }
                        break;
                    }
                case 4:
                    {// for average histogram

                        for (int i = 0; i < size; i += BitsPerPixel / 8)// kazda komórka zawiera jedną z trzech wartości- więc przesuwa się co 3 pixele
                        {
                            Histogram[(Int16)Math.Sqrt(Math.Pow(data[i], 2) * 0.299 + Math.Pow(data[i + 1], 2) * 0.587 + Math.Pow(data[i + 2], 2) * 0.112)]++;
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception("Wrong color value!!!");
                    }
            }

            image.UnlockBits(bitmapData);

            return Histogram;
        }


    }
}
