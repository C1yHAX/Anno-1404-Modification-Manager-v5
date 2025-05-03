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
using System.IO;
using System.Diagnostics;
using DevelopmentTools.Controls;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.Misc;
using fs = Microsoft.VisualBasic.FileIO;

namespace DevelopmentTools.Editors.FileSystem
{
    /// <summary>
    /// Interaction logic for FileSystemEditor_Main.xaml
    /// </summary>
    public partial class FileSystemEditor_Main : UserControl
    {
        public string RootFolder;
        public string CurrentSubFolder;

        public string CurrentFolder
        {
            get
            {
                return RootFolder + "\\" + CurrentSubFolder;
            }
        }

        public FileSystemEditor_Main()
        {
            InitializeComponent();
        }

        public void Refresh()
        {
            FileList.Items.Clear();
            Folder_Toolbar_SubFolders.Children.Clear();

            if (!Directory.Exists(CurrentFolder))
                Directory.CreateDirectory(CurrentFolder);

            foreach (string f in Directory.GetDirectories(CurrentFolder, "*", SearchOption.TopDirectoryOnly))
            {
                FileList.Items.Add(new FileItem() { ItemPath = f, IsFolder = true });
            }
            foreach (string f in Directory.GetFiles(CurrentFolder, "*", SearchOption.TopDirectoryOnly))
            {
                FileList.Items.Add(new FileItem() { ItemPath = f, IsFolder = false });
            }

            //if (!string.IsNullOrEmpty(CurrentSubFolder))
            {
                #region Root Item
                ContentButton rootitem = new ContentButton();
                rootitem.Style = Resources["ButtonStyle_SubFolder"] as Style;
                rootitem.Click += new RoutedEventHandler(Folder_Toolbar_SubFolders_Button_Click);
                rootitem.Binding = "";
                rootitem.Content = new TextBlock() { Text = "Root" };

                Folder_Toolbar_SubFolders.Children.Add(rootitem);
                #endregion

                if (!string.IsNullOrEmpty(CurrentSubFolder))
                {
                    string csub = "";
                    foreach (string t in CurrentSubFolder.Split('\\'))
                    {
                        if (!string.IsNullOrEmpty(t))
                        {
                            csub += "\\" + t;
                            csub = csub.Trim('\\');

                            ContentButton item = new ContentButton();
                            item.Style = Resources["ButtonStyle_SubFolder"] as Style;
                            item.Click += new RoutedEventHandler(Folder_Toolbar_SubFolders_Button_Click);
                            item.Content = new TextBlock() { Text = t };
                            item.Binding = csub;

                            Folder_Toolbar_SubFolders.Children.Add(item);
                        }
                    }
                }

                (Folder_Toolbar_SubFolders.Children[Folder_Toolbar_SubFolders.Children.Count - 1] as ContentButton).FontWeight = FontWeights.Bold;
            }

            FileList_GridViewColumn_FileName.Width = 0;
            FileList_GridViewColumn_FileName.Width = double.NaN;           
        }

        void Folder_Toolbar_SubFolders_Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentSubFolder = (sender as ContentButton).Binding.ToString();
            Refresh();
        }

        private void Folder_OpenWindowsExplorer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(CurrentFolder);
        }

        private void Folder_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileList.SelectedItem != null)
            {
                FileItem itm = FileList.SelectedItem as FileItem;

                if (!itm.IsFolder)
                {
                    Process.Start(itm.ItemPath);
                }
                else
                {
                    CurrentSubFolder += "\\" + itm.Name;
                    CurrentSubFolder = CurrentSubFolder.Trim('\\');

                    Refresh();
                }
            }
        }

        private void Folder_NewFolder_Click(object sender, RoutedEventArgs e)
        {
            string name = MessageWindow.GetText("Folder Name:", "New Folder");

            if (name != null)
            {
                foreach (char c in Path.GetInvalidPathChars())
                {
                    name = name.Replace(c.ToString(), "\0");
                }
                name = name.Replace("\r", "").Replace("\n", "");

                if (!Directory.Exists(CurrentFolder + "\\" + name))
                {
                    Directory.CreateDirectory(CurrentFolder + "\\" + name);
                    Refresh();
                }
            }
        }

        private void FileList_ContextMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            FileList_MouseDoubleClick(null, null);
        }

        private void FileList_ContextMenu_Paste_Click(object sender, RoutedEventArgs e)
        {
            List<string> obj = Clipboard.GetFileDropList().OfType<string>().ToList();
            foreach (string ff in obj)
            {
                try
                {
                    if (Directory.Exists(ff))
                    {
                        DirectoryExtension.copyDirectory(ff, CurrentFolder + "\\" + Path.GetFileName(ff));
                    }
                    else
                    {
                        File.Copy(ff, CurrentFolder + "\\" + Path.GetFileName(ff));
                    }
                }
                catch (Exception ex)
                {
                   MessageWindow.Show("Error with File/Folder \"" + Path.GetFileName(ff) + "\":\n" + ex.Message);
                }      
            }
            Refresh();
        }

        private void FileList_ContextMenu_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItems.Count != 0)
            {
                System.Collections.Specialized.StringCollection coll = new System.Collections.Specialized.StringCollection();

                foreach (FileItem itm in FileList.SelectedItems)
                {
                    coll.Add(itm.ItemPath);
                }

                Clipboard.SetFileDropList(coll);
            }
        }

        private void FileList_ContextMenu_Cut_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItem != null)
            {
                FileItem itm = FileList.SelectedItem as FileItem;


            }
        }

        private void FileList_ContextMenu_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItems.Count != 0)
            {
                if (MessageWindow.Show("Do you really want to delete these files/folders?", MessageWindow.MessageWindowType.YesNo) == MessageBoxResult.Yes)
                {
                    foreach (FileItem itm in FileList.SelectedItems)
                    {
                        try
                        {
                            if (Directory.Exists(itm.ItemPath))
                            {
                                fs.FileSystem.DeleteDirectory(itm.ItemPath, fs.UIOption.OnlyErrorDialogs, fs.RecycleOption.SendToRecycleBin, fs.UICancelOption.DoNothing);
                            }
                            else
                            {
                                fs.FileSystem.DeleteFile(itm.ItemPath, fs.UIOption.OnlyErrorDialogs, fs.RecycleOption.SendToRecycleBin);
                            }
                        }
                        catch (Exception ex)
                        {
                           MessageWindow.Show("Error with File/Folder \"" + Path.GetFileName(itm.ItemPath) + "\":\n" + ex.Message);
                        }
                    }
                    Refresh();
                }
            }
        }
    }
}
