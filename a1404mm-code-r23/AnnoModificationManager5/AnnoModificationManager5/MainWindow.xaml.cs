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
using AnnoModificationManager5.Components;
using AnnoModificationManager5.Misc;
using System.Collections;
using System.Collections.ObjectModel;
using AnnoModificationManager5.ModificationTypes;
using AnnoModificationManager5.UserInterface.MainUI;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using AnnoModificationManager5.Language.DictionarySystem;
using AnnoModificationManager5.UserInterface.Misc;
using System.Reflection;

namespace AnnoModificationManager5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow CurrentMainWindow;
        private OpenFileDialog AddModificationDialog = new OpenFileDialog()
        {
            Filter = "Modification Project|*.zip",
            Multiselect = true
        };
        private DispatcherTimer AnnoRunningTimer = new DispatcherTimer();

        #region Blocking GUI
        public enum EUGUIBlockingType
        {
            None,
            NoEngineIni,
            EngineIniDisabled,
            AnnoRunning
        }

        private EUGUIBlockingType _CurrentBlocking = EUGUIBlockingType.None;
        public EUGUIBlockingType CurrentBlocking
        {
            get
            {
                return _CurrentBlocking;
            }
            set
            {
                _CurrentBlocking = value;
                switch (value)
                {
                    case EUGUIBlockingType.None:
                        modificationsDisabledPanel.Visibility = System.Windows.Visibility.Hidden;
                        modificationPanel.Visibility = System.Windows.Visibility.Visible;
                        downloadPackageButton.IsHitTestVisible = true;
                        Organize.IsEnabled = true;
                        break;
                    case EUGUIBlockingType.AnnoRunning:
                        modificationPanel.Visibility = System.Windows.Visibility.Hidden;
                        modificationsDisabledPanel.Visibility = System.Windows.Visibility.Visible;
                        modificationsDisabledPanel_Message.Text = LanguageDictionary.Get("MainUI", "DisabledPanel_AnnoRunning");
                        downloadPackageButton.IsHitTestVisible = false;
                        break;
                }
            }
        }
        #endregion

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

            ShowInTaskbar = false;
            Opacity = 0;
        }

        #region Loading saving
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
            #region Apply changes
            if (Modification.AMMRDA.Pending)
            {
                switch (MessageWindow.Show(LanguageDictionary.Get("MainUI", "Exit_ApplyChanges_Message"), MessageWindow.MessageWindowType.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        rdaChangesButton.ApplyChanges(false);
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
            #endregion
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = "Anno 1404 Modification Manager Version " + Assembly.GetExecutingAssembly().GetName().Version;

            CurrentBlocking = EUGUIBlockingType.None;

            modificationList.Refresh(true);
            ReloadModifications(true);

            //Organize -> Start Addon
            if (AnnoVersionHandler.GetCurrent() != AnnoVersionHandler.AnnoVersion.Addon1)
            {
                Organize_StartAnno_Addon.Visibility = System.Windows.Visibility.Collapsed;
            }

            //Check if Anno is Running
            AnnoRunningTimer.Tick += new EventHandler(AnnoRunningTimer_Tick);
            AnnoRunningTimer.Interval = new TimeSpan(0, 0, 5);
            AnnoRunningTimer.Start();

            //Startup
            StartupHandler.Run();

            UserInterface.Modern.ModernMainWindow modern = new UserInterface.Modern.ModernMainWindow();
            modern.Closed += delegate { try { Close(); } catch (Exception) { } };
            modern.Show();
            Hide();
        }

        void AnnoRunningTimer_Tick(object sender, EventArgs e)
        {
            if (Process.GetProcesses().ToList().Find(pr =>
                {
                    return pr.ProcessName == "Anno4" | pr.ProcessName == "Addon";
                }) != null)
            {
                App.Current.Dispatch(app =>
                    {
                        CurrentBlocking = EUGUIBlockingType.AnnoRunning;
                    });
            }
            //else
            //{
            //    if (_CurrentBlocking == EUGUIBlockingType.AnnoRunning)
            //    {
            //        App.Current.Dispatch(app =>
            //        {
            //            if (engineIniButton.ModificationsEnabled == true)
            //                CurrentBlocking = EUGUIBlockingType.None;
            //            else
            //                engineIniButton.RefreshState();
            //        });
            //    }
            //}
        }
        #endregion

        private void nexusBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Nexus.NexusBrowseWindow window = new Nexus.NexusBrowseWindow();
            window.ShowDialog();
            if (window.HasDownloaded)
                ReloadModifications(true);
        }

        #region Loading Modifications
        #region Reload Modifications
        public void ReloadModifications(bool searchonline)
        {
            analyzerPanel.Visibility = System.Windows.Visibility.Collapsed;
            loadingProgress.Visibility = System.Windows.Visibility.Visible;
            modificationList.IsHitTestVisible = false;
            downloadPackageButton.IsHitTestVisible = false;
            Organize.IsEnabled = false;
            //engineIniButton.IsHitTestVisible = false;

            modificationList.SetUIToLoading();

            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            wrk.ProgressChanged += new ProgressChangedEventHandler(ReloadModifications_ProgressChanged);
            wrk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ReloadModifications_Completed);
            if (searchonline)
            {
                wrk.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                {
                    App.Current.Dispatch(app => downloadPackageButton.RunSearch(false));
                };
            }

            //Stopwatch wtch = new Stopwatch();
            //wrk.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            //    {
            //        wtch.Stop();
            //        App.Current.Dispatch(app => MessageBox.Show(wtch.ElapsedMilliseconds.ToString()));
            //    };
            //
            //wtch.Start();

            ModificationHandler.Instance.LoadModifications(wrk);


            //ModificationHandler.Instance._LoadModifications(null);

            ////DEV           
            //{
            //    analyzerPanel.Visibility = System.Windows.Visibility.Visible;
            //    loadingProgress.Visibility = System.Windows.Visibility.Collapsed;
            //    modificationList.IsHitTestVisible = true;
            //    downloadPackageButton.IsHitTestVisible = true;
            //    //engineIniButton.IsHitTestVisible = true;

            //    //ModificationHandler_ViewSource_Update(false);
            //    modificationList.Refresh(false);
            //    modificationList.UpdateSelectionUI(this);
            //}
        }

        void ReloadModifications_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.Dispatch(app =>
            {
                analyzerPanel.Visibility = System.Windows.Visibility.Visible;
                loadingProgress.Visibility = System.Windows.Visibility.Collapsed;
                modificationList.IsHitTestVisible = true;
                downloadPackageButton.IsHitTestVisible = true;
                //engineIniButton.IsHitTestVisible = true;

                //ModificationHandler_ViewSource_Update(false);
                modificationList.Refresh(false);
                modificationList.UpdateSelectionUI(this);
                Organize.IsEnabled = true;

                if (UserInterface.Modern.ModernMainWindow.Current != null)
                    UserInterface.Modern.ModernMainWindow.Current.RefreshData();
            });
        }

        void ReloadModifications_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            App.Current.Dispatch(app =>
            {
                if (e.ProgressPercentage != 0)
                    loadingProgress.Value = e.ProgressPercentage;
                else
                {
                    //ModificationHandler_ViewSource_Update(false);
                    modificationList.RefreshLoading();
                }
            });
        }
        #endregion
        #region Update Activation Respones
        public void UpdateActivationResponses()
        {
            //analyzerPanel.Visibility = System.Windows.Visibility.Collapsed;
            analyzerPanel.IsHitTestVisible = false;
            loadingProgress.Visibility = System.Windows.Visibility.Visible;
            modificationList.IsHitTestVisible = false;

            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;

            wrk.ProgressChanged += (sender, e) =>
                {
                    modificationList.Refresh(false);
                };

            wrk.RunWorkerCompleted += (sender, e) =>
            {
                Application.Current.Dispatch(app =>
                {
                    modificationList.Refresh(false);

                    analyzerPanel.Visibility = System.Windows.Visibility.Visible;
                    analyzerPanel.IsHitTestVisible = true;
                    loadingProgress.Visibility = System.Windows.Visibility.Collapsed;
                    modificationList.IsHitTestVisible = true;

                    /*if (modificationList.Items.Count != 0)
                        modificationList.SelectedIndex = 0;*/
                    modificationList.UpdateSelectionUI(this);
                });
            };

            ModificationHandler.Instance.UpdateActivationResponses(wrk);
        }
        #endregion

        //public void ModificationHandler_ViewSource_Update(bool UpdateDescriptions)
        //{            
        //    object selected = modificationList.SelectedItem;

        //    CollectionViewSource vsrc = Resources["modificationList_ViewSource"] as CollectionViewSource;
        //    vsrc.Source = null;
        //    vsrc.Source = new ObservableCollection<Modification>(ModificationHandler.Modifications);

        //    if (UpdateDescriptions)
        //    {
        //        vsrc.GroupDescriptions.Clear();
        //        vsrc.SortDescriptions.Clear();
        //        vsrc.GroupDescriptions.Add(new UserInterface.Group.ModificationGroupDescription());

        //        modificationList_Column_Category.Width = 120;
        //        modificationList_Column_Author.Width = 200;
        //        //modificationList_Column_AnnoExecutable.Width = 110;            

        //        switch (Properties.Settings.Default.modificationList_SortProperty)
        //        {
        //            case "Category":
        //                vsrc.SortDescriptions.Add(new SortDescription("UICollector.Category", ListSortDirection.Ascending));
        //                modificationList_Column_Category.Width = 0;
        //                break;
        //            case "Author":
        //                vsrc.SortDescriptions.Add(new SortDescription("UICollector.Author", ListSortDirection.Ascending));
        //                modificationList_Column_Author.Width = 0;
        //                break;                   

        //        }

        //        //Add Name as Sort Descr
        //        vsrc.SortDescriptions.Add(new SortDescription("UICollector.Name", ListSortDirection.Ascending)); 
        //    }

        //    if (selected != null && modificationList.Items.Contains(selected))
        //        modificationList.SelectedItem = selected;
        //    else if (modificationList.Items.Count != 0)
        //        modificationList.SelectedIndex = 0;
        //    else
        //        analyzerPanel.Visibility = System.Windows.Visibility.Collapsed;
        //}
        #endregion
        #region UI Functions
        private void modificationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modificationList.SelectedItem != null)
            {
                analyzerPanel.LoadModification(modificationList.SelectedItem as Modification);
            }
        }
        #endregion
        #region Menu
        private void Organize_AddModification_Click(object sender, RoutedEventArgs e)
        {
            if (AddModificationDialog.ShowDialog() == true)
            {
                List<bool> added = new List<bool>();

                foreach (string file in AddModificationDialog.FileNames)
                {
                    added.Add(ModificationHandler.Instance.AddModification(file));
                }

                if (added.Contains(true))
                {
                    ReloadModifications(true);
                    try
                    {
                        downloadPackageButton.RunSearch(false);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void Organize_Refresh_View_Click(object sender, RoutedEventArgs e)
        {
            object selected = modificationList.SelectedItem;
            modificationList.Refresh(true);

            if (selected != null)
            {
                modificationList.SelectedItem = selected;
            }
        }

        private void Organize_Refresh_List_Click(object sender, RoutedEventArgs e)
        {
            ReloadModifications(true);
        }

        private void Organize_Sort_Category_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.modificationList_SortProperty = "Category";
            Properties.Settings.Default.Save();
            modificationList.Refresh(true);
        }

        private void Organize_Sort_Author_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.modificationList_SortProperty = "Author";
            Properties.Settings.Default.Save();
            modificationList.Refresh(true);
        }

        private void Organize_Sort_Status_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.modificationList_SortProperty = "Status";
            Properties.Settings.Default.Save();
            modificationList.Refresh(true);
        }

        private void Organize_Sort_AnnoVersions_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.modificationList_SortProperty = "AnnoVersions";
            Properties.Settings.Default.Save();
            modificationList.Refresh(true);
        }

        private void Organize_RestoreManager_Click(object sender, RoutedEventArgs e)
        {
            RestoreManager rest = new RestoreManager();
            rest.ShowDialog();
            if (rest.HasRestored)
                UpdateActivationResponses();
        }

        private void Organize_Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog dlg = new SettingsDialog();
            dlg.ShowDialog();
        }

        private void Organize_StartAnno_Click(object sender, RoutedEventArgs e)
        {
            if (MessageWindow.Show(LanguageDictionary.Get("MainUI", "StartAnno_Message"), MessageWindow.MessageWindowType.YesNo)
                == MessageBoxResult.Yes)
            {
                ProcessStartInfo info = new ProcessStartInfo(AnnoDirectoryHandler.GetCurrent() + "\\Anno4.exe");
                info.WorkingDirectory = AnnoDirectoryHandler.GetCurrent();
                Process.Start(info);
            }
        }

        private void Organize_StartAnno_Addon_Click(object sender, RoutedEventArgs e)
        {
            if (MessageWindow.Show(LanguageDictionary.Get("MainUI", "StartAnno_Message"), MessageWindow.MessageWindowType.YesNo)
               == MessageBoxResult.Yes)
            {
                ProcessStartInfo info = new ProcessStartInfo(AnnoDirectoryHandler.GetCurrent() + "\\Addon.exe");
                info.WorkingDirectory = AnnoDirectoryHandler.GetCurrent();
                Process.Start(info);
            }
        }

        private void Organize_About_Click(object sender, RoutedEventArgs e)
        {
            (new AboutDialog()).ShowDialog();
        }

        private void Organize_ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Language == "German")
            {
                Process.Start(DirectoryExtension.GetApplicationFolder() + "\\Help\\AMM4Help_Deutsch.chm");
            }
            else if (Properties.Settings.Default.Language == "English")
            {
                Process.Start(DirectoryExtension.GetApplicationFolder() + "\\Help\\AMM4Help_English.chm");
            }
        }

        private void Organize_Modification_Append_Click(object sender, RoutedEventArgs e)
        {
            rdaChangesButton.ApplyChanges(true);
        }

        private void Organize_Modification_Undo_Click(object sender, RoutedEventArgs e)
        {
            if (Modification.AMMRDA.Pending)
            {
                if (MessageWindow.Show(LanguageDictionary.Get("MainUI", "Modification_Undo_Message"), MessageWindow.MessageWindowType.YesNo) == MessageBoxResult.Yes)
                {
                    Modification.AMMRDA.Clear();
                    RDAManagerExtension.Clear();
                    Modification.RDAManager.DisposeAll();

                    UpdateActivationResponses();
                }
            }
        }
        #endregion


    }
}
