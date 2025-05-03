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
using System.Windows.Shapes;
using RDAExplorer;
using RDAExplorerGUI.Misc;
using ammisc = AnnoModificationManager5.Misc;
using System.ComponentModel;

namespace RDAExplorerGUI.Controls
{
    /// <summary>
    /// Interaction logic for RDATreeView.xaml
    /// </summary>
    public partial class RDATreeView : UserControl
    {
        ///////////
        //Property RDAFolder
        ///////////
        private RDAFolder _RDAFolder;
        public RDAFolder RDAFolder
        {
            get
            {
                return _RDAFolder;
            }
            set
            {
                _RDAFolder = value;
                RefreshTree();
            }
        }

        public List<RDAFile> SelectedFiles
        {
            get
            {
                List<RDAFile> sel = new List<RDAFile>();

                if (treeView.SelectedItem != null)
                {
                    foreach (var item in treeView.SelectedItems)
                    {
                        if (item is RDAFileTreeViewItem)
                        {
                            sel.Add(((RDAFileTreeViewItem)item).File);
                        }
                    }
                }

                return sel;
            }
        }

        public RDAFolder SelectedFolder
        {
            get
            {
                if (treeView.SelectedItem != null && treeView.SelectedItem is RDAFolderTreeViewItem)
                {
                    return ((RDAFolderTreeViewItem)treeView.SelectedItem).Folder;
                }

                return null;
            }
        }

        BackgroundWorker wrk;


        public RDATreeView()
        {
            InitializeComponent();
        }

        public void RefreshTree()
        {
            if (_RDAFolder != null)
            {
                if (wrk != null && wrk.IsBusy)
                {
                    wrk.CancelAsync();
                }

                wrk = new BackgroundWorker();
                wrk.WorkerReportsProgress = true;
                wrk.WorkerSupportsCancellation = true;
                progressBar_Status.Visibility = System.Windows.Visibility.Visible;

                wrk.ProgressChanged += (s, e) =>
                {
                    App.Current.Dispatch(() =>
                    {
                        progressBar_Status.Value = e.ProgressPercentage;
                    });
                };
                wrk.DoWork += (s, e) => { _RebuildTreeView(e); };
                wrk.RunWorkerCompleted += (s, e) =>
                {
                    App.Current.Dispatch(() =>
                    {
                        progressBar_Status.Visibility = System.Windows.Visibility.Collapsed;
                    });
                };

                wrk.RunWorkerAsync();
            }
        }

        private void _RebuildTreeView(DoWorkEventArgs e)
        {
            App.Current.Dispatch(() =>
            {
                treeView.Items.Clear();

                //Read
                RDAFolder fold = _RDAFolder;

                foreach (RDAFolder subf in fold.Folders)
                {
                    if (wrk.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    RDAFolderTreeViewItem tvfolder = new RDAFolderTreeViewItem();
                    tvfolder.Folder = subf;
                    tvfolder.Header = RDAExplorerGUI.Misc.ControlExtension.BuildImageTextblock(
                        "pack://application:,,,/Images/Icons/folder.png",
                        subf.Name);

                    treeView.Items.Add(tvfolder);
                }
                foreach (RDAFile subf in fold.Files)
                {
                    if (wrk.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    treeView.Items.Add(subf.ToTreeViewItem());
                }
            });
        }
    }
}
