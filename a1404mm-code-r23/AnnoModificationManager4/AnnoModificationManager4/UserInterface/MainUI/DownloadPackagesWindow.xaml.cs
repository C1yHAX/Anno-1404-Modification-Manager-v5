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
using AnnoModificationManager4.DownloadService;
using AnnoModificationManager4.Misc;
using System.ComponentModel;
using AnnoModificationManager4.Controls;
using System.IO;
using AnnoModificationManager4.UserInterface.Misc;
using Ionic.Zip;
using AnnoModificationManager4.Components;

namespace AnnoModificationManager4.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for DownloadPackagesWindow.xaml
    /// </summary>
    public partial class DownloadPackagesWindow : Window
    {
        public bool HasDownloaded = false;
        private ModificationInfo lastModificationInfo;
        private TimeoutWebClient currentDownloadingClient;

        public DownloadPackagesWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            filter_All.IsChecked = true;
        }

        private void Reload()
        {
            CollectionViewSource src = new CollectionViewSource();
            src.GroupDescriptions.Add(new PropertyGroupDescription("UI_Category"));
            src.SortDescriptions.Add(new SortDescription("UI_Category", ListSortDirection.Ascending));
            src.SortDescriptions.Add(new SortDescription("UI_Name", ListSortDirection.Ascending));
            src.Source = ModificationInfoConnector.Instance.Filter(check_ShowHidden.IsChecked.Value).FindAll(mi=>
                {
                    if (filter_All.IsChecked == true)
                        return true;
                    if (filter_AllVersions.IsChecked == true)
                    {
                        if (mi.AnnoVersions.Contains("All") ||
                            (mi.AnnoVersions.Find(i => i.Contains("Addon")) != null
                            & mi.AnnoVersions.Count > 1))
                        {
                            return true;
                        }
                    }
                    else if (filter_Anno.IsChecked == true)
                    {
                        if (mi.AnnoVersions.Find(i => i.Contains("Addon")) == null
                            & !mi.AnnoVersions.Contains("All"))
                        {
                            return true;
                        }
                    }
                    else if (filter_Addon.IsChecked == true)
                    {
                        if (mi.AnnoVersions.Find(i => i.Contains("Addon")) != null)
                        {
                            return true;
                        }
                    }

                    return false;
                });

            list_Packages.SetBinding(ListView.ItemsSourceProperty, new Binding() { Source = src });            
        }

        private void check_ShowHidden_Checked(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void check_ModificationHidden_Unchecked(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void button_Download_Click(object sender, RoutedEventArgs e)
        {
            ContentButton Sender = sender as ContentButton;
            ModificationInfo info=(Sender.Binding as ModificationInfo);
            lastModificationInfo = info;

            downloadProgressGrid.Visibility = System.Windows.Visibility.Visible;
            button_ProgressCancel.IsEnabled = true;
            progress_Download.Value = 0;
            check_ShowHidden.Visibility = System.Windows.Visibility.Collapsed;
            list_Packages.IsHitTestVisible = false;
            versionFilterToolBar.IsHitTestVisible = false;
            list_Packages.Opacity = 0.75;
            Closing += new System.ComponentModel.CancelEventHandler(DownloadPackagesWindow_Closing_Cancel);

            TimeoutWebClient client = new TimeoutWebClient();
            currentDownloadingClient = client;
            client.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            try
            {
                if (File.Exists(Path.GetTempPath().Trim('\\') + "\\" + info.GetShortIdentificationString + ".zip"))
                    File.Delete(Path.GetTempPath().Trim('\\') + "\\" + info.GetShortIdentificationString + ".zip");
                client.DownloadFileAsync(new Uri(info.DownloadUrl), Path.GetTempPath().Trim('\\') + "\\" + info.GetShortIdentificationString + ".zip");
            }
            catch (Exception ex)
            {                
                MessageWindow.Show(ex.Message);

                downloadProgressGrid.Visibility = System.Windows.Visibility.Collapsed;
                check_ShowHidden.Visibility = System.Windows.Visibility.Visible;
                list_Packages.IsHitTestVisible = true;
                versionFilterToolBar.IsHitTestVisible = true;
                list_Packages.Opacity = 1;
                Closing -= new System.ComponentModel.CancelEventHandler(DownloadPackagesWindow_Closing_Cancel);
            }            
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            App.Current.Dispatch(app =>
                {
                    downloadProgressGrid.Visibility = System.Windows.Visibility.Collapsed;
                    check_ShowHidden.Visibility = System.Windows.Visibility.Visible;
                    list_Packages.IsHitTestVisible = true;
                    versionFilterToolBar.IsHitTestVisible = true;
                    list_Packages.Opacity = 1;
                    Closing -= new System.ComponentModel.CancelEventHandler(DownloadPackagesWindow_Closing_Cancel);
                });
            try
            {
                ModificationHandler.Instance.AddModificationNoErrorRoutine(Path.GetTempPath().Trim('\\') + "\\"
                    + lastModificationInfo.GetShortIdentificationString + ".zip");
                HasDownloaded = true;

                App.Current.Dispatch(app =>
                    {
                        ModificationInfoConnector.Instance.AvailablePackages.Remove(lastModificationInfo);
                        Reload();

                        if (ModificationInfoConnector.Instance.Filter(true).Count == 0)
                            Close();
                    });
            }
            catch (Exception ex)
            {
                if (App.Current.Dispatch(app => button_ProgressCancel.IsEnabled) == true)
                    App.Current.Dispatch(app => MessageWindow.Show(ex.Message));
            }
        }

        void client_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            App.Current.Dispatch(app => progress_Download.Value = e.ProgressPercentage);
        }        

        void DownloadPackagesWindow_Closing_Cancel(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void button_ProgressCancel_Click(object sender, RoutedEventArgs e)
        {
            currentDownloadingClient.CancelAsync();
            button_ProgressCancel.IsEnabled = false;
        }

        private void filter_All_Checked(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void filter_AllVersions_Checked(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void filter_Anno_Checked(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void filter_Addon_Checked(object sender, RoutedEventArgs e)
        {
            Reload();
        }      
    }
}
