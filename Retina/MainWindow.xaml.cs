using Microsoft.Win32;
using Retina.ExtraWindows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Retina
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog imageDialog = new OpenFileDialog();
        SaveFileDialog imgSaveDialog = new SaveFileDialog();

        TheresholdInput inputwindow;

        Bitmap Img;
        List<System.Drawing.Point> singlePointsList = new List<System.Drawing.Point>();
        List<System.Drawing.Point> endsOfEdgeList = new List<System.Drawing.Point>();
        List<System.Drawing.Point> forksList = new List<System.Drawing.Point>();
        List<System.Drawing.Point> intersectionsList = new List<System.Drawing.Point>();

        Bitmap ZapisanyRozmytyObraz;
        public MainWindow()
        {
            InitializeComponent();

            imgSaveDialog.Filter = "Png image(*.png)|*.png|Jpeg image(*.jpeg)|*.jpeg|Bmp image(*.bmp)|*.bmp|gif image(*.gif)|*.gif|Tiff image(*.tiff)|*.tiff";
            imgSaveDialog.Title = "Save image as...";

            imageDialog.Title = "Select a picture";
            imageDialog.Filter = "Image files (.jpg,png,bmp,gif,tiff,tif)|*.tiff;*.jpg;*.png;*.gif;*.tif";

        }


        /// <summary>
        /// Funkcja aktualizuje obraz na ekranie, warunkiem jest to że nowy obraz, który ma zostac wyświetlony musi być
        /// zapisany do obiektu Img typu Bitmap 
        /// </summary>
        public void UpdateImageOnScreen()
        {

            using (MemoryStream memory = new MemoryStream())
            {
                Img.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                GC.KeepAlive(memory);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                modifiedImage.Source = bitmapimage;
            }
        }

        public void SwitchImageOnScreen(Bitmap tmpImage)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                Img.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                GC.KeepAlive(memory);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                modifiedImage.Source = bitmapimage;
            }
            Img = tmpImage;
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // zapisywanie obrazu 
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            imgSaveDialog.FileName = "";
            if (imgSaveDialog.ShowDialog() == true)
            {
                System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                string extension = System.IO.Path.GetExtension(imgSaveDialog.FileName);
                switch (extension)
                {
                    case ".jpeg":
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case ".gif":
                        format = System.Drawing.Imaging.ImageFormat.Gif;
                        break;
                    case ".tiff":
                        format = System.Drawing.Imaging.ImageFormat.Tiff;
                        break;
                    case ".png":
                        format = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                    case ".bmp":
                        format = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;
                }

                Img.Save(imgSaveDialog.FileName, format);
            }
        }

        // wczytywanie obrazu 
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            save.IsEnabled = true;
            if (imageDialog.ShowDialog() == true)
            {
                Img = new Bitmap(imageDialog.FileName);// creating img for workshop

                MemoryStream ms = new MemoryStream();
                Img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                Img = new Bitmap(ms);
                using (MemoryStream memory = new MemoryStream())
                {
                    Img.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    GC.KeepAlive(memory);
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    loadedImage.Source = bitmapimage;
                }

                UpdateImageOnScreen();// set workshio displaying image
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double newUserZoomValue = e.NewValue;
            ScaleTransform scaleTransform = new ScaleTransform(newUserZoomValue, newUserZoomValue);
            modifiedImage.LayoutTransform = scaleTransform;
        }


        private void Otsu_Click(object sender, RoutedEventArgs e)
        {
            var image = Binarization.GetInstance().OtsuMethod(Img, true);/// to zmienić na true żeby zamienić kolory na przeciwne
            Img = image;
            UpdateImageOnScreen();
        }

        private void TheresholdBin_Click(object sender, RoutedEventArgs e)
        {
            inputwindow = new TheresholdInput();
            if (inputwindow.ShowDialog() == true)
            {
                int value = inputwindow.result;
                var tmp = Binarization.GetInstance().BinarizeThereshol(Img, value, true);
                Img = tmp;
                UpdateImageOnScreen();
            }
        }

        private void GrayScale_Click(object sender, RoutedEventArgs e)
        {
            var tmp = GrayScale.GetInstance().TurnIntoGrayWithGreen(Img);
            Img = tmp;
            UpdateImageOnScreen();
        }

        private void Szkieletyzacja_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
                var tmp = SzkieletyzacjaK3M.GetInstance().UseK3M(Img);
                Img = tmp;
                MessageBox.Show("Operacja zakończona.");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);

            //}

            UpdateImageOnScreen();
        }

        private void Rozgalezienia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tmpImage = Img.Clone(new System.Drawing.Rectangle(0, 0, Img.Width, Img.Height), Img.PixelFormat);
                var tmp = Rozgalezienia.GetInstance().Find(Img);
                Img = tmp;
                this.singlePointsList = Rozgalezienia.GetInstance().getSinglePointsList();
                this.endsOfEdgeList = Rozgalezienia.GetInstance().getEndsOfEdgeList();
                this.forksList = Rozgalezienia.GetInstance().getForksList();
                this.intersectionsList = Rozgalezienia.GetInstance().getIntersectionsList();
                MessageBox.Show("Operacja zakończona.");
                SwitchImageOnScreen(tmpImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Usuwanie_minucji_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UsuniecieMinucji.GetInstance().SetSinglePointsList(this.singlePointsList);
                UsuniecieMinucji.GetInstance().SetEndsOfEdgeList(this.endsOfEdgeList);
                UsuniecieMinucji.GetInstance().SetForksList(this.forksList);
                UsuniecieMinucji.GetInstance().SetIntersectionsList(this.intersectionsList);
                var tmp = UsuniecieMinucji.GetInstance().UsunMinucje(Img);
                Img = tmp;
                MessageBox.Show("Operacja zakończona.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            UpdateImageOnScreen();
        }

        private void Rozmycie_Click(object sender, RoutedEventArgs e)
        {
            ZapisanyRozmytyObraz = RozmycieObrazu.GetInstance().RozmyjObraz(Img);
            MessageBox.Show("Rozmycie obrazu zakonczone i zapisane w zmiennej");
            UpdateImageOnScreen();

        }

        private void Odjecie_Click(object sender, RoutedEventArgs e)
        {
            var tmp =  OdjecieObrazu.GetInstance().OdjecieRozmytegoOdOryginalu(Img, ZapisanyRozmytyObraz);
            if (tmp == null)
            {
                MessageBox.Show("Któryś z obrazów jest nullem!!");
                return;
            }
            Img = tmp;
            MessageBox.Show("Odjęcie wykonane!");
            UpdateImageOnScreen();
        }

        private void Zamkniecie_morfologiczne_Click(object sender, RoutedEventArgs e)
        {
            var tmp = Dylatacja.GetInstance().Dilation(Img);
            Img = tmp;
            UpdateImageOnScreen();
        }

        private void Medianowy_Click(object sender, RoutedEventArgs e)
        {
            var tmp = FiltrMedianowy.GetInstance().PrzeprowadzFiltrMedianowy(Img);
            Img = tmp;
            UpdateImageOnScreen();
        }

        //private void Uruchom_proces_Click(object sender, RoutedEventArgs e)
        //{
        //    ZapisanyRozmytyObraz = RozmycieObrazu.GetInstance().RozmyjObraz(Img);
        //    var tmp = OdjecieObrazu.GetInstance().OdjecieRozmytegoOdOryginalu(Img, ZapisanyRozmytyObraz);
        //    Img = tmp;
        //    var image = Binarization.GetInstance().OtsuMethod(Img, true);/// to zmienić na true żeby zamienić kolory na przeciwne
        //    Img = image;
        //    tmp = FiltrMedianowy.GetInstance().PrzeprowadzFiltrMedianowy(Img);
        //    Img = tmp;
        //    tmp = Dylatacja.GetInstance().Dilation(Img);
        //    Img = tmp;
        //    tmp = SzkieletyzacjaK3M.GetInstance().UseK3M(Img);
        //    Img = tmp;
        //}
    }
}
