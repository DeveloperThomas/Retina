using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retina
{
    class Functions
    {

        private static Functions instance = new Functions();

        private Functions()
        {

        }


        public static Functions GetInstance()
        {
            return instance;
        }

        /// <summary>
        /// W przypadku kiedy mamy zbinaryzowany obraz (lub w skali szarości) wszystkie wartości 
        /// danego pixela są jednakowe, więc dany pixel można wyrazić tylko jedną wartością 
        /// (tą która jest aktualnie we wszystkich kolorach w danym pixelu). funkcja zwraca tablice 2D
        /// która jest reprezentacją danego obrazu (piksele od lewej do prawej wierszami). Zwracana tablica posiadan nastepujące indexy:
        /// table [y,x] więc jeżeli chcemy pobrać wartośc pixela, który jest w drugim rzędzie, piąty od
        /// lewej to piszemy tablica[1,4] (numeracja od 0)
        /// </summary>
        /// <param name="data">Tablica obrazu</param>
        /// <returns></returns>  
        /// 
        public int[,] Create2DTable(byte[] data, int imageWidth, int imageHeight)
        {
            int[,] ImageInTable = new int[imageHeight, imageWidth];

            for (int i = 0; i < imageHeight; i++)
            {
                for (int j = 0; j < imageWidth; j++)
                {
                    ImageInTable[i, j] = data[i * (imageWidth * 3) + j * 3];
                }
            }// tablica z wartościami poszczególnych pikseli

            return ImageInTable;
        }

        /// <summary>
        /// Funkcja zwraca histogram stworzony na podstawie wycinka obrazu
        /// </summary>
        /// <param name="area">Obiekt wycinka w którym chcemy policzyć histogram </param>
        /// <param name="allArea">Obraz w tablicy 2D</param>
        /// <returns></returns>
        public int[] CreateHistogramFromrect(System.Drawing.Rectangle area, int[,] allArea)
        {
            int[] result = new int[256];

            for (Int32 y = 0; y < area.Height; ++y)
            {
                for (Int32 x = 0; x < area.Width; ++x)
                {
                    result[allArea[y + area.Y, x + area.X]]++;
                }
            }
            return result;
        }

        /// <summary>
        /// Funkcja zamienia obraz z tablicy 2D na tablice bajtów (którą można od razu wsadzić do obiektu typu Bitmap
        ///  i wyświetlić). Zakładmy również że obraz jest zbinaryzowany lub w skali szarości.
        /// </summary>
        /// <param name="inageIn2DTable">Obraz w tablicy 2D</param>
        /// <param name="dataLength">Wielkość tablicy z danymi, często w funkcjach ta zmienna jest nazywana size</param>
        /// <param name="imageWidth">Szerokośc obrazu</param>
        /// <param name="imageHright">Wysokośc obrazu</param>
        /// <returns></returns>
        public byte[] CreateBytetableFrom2DImage(int[,] inageIn2DTable, int dataLength, int imageWidth, int imageHright)
        {
            byte[] data = new byte[dataLength];

            int firstIndex = 0;
            int secondIndex = 0;

            for (int l = 0; l < data.Length; l += 3)// skakanie po pierwszych wartosciach piksela
            {
                int pixelValue = inageIn2DTable[firstIndex, secondIndex];
                secondIndex++;
                if (secondIndex >= imageWidth)
                {
                    firstIndex++;// nastepny poziom
                    if (firstIndex >= imageHright)
                        break;
                    secondIndex = 0;// poczatek 
                }

                data[l] = (byte)pixelValue;
                data[l + 1] = (byte)pixelValue;
                data[l + 2] = (byte)pixelValue;
            }

            return data;
        }

        /// <summary>
        /// Funkcja pobiera obraz w tablicy 2D i zamienia czarne pixele (wartość 0) na 1, a białe pixele (wartość 255 ) na 0 
        /// jeżlei znajdzie sie jakaś wartośc która jest różna od 0 lub 255 funkcja wyrzuci wyjątek
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public int[,] ChangeBinarizedImage(int[,] image, BitmapData bitmap, Bitmap Img)
        {
            int[,] newImage = new int[image.GetLength(0), image.GetLength(1)];

            for (int k = 0; k < image.GetLength(0); k++)
            {
                for (int l = 0; l < image.GetLength(1); l++)
                {
                    if (image[k, l] == 255)// kiedy jest kolor biały daj wartosc 0
                    {
                        newImage[k, l] = 0;
                    }
                    else if (image[k, l] == 0)//kiedy kolor  jest czarny zapisz 1
                    {
                        newImage[k, l] = 1;
                    }
                    else
                    {
                        Img.UnlockBits(bitmap);
                        throw new Exception("Zbinaryzowany obraz 2D zawiera w sobie wartosc inna niz 255 lub 0. czy na pewno jest zbinaryzowany ?");
                    }
                }
            }
            return newImage;
        }




    }
}
