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
    class RozmycieObrazu
    {
        private static RozmycieObrazu instance = new RozmycieObrazu();

        private RozmycieObrazu()
        {

        }


        public static RozmycieObrazu GetInstance()
        {
            return instance;
        }



        public Bitmap RozmyjObraz(Bitmap Img)
        {

            if (Img == null)
                return null;

            int matrixSum = 49;// maska wypełniona jedynkami
            int edgeValue = 7;

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Img.Width, Img.Height);
            BitmapData bitmapData = Img.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bitmapData.Scan0;// wskaźnik na pierwszą linię obrazka
            int size = bitmapData.Width * bitmapData.Height * 3;


            byte[] data = new byte[size];// tablica z wartościami RGB
            byte[] newData = new byte[size];// tablica z wartościami RGB
            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(mem, data, y * bitmapData.Width * 3, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            int halfEdge = (edgeValue - 1) / 2;

            int[,] ImageInTableR = new int[Img.Height, Img.Width];
            int[,] ImageInTableG = new int[Img.Height, Img.Width];
            int[,] ImageInTableB = new int[Img.Height, Img.Width];

            int[,] NewImageValueR = new int[Img.Height, Img.Width];
            int[,] NewImageValueG = new int[Img.Height, Img.Width];
            int[,] NewImageValueB = new int[Img.Height, Img.Width];

            int differencee = bitmapData.Stride - Img.Width * 3;

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
                    {// pixel ma wyamaganą wielkośc okna 
                     // i pierwszy argument oznacza Y
                     // j drugi argument oznacza X
                        int X = j - halfEdge;
                        int Y = i - halfEdge;

                        System.Drawing.Rectangle MyRect = new System.Drawing.Rectangle(X, Y, edgeValue, edgeValue);

                        NewImageValueR[i, j] = GetNewValue(MyRect, ImageInTableR, matrixSum);
                        NewImageValueG[i, j] = GetNewValue(MyRect, ImageInTableG, matrixSum);
                        NewImageValueB[i, j] = GetNewValue(MyRect, ImageInTableB, matrixSum);
                    }

                }
            }

            //wsadzenie obrazu z data z powrotem do img
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

                newData[l] = (byte)binValueR;
                newData[l + 1] = (byte)binValueG;
                newData[l + 2] = (byte)binValueB;

            }
           
            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(data, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Img.UnlockBits(bitmapData);


            Bitmap Rozmyty = new Bitmap(bitmapData.Width, bitmapData.Height);
            Rectangle newRect = new Rectangle(0, 0, Img.Width, Img.Height);
            BitmapData rozmytyBitmapData = Rozmyty.LockBits(newRect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            IntPtr rozmytyWskaznik = rozmytyBitmapData.Scan0;

            for (int y = 0; y < rozmytyBitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)rozmytyBitmapData.Scan0 + y * rozmytyBitmapData.Stride);
                Marshal.Copy(newData, y * rozmytyBitmapData.Width * 3, mem, rozmytyBitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Rozmyty.UnlockBits(rozmytyBitmapData);

            return Rozmyty;
        }


        public byte GetNewValue(System.Drawing.Rectangle area, int[,] allArea, int matrixSum)
        {
            //int[,] result = new int[area.Width, area.Height];
            //List<int> result = new List<int>();
            //int[] result = new int[256];
            byte result = 0;
            int sum = 0;

            for (Int32 y = 0; y < area.Height; ++y)
            {
                for (Int32 x = 0; x < area.Width; ++x)
                {
                    sum += (allArea[y + area.Y, x + area.X] * 1);
                    //result[allArea[y + area.Y, x + area.X]]++;
                }
            }
            result = (byte)(Math.Abs(sum) / matrixSum);
            return result;
        }

    }
}
