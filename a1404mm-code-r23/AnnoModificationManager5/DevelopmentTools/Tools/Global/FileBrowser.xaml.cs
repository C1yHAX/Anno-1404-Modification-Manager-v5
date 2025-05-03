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
using RDAExplorer;
using System.Diagnostics;

namespace DevelopmentTools.Tools.Global
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : Window
    {
        public string ChoosenFile;
        private OpenFileDialog OpenFile = new OpenFileDialog() { Multiselect = true };

        public FileBrowser()
        {
            InitializeComponent();
            Load();
        }

        public static void Process_OpenFile(string path)
        {
            RDAFile existing = RDAManagerExtension.GetRDAFileFromPath(path);

            if (existing != null)
            {
                string tempfile = Path.GetTempPath().Trim('\\') +
                "\\DevelopmentTools4\\Temp\\" + existing.FileName.Replace("/", "_").Replace("\\", "_");
                tempfile = FileExtension.MakeFileUnique(tempfile);

                if (!Directory.Exists(Path.GetDirectoryName(tempfile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(tempfile));

                using (FileStream fs = new FileStream(tempfile, FileMode.Create))
                {
                    var data = existing.GetData();
                    fs.Write(data, 0, data.Length);
                }

                Process.Start(tempfile);
            }
        }

        public void Load()
        {
            var collct = new List<string>();

            FileList.Items.Clear();

            if (Properties.Settings.Default.Tools_Misc_FileBrowser_Files == null)
            {
                Properties.Settings.Default.Tools_Misc_FileBrowser_Files = new System.Collections.Specialized.StringCollection();
                Properties.Settings.Default.Save();
            }

            collct.AddRange(Properties.Settings.Default.Tools_Misc_FileBrowser_Files.OfType<string>());

            if (Project.Development_CurrentProject != null)
            {
                collct.AddRange(Project.Development_CurrentProject.Modification.CollectedFiles_Xml_List);
                collct.AddRange(Project.Development_CurrentProject.Modification.CollectedFiles_List_List);
            }

            collct = collct.Distinct().ToList();

            foreach (var it in collct)
            {
                FileList.Items.Add(new ListViewItem() { Content = it });
            }
        }

        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItem != null)
            {
                var torem = ((ListViewItem)FileList.SelectedItem).Content.ToString();
                FileList.Items.Remove(FileList.SelectedItem);

                Properties.Settings.Default.Tools_Misc_FileBrowser_Files.Remove(torem);
                Properties.Settings.Default.Save();
            }
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItem != null && FileList.SelectedItem is ListViewItem)
            {
                ChoosenFile = (FileList.SelectedItem as ListViewItem).Content.ToString();
                DialogResult = true;
            }
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void button_temporary_Click(object sender, RoutedEventArgs e)
        {
            OpenFile.Multiselect = false;
            if (OpenFile.ShowDialog() == true)
            {
                ChoosenFile = OpenFile.FileName;
                DialogResult = true;
            }
            OpenFile.Multiselect = true;
        }

        private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            button_ok_Click(sender, new RoutedEventArgs());
        }

        private void b_Add_RDAFile_Click(object sender, RoutedEventArgs e)
        {
            RDAFileBrowser dlg = new RDAFileBrowser();

            if (dlg.ShowDialog() == true)
            {
                foreach (var selfile in dlg.SelectedRDAFiles)
                {
                    string path = dlg.SelectedRDAContainer + ";" + selfile.FileName.Replace("/", "\\").Trim('\\');

                    if (FileList.Items.OfType<ListViewItem>().ToList().Find(l => l.Content.ToString() == path) == null)
                    {
                        FileList.Items.Add(new ListViewItem() { Content = path, ToolTip = "File in RDA" });
                    }

                    if (!Properties.Settings.Default.Tools_Misc_FileBrowser_Files.Contains(path))
                        Properties.Settings.Default.Tools_Misc_FileBrowser_Files.Add(path);

                }

                Properties.Settings.Default.Save();
            }
        }

        private void b_Add_File_Click(object sender, RoutedEventArgs e)
        {
            if (OpenFile.ShowDialog() == true)
            {
                foreach (string file in OpenFile.FileNames)
                {
                    if (FileList.Items.OfType<ListViewItem>().ToList().Find(l => l.Content.ToString() == file) == null)
                    {
                        FileList.Items.Add(new ListViewItem() { Content = file, ToolTip = "File" });
                    }
                }
                Load();
            }
        }

        private void drp_tmp_RDAFile_Click(object sender, RoutedEventArgs e)
        {
            RDAFileBrowser dlg = new RDAFileBrowser();

            if (dlg.ShowDialog() == true)
            {
                if (dlg.SelectedRDAFiles.Count != 0)
                {
                    var selfile = dlg.SelectedRDAFiles[0];
                    string path = dlg.SelectedRDAContainer + ";" + selfile.FileName.Replace("/", "\\").Trim('\\');

                    ChoosenFile = path;
                    DialogResult = true;
                }
            }
        }

        private void drp_tmp_File_Click(object sender, RoutedEventArgs e)
        {
            OpenFile.Multiselect = false;
            if (OpenFile.ShowDialog() == true)
            {
                ChoosenFile = OpenFile.FileName;
                DialogResult = true;
            }
            OpenFile.Multiselect = true;
        }
    }
}
