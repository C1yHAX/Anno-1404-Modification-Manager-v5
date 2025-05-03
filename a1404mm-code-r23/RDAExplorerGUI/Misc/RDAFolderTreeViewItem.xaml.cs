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
using RDAExplorer;
using ammctrl = AnnoModificationManager5.Controls;
using ammmisc = AnnoModificationManager5.Misc;
using RDAExplorerGUI.Controls;
using wf = System.Windows.Forms;
using System.ComponentModel;
using AnnoModificationManager5.UserInterface.Misc;
using System.IO;
using RDAExplorer.Misc;

namespace RDAExplorerGUI.Misc
{
    /// <summary>
    /// Interaction logic for RDAFolderTreeViewItem.xaml
    /// </summary>
    public partial class RDAFolderTreeViewItem : ModifiedTreeViewItem
    {
        public RDAFolder Folder;
        private bool AlreadyExpanded = false;

        public RDAFolderTreeViewItem()
        {
            InitializeComponent();

            SelectOnRightClick = true;

            Expanded += new System.Windows.RoutedEventHandler(RDAFolderTreeViewItem_Expanded);
            Items.Add(new ModifiedTreeViewItem());
        }

        public void UpdateSubItems()
        {
            Items.Clear();

            foreach (RDAFolder subf in Folder.Folders.OrderBy(f => f.Name))
            {
                RDAFolderTreeViewItem tvfolder = new RDAFolderTreeViewItem();
                tvfolder.Folder = subf;
                tvfolder.Header = ControlExtension.BuildImageTextblock(
                    "pack://application:,,,/Images/Icons/folder.png",
                    subf.Name);

                Items.Add(tvfolder);

                if ((this.GetTreeView() as MultiSelectTreeView).SelectedItems.Contains(this))
                {
                    (this.GetTreeView() as MultiSelectTreeView).SelectItem(tvfolder);
                    (this.GetTreeView() as MultiSelectTreeView).UpdateSelectedItems();
                }
            }
            foreach (RDAFile subf in Folder.Files.OrderBy(f => f.FileName))
            {
                RDAFileTreeViewItem it = subf.ToTreeViewItem();
                it.SelectOnRightClick = true;
                Items.Add(it);

                if ((this.GetTreeView() as MultiSelectTreeView).SelectedItems.Contains(this))
                {
                    (this.GetTreeView() as MultiSelectTreeView).SelectItem(it);
                    (this.GetTreeView() as MultiSelectTreeView).UpdateSelectedItems();
                }
            }

            AlreadyExpanded = true;
            IsExpanded = true;
        }

