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
    class OdjecieObrazu
    {
        private static OdjecieObrazu instance = new OdjecieObrazu();

        private OdjecieObrazu()
        {

        }


        public static OdjecieObrazu GetInstance()
        {
            return instance;
        }

        public Bitmap OdjecieRozmytegoOdOryginalu(Bitmap Oryginal, Bitmap Rozmyty)
        {
            if (Oryginal == null || Rozmyty==null)
                return null;

            System.Drawing.Rectangle OriginalRect = new System.Drawing.Rectangle(0, 0, Oryginal.Width, Oryginal.Height);
            BitmapData OriginalBitmapData = Oryginal.LockBits(OriginalRect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = OriginalBitmapData.Scan0;// wskaźnik na pierwszą linię obrazka
            int Originalsize = OriginalBitmapData.Width * OriginalBitmapData.Height * 3;

            byte[] OriginalData = new byte[Originalsize];// tablica z wartościami RGB
            for (int y = 0; y < OriginalBitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)OriginalBitmapData.Scan0 + y * OriginalBitmapData.Stride);
                Marshal.Copy(mem, OriginalData, y * OriginalBitmapData.Width * 3, OriginalBitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Oryginal.UnlockBits(OriginalBitmapData);
            //
            //Tu powyzej jest określenaie danych z oryginału.
            //
            ///
            // Ponizej jest okreslenie danych obrazu rozmytego 
            ///
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Rozmyty.Width, Rozmyty.Height);
            BitmapData bitmapData = Rozmyty.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
           // IntPtr ptr = bitmapData.Scan0;// wskaźnik na pierwszą linię obrazka
            int Rozmytysize = bitmapData.Width * bitmapData.Height * 3;

            byte[] RozmytyData = new byte[Rozmytysize];// tablica z wartościami RGB
            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(mem, RozmytyData, y * bitmapData.Width * 3, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }
            /// w tym momencie mamy dane w dwóch tablicach  RozmytyData i OriginalData
            /// 

            for (int i = 0; i < Rozmytysize; i+=3)
            {

                int newValue = RozmytyData[i] - OriginalData[i];

                if (newValue < 0)
                {
                    RozmytyData[i] = 0;
                    RozmytyData[i + 1] = 0;
                    RozmytyData[i + 2] = 0;
                }
                else
                {
                    RozmytyData[i] = (byte)newValue;
                    RozmytyData[i + 1] = (byte)newValue;
                    RozmytyData[i + 2] = (byte)newValue;
                }
            }



            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(RozmytyData, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Rozmyty.UnlockBits(bitmapData);

            return Rozmyty;
        }


    }
}
