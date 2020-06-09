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
    class SzkieletyzacjaK3M
    {
        private static SzkieletyzacjaK3M instance = new SzkieletyzacjaK3M();

        private SzkieletyzacjaK3M()
        {

        }


        public static SzkieletyzacjaK3M GetInstance()
        {
            return instance;
        }
        private readonly List<int> A0 = new List<int>{3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56, 60,
        62, 63, 96, 112, 120, 124, 126, 127, 129, 131, 135,
        143, 159, 191, 192, 193, 195, 199, 207, 223, 224,
        225, 227, 231, 239, 240, 241, 243, 247, 248, 249,
        251, 252, 253, 254 };


        private static readonly List<int> A1 = new List<int>() { 7, 14, 28, 56, 112, 131, 193, 224 };
        private static readonly List<int> A2 = new List<int>() {7, 14, 15, 28, 30, 56, 60, 112, 120, 131, 135,
        193, 195, 224, 225, 240};

        private static readonly List<int> A3 = new List<int>(){7, 14, 15, 28, 30, 31, 56, 60, 62, 112, 120,
        124, 131, 135, 143, 193, 195, 199, 224, 225, 227, 240, 241, 248};

        private static readonly List<int> A4 = new List<int>() {7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120,
        124, 126, 131, 135, 143, 159, 193, 195, 199, 207,
        224, 225, 227, 231, 240, 241, 243, 248, 249, 252};

        private static readonly List<int> A5 = new List<int>() {7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120,
        124, 126, 131, 135, 143, 159, 191, 193, 195, 199,
        207, 224, 225, 227, 231, 239, 240, 241, 243, 248,
        249, 251, 252, 254};

        private static readonly List<int> A1pix = new List<int>() { 3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56,
        60, 62, 63, 96, 112, 120, 124, 126, 127, 129, 131,
        135, 143, 159, 191, 192, 193, 195, 199, 207, 223,
        224, 225, 227, 231, 239, 240, 241, 243, 247, 248,
        249, 251, 252, 253, 254};

        private readonly List<List<int>> PhaseList = new List<List<int>>() { new List<int>(){ 7, 14, 28, 56, 112, 131, 193, 224 },
            new List<int>(){7, 14, 15, 28, 30, 56, 60, 112, 120, 131, 135,
        193, 195, 224, 225, 240},
            new List<int>(){7, 14, 15, 28, 30, 31, 56, 60, 62, 112, 120,
        124, 131, 135, 143, 193, 195, 199, 224, 225, 227, 240, 241, 248 },
            new List<int>(){7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120,
        124, 126, 131, 135, 143, 159, 193, 195, 199, 207,
        224, 225, 227, 231, 240, 241, 243, 248, 249, 252 } ,
            new List<int>(){3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56,
        60, 62, 63, 96, 112, 120, 124, 126, 127, 129, 131,
        135, 143, 159, 191, 192, 193, 195, 199, 207, 223,
        224, 225, 227, 231, 239, 240, 241, 243, 247, 248,
        249, 251, 252, 253, 254 }};

        public Bitmap UseK3M(Bitmap Img)
        {
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

            // stwórz 2D tablicę obtrazu 
            int[,] imageIn2DTable = Functions.GetInstance().Create2DTable(data, bitmapData.Width, bitmapData.Height);

            // zamień czarny kolor na 1 a bialy na 0 
            imageIn2DTable = Functions.GetInstance().ChangeBinarizedImage(imageIn2DTable, bitmapData, Img);
            int[,] tmpImageInArray = (int[,])imageIn2DTable.Clone();


            int halfEdge = 1;// połowa okienka to jeden piksel ponieważ analizujemy okno 3x3 wokół każdego pixela
            int windowWidth = 3;// okno ma 3pixele 

            Boolean deletedSth = false;

            do
            {
                deletedSth = false;// jeżeli po pentli nic nie zostało usuniete to znaczy że koniec algorytmu

                for (int i = 0; i < Img.Height; i++)
                {
                    for (int j = 0; j < Img.Width; j++)
                    {
                        // pixel ma wyamaganą wielkośc okna - nie sa brane pod uwage pixele koło krawędzi
                        if (!(i - halfEdge < 0 || i + halfEdge >= Img.Height || j - halfEdge < 0 || j + halfEdge >= Img.Width))
                        {
                            if (imageIn2DTable[i, j] != 0)
                            {
                                // i pierwszy argument oznacza Y
                                // j drugi argument oznacza X 
                                int X = j - halfEdge;
                                int Y = i - halfEdge;
                                // i oznacza pozycje Y pixela , a j pozycje X pixela
                                // zmienne powużej , X,Y oznaczają lewy górny góg okna 3x3

                                tmpImageInArray[i, j] = MarkAsBorder(new Point(j, i), imageIn2DTable);
                                //tmpImageInArray[i, j] = FindTwoAndThree(new Point(j, i), imageIn2DTable);
                            }
                        }
                    }
                }



                imageIn2DTable = (int[,])tmpImageInArray.Clone();
                /// KONIEC PIERWSZEGO ETAPU OZNACZANIA KRAWEDZI

                for(int phase=0; phase <= 4; phase++)
                {
                    // pentla dla wartosci N
                    for (int N = 2; N <= 3; N++)
                    {
                        for (int i = 0; i < Img.Height; i++)
                        {
                            for (int j = 0; j < Img.Width; j++)
                            {
                                // pixel ma wyamaganą wielkośc okna - nie sa brane pod uwage pixele koło krawędzi
                                if (!(i - halfEdge < 0 || i + halfEdge >= Img.Height || j - halfEdge < 0 || j + halfEdge >= Img.Width))
                                {
                                    if (imageIn2DTable[i, j] == 2)// jeżeli jest to pixel z brzegu
                                    {
                                        // i pierwszy argument oznacza Y
                                        // j drugi argument oznacza X 
                                        int X = j - halfEdge;
                                        int Y = i - halfEdge;
                                        // i oznacza pozycje Y pixela , a j pozycje X pixela
                                        // zmienne powużej , X,Y oznaczają lewy górny góg okna 3x3


                                        tmpImageInArray[i, j] = DeleteIfInTable(new Point(j, i), imageIn2DTable, ref deletedSth, PhaseList[phase]);

                                    }
                                }
                            }
                        }
                    }
                    imageIn2DTable = (int[,])tmpImageInArray.Clone();
                }


                ///ZAMIANA PIKSELI GRANICZNYCH Z POWROTEM NA CZARNE
                for (int N = 2; N <= 3; N++)
                {
                    for (int i = 0; i < Img.Height; i++)
                    {
                        for (int j = 0; j < Img.Width; j++)
                        {
                            // pixel ma wyamaganą wielkośc okna - nie sa brane pod uwage pixele koło krawędzi
                            if (!(i - halfEdge < 0 || i + halfEdge >= Img.Height || j - halfEdge < 0 || j + halfEdge >= Img.Width))
                            {
                                if (imageIn2DTable[i, j] == 2)// jeżeli jest to pixel z brzegu
                                {
                                    // i pierwszy argument oznacza Y
                                    // j drugi argument oznacza X 
                                    int X = j - halfEdge;
                                    int Y = i - halfEdge;

                                    imageIn2DTable[i, j] = 1;// pokoloruj na czarno
                                }
                            }
                        }
                    }
                }

                imageIn2DTable = (int[,])tmpImageInArray.Clone();

            } while (deletedSth);
            /// pętla iterująca po każdym pikselu w obrazie

            // pentla dla wartosci N
            for (int N = 2; N <= 3; N++)
            {
                for (int i = 0; i < Img.Height; i++)
                {
                    for (int j = 0; j < Img.Width; j++)
                    {
                        // pixel ma wyamaganą wielkośc okna - nie sa brane pod uwage pixele koło krawędzi
                        if (!(i - halfEdge < 0 || i + halfEdge >= Img.Height || j - halfEdge < 0 || j + halfEdge >= Img.Width))
                        {
                            if (imageIn2DTable[i, j] !=0)// jeżeli jest to pixel z brzegu
                            {
                                // i pierwszy argument oznacza Y
                                // j drugi argument oznacza X 
                                int X = j - halfEdge;
                                int Y = i - halfEdge;
                                // i oznacza pozycje Y pixela , a j pozycje X pixela
                                // zmienne powużej , X,Y oznaczają lewy górny góg okna 3x3


                                tmpImageInArray[i, j] = DeleteIfInTable(new Point(j, i), imageIn2DTable, ref deletedSth, A1pix);

                            }
                        }
                    }
                }
            }

            data = Functions.GetInstance().CreateBytetableFrom2DImage(tmpImageInArray, size, bitmapData.Width, bitmapData.Height);

            for (int i = 0; i < size; i++)
            {
                if (data[i] != 0)
                {
                    data[i] = 0;
                    ;
                }
                else
                {
                    data[i] = 255;
                }
            }


            /// Poniżej kod do zamiany obrazu z data do obiektu Bitmap - TEGO NIE ZMIENIAĆ 
            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(data, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Img.UnlockBits(bitmapData);

            return Img;
        }


        private int DeleteIfInTable(Point coordinates, int[,] allArea, ref Boolean deleted, List<int> deleteList)
        {
            int edgeSize = 3;
            int leftUpX = coordinates.X - 1;// koordynaty górnego lewego pixela w oknie 
            int leftUpY = coordinates.Y - 1;

            //macierz z wagami
            int[,] weights = new int[3, 3] { { 128, 1, 2 }, { 64, 0, 4 }, { 32, 16, 8 } };

            int sum = 0;

            for (Int32 y = 0; y < edgeSize; ++y)
            {
                for (Int32 x = 0; x < edgeSize; ++x)
                {
                    if (allArea[y + leftUpY, x + leftUpX] != 0)
                    {
                        sum += weights[y, x];
                    }
                }
            }

            if (deleteList.Contains(sum))
            {
                deleted = true;
                return 0;

            }
            else
            {
                return allArea[coordinates.Y, coordinates.X];
            }

        }


        /// <summary>
        /// Border pixels are marked as 2 
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="allArea"></param>
        /// <returns></returns>
        private int MarkAsBorder(Point coordinates, int[,] allArea)
        {
            int edgeSize = 3;
            int leftUpX = coordinates.X - 1;// koordynaty górnego lewego pixela w oknie 
            int leftUpY = coordinates.Y - 1;

            //macierz z wagami
            int[,] weights = new int[3, 3] { { 128, 1, 2 }, { 64, 0, 4 }, { 32, 16, 8 } };

            int sum = 0;

            for (Int32 y = 0; y < edgeSize; ++y)
            {
                for (Int32 x = 0; x < edgeSize; ++x)
                {
                    if (allArea[y + leftUpY, x + leftUpX] != 0)
                    {
                        sum += weights[y, x];
                    }
                }
            }

            if (A0.Contains(sum))
            { 
                return 2;

            }
            else
            {
                return allArea[coordinates.Y, coordinates.X];
            }

        }

    }
}
