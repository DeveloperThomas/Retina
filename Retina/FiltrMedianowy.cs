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
    class FiltrMedianowy
    {

        private static FiltrMedianowy instance = new FiltrMedianowy();

        private FiltrMedianowy()
        {

        }


        public static FiltrMedianowy GetInstance()
        {
            return instance;
        }


        public Bitmap PrzeprowadzFiltrMedianowy(Bitmap Img, int windowWidth=5)
        {

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Img.Width, Img.Height);
            BitmapData bitmapData = Img.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bitmapData.Scan0;// wskaźnik na pierwszą linię obrazka
            int size = bitmapData.Width * bitmapData.Height * 3;

            byte[] data = new byte[size];// tablica z wartościami RGB

            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(mem, data, y * bitmapData.Width * 3, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            int halfEdge = (windowWidth - 1) / 2;

            int[,] ImageInTableR = new int[Img.Height, Img.Width];
            int[,] ImageInTableG = new int[Img.Height, Img.Width];
            int[,] ImageInTableB = new int[Img.Height, Img.Width];

            int[,] NewImageValueR = new int[Img.Height, Img.Width];
            int[,] NewImageValueG = new int[Img.Height, Img.Width];
            int[,] NewImageValueB = new int[Img.Height, Img.Width];

            for (int i = 0; i < Img.Height; i++)
            {
                for (int j = 0; j < Img.Width; j++)
                {
                    ImageInTableR[i, j] = data[i * (Img.Width * 3) + j * 3];
                    ImageInTableG[i, j] = data[i * (Img.Width * 3) + j * 3 + 1];
                    ImageInTableB[i, j] = data[i * (Img.Width * 3) + j * 3 + 2];
                }
            }// tablica z wartościami poszczególnych pikseli

            for (int i = 0; i < Img.Height; i++)
            {
                for (int j = 0; j < Img.Width; j++)
                {
                    if (!(i - halfEdge < 0 || i + halfEdge >= Img.Height || j - halfEdge < 0 || j + halfEdge >= Img.Width))
                    {   // pixel ma wyamaganą wielkośc okna 
                        // i pierwszy argument oznacza Y
                        // j drugi argument oznacza X
                        int X = j - halfEdge;
                        int Y = i - halfEdge;

                        System.Drawing.Rectangle MyRect = new System.Drawing.Rectangle(X, Y, (int)windowWidth, (int)windowWidth);

                        NewImageValueR[i, j] = GetMedianFromArea(ImageInTableR, MyRect);
                        NewImageValueG[i, j] = GetMedianFromArea(ImageInTableG, MyRect);
                        NewImageValueB[i, j] = GetMedianFromArea(ImageInTableB, MyRect);

                    }
                }
            }

            int firstIndex = 0;
            int secondIndex = 0;

            for (int l = 0; l < data.Length; l += 3)// skakanie po pierwszych wartosciach piksela
            {
                int binValueR = NewImageValueR[firstIndex, secondIndex];
                int binValueG = NewImageValueG[firstIndex, secondIndex];
                int binValueB = NewImageValueB[firstIndex, secondIndex];
                secondIndex++;
                if (secondIndex >= Img.Width)
                {
                    firstIndex++;// nastepny poziom
                    if (firstIndex >= Img.Height)
                        break;
                    secondIndex = 0;// poczatek 
                }

                data[l] = (byte)binValueR;
                data[l + 1] = (byte)binValueG;
                data[l + 2] = (byte)binValueB;

            }

            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(data, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Img.UnlockBits(bitmapData);

            return Img;
        }


        private int GetMedianFromArea(int[,] AllArea, System.Drawing.Rectangle window)
        {
            int result = 0;
            List<int> Values = new List<int>();

            for (Int32 y = 0; y < window.Height; ++y)
            {
                for (Int32 x = 0; x < window.Width; ++x)
                {
                    Values.Add(AllArea[y + (int)window.Y, x + (int)window.X]);
                }
            }

            Values.Sort();

            result = Values[(Values.Count - 1) / 2];

            return result;
        }


    }
}
