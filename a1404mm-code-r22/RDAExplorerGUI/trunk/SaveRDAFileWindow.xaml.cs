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
using System.Windows.Shapes;
using RDAExplorer;

namespace RDAExplorerGUI
{
    /// <summary>
    /// Interaction logic for SaveRDAFileWindow.xaml
    /// </summary>
    public partial class SaveRDAFileWindow : Window
    {
        public RDAFolder Folder;

        public SaveRDAFileWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string ext in Folder.GetAllExtensions())
            {
                CheckBox check = new CheckBox();
                check.Content = ext;

                if (RDABlockCreator.FileType_CompressedExtensions.Contains(ext))
                    check.IsChecked = true;

                compressedTypesPanel.Children.Add(check);
            }
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            //Set Values in RDABlockCreator
            foreach (CheckBox check in compressedTypesPanel.Children)
            {
                string ext = check.Content.ToString();

                if (check.IsChecked == true)
                {
                    if (!RDABlockCreator.FileType_CompressedExtensions.Contains(ext))
                        RDABlockCreator.FileType_CompressedExtensions.Add(ext);
                }
                else
                {
                    if (RDABlockCreator.FileType_CompressedExtensions.Contains(ext))
                        RDABlockCreator.FileType_CompressedExtensions.Remove(ext);
                }
            }

            DialogResult = true;
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
