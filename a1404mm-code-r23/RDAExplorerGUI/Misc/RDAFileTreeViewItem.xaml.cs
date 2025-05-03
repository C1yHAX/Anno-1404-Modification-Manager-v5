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
using ammctrls = AnnoModificationManager5.Controls;
using RDAExplorer;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.UserInterface.Misc;
using System.IO;
using System.Diagnostics;
using RDAExplorerGUI.Controls;

namespace RDAExplorerGUI.Misc
{
    /// <summary>
    /// Interaction logic for RDAFileTreeViewItem.xaml
    /// </summary>
    public partial class RDAFileTreeViewItem : ModifiedTreeViewItem
    {
        public RDAFile File;

        public RDAFileTreeViewItem()
        {
            InitializeComponent();

            SelectOnRightClick = true;
        }

        private void context_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!System.IO.File.Exists(DirectoryExtension.GetTempWorkingDirectory() + "\\" + File.FileName))
                {
                    File.ExtractToRoot(DirectoryExtension.GetTempWorkingDirectory());
                }
                Process.Start(DirectoryExtension.GetTempWorkingDirectory() + "\\" + File.FileName);

                MainWindow.CurrentMainWindow.FileWatcher.Changed += new FileSystemEventHandler(FileWatcher_Changed);
                MainWindow.CurrentMainWindow.FileWatcher.Deleted += new FileSystemEventHandler(FileWatcher_Deleted);
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
            }
        }

        void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            MainWindow.CurrentMainWindow.FileWatcher.Changed -= new FileSystemEventHandler(FileWatcher_Changed);
            MainWindow.CurrentMainWindow.FileWatcher.Deleted -= new FileSystemEventHandler(FileWatcher_Deleted);
        }

        void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string relativeFile = e.FullPath.Replace(DirectoryExtension.GetTempWorkingDirectory(), "").Trim('\\');

            if (relativeFile == File.FileName.Replace("/", "\\"))
            {
                if (!MainWindow.CurrentMainWindow.FileWatcher_ToUpdate.Contains(File))
                    MainWindow.CurrentMainWindow.FileWatcher_ToUpdate.Add(File);
            }
        }

        private void context_Extract_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = Path.GetFileName(File.FileName);

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    File.Extract(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageWindow.Show(ex.Message);
                }
            }
        }

        private void context_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is RDAFolderTreeViewItem)
            {
                RDAFolderTreeViewItem foldertv = Parent as RDAFolderTreeViewItem;
                foldertv.Folder.Files.Remove(File);
                foldertv.UpdateSubItems();
            }
            else
            {
                MainWindow.CurrentMainWindow.CurrentReader.rdaFolder.Files.Remove(File);
                MainWindow.CurrentMainWindow.RebuildTreeView();
            }
        }

        private void ModifiedTreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            context_Open_Click(null, null); //View
        }
    }
}
