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
using System.IO;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.ModificationTypes;
using AnnoModificationManager5.Components;


namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : UserControl
    {
        private OpenFileDialog RecentFiles_OpenFileDialog = new OpenFileDialog()
        {
            Multiselect = true
        };

        /// <summary>View that was shown before navigating here; restored on "Zurück".</summary>
        public object PreviousContent { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(SettingsWindow_Loaded);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (PreviousContent != null)
                MainWindow.CurrentMainWindow.Content = PreviousContent;
            else
                MainWindow.CurrentMainWindow.MainWindow_Loaded(null, null);
        }

        void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            //RDA W Dir
            txb_RDAWorkingDir.Text = Properties.Settings.Default.RDAWorkingDir;

            //Recent Files
            foreach (string file in Properties.Settings.Default.StartPage_RecentFiles.Split(';'))
            {
                if (!string.IsNullOrEmpty(file))
                {
                    ListViewItem item = new ListViewItem();
                    item.Content = file;

                    RecentFiles_Files.Items.Add(item);
                }
            }
        }

        #region Recent Files
        private void UpdateRecentFiles()
        {
            string p = "";
            foreach (ListViewItem item in RecentFiles_Files.Items)
            {
                p += item.Content + ";";
            }

            Properties.Settings.Default.StartPage_RecentFiles = p;
            Properties.Settings.Default.Save();
        }

        private void RecentFiles_Files_Add_Click(object sender, RoutedEventArgs e)
        {
            if (RecentFiles_OpenFileDialog.ShowDialog() == true)
            {
                foreach (string f in RecentFiles_OpenFileDialog.FileNames)
                {
                    ListViewItem item = new ListViewItem();
                    item.Content = f;

                    RecentFiles_Files.Items.Add(item);
                }
                UpdateRecentFiles();
            }
        }

        private void RecentFiles_Files_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (RecentFiles_Files.SelectedItem != null)
            {
                RecentFiles_Files.Items.Remove(RecentFiles_Files.SelectedItem);

                if (RecentFiles_Files.Items.Count != 0)
                    RecentFiles_Files.SelectedIndex = 0;

                UpdateRecentFiles();
            }
        }
        #endregion

        private void b_SetRDAWorkingDir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            string msg;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!BackupHandler.IsValid(dlg.SelectedPath, out msg))
                {
                    MessageBox.Show(msg);
                    return;
                }

                Properties.Settings.Default.RDAWorkingDir = dlg.SelectedPath;
                Properties.Settings.Default.Save();

                txb_RDAWorkingDir.Text = Properties.Settings.Default.RDAWorkingDir;
                Modification.Development_RDADirectory = Properties.Settings.Default.RDAWorkingDir;
            }
        }

        public static bool CheckRDAWorkingDir()
        {
            string currdir = Properties.Settings.Default.RDAWorkingDir;
            string msg;

            if (!BackupHandler.IsValid(currdir, out msg))
            {
                MessageBox.Show("You have to set the RDA working directory before continuing.");

                // Prompt directly for the directory instead of opening a separate
                // settings window (settings is now an embedded in-app view).
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                dlg.Description = "Select the RDA working directory (Anno 'maindata' folder).";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (BackupHandler.IsValid(dlg.SelectedPath, out msg))
                    {
                        Properties.Settings.Default.RDAWorkingDir = dlg.SelectedPath;
                        Properties.Settings.Default.Save();
                        Modification.Development_RDADirectory = dlg.SelectedPath;
                    }
                }

                currdir = Properties.Settings.Default.RDAWorkingDir;
                if (!BackupHandler.IsValid(currdir, out msg))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
