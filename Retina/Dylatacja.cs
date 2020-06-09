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
    class Dylatacja
    {
        private static Dylatacja instance = new Dylatacja();

        private Dylatacja()
        {

        }

        public static Dylatacja GetInstance()
        {
            return instance;
        }

        public Bitmap Dilation(Bitmap Img)
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

            int maskSize = 5;
            int halfEdge = maskSize / 2;
            bool changePixelFlag = false;
            int maxCoordsDifference;


            for (int i = 0; i < Img.Height; i++)
            {
                for (int j = 0; j < Img.Width; j++)
                {
                    changePixelFlag = false;
                    if (!(i - halfEdge < 0 || i + halfEdge >= Img.Height || j - halfEdge < 0 || j + halfEdge >= Img.Width))
                    {
                        maxCoordsDifference = Math.Abs(i + j) + 2;
                        for (int k = i - halfEdge; k < i + halfEdge; k++)
                        {
                            for (int l = j - halfEdge; l < j + halfEdge; l++)
                            {
                                if (imageIn2DTable[k, l] == 1)
                                {
                                    changePixelFlag = true;
                                }
                            }
                            if (changePixelFlag)
                            {
                                break;
                            }
                        }
                        if (changePixelFlag)
                        {
                            tmpImageInArray[i, j] = 1;
                        }
                    }
                }
            }

            //for (int i = 0; i < Img.Height; i++)
            //{
            //    for (int j = 0; j < Img.Width; j++)
            //    {
            //        changePixelFlag = false;
            //        if (!(i - halfEdge < 0 || i + halfEdge >= Img.Height || j - halfEdge < 0 || j + halfEdge >= Img.Width))
            //        {
            //            maxCoordsDifference = Math.Abs(i + j) + 2;
            //            for (int k = i - halfEdge; k < i + halfEdge; k++)
            //            {
            //                for (int l = j - halfEdge; l < j + halfEdge; l++)
            //                {
            //                    //if (Math.Abs(k + l) <= maxCoordsDifference)
            //                    //{
            //                        if (imageIn2DTable[k, l] == 0)
            //                        {
            //                            changePixelFlag = true;
            //                        }
            //                   // }

            //                }
            //                if (changePixelFlag)
            //                {
            //                    break;
            //                }
            //            }
            //            if (changePixelFlag)
            //            {
            //                tmpImageInArray[i, j] = 0;
            //            }
            //        }
            //    }
            //}

            data = Functions.GetInstance().CreateBytetableFrom2DImage(tmpImageInArray, size, bitmapData.Width, bitmapData.Height);

            for (int i = 0; i < size; i++)
            {
                if (data[i] != 0)
                {
                    data[i] = 0;
                }
                else
                {
                    data[i] = 255;
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


    }


}
