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
    class GrayScale
    {
        private static GrayScale instance = new GrayScale();

        private GrayScale()
        {

        }


        public static GrayScale GetInstance()
        {
            return instance;
        }

        /// <summary>
        /// Return Image in Gray scale
        /// </summary>
        /// <param name="Img"></param>
        /// <returns></returns>
        public Bitmap TurnIntoGray(Bitmap Img)
        {
            if (Img == null)
                return null;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Img.Width, Img.Height);
            BitmapData bitmapData = Img.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bitmapData.Scan0;// wskaźnik na pierwszą linię obrazka
            int BitsPerPixel = 24;// na jeden pixel składają się trzy 8-bitowe wartości RGB
            int size = bitmapData.Width * bitmapData.Height * 3;

            byte[] data = new byte[size];// tablica z wartościami RGB
            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(mem, data, y * bitmapData.Width * 3, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }


            for (int i = 0; i < size; i += BitsPerPixel / 8)// kazda komórka zawiera jedną z trzech wartości- więc przesuwa się co 3 pixele
            {
                byte GrayValue = (byte)(data[i] * 0.299 + data[i + 1] * 0.587 + data[i + 2] * 0.112);
                data[i] = GrayValue > 255 ? (byte)255 : GrayValue;
                data[i + 1] = GrayValue > 255 ? (byte)255 : GrayValue;
                data[i + 2] = GrayValue > 255 ? (byte)255 : GrayValue;
            }

            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(data, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Img.UnlockBits(bitmapData);

            return Img;
        }

        public Bitmap TurnIntoGrayWithGreen(Bitmap Img)
        {
            if (Img == null)
                return null;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Img.Width, Img.Height);
            BitmapData bitmapData = Img.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bitmapData.Scan0;// wskaźnik na pierwszą linię obrazka
            int BitsPerPixel = 24;// na jeden pixel składają się trzy 8-bitowe wartości RGB
            int size = bitmapData.Width * bitmapData.Height * 3;

            byte[] data = new byte[size];// tablica z wartościami RGB
            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(mem, data, y * bitmapData.Width * 3, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }


            for (int i = 0; i < size; i += BitsPerPixel / 8)// kazda komórka zawiera jedną z trzech wartości- więc przesuwa się co 3 pixele
            {
                byte GrayValue = data[i + 1];
                data[i] = GrayValue > 255 ? (byte)255 : GrayValue;
                data[i + 1] = GrayValue > 255 ? (byte)255 : GrayValue;
                data[i + 2] = GrayValue > 255 ? (byte)255 : GrayValue;
            }

            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(data, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Img.UnlockBits(bitmapData);

            return Img;
        }

    }
}
