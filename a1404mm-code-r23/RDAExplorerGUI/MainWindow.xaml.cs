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
using RDAExplorerGUI.Misc;
using System.ComponentModel;
using ammmisc = AnnoModificationManager4.Misc;
using System.IO;
using ammc = AnnoModificationManager4.Controls;
using AnnoModificationManager4.UserInterface.Misc;
using wf = System.Windows.Forms;
using vbio = Microsoft.VisualBasic.FileIO;
using System.Reflection;
using RDAExplorer.Misc;

namespace RDAExplorerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow CurrentMainWindow; //To Acces to the Window from everywhere
        public RDAReader CurrentReader = new RDAReader(); //Current RDA Reader
        public string CurrentFileName = "";

        public FileSystemWatcher FileWatcher; //To watch editing of a file ("View")
        public List<RDAFile> FileWatcher_ToUpdate = new List<RDAFile>();
        public bool FileWatcher_Updating = false;

        public MainWindow()
        {
            CurrentMainWindow = this;
            InitializeComponent();

            #region Load settings
            Width = Properties.Settings.Default.Window_Width;
            Height = Properties.Settings.Default.Window_Height;
            Left = Properties.Settings.Default.Window_X;
            Top = Properties.Settings.Default.Window_Y;
            WindowState = Properties.Settings.Default.Window_IsMaximized ? WindowState.Maximized : WindowState.Normal;
            #endregion
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NewFile();
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            #region save settings
            if (WindowState == System.Windows.WindowState.Normal)
            {
                Properties.Settings.Default.Window_Width = Width;
                Properties.Settings.Default.Window_Height = Height;
                Properties.Settings.Default.Window_X = Left;
                Properties.Settings.Default.Window_Y = Top;
            }
            Properties.Settings.Default.Window_IsMaximized = WindowState == WindowState.Maximized;

            Properties.Settings.Default.Save();
            #endregion

            ResetDocument();
            try
            {
                Directory.Delete(DirectoryExtension.GetTempWorkingDirectory(), true);
            }
            catch (Exception)
            {
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            FileWatcher_ToUpdate = FileWatcher_ToUpdate.Distinct().ToList();
            if (FileWatcher_ToUpdate.Count != 0 && !FileWatcher_Updating)
            {
                string msg = "Following files has changed:\n";
                foreach (RDAFile f in FileWatcher_ToUpdate)
                    msg += f.FileName + "\n";
                msg += "\nDo you want to update the RDA File Items?";

                FileWatcher_Updating = true;
                if (MessageWindow.Show(msg, MessageWindow.MessageWindowType.YesNo)
                    == MessageBoxResult.Yes)
                {

                    foreach (RDAFile file in FileWatcher_ToUpdate)
                    {
                        string tempfile = DirectoryExtension.GetTempWorkingDirectory() + "\\" + file.FileName;
                        string newtempfile = StringExtension.MakeUnique(Path.ChangeExtension(tempfile, null)
                            + "$", Path.GetExtension(tempfile), d => Directory.Exists(d));

                        File.Copy(tempfile, newtempfile);

                        file.SetFile(newtempfile);
                    }
                }

                FileWatcher_Updating = false;
                FileWatcher_ToUpdate.Clear();
            }
        }

        #region TreeView
        #region Rebuild
        public void RebuildTreeView()
        {
            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            progressBar_Status.Visibility = System.Windows.Visibility.Visible;

            wrk.ProgressChanged += (s, e) =>
                {
                    App.Current.Dispatch(() =>
                    {
                        progressBar_Status.Value = e.ProgressPercentage;
                        label_Status.Text = "Updating UI";
                    });
                };
            wrk.DoWork += (s, e) => { _RebuildTreeView(wrk); };
            wrk.RunWorkerCompleted += (s, e) =>
                {
                    App.Current.Dispatch(() =>
                        {
                            progressBar_Status.Visibility = System.Windows.Visibility.Collapsed;
                        });
                };

            wrk.RunWorkerAsync();
        }
        private void _RebuildTreeView(BackgroundWorker wrk)
        {
            App.Current.Dispatch(() =>
                   {
                       treeView.Items.Clear();

                       //Read
                       RDAFolder fold = CurrentReader.rdaFolder;

                       foreach (RDAFolder subf in fold.Folders)
                       {
                           RDAFolderTreeViewItem tvfolder = new RDAFolderTreeViewItem();
                           tvfolder.Folder = subf;
                           tvfolder.Header = ControlExtension.BuildImageTextblock(
                               "pack://application:,,,/Images/Icons/folder.png",
                               subf.Name);

                           treeView.Items.Add(tvfolder);
                       }
                       foreach (RDAFile subf in fold.Files)
                       {
                           treeView.Items.Add(subf.ToTreeViewItem());
                       }
                   });
        }
        #endregion
        #region ContextMenu
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
                        file, CurrentReader.rdaFolder.FullPath);

                    RDAFile fAlready = CurrentReader.rdaFolder.Files.Find
                        (f => f.FileName == generatedRDAFileName);

                    if (fAlready == null)
                    {
                        RDAFile rdafile = RDAFile.Create(file, CurrentReader.rdaFolder.FullPath);

                        if (rdafile != null)
                            CurrentReader.rdaFolder.Files.Add(rdafile);
                    }
                    else
                    {
                        fAlready.SetFile(file, true);
                    }
                }

                RebuildTreeView();
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
                    string rdadir = (CurrentReader.rdaFolder.FullPath + "\\" + dir).Trim('\\');
                    string rdaDestFile = RDAFile.FileNameToRDAFileName(file, rdadir);

                    RDAFile tAlready = CurrentReader.rdaFolder.GetAllFiles().Find(f => f.FileName == rdaDestFile);

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

                CurrentReader.rdaFolder.AddFiles(coll);

                RebuildTreeView();
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
                    string rdadir = (CurrentReader.rdaFolder.FullPath + "\\" + dir).Trim('\\');
                    string rdaDestFile = RDAFile.FileNameToRDAFileName(file, rdadir);

                    RDAFile tAlready = CurrentReader.rdaFolder.GetAllFiles().Find(f => f.FileName == rdaDestFile);

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

                CurrentReader.rdaFolder.AddFiles(coll);

                RebuildTreeView();
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
                        f => CurrentReader.rdaFolder.Folders.Find(n => n.Name == f) != null);

                    CurrentReader.rdaFolder.Folders.Add(new RDAFolder()
                    {
                        FullPath = "\\" + newfoldername,
                        Name = newfoldername,
                        Parent = CurrentReader.rdaFolder
                    });

                    RebuildTreeView();
                }
            }
        }
        #endregion
        #endregion
        #region Menu
        private void file_New_Click(object sender, RoutedEventArgs e)
        {
            NewFile();
        }

        private void file_OpenReadOnly_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "Valid Files|*.rda;*.sww;*.rdu|All files|*.*";

            if (openfile.ShowDialog() == true)
            {
                OpenFile(openfile.FileName, true);
            }
        }

        private void file_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "Valid Files|*.rda;*.sww;*.rdu|All files|*.*";

            if (openfile.ShowDialog() == true)
            {
                OpenFile(openfile.FileName, false);
            }
        }

        private void file_Save_Click(object sender, RoutedEventArgs e)
        {
            //Only save not empty
            if (CurrentReader.rdaFolder.GetAllFiles().Count == 0)
            {
                MessageWindow.Show("Cannot save an empty file!");
                return;
            }

            if (string.IsNullOrEmpty(CurrentFileName))
            {
                file_SaveAs_Click(null, null);
            }
            else
            {
                SaveRDAFileWindow saveDlg = new SaveRDAFileWindow();
                saveDlg.Folder = CurrentReader.rdaFolder;
                saveDlg.field_OutputFile.Text = CurrentFileName;

                if (saveDlg.ShowDialog() == true)
                {
                    string fileName = saveDlg.field_OutputFile.Text;

                    bool compress = (bool)saveDlg.check_IsCompressed.IsChecked;

                    if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                    #region Write
                    RDAWriter writer = new RDAWriter(CurrentReader.rdaFolder);

                    BackgroundWorker wrk = new BackgroundWorker();
                    wrk.WorkerReportsProgress = true;

                    progressBar_Status.Visibility = System.Windows.Visibility.Visible;
                    wrk.ProgressChanged += (s, e2) =>
                    {
                        App.Current.Dispatch(() =>
                        {
                            label_Status.Text = writer.UI_LastMessage;
                            progressBar_Status.Value = e2.ProgressPercentage;
                        });
                    };
                    wrk.RunWorkerCompleted += (s, e2) =>
                    {
                        App.Current.Dispatch(() =>
                        {
                            label_Status.Text = CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                            progressBar_Status.Visibility = System.Windows.Visibility.Collapsed;
                        });
                    };

                    wrk.DoWork += (s, e2) =>
                    {
                        try
                        {
                            writer.Write(fileName, compress, wrk);
                        }
                        catch (Exception ex)
                        {
                            App.Current.Dispatch(() => MessageWindow.Show(ex.Message));
                        }
                    };

                    wrk.RunWorkerAsync();
                    #endregion
                }
            }
        }

        private void file_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            //Only save not empty
            if (CurrentReader.rdaFolder.GetAllFiles().Count == 0)
            {
                MessageWindow.Show("Cannot save an empty file!");
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "RDA File|*.rda|Savegame|*.sww|Scenario|*.rdu";

            if (dlg.ShowDialog() == true)
            {
                SaveRDAFileWindow saveDlg = new SaveRDAFileWindow();
                saveDlg.Folder = CurrentReader.rdaFolder;
                saveDlg.field_OutputFile.Text = dlg.FileName;

                if (saveDlg.ShowDialog() == true)
                {
                    string fileName = saveDlg.field_OutputFile.Text;
                    CurrentFileName = fileName;

                    bool compress = (bool)saveDlg.check_IsCompressed.IsChecked;

                    if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                    #region Write
                    RDAWriter writer = new RDAWriter(CurrentReader.rdaFolder);

                    BackgroundWorker wrk = new BackgroundWorker();
                    wrk.WorkerReportsProgress = true;

                    progressBar_Status.Visibility = System.Windows.Visibility.Visible;
                    wrk.ProgressChanged += (s, e2) =>
                        {
                            App.Current.Dispatch(() =>
                                {
                                    label_Status.Text = writer.UI_LastMessage;
                                    progressBar_Status.Value = e2.ProgressPercentage;
                                });
                        };
                    wrk.RunWorkerCompleted += (s, e2) =>
                        {
                            App.Current.Dispatch(() =>
                                {
                                    label_Status.Text = CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                                    progressBar_Status.Visibility = System.Windows.Visibility.Collapsed;
                                });
                        };

                    wrk.DoWork += (s, e2) =>
                        {
                            try
                            {
                                writer.Write(fileName, compress, wrk);
                            }
                            catch (Exception ex)
                            {
                                App.Current.Dispatch(() => MessageWindow.Show(ex.Message));
                            }
                        };

                    wrk.RunWorkerAsync();
                    #endregion
                }
            }
        }

        private void file_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void archive_ExtractAll_Click(object sender, RoutedEventArgs e)
        {
            wf.FolderBrowserDialog dlg = new wf.FolderBrowserDialog();

            if (dlg.ShowDialog() == wf.DialogResult.OK)
            {
                BackgroundWorker wrk = new BackgroundWorker();
                wrk.WorkerReportsProgress = true;

                progressBar_Status.Visibility = System.Windows.Visibility.Visible;
                wrk.ProgressChanged += (s, e2) =>
                    {
                        App.Current.Dispatch(() =>
                            {
                                label_Status.Text = RDAExplorer.RDAFileExtension.ExtractAll_LastMessage;
                                progressBar_Status.Value = e2.ProgressPercentage;
                            });
                    };
                wrk.RunWorkerCompleted += (s, e2) =>
                    {
                        App.Current.Dispatch(() =>
                            {
                                label_Status.Text = CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                                progressBar_Status.Visibility = System.Windows.Visibility.Collapsed;
                            });
                    };

                wrk.DoWork += (s, e2) =>
                    {
                        try
                        {
                            CurrentReader.rdaFolder.GetAllFiles().ExtractAll(dlg.SelectedPath, wrk);
                        }
                        catch (Exception ex)
                        {
                            App.Current.Dispatch(() => MessageWindow.Show(ex.Message));
                        }
                    };

                wrk.RunWorkerAsync();
            }
        }

        private void archive_ExtractSelected_Click(object sender, RoutedEventArgs e)
        {
            wf.FolderBrowserDialog dlg = new wf.FolderBrowserDialog();

            if (dlg.ShowDialog() == wf.DialogResult.OK)
            {
                BackgroundWorker wrk = new BackgroundWorker();
                wrk.WorkerReportsProgress = true;

                progressBar_Status.Visibility = System.Windows.Visibility.Visible;
                wrk.ProgressChanged += (s, e2) =>
                {
                    App.Current.Dispatch(() =>
                    {
                        label_Status.Text = RDAExplorer.RDAFileExtension.ExtractAll_LastMessage;
                        progressBar_Status.Value = e2.ProgressPercentage;
                    });
                };
                wrk.RunWorkerCompleted += (s, e2) =>
                {
                    App.Current.Dispatch(() =>
                    {
                        label_Status.Text = CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                        progressBar_Status.Visibility = System.Windows.Visibility.Collapsed;
                    });
                };

                wrk.DoWork += (s, e2) =>
                {
                    try
                    {
                        List<RDAFile> toextract = new List<RDAFile>();

                        foreach (RDAFileTreeViewItem item in treeView.SelectedItems.OfType<RDAFileTreeViewItem>())
                        {
                            toextract.Add(item.File);
                        }

                        foreach (RDAFolderTreeViewItem item in treeView.SelectedItems.OfType<RDAFolderTreeViewItem>())
                        {
                            toextract.AddRange(item.Folder.GetAllFiles());
                        }

                        toextract = toextract.Distinct().ToList();

                        toextract.ExtractAll(dlg.SelectedPath, wrk);
                    }
                    catch (Exception ex)
                    {
                        App.Current.Dispatch(() => MessageWindow.Show(ex.Message));
                    }
                };

                wrk.RunWorkerAsync();
            }
        }

        private void archive_SearchFile_Click(object sender, RoutedEventArgs e)
        {
            string tosearch = MessageWindow.GetText("Search File with Name", "File.ext");

            if (tosearch != null)
            {
                foreach (RDAFolderTreeViewItem item in treeView.Items.OfType<RDAFolderTreeViewItem>())
                {
                    RDAFileTreeViewItem n = item.SearchFile(tosearch);

                    if (n != null)
                    {
                        n.IsSelected = true;
                        return;
                    }
                }
            }
        }

        private void archive_SearchFolder_Click(object sender, RoutedEventArgs e)
        {
            string tosearch = MessageWindow.GetText("Search Folder with Name", "Folder");

            if (tosearch != null)
            {
                foreach (RDAFolderTreeViewItem item in treeView.Items.OfType<RDAFolderTreeViewItem>())
                {
                    RDAFolderTreeViewItem n = item.SearchFolder(tosearch);

                    if (n != null)
                    {
                        n.IsSelected = true;
                        return;
                    }
                }
            }
        }
        #endregion
        #region MiscUI
        private void button_Filter_Refresh_Click(object sender, RoutedEventArgs e)
        {
            RebuildTreeView();
        }
        #endregion
        #region New|Open
        private void ResetDocument()
        {
            CurrentFileName = "";

            //For readonly
            file_Save.IsEnabled = true;

            if (FileWatcher != null)
                FileWatcher.Dispose();
            FileWatcher = new FileSystemWatcher();
            FileWatcher.IncludeSubdirectories = true;
            FileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            CurrentReader.Dispose();

            DirectoryExtension.CleanDirectory(DirectoryExtension.GetTempWorkingDirectory());

            FileWatcher.Path = DirectoryExtension.GetTempWorkingDirectory();
            FileWatcher.EnableRaisingEvents = true;
        }

        private void NewFile()
        {
            Title = GetTitle();
            label_Status.Text = "";

            ResetDocument();
            CurrentReader = new RDAReader();
            RebuildTreeView();
        }

        private void OpenFile(string fileName, bool isreadonly)
        {
            RDAReader reader = new RDAReader();
            ResetDocument();

            CurrentFileName = fileName;

            //First, copy original file to Instance Dir (Enable direct save); ifreadonly
            if (!isreadonly)
            {
                fileName = DirectoryExtension.GetTempWorkingDirectory() + "\\" + Path.GetFileName(fileName);
            }
            else
            {
                //Disable menu item "save":
                file_Save.IsEnabled = false;
            }

            CurrentReader = reader;
            reader.FileName = fileName;

            progressBar_Status.Visibility = System.Windows.Visibility.Visible;
            Title = GetTitle() + " - " + Path.GetFileName(reader.FileName);

            reader.backgroundWorker = new BackgroundWorker();
            reader.backgroundWorker.WorkerReportsProgress = true;
            reader.backgroundWorker.ProgressChanged += (sender2, e2) =>
                {
                    App.Current.Dispatch(() =>
                        {
                            progressBar_Status.Value = e2.ProgressPercentage;
                            label_Status.Text = reader.backgroundWorkerLastMessage;
                        });
                };
            reader.backgroundWorker.DoWork += (sender2, e2) =>
                {
                    try
                    {
                        if (!isreadonly)
                        {
                            App.Current.Dispatch(() => label_Status.Text = "Copying *.rda file to a temparary directory ...");
                            vbio.FileSystem.CopyFile(CurrentFileName, fileName, vbio.UIOption.AllDialogs, vbio.UICancelOption.ThrowException);
                        }

                        reader.ReadRDAFile();
                    }
                    catch (Exception ex)
                    {
                        App.Current.Dispatch(() =>
                        {
                            MessageWindow.Show(ex.Message);
                            NewFile();
                        });
                    }
                };
            reader.backgroundWorker.RunWorkerCompleted += (sender2, e2) =>
                {
                    progressBar_Status.Visibility = System.Windows.Visibility.Collapsed;

                    //TreeView
                    RebuildTreeView();
                };

            reader.backgroundWorker.RunWorkerAsync();
        }
        #endregion
        #region Misc
        public string GetTitle()
        {
            return "Anno 1404 RDA Explorer Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        #endregion
    }
}
