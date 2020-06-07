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
    class Rozgalezienia
    {
        private static Rozgalezienia instance = new Rozgalezienia();
        List<Point> singlePointsList = new List<Point>();
        List<Point> endsOfEdgeList = new List<Point>();
        List<Point> forksList = new List<Point>();
        List<Point> intersectionsList = new List<Point>();

        private Rozgalezienia()
        {

        }

        public static Rozgalezienia GetInstance()
        {
            return instance;
        }

        public List<Point> getSinglePointsList()
        {
            return this.singlePointsList;
        }

        public List<Point> getEndsOfEdgeList()
        {
            return this.endsOfEdgeList;
        }

        public List<Point> getForksList()
        {
            return this.forksList;
        }

        public List<Point> getIntersectionsList()
        {
            return this.intersectionsList;
        }

        public Bitmap Find(Bitmap Img)
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

            int[,] imageIn2DTable = Functions.GetInstance().Create2DTable(data, bitmapData.Width, bitmapData.Height);

            // zamień czarny kolor na 1 a bialy na 0 
            imageIn2DTable = Functions.GetInstance().ChangeBinarizedImage(imageIn2DTable, bitmapData, Img);
            int[,] tmpImageInArray = (int[,])imageIn2DTable.Clone();

            int halfEdge = 1;// połowa okienka to jeden piksel ponieważ analizujemy okno 3x3 wokół każdego pixela
            int CN = 0;
            int sum = 0;


            for (int i = 0; i < Img.Height; i++)
            {
                for (int j = 0; j < Img.Width; j++)
                {
                    if (!(i - halfEdge < 0 || i + halfEdge >= Img.Height || j - halfEdge < 0 || j + halfEdge >= Img.Width))
                    {
                        if (imageIn2DTable[i, j] != 0)
                        {
                            sum += Math.Abs(imageIn2DTable[i + halfEdge, j] - imageIn2DTable[i + halfEdge, j - halfEdge]);
                            sum += Math.Abs(imageIn2DTable[i + halfEdge, j - halfEdge] - imageIn2DTable[i, j - halfEdge]);
                            sum += Math.Abs(imageIn2DTable[i, j - halfEdge] - imageIn2DTable[i - halfEdge, j - halfEdge]);
                            sum += Math.Abs(imageIn2DTable[i - halfEdge, j - halfEdge] - imageIn2DTable[i - halfEdge, j]);
                            sum += Math.Abs(imageIn2DTable[i - halfEdge, j] - imageIn2DTable[i - halfEdge, j + halfEdge]);
                            sum += Math.Abs(imageIn2DTable[i - halfEdge, j + halfEdge] - imageIn2DTable[i, j + halfEdge]);
                            sum += Math.Abs(imageIn2DTable[i, j + halfEdge] - imageIn2DTable[i + halfEdge, j + halfEdge]);
                            sum += Math.Abs(imageIn2DTable[i + halfEdge, j + halfEdge] - imageIn2DTable[i + halfEdge, j]);
                            CN = sum / 2;

                            switch (CN)
                            {
                                case 0:
                                    // Pojedynczy punkt
                                    singlePointsList.Add(new Point(j, i));
                                    drawCharacteristicPoints(new Point(j, i), tmpImageInArray, Img);
                                    break;
                                case 1:
                                    // Zakończenie krawędzi
                                    endsOfEdgeList.Add(new Point(j, i));
                                    drawCharacteristicPoints(new Point(j, i), tmpImageInArray, Img);
                                    break;
                                case 2:
                                    // Kontynuacja krawędzi
                                    break;
                                case 3:
                                    // Rozwidlenie
                                    forksList.Add(new Point(j, i));
                                    drawCharacteristicPoints(new Point(j, i), tmpImageInArray, Img);
                                    break;
                                case 4:
                                    // Skrzyżowanie
                                    intersectionsList.Add(new Point(j, i));
                                    drawCharacteristicPoints(new Point(j, i), tmpImageInArray, Img);
                                    break;
                            }
                            CN = 0;
                            sum = 0;
                        }
                    }
                }
            }

            /// /////////////////////////////////////// 
            /// Miejsce na kod, w tablicy data zajduje sie obraz
            /// znajduje się on w taki sposób że co trzy komórki znajduje się pierwszy odcień (R) danego pixela
            /// to znaczy żeby sie dostać do wartosci R w drugim pikselu trzeba iśc do 3 komórki
            /// ponieważ trzy pierwsze komórki są zajmopwane przez wartości R,G,B pierwszego pixela
            /// żeby zapisać zaminy które wykonał algorytm trzeba zapisać nowe wartości do tablicy data
            /// wtedy kod na dole funkcji wsadzi te dane do obiektu Img i będzie git.
            /// Sprawdz wszystkie Funkcje znajdujące się w Singletonie Functions może ci się przydadzą :)
            /// Może dodatkowo niech funkcja zwraca tablice obiektów typu Point które okreslają w ktorych miejscach te
            /// rozgałęzienia wystepują
            data = Functions.GetInstance().CreateBytetableFrom2DImage(tmpImageInArray, size, bitmapData.Width, bitmapData.Height);

            for (int i = 0; i < size; i += 3)
            {
                if (data[i] == 100)
                {
                    data[i] = 0;
                    data[i + 1] = 0;
                    data[i + 2] = 255;
                }
                else if (data[i] == 0)
                {
                    data[i] = 255;
                    data[i + 1] = 255;
                    data[i + 2] = 255;
                }
                else
                {
                    data[i] = 0;
                    data[i + 1] = 0;
                    data[i + 2] = 0;
                }
            }

            /// Poniżej kod do zamiany obrazu z data do obiektu Bitmap
            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(data, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Img.UnlockBits(bitmapData);

            return Img;
        }

        public int[,] drawCharacteristicPoints(Point coordinates, int[,] table, Bitmap bitmapImage)
        {
            int maskSize = 5;
            int scope = maskSize / 2;
            int xCoordinate = coordinates.X;// koordynaty górnego lewego pixela w oknie 
            int yCoordinate = coordinates.Y;


            if (!(yCoordinate - scope < 0 || yCoordinate + scope >= bitmapImage.Height || xCoordinate - scope < 0 || xCoordinate + scope >= bitmapImage.Width))
            {
                for (int y = yCoordinate - scope; y <= yCoordinate + scope; y++)
                {
                    for (int x = xCoordinate - scope; x <= xCoordinate + scope; x++)
                    {
                        if (!isPixelNeighbour(xCoordinate, yCoordinate, x, y))
                        {
                            table[y, x] = 100;
                        }
                    }
                }
            }
            else
            {
                return table;
            }
            return table;
        }

        public bool isPixelNeighbour(int rootX, int rootY, int inspectedX, int inspectedY)
        {
            if (Math.Abs(rootX - inspectedX) <= 1 && Math.Abs(rootY - inspectedY) <= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
