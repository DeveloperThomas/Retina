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
    class Szkieletyzacja
    {
        private static Szkieletyzacja instance = new Szkieletyzacja();


        private Szkieletyzacja()
        {

        }

        public static Szkieletyzacja GetInstance()
        {
            return instance;
        }


        private readonly List<int> WeightToDelete = new List<int>() {3, 5, 7 ,12 ,13 ,14 ,15 ,20 ,21 ,22 ,23 ,28 ,29 ,30 ,31 ,48
        ,52 ,53 ,54 ,55 ,56 ,60 ,61 ,62 ,63 ,65 ,67 ,69 ,71 ,77 ,79 ,80 ,81 ,83 ,84 ,85 ,86 ,87 ,88 ,89
        ,91 ,92 ,93 ,94 ,95 ,97 ,99 ,101 ,103 ,109 ,111 ,112 ,113 ,115 ,116 ,117 ,118 ,119 ,120 ,121 ,123 ,124 ,125 ,126
        ,127 ,131 ,133 ,135 ,141 ,143 ,149 ,151 ,157 ,159 ,181 ,183 ,189 ,191 ,192 ,193 ,195 ,197 ,199 ,205 ,207 ,208 ,209 ,211
        ,212 ,213 ,214 ,215 ,216 ,217 ,219 ,220 ,221 ,222 ,223 ,224 ,225 ,227 ,229 ,231 ,237 ,239 ,240 ,241 ,243 ,244 ,245 ,246
        ,247 ,248 ,249 ,251 ,252 ,253 ,254 ,255 };

        // tablica zawierająca wagi pixela który ma dwóch, trzech, czeterech sąsiadów
        private readonly List<int> WeightOf4 = new List<int>() {3,6,12,24,48,96,192,129,7,14,28,
            56,112,224,193,131,15,30,60,120,240,225,195,135};


        public Bitmap Szkieletyzuj(Bitmap Img)
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
            /// //////////////////////// ZMIEŃ KOD PONIŻEJ 

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

                                tmpImageInArray[i, j] = FindTwoAndThree(new Point(j, i), imageIn2DTable);

                                tmpImageInArray[i, j] = FindFourAndDelete(new Point(j, i), tmpImageInArray, ref deletedSth);

                            }
                        }
                    }
                }

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
                                if (tmpImageInArray[i, j] != 0)
                                {
                                    // i pierwszy argument oznacza Y
                                    // j drugi argument oznacza X 
                                    int X = j - halfEdge;
                                    int Y = i - halfEdge;
                                    // i oznacza pozycje Y pixela , a j pozycje X pixela
                                    // zmienne powużej , X,Y oznaczają lewy górny góg okna 3x3

                                    if (tmpImageInArray[i, j] == N)
                                    {
                                        if (WeightToDelete.Contains(CalculateWeight(new Point(j, i), tmpImageInArray)))
                                        {
                                            tmpImageInArray[i, j] = 0;
                                            deletedSth = true;
                                        }
                                        else
                                        {
                                            tmpImageInArray[i, j] = 1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                imageIn2DTable = (int[,])tmpImageInArray.Clone();

            } while (deletedSth);
            /// pętla iterująca po każdym pikselu w obrazie



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

        /// <summary>
        /// Funlkcja zmianająca wartości na obrazie na 2 lub 3 - w zależności od otopczenia pixela
        /// </summary>
        /// <param name="coordinates">Pozycja pixela</param>
        /// <param name="allArea"></param>
        /// <returns></returns>
        private int FindTwoAndThree(Point coordinates, int[,] allArea)
        {
            int edgeSize = 3;
            int leftUpX = coordinates.X - 1;// koordynaty górnego lewego pixela w oknie 
            int leftUpY = coordinates.Y - 1;

            int blackAround = -1;// number of pixels around our pixel, it starts from -1 because loop 
                                 // on the bottom iterates count pixel itself too (gramar nazi)

            int[,] nearArea = new int[3, 3];// wycinek, który na srodku zawiera w sobie sprawdzany pixel

            for (Int32 y = 0; y < edgeSize; ++y)
            {
                for (Int32 x = 0; x < edgeSize; ++x)
                {
                    if (allArea[y + leftUpY, x + leftUpX] != 0)
                    {
                        blackAround++;
                    }
                    nearArea[y, x] = allArea[y + leftUpY, x + leftUpX];

                }
            }

            if (blackAround == 8)// pixel is in the middle of structure - return 1 (no changes)
                return 1;

            if (blackAround == -1)// pixel have no other black pixels near
                return 1;

            if (blackAround == 1)// pixel is on the very end of line
                return 1;

            if (blackAround == 7) //  pixel have one white pixel nearby - it can be changed into 2 or 3 
            {
                if (nearArea[0, 0] == 0 || nearArea[0, 2] == 0 || nearArea[2, 0] == 0 || nearArea[2, 2] == 0)
                {
                    return 3;// pixel jest w narożniku
                }
                else
                {
                    return 2;
                }

            }
            else
            {//check if 
                return 2;
            }

        }

        /// <summary>
        /// Funkcja sprawdza czy pixel powinien mieć wartośc 4 i go usuwa (daje mu 0 jako wartosc)
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="allArea"></param>
        /// <returns></returns>
        private int FindFourAndDelete(Point coordinates, int[,] allArea, ref Boolean deleted)
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

            if (WeightOf4.Contains(sum))
            {
                deleted = true;
                return 0;

            }
            else
            {
                return allArea[coordinates.Y, coordinates.X];
            }

        }

        private int CalculateWeight(Point coordinates, int[,] allArea)
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
            return sum;
        }


    }
}
