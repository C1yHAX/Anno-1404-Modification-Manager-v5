using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using AnnoModificationManager5.Components;

namespace AnnoModificationManager5.Nexus
{
    public partial class NexusBrowseWindow : Window
    {
        private const string Game = "anno1404historyedition";
        private NexusApiClient _api;
        private bool _isPremium;

        public bool HasDownloaded { get; private set; }

        public NexusBrowseWindow()
        {
            InitializeComponent();
            Loaded += NexusBrowseWindow_Loaded;
        }

        private void NexusBrowseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NexusLoginWindow.EnsureLogin())
            {
                Close();
                return;
            }

            _api = new NexusApiClient(NexusApiKeyStore.Get());
            try
            {
                NexusUser user = _api.ValidateUser();
                _isPremium = user.IsPremium;
                lbl_Login.Text = "Angemeldet: " + user.Name + (_isPremium ? " (Premium)" : " (free)");
                btn_Action.Content = _isPremium ? "Herunterladen" : "Auf Nexus öffnen";
            }
            catch (Exception ex)
            {
                lbl_Login.Text = "Login-Problem: " + ex.Message;
            }

            LoadList();
        }

        private string SelectedCategory()
        {
            ComboBoxItem item = cmb_Category.SelectedItem as ComboBoxItem;
            return item != null ? item.Tag as string : "latest_added";
        }

        private void LoadList()
        {
            if (_api == null)
                return;

            if (SelectedCategory() == "all")
            {
                LoadAll();
                return;
            }

            lbl_Status.Text = "Lade Mod-Liste …";
            Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                List<NexusMod> mods = _api.GetModList(Game, SelectedCategory());
                lst_Mods.ItemsSource = mods;
                lbl_Status.Text = mods.Count + " Mods.";
            }
            catch (Exception ex)
            {
                lbl_Status.Text = "Konnte Liste nicht laden: " + ex.Message;
            }
            finally
            {
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void LoadAll()
        {
            lbl_Status.Text = "Lade alle Mods …";
            btn_Refresh.IsEnabled = false;
            cmb_Category.IsEnabled = false;
            Cursor = System.Windows.Input.Cursors.Wait;

            System.Threading.Tasks.Task.Factory.StartNew((Action)delegate
            {
                List<NexusMod> all = new List<NexusMod>();
                try
                {
                    int maxId = 0;
                    foreach (NexusMod m in _api.GetModList(Game, "latest_added"))
                        if (m.ModId > maxId)
                            maxId = m.ModId;
                    if (maxId <= 0)
                        maxId = 40;

                    for (int id = 1; id <= maxId; id++)
                    {
                        NexusMod mod = null;
                        try { mod = _api.GetMod(Game, id); }
                        catch (Exception) { mod = null; }
                        if (mod != null)
                            all.Add(mod);

                        int done = id;
                        int total = maxId;
                        Dispatcher.Invoke((Action)delegate
                        {
                            lbl_Status.Text = "Lade alle Mods … " + done + "/" + total;
                        });
                    }
                }
                catch (Exception) { }

                Dispatcher.Invoke((Action)delegate
                {
                    all.Sort(delegate (NexusMod a, NexusMod b) { return b.ModId.CompareTo(a.ModId); });
                    lst_Mods.ItemsSource = all;
                    lbl_Status.Text = all.Count + " Mods.";
                    btn_Refresh.IsEnabled = true;
                    cmb_Category.IsEnabled = true;
                    Cursor = System.Windows.Input.Cursors.Arrow;
                });
            });
        }

        private void cmb_Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                LoadList();
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadList();
        }

        private void btn_Action_Click(object sender, RoutedEventArgs e)
        {
            NexusMod mod = lst_Mods.SelectedItem as NexusMod;
            if (mod == null)
            {
                lbl_Status.Text = "Bitte zuerst eine Mod auswählen.";
                return;
            }

            if (!_isPremium)
            {
                try { Process.Start("https://www.nexusmods.com/" + Game + "/mods/" + mod.ModId); }
                catch (Exception ex) { lbl_Status.Text = "Konnte den Browser nicht öffnen: " + ex.Message; }
                return;
            }

            DownloadPremium(mod);
        }

        private void DownloadPremium(NexusMod mod)
        {
            btn_Action.IsEnabled = false;
            Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                NexusFile file = _api.GetPrimaryFile(Game, mod.ModId);
                if (file == null)
                {
                    lbl_Status.Text = "Keine Datei zum Herunterladen gefunden.";
                    return;
                }
                if (file.FileName == null || !file.FileName.ToLowerInvariant().EndsWith(".zip"))
                {
                    lbl_Status.Text = "Diese Mod ist kein .zip (AMM-Format) und wird nicht unterstützt.";
                    return;
                }

                string uri = _api.GetDownloadUri(Game, mod.ModId, file.FileId, null, 0);
                if (string.IsNullOrEmpty(uri))
                {
                    lbl_Status.Text = "Kein Download-Link erhalten.";
                    return;
                }

                string destination = Path.Combine(Path.GetTempPath(), "nexus_" + mod.ModId + "_" + file.FileId + ".zip");
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.UserAgent, "AnnoModificationManager5/5.0");
                    client.DownloadFile(uri, destination);
                }

                if (ModificationHandler.Instance.AddModification(destination))
                {
                    HasDownloaded = true;
                    lbl_Status.Text = "Installiert: " + mod.Name;
                }
            }
            catch (Exception ex)
            {
                lbl_Status.Text = "Download fehlgeschlagen: " + ex.Message;
            }
            finally
            {
                btn_Action.IsEnabled = true;
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