        void RDAFolderTreeViewItem_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!AlreadyExpanded)
            {
                UpdateSubItems();
            }
        }

        #region Search Methods
        public RDAFolderTreeViewItem SearchFolder(string text)
        {
            RDAFolderTreeViewItem_Expanded(null, null);

            if (Folder.Name.Contains(text))
            {
                return this;
            }

            foreach (RDAFolderTreeViewItem item in Items.OfType<RDAFolderTreeViewItem>())
            {
                RDAFolderTreeViewItem returned = item.SearchFolder(text);

                if (returned != null)
                    return returned;
            }

            IsExpanded = false;

            return null;
        }

        public RDAFileTreeViewItem SearchFile(string text)
        {
            RDAFolderTreeViewItem_Expanded(null, null);

            foreach (RDAFileTreeViewItem item in Items.OfType<RDAFileTreeViewItem>())
            {
                if (Path.GetFileName(item.File.FileName).Contains(text))
                    return item;
            }

            foreach (RDAFolderTreeViewItem item in Items.OfType<RDAFolderTreeViewItem>())
            {
                RDAFileTreeViewItem returned = item.SearchFile(text);

                if (returned != null)
                    return returned;
            }

            IsExpanded = false;

            return null;
        }
        #endregion
        #region ContextMenu
        private void context_Extract_Click(object sender, RoutedEventArgs e)
        {
            wf.FolderBrowserDialog dlg = new wf.FolderBrowserDialog();

            if (dlg.ShowDialog() == wf.DialogResult.OK)
            {
                BackgroundWorker wrk = new BackgroundWorker();

                MainWindow.CurrentMainWindow.progressBar_Status.Visibility = System.Windows.Visibility.Visible;
                wrk.ProgressChanged += (s, e2) =>
                    {
                        App.Current.Dispatch(() =>
                        {
                            MainWindow.CurrentMainWindow.label_Status.Text = RDAExplorer.RDAFileExtension.ExtractAll_LastMessage;
                            MainWindow.CurrentMainWindow.progressBar_Status.Value = e2.ProgressPercentage;
                        });
                    };
                wrk.RunWorkerCompleted += (s, e2) =>
                    {
                        App.Current.Dispatch(() =>
                            {
                                MainWindow.CurrentMainWindow.label_Status.Text =
                                    MainWindow.CurrentMainWindow.CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                                MainWindow.CurrentMainWindow.progressBar_Status.Visibility = System.Windows.Visibility.Collapsed;
                            });
                    };

                Folder.GetAllFiles().ExtractAll(dlg.SelectedPath, wrk);

                wrk.RunWorkerAsync();
            }
        }

        private void context_AddFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All files|*.*";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == true)
            {
                foreach (string file in dlg.FileNames)
                {
                    string generatedRDAFileName = RDAFile.FileNameToRDAFileName(
                        file, Folder.FullPath);

                    RDAFile fAlready = Folder.Files.Find
                        (f => f.FileName == generatedRDAFileName);

                    if (fAlready == null)
                    {
                        RDAFile rdafile = RDAFile.Create(file, Folder.FullPath);

                        if (rdafile != null)
                            Folder.Files.Add(rdafile);
                    }
                    else
                    {
                        fAlready.SetFile(file, true);
                    }
                }

                UpdateSubItems();
            }
        }

        private void context_AddFolder_Click(object sender, RoutedEventArgs e)
        {
            wf.FolderBrowserDialog dlg = new wf.FolderBrowserDialog();

            if (dlg.ShowDialog() == wf.DialogResult.OK)
            {
                List<RDAFile> coll = new List<RDAFile>();

                foreach (string file in Directory.GetFiles(dlg.SelectedPath, "*", SearchOption.AllDirectories))
                {
                    string dir = (Path.GetFileName(dlg.SelectedPath) + "\\" +
                       Path.GetDirectoryName(file).Replace(dlg.SelectedPath, "")).Trim('\\');
                    string rdadir = (Folder.FullPath + "\\" + dir).Trim('\\');
                    string rdaDestFile = RDAFile.FileNameToRDAFileName(file, rdadir);

                    RDAFile tAlready = Folder.GetAllFiles().Find(f => f.FileName == rdaDestFile);

                    if (tAlready == null)
                    {
                        RDAFile rdafile = RDAFile.Create(file, rdadir);

                        if (rdafile != null)
                        {
                            coll.Add(rdafile);
                        }
                    }
                    else
                    {
                        tAlready.SetFile(file, true);
                    }
                }

                Folder.AddFiles(coll);

                UpdateSubItems();
            }
        }

        private void context_AddFolderAsRoot_Click(object sender, RoutedEventArgs e)
        {
            wf.FolderBrowserDialog dlg = new wf.FolderBrowserDialog();

            if (dlg.ShowDialog() == wf.DialogResult.OK)
            {
                List<RDAFile> coll = new List<RDAFile>();

                foreach (string file in Directory.GetFiles(dlg.SelectedPath, "*", SearchOption.AllDirectories))
                {
                    string dir = Path.GetDirectoryName(file).Replace(dlg.SelectedPath, "");
                    string rdadir = (Folder.FullPath + "\\" + dir).Trim('\\');
                    string rdaDestFile = RDAFile.FileNameToRDAFileName(file, rdadir);

                    RDAFile tAlready = Folder.GetAllFiles().Find(f => f.FileName == rdaDestFile);

                    if (tAlready == null)
                    {
                        RDAFile rdafile = RDAFile.Create(file, rdadir);

                        if (rdafile != null)
                        {
                            coll.Add(rdafile);
                        }
                    }
                    else
                    {
                        tAlready.SetFile(file, true);
                    }
                }

                Folder.AddFiles(coll);

                UpdateSubItems();
            }
        }

        private void context_NewFolder_Click(object sender, RoutedEventArgs e)
        {
            string newfoldername = MessageWindow.GetText("Folder name:", "New Folder");

            if (newfoldername != null)
            {
                newfoldername = newfoldername.Replace(Path.GetInvalidPathChars(), "").Replace("\\", "").Replace("/", "");
                if (!string.IsNullOrEmpty(newfoldername))
                {
                    newfoldername = StringExtension.MakeUnique(newfoldername, "",
                       f => Folder.Folders.Find(n => n.Name == f) != null);

                    Folder.Folders.Add(new RDAFolder()
                    {
                        FullPath = Folder.FullPath + "\\" + newfoldername,
                        Name = newfoldername,
                        Parent = Folder
                    });

                    UpdateSubItems();
                }
            }
        }

        private void context_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageWindow.Show("Do you really want to delete this folder?", MessageWindow.MessageWindowType.YesNo)
                == MessageBoxResult.Yes)
            {
                //Get parent folder and remove 
                Folder.Parent.Folders.Remove(Folder);

                //Now refresh
                if (Parent == this.GetTreeView()) //If is treeView -> Use window
                {
                    MainWindow.CurrentMainWindow.RebuildTreeView();
                }
                else
                {
                    (Parent as RDAFolderTreeViewItem).UpdateSubItems();
                }
            }
        }
        #endregion
    }
}
