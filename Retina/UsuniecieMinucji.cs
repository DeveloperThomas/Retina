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
    class UsuniecieMinucji
    {
        private static UsuniecieMinucji instance = new UsuniecieMinucji();
        List<Point> singlePointsList = new List<Point>();
        List<Point> endsOfEdgeList = new List<Point>();
        List<Point> forksList = new List<Point>();
        List<Point> intersectionsList = new List<Point>();

        private UsuniecieMinucji()
        {

        }

        public static UsuniecieMinucji GetInstance()
        {
            return instance;
        }

        public void SetSinglePointsList(List<Point> singlePointsList)
        {
            this.singlePointsList = singlePointsList;
        }

        public void SetEndsOfEdgeList(List<Point> endsOfEdgeList)
        {
            this.endsOfEdgeList = endsOfEdgeList;
        }

        public void SetForksList(List<Point> forksList)
        {
            this.forksList = forksList;
        }

        public void SetIntersectionsList(List<Point> intersectionsList)
        {
            this.intersectionsList = intersectionsList;
        }

        public Bitmap UsunMinucje(Bitmap Img)
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

            tmpImageInArray = this.ClearFalseMinutiaeOfOneType(singlePointsList, tmpImageInArray);
            tmpImageInArray = this.ClearFalseMinutiaeOfOneType(endsOfEdgeList, tmpImageInArray);
            tmpImageInArray = this.ClearFalseMinutiaeOfOneType(forksList, tmpImageInArray);
            tmpImageInArray = this.ClearFalseMinutiaeOfOneType(intersectionsList, tmpImageInArray);

            imageIn2DTable = (int[,])tmpImageInArray.Clone();

            singlePointsList.Clear();
            endsOfEdgeList.Clear();
            forksList.Clear();
            intersectionsList.Clear();

            int halfEdge = 1;
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


            for (int y = 0; y < bitmapData.Height; y++)
            {
                IntPtr mem = (IntPtr)((long)bitmapData.Scan0 + y * bitmapData.Stride);
                Marshal.Copy(data, y * bitmapData.Width * 3, mem, bitmapData.Width * 3);// skopiuj całą bitmapę do tablicy
            }

            Img.UnlockBits(bitmapData);

            return Img;
        }

        public Point IsInMask(Point parent, Point child)
        {
            if (Math.Abs(parent.X - child.X) <= 3 && Math.Abs(parent.Y - child.Y) <= 3)
            {
                if ((Math.Abs(parent.X - child.X) == 0 && Math.Abs(parent.Y - child.Y) == 0))
                {
                    return new Point(-1, -1);
                }
                else
                {
                    return new Point(child.X, child.Y);
                }

            }
            else
            {
                return new Point(-1, -1);
            }
        }

        public int[,] ClearFalseMinutiaeOfOneType(List<Point> list, int[,] imageIn2DTable)
        {
            int counter = 0;
            int overallCounter = 0;
            List<Point> pointsToDelete = new List<Point>();

            foreach (var parentSinglePoint in list)
            {
                counter = 0;
                foreach (var childSinglePoint in list)
                {
                    var investigatedPoint = this.IsInMask(parentSinglePoint, childSinglePoint);
                    if (investigatedPoint.X == -1)
                    {
                        continue;
                    }
                    else
                    {
                        pointsToDelete.Add(investigatedPoint);
                        counter++;
                        overallCounter++;
                    }
                }
                if (pointsToDelete.Count < 2)
                {
                    continue;
                }
                else
                {
                    // usun
                    imageIn2DTable[parentSinglePoint.Y, parentSinglePoint.X] = 0;
                    foreach (var pointToDelete in pointsToDelete)
                    {
                        imageIn2DTable[pointToDelete.Y, pointToDelete.X] = 0;
                    }

                }
                pointsToDelete.Clear();
            }
            return imageIn2DTable;
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
