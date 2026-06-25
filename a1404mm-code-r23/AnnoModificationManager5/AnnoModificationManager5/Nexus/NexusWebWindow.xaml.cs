using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AnnoModificationManager5.Components;
using AnnoModificationManager5.Misc;
using Microsoft.Web.WebView2.Core;

namespace AnnoModificationManager5.Nexus
{
    public partial class NexusWebWindow : Window
    {
        private const string Game = "anno1404historyedition";
        private const string ModsUrl = "https://www.nexusmods.com/" + Game + "/mods/";
        private readonly string _startUrl;

        public bool HasDownloaded { get; private set; }

        public NexusWebWindow(string startUrl)
        {
            InitializeComponent();
            _startUrl = string.IsNullOrEmpty(startUrl) ? ModsUrl : startUrl;
            Loaded += NexusWebWindow_Loaded;
        }

        private async void NexusWebWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string userData = Path.Combine(DirectoryExtension.GetAMM4ApplicationDataFolder(), "WebView2");
                Directory.CreateDirectory(userData);

                CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, userData);
                await web.EnsureCoreWebView2Async(env);

                web.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;
                web.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
                web.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;

                web.Source = new Uri(_startUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("WebView2 konnte nicht gestartet werden: " + ex.Message);
                Close();
            }
        }

        private void CoreWebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            txt_Url.Text = web.Source != null ? web.Source.ToString() : "";
        }

        private void CoreWebView2_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            try
            {
                string fileName = Path.GetFileName(e.ResultFilePath);
                string extension = Path.GetExtension(e.ResultFilePath);
                if (!string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
                {
                    lbl_Status.Text = "Download '" + fileName + "': keine .zip – nicht automatisch importierbar.";
                    return;
                }

                string destination = Path.Combine(Path.GetTempPath(),
                    "nexus_dl_" + Guid.NewGuid().ToString("N") + ".zip");
                e.ResultFilePath = destination;
                lbl_Status.Text = "Lade '" + fileName + "' …";

                CoreWebView2DownloadOperation operation = e.DownloadOperation;
                operation.StateChanged += delegate
                {
                    if (operation.State == CoreWebView2DownloadState.Completed)
                        Dispatcher.Invoke((Action)delegate { ImportDownloaded(operation.ResultFilePath); });
                    else if (operation.State == CoreWebView2DownloadState.Interrupted)
                        Dispatcher.Invoke((Action)delegate { lbl_Status.Text = "Download unterbrochen."; });
                };
            }
            catch (Exception)
            {
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            try
            {
                e.Handled = true;
                if (web.CoreWebView2 != null && !string.IsNullOrEmpty(e.Uri))
                    web.CoreWebView2.Navigate(e.Uri);
            }
            catch (Exception)
            {
            }
        }

        private void ImportDownloaded(string file)
        {
            try
            {
                if (ModificationHandler.Instance.AddModification(file))
                {
                    HasDownloaded = true;
                    lbl_Status.Text = "Importiert: " + Path.GetFileName(file);
                }
                else
                {
                    lbl_Status.Text = "Import fehlgeschlagen.";
                }
            }
            catch (Exception ex)
            {
                lbl_Status.Text = "Fehler beim Import: " + ex.Message;
            }
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (web.CoreWebView2 != null && web.CoreWebView2.CanGoBack)
                web.CoreWebView2.GoBack();
        }

        private void btn_Forward_Click(object sender, RoutedEventArgs e)
        {
            if (web.CoreWebView2 != null && web.CoreWebView2.CanGoForward)
                web.CoreWebView2.GoForward();
        }

        private void btn_Home_Click(object sender, RoutedEventArgs e)
        {
            if (web.CoreWebView2 != null)
                web.Source = new Uri(ModsUrl);
        }

        private void txt_Url_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || web.CoreWebView2 == null)
                return;
            string url = (txt_Url.Text ?? "").Trim();
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = "https://" + url;
            try { web.Source = new Uri(url); } catch (Exception) { }
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
