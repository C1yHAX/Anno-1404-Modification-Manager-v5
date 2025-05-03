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
using AnnoModificationManager5.Language.DictionarySystem;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.DownloadService;
using AnnoModificationManager5.UserInterface.MainUI;
using System.ComponentModel;
using System.Threading;

namespace AnnoModificationManager5.MainControls.Buttons
{
    /// <summary>
    /// Interaction logic for DownloadPackagesButton.xaml
    /// </summary>
    public partial class DownloadPackagesButton : Button
    {
        //o = normal, 1=searching, 2=found, 3=message
        int currentMode = 0;

        public DownloadPackagesButton()
        {
            InitializeComponent();
        }        

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/world.png",
                LanguageDictionary.Get("MainUI", "ToolBar_PackageService_Search"));

            //RunSearch();
        }

        public void RunSearch(bool showerrormessages)
        {
            if (currentMode == 1 || !WebExtension.ConnectionExists())
            {
                return;
            }

            currentMode = 1;
            Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/hourglass.png",
                LanguageDictionary.Get("MainUI", "ToolBar_PackageService_Searching"));

            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            wrk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Instance_OnAsyncRefreshFinished);
            ModificationInfoConnector.Instance.RefreshAsync(wrk, showerrormessages);
        }

        void Instance_OnAsyncRefreshFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            (new Thread(new ParameterizedThreadStart(delegate
                {
                    List<ModificationInfo> filtered = ModificationInfoConnector.Instance.Filter(false);

                    if (filtered.Count == 0)
                    {
                        Application.Current.Dispatch(app =>
                        {
                            currentMode = 3;
                            Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/information.png",
                                LanguageDictionary.Get("MainUI", "ToolBar_PackageService_NothingFound"));
                        });

                        System.Threading.Thread.Sleep(2500);

                        Application.Current.Dispatch(app =>
                            {
                                currentMode = 0;
                                Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/world.png",
                                    LanguageDictionary.Get("MainUI", "ToolBar_PackageService_Search"));
                            });
                    }
                    else
                    {
                        Application.Current.Dispatch(app =>
                        {
                            currentMode = 2;
                            Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/information.png",
                                LanguageDictionary.Get("MainUI", "ToolBar_PackageService_Found").Replace("{0}", filtered.Count.ToString()));
                        });
                    }
                }))).Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == 0)
                RunSearch(true);
            if (currentMode == 2)
            {
                DownloadPackagesWindow wnd = new DownloadPackagesWindow();
                wnd.ShowDialog();

                if (wnd.HasDownloaded)
                    MainWindow.CurrentMainWindow.ReloadModifications(true);
                else
                    RunSearch(false);
            }
        }

        private void menuItem_OpenWindow_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == 0 || currentMode == 2)
            {
                DownloadPackagesWindow wnd = new DownloadPackagesWindow();
                wnd.ShowDialog();

                if (wnd.HasDownloaded)
                    MainWindow.CurrentMainWindow.ReloadModifications(true);
                else
                    RunSearch(false);
            }
        }
    }
}
