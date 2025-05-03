using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AnnoModificationManager4.ModificationTypes;
using System.IO;
using AnnoModificationManager4.Misc;
using System.Diagnostics;

namespace AnnoModificationManager4.Controls
{
    /// <summary>
    /// Interaction logic for ImagePresenter.xaml
    /// </summary>
    public partial class ImagePresenter : UserControl
    {
        private Dictionary<Button, ImageSource> imageDictionary = new Dictionary<Button, ImageSource>();

        public ImagePresenter()
        {
            InitializeComponent();
        }

        public void LoadImages(Modification mod)
        {
            ImageList.Children.Clear();
            imageDictionary.Clear();
            ImageControl.Source = null;

            foreach (string file in mod.Info.Images)
            {
                AddImage(mod.Folder + "\\Images\\" + file);              
            }

            if (ImageList.Children.Count == 0)
            {
                AddImage("pack://application:,,,/Images/AlternativePreview.jpg");
            }

            if (ImageList.Children.Count != 0)
                ImageControl.Source = imageDictionary[ImageList.Children[0] as Button];
        }

        private void AddImage(string file)
        {
            Button button = new Button();
            button.Width = 40;
            button.Height = 40;
            button.Click += new RoutedEventHandler(button_Click);
            button.Style = App.Current.Resources["ButtonStyle_Button_NoBorder"] as Style;
            button.BorderBrush = new SolidColorBrush(new Color()
            {
                A = 128,
                R = 100,
                G = 100,
                B = 100
            });
            button.Background = new SolidColorBrush(new Color()
            {
                A = 50,
                R = 255,
                G = 255,
                B = 255
            });
            button.Margin = new Thickness(3, 0, 0, 0);

            Image img = new Image();
            img.Source = BitmapImageExtension.Load(file);
            img.Stretch = Stretch.UniformToFill;
            img.Width = 30;
            img.Height = 30;

            button.Content = img;

            ImageList.Children.Add(button);
            imageDictionary.Add(button, img.Source);
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            ImageControl.Source = imageDictionary[sender as Button];
        }

        private void button_ZoomImage_Click(object sender, RoutedEventArgs e)
        {
            string imagefile = Path.GetTempPath().Trim('\\') + "\\amm4tempimage.jpg";
            imagefile = FileExtension.MakeFileUnique(imagefile);           

            using (FileStream stream = new FileStream(imagefile, FileMode.Create))
            {
                JpegBitmapEncoder dec = new JpegBitmapEncoder();
                dec.Frames.Add(BitmapFrame.Create(ImageControl.Source as BitmapSource));

                dec.Save(stream);
            }

            Process.Start(imagefile);
        }
    }
}
