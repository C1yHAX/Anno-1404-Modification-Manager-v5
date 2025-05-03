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
using wf = System.Windows.Forms;
using AnnoModificationManager5.Misc;
using System.IO;

namespace AnnoModificationManager5.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for ChangeDataFolderDialog.xaml
    /// </summary>
    public partial class ChangeDataFolderDialog : Window
    {
        private wf.FolderBrowserDialog openDataFolder = new wf.FolderBrowserDialog();

        public ChangeDataFolderDialog()
        {
            InitializeComponent();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            string olddata = DirectoryExtension.GetAMM4ApplicationDataFolder();

            if (radio_DataDirectory_Choose.IsChecked == true)
                Properties.Settings.Default.OverwrittenDataFolder = field_DataDirectory.Text;
            else
                Properties.Settings.Default.OverwrittenDataFolder = "";
            Properties.Settings.Default.Save();

            if (Directory.Exists(DirectoryExtension.GetAMM4ApplicationDataFolder()))
                Directory.Delete(DirectoryExtension.GetAMM4ApplicationDataFolder(), true);
            DirectoryExtension.copyDirectory(olddata, DirectoryExtension.GetAMM4ApplicationDataFolder());

            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Data Directory
            field_DataDirectory.Text = DirectoryExtension.GetAMM4ApplicationDataFolder();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.OverwrittenDataFolder))
            {
                radio_DataDirectory_Choose.IsChecked = true;
            }
            else
            {
                radio_DataDirectory_Auto.IsChecked = true;
            }
            #endregion

            radio_DataDirectory_Auto.Checked += new RoutedEventHandler(radio_DataDirectory_Auto_Checked);
            radio_DataDirectory_Choose.Checked += new RoutedEventHandler(radio_DataDirectory_Choose_Checked);
        }

        #region DataDirectory
        void radio_DataDirectory_Choose_Checked(object sender, RoutedEventArgs e)
        {
            if (openDataFolder.ShowDialog() == wf.DialogResult.OK)
            {
                field_DataDirectory.Text = openDataFolder.SelectedPath;
            }
            else
                radio_DataDirectory_Auto.IsChecked = true;
        }

        void radio_DataDirectory_Auto_Checked(object sender, RoutedEventArgs e)
        {
            field_DataDirectory.Text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).
                Trim('\\') + "\\AnnoModificationManager5";
        }
        #endregion
    }
}
