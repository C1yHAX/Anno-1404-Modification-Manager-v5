using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoModificationManager5.Components;
using AnnoModificationManager5.ModificationTypes;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.UserInterface.MainUI;
using AnnoModificationManager5.MainControls.Buttons;

namespace AnnoModificationManager5.UserInterface.Modern
{
    public partial class ModernMainWindow : Window
    {
        public static ModernMainWindow Current;

        private readonly List<ModRow> _rows = new List<ModRow>();
        private Modification _selected;
        private string _filterCategory;

        public ModernMainWindow()
        {
            InitializeComponent();
            Current = this;

            try
            {
                lbl_Version.Text = "Version " + Assembly.GetExecutingAssembly().GetName().Version;
                lbl_SideVersion.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version;
                set_UpdateInfo.Text = L("Set_InstalledVersion") + " " + Assembly.GetExecutingAssembly().GetName().Version;
            }
            catch (Exception) { }

            Loaded += delegate
            {
                Populate();
                SetActiveNav(nav_Overview);
                try { if (App.Splash != null) { App.Splash.Close(TimeSpan.FromMilliseconds(400)); App.Splash = null; } }
                catch (Exception) { }
            };
            Closed += delegate { Current = null; };
        }

        public void RefreshData()
        {
            if (Dispatcher.CheckAccess()) Populate();
            else Dispatcher.Invoke((Action)Populate);
        }

        private void Populate()
        {
            List<Modification> mods = ModificationHandler.Modifications ?? new List<Modification>();

            _rows.Clear();
            foreach (Modification mod in mods)
            {
                _rows.Add(new ModRow
                {
                    Name = Safe(mod.UICollector.Name),
                    Meta = Safe(mod.UICollector.VersionString) + "   ·   " + Safe(mod.UICollector.Author),
                    Category = Safe(mod.UICollector.Category),
                    Source = mod
                });
            }

            List<CategoryCard> cards = _rows
                .GroupBy(r => CatKey(r.Category))
                .OrderByDescending(g => g.Count())
                .Select(g => new CategoryCard
                {
                    Title = g.Key,
                    Subtitle = SubtitleFor(g.Key),
                    Glyph = GlyphFor(g.Key),
                    IconPath = IconFor(g.Key),
                    CountText = g.Count() + (g.Count() == 1 ? " Mod" : " Mods")
                }).ToList();
            ic_Categories.ItemsSource = cards;
            ic_CategoriesFull.ItemsSource = cards;

            if (_selected == null && _rows.Count > 0)
                _selected = _rows[0].Source;

            ShowOverviewCard();
            RefreshList();
            UpdateDetail(_selected);
            UpdateApplyButton();
        }

        private void RefreshList()
        {
            IEnumerable<ModRow> rows = _rows;

            if (!string.IsNullOrEmpty(_filterCategory))
                rows = rows.Where(r => CatKey(r.Category) == _filterCategory);

            string q = (txt_Search.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(q))
                rows = rows.Where(r =>
                    (r.Name ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Category ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);

            List<ModRow> list = rows.ToList();
            lst_Mods.ItemsSource = list;
            lbl_Filter.Text = string.IsNullOrEmpty(_filterCategory)
                ? list.Count + (list.Count == 1 ? " Mod" : " Mods")
                : L("Filter_Category").Replace("{0}", _filterCategory).Replace("{1}", list.Count.ToString());
        }

        private static string CatKey(string category)
        {
            return string.IsNullOrEmpty(category) ? L("Category_Other") : category;
        }

        private void ShowOverviewCard()
        {
            if (_selected == null)
            {
                lbl_ModName.Text = L("NoModsInstalled");
                lbl_ModMeta.Text = L("LoadModsHint");
                return;
            }
            lbl_ModName.Text = Safe(_selected.UICollector.Name);
            lbl_ModMeta.Text = Safe(_selected.UICollector.VersionString) + "   •   " + L("CreatedBy") + " " + Safe(_selected.UICollector.Author);
        }

        private void UpdateDetail(Modification mod)
        {
            if (mod == null)
            {
                d_Name.Text = L("NoModSelected");
                d_Meta.Text = "";
                d_Desc.Text = L("SelectModHint");
                d_StatusChip.Visibility = Visibility.Collapsed;
                btn_Activate.Visibility = Visibility.Collapsed;
                btn_Deactivate.Visibility = Visibility.Collapsed;
                return;
            }

            d_Name.Text = Safe(mod.UICollector.Name);
            d_Meta.Text = Safe(mod.UICollector.VersionString) + "   ·   " + Safe(mod.UICollector.Author);
            try { d_Desc.Text = mod.Info.Description.Get; } catch (Exception) { d_Desc.Text = ""; }

            d_StatusChip.Visibility = Visibility.Visible;
            try
            {
                // Use the cached activation status if available; compute (and cache) it
                // only for this single mod on first selection — never for the whole list.
                AnnoModificationManager5.ModificationTypes.ModificationActivationResponse resp;
                if (!ModificationHandler.ActivationResponses.TryGetValue(mod, out resp) || resp == null)
                {
                    resp = mod.CheckActivation();
                    ModificationHandler.ActivationResponses[mod] = resp;
                }
                switch (resp.Result())
                {
                    case Enums.Modification_ActivationStatus.Activated:
                        d_Status.Text = L("Status_Active");
                        btn_Activate.Visibility = Visibility.Collapsed;
                        btn_Deactivate.Visibility = Visibility.Visible;
                        break;
                    case Enums.Modification_ActivationStatus.Partially:
                        d_Status.Text = L("Status_Partially");
                        btn_Activate.Visibility = Visibility.Visible;
                        btn_Deactivate.Visibility = Visibility.Visible;
                        break;
                    default:
                        d_Status.Text = L("Status_Inactive");
                        btn_Activate.Visibility = Visibility.Visible;
                        btn_Deactivate.Visibility = Visibility.Collapsed;
                        break;
                }
            }
            catch (Exception)
            {
                d_StatusChip.Visibility = Visibility.Collapsed;
            }
        }

        private Modification SelectedMod()
        {
            ModRow row = lst_Mods.SelectedItem as ModRow;
            return row != null ? row.Source : _selected;
        }

        private void lst_Mods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModRow row = lst_Mods.SelectedItem as ModRow;
            if (row != null)
            {
                _selected = row.Source;
                ShowOverviewCard();
                UpdateDetail(_selected);
            }
        }

        private static string Safe(string s) { return string.IsNullOrEmpty(s) ? "" : s; }

        /// <summary>Shortcut for the ModernUI language dictionary.</summary>
        private static string L(string key)
        {
            return AnnoModificationManager5.Language.DictionarySystem.LanguageDictionary.Get("ModernUI", key);
        }

        private static string GlyphFor(string category)
        {
            string c = (category ?? "").ToLowerInvariant();
            if (c.Contains("venedig") || c.Contains("venice")) return "⛵";
            if (c.Contains("i.a.a.m") || c.Contains("iaam")) return "⚔";
            if (c.Contains("verschieden") || c.Contains("sonstige") || c.Contains("allgemein")) return "📦";
            if (c.Contains("anno")) return "⚓";
            return "🏰";
        }

        private static string SubtitleFor(string category)
        {
            string c = (category ?? "").ToLowerInvariant();
            if (c.Contains("venedig") || c.Contains("venice")) return L("Subtitle_Venice");
            if (c.Contains("i.a.a.m") || c.Contains("iaam")) return L("Subtitle_IAAM");
            return L("Subtitle_Default");
        }

        private static string IconFor(string category)
        {
            string c = (category ?? "").ToLowerInvariant();
            string name = "Icon_Retail_50.png";
            if (c.Contains("venedig") || c.Contains("venice")) name = "Icon_Addon_50.png";
            else if (c.Contains("i.a.a.m") || c.Contains("iaam")) name = "Icon_IAAM_50.png";
            return "pack://application:,,,/Images/" + name;
        }

        private void SetActiveNav(Button active)
        {
            Button[] all = { nav_Overview, nav_Mods, nav_Categories, nav_Settings, nav_Restore, nav_About };
            foreach (Button b in all)
            {
                b.Background = Brushes.Transparent;
                b.Foreground = (Brush)FindResource("TextDim");
            }
            active.Background = new SolidColorBrush(Color.FromArgb(0x22, 0x5B, 0x6C, 0xF0));
            active.Foreground = (Brush)FindResource("TextMain");
        }

        private void ShowView(UIElement view, Button nav, string title)
        {
            view_Overview.Visibility = Visibility.Collapsed;
            view_Mods.Visibility = Visibility.Collapsed;
            view_Categories.Visibility = Visibility.Collapsed;
            view_About.Visibility = Visibility.Collapsed;
            view_Settings.Visibility = Visibility.Collapsed;
            view_Restore.Visibility = Visibility.Collapsed;
            view.Visibility = Visibility.Visible;
            SetActiveNav(nav);
            lbl_Title.Text = title;
        }

        private bool ShowDialogModal(Window dlg)
        {
            try
            {
                dlg.Owner = this;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            catch (Exception) { }
            dimOverlay.Visibility = Visibility.Visible;
            try { return dlg.ShowDialog() == true; }
            finally { dimOverlay.Visibility = Visibility.Collapsed; }
        }

        private void nav_Overview_Click(object sender, RoutedEventArgs e) { ShowView(view_Overview, nav_Overview, L("Title_Overview")); }

        private void nav_Mods_Click(object sender, RoutedEventArgs e)
        {
            _filterCategory = null;
            RefreshList();
            ShowView(view_Mods, nav_Mods, L("Title_Mods"));
        }

        private void nav_Categories_Click(object sender, RoutedEventArgs e) { ShowView(view_Categories, nav_Categories, L("Title_Categories")); }

        private string _setAnnoDir;
        private string _setDataDir;

        private static readonly AnnoVersionHandler.AnnoVersion[] _versions = new AnnoVersionHandler.AnnoVersion[]
        {
            AnnoVersionHandler.AnnoVersion.Retail,
            AnnoVersionHandler.AnnoVersion.Patch1,
            AnnoVersionHandler.AnnoVersion.Patch2,
            AnnoVersionHandler.AnnoVersion.Patch3,
            AnnoVersionHandler.AnnoVersion.IAAM,
            AnnoVersionHandler.AnnoVersion.Addon1,
            AnnoVersionHandler.AnnoVersion.Addon1_Patch1,
            AnnoVersionHandler.AnnoVersion.HistoryEdition,
            AnnoVersionHandler.AnnoVersion.HistoryEdition_Addon,
        };

        private static string VersionLabel(AnnoVersionHandler.AnnoVersion v)
        {
            switch (v)
            {
                case AnnoVersionHandler.AnnoVersion.Patch1: return "Patch 1";
                case AnnoVersionHandler.AnnoVersion.Patch2: return "Patch 2";
                case AnnoVersionHandler.AnnoVersion.Patch3: return "Patch 3";
                case AnnoVersionHandler.AnnoVersion.IAAM: return "IAAM Mod";
                case AnnoVersionHandler.AnnoVersion.Addon1: return "Addon 1";
                case AnnoVersionHandler.AnnoVersion.Addon1_Patch1: return "Addon 1, Patch 1";
                case AnnoVersionHandler.AnnoVersion.HistoryEdition: return "History Edition";
                case AnnoVersionHandler.AnnoVersion.HistoryEdition_Addon: return "History Edition Addon";
                default: return "Retail";
            }
        }

        private void nav_Settings_Click(object sender, RoutedEventArgs e)
        {
            LoadSettingsView();
            ShowView(view_Settings, nav_Settings, L("Title_Settings"));
        }

        private void nav_Restore_Click(object sender, RoutedEventArgs e)
        {
            try { restoreView.ReloadItems(); } catch (Exception) { }
            ShowView(view_Restore, nav_Restore, L("Title_Restore"));
        }

        /// <summary>Show the embedded restore manager (used by the legacy menu entry too).</summary>
        public void ShowRestoreView()
        {
            nav_Restore_Click(null, null);
        }

        private void LoadSettingsView()
        {
            set_LangEn.IsChecked = Properties.Settings.Default.Language == "English";
            set_LangDe.IsChecked = Properties.Settings.Default.Language != "English";

            _setAnnoDir = Properties.Settings.Default.OverwrittenAnnoDirectory;
            _setDataDir = Properties.Settings.Default.OverwrittenDataFolder;

            try { set_AnnoDir.Text = AnnoDirectoryHandler.GetCurrent(); } catch (Exception) { set_AnnoDir.Text = _setAnnoDir; }
            try { set_DataDir.Text = DirectoryExtension.GetAMM4ApplicationDataFolder(); } catch (Exception) { }

            set_Version.Items.Clear();
            foreach (AnnoVersionHandler.AnnoVersion v in _versions)
                set_Version.Items.Add(VersionLabel(v));
            AnnoVersionHandler.AnnoVersion cur;
            try { cur = AnnoVersionHandler.GetCurrent(); } catch (Exception) { cur = AnnoVersionHandler.AnnoVersion.Retail; }
            int idx = Array.IndexOf(_versions, cur);
            set_Version.SelectedIndex = idx >= 0 ? idx : 0;
            try { set_VersionAuto.Text = "Automatisch erkannt: " + VersionLabel(AnnoVersionHandler.GetCurrentViaFilesize()); }
            catch (Exception) { set_VersionAuto.Text = ""; }

            try { set_BackupDir.Text = Properties.Settings.Default.RDABackupDir; } catch (Exception) { }
        }

        private void Set_ChooseAnno_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Anno 1404 (Anno4.exe / Anno1404.exe / Addon)|Anno4.exe;Anno1404.exe;Addon.exe;Anno1404Addon.exe";
            if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.FileName))
            {
                _setAnnoDir = System.IO.Path.GetDirectoryName(dlg.FileName);
                set_AnnoDir.Text = _setAnnoDir;
            }
        }

        private void Set_AutoAnno_Click(object sender, RoutedEventArgs e)
        {
            _setAnnoDir = "";
            set_AnnoDir.Text = "(wird beim Neustart automatisch erkannt)";
        }

        private void Set_ChooseData_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _setDataDir = dlg.SelectedPath;
                set_DataDir.Text = _setDataDir;
            }
        }

        private void Set_StandardData_Click(object sender, RoutedEventArgs e)
        {
            _setDataDir = "";
            set_DataDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).TrimEnd('\\') + "\\AnnoModificationManager5";
        }

        private void Set_ChooseBackup_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string seldir = dlg.SelectedPath.Trim('\\');
                string msg;
                if (!BackupHandler.IsValid(seldir, out msg))
                {
                    AnnoModificationManager5.UserInterface.Misc.MessageWindow.Show(msg);
                    return;
                }
                set_BackupDir.Text = seldir;
            }
        }

        private void Set_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.Language = set_LangEn.IsChecked == true ? "English" : "German";
                Properties.Settings.Default.OverwrittenAnnoDirectory = string.IsNullOrEmpty(_setAnnoDir) ? "" : _setAnnoDir;
                Properties.Settings.Default.OverwrittenDataFolder = string.IsNullOrEmpty(_setDataDir) ? "" : _setDataDir;

                if (set_Version.SelectedIndex >= 0 && set_Version.SelectedIndex < _versions.Length)
                {
                    AnnoVersionHandler.AnnoVersion sel = _versions[set_Version.SelectedIndex];
                    AnnoVersionHandler.AnnoVersion detected;
                    try { detected = AnnoVersionHandler.GetCurrentViaFilesize(); } catch (Exception) { detected = sel; }
                    Properties.Settings.Default.OverwrittenAnnoVersion = sel.Equals(detected) ? "" : sel.ToString();
                }
                else
                {
                    Properties.Settings.Default.OverwrittenAnnoVersion = "";
                }

                if (!string.IsNullOrEmpty(set_BackupDir.Text))
                    Properties.Settings.Default.RDABackupDir = set_BackupDir.Text;

                Properties.Settings.Default.StartupShown = true;
                Properties.Settings.Default.Save();
                set_Status.Text = L("Set_Saved");
                ApplicationExtension.RestartManager();
            }
            catch (Exception ex)
            {
                set_Status.Text = L("Error") + " " + ex.Message;
            }
        }

        #region Update-Check (Einstellungen)
        private const string UpdateApiLatest = "https://api.github.com/repos/C1yHAX/Anno-1404-Modification-Manager-v5/releases/latest";
        private const string UpdatePage = "https://github.com/C1yHAX/Anno-1404-Modification-Manager-v5/releases/latest";

        /// <summary>Download-URL des MSI-Assets aus dem neuesten GitHub-Release (gesetzt vom Update-Check).</summary>
        private string _updateMsiUrl;
        private Version _updateVersion;

        private void Set_CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            set_UpdateStatus.Text = L("Upd_Searching");
            set_OpenRelease.Visibility = Visibility.Collapsed;
            set_InstallUpdate.Visibility = Visibility.Collapsed;
            _updateMsiUrl = null;
            _updateVersion = null;

            System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
            string json = null;
            Exception error = null;

            worker.DoWork += delegate(object s, System.ComponentModel.DoWorkEventArgs args)
            {
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        client.Encoding = System.Text.Encoding.UTF8;
                        client.Headers.Add(System.Net.HttpRequestHeader.UserAgent, "AnnoModificationManager5");
                        json = client.DownloadString(UpdateApiLatest);
                    }
                }
                catch (Exception ex) { error = ex; }
            };

            worker.RunWorkerCompleted += delegate(object s, System.ComponentModel.RunWorkerCompletedEventArgs args)
            {
                if (error != null || string.IsNullOrEmpty(json))
                {
                    set_UpdateStatus.Text = L("Upd_Failed");
                    return;
                }

                try
                {
                    System.Web.Script.Serialization.JavaScriptSerializer ser =
                        new System.Web.Script.Serialization.JavaScriptSerializer();
                    Dictionary<string, object> release = ser.Deserialize<Dictionary<string, object>>(json);
                    string relName = release != null && release.ContainsKey("name") ? Convert.ToString(release["name"]) : "";
                    string tagName = release != null && release.ContainsKey("tag_name") ? Convert.ToString(release["tag_name"]) : "";

                    Version latest = ExtractVersion(relName);
                    if (latest == null)
                        latest = ExtractVersion(tagName);
                    Version current = NormalizeVersion(Assembly.GetExecutingAssembly().GetName().Version);

                    if (latest == null)
                    {
                        // Release found, but no parseable version number in name/tag.
                        set_UpdateStatus.Text = L("Upd_LatestOnGitHub")
                            .Replace("{0}", string.IsNullOrEmpty(relName) ? tagName : relName);
                        set_OpenRelease.Visibility = Visibility.Visible;
                    }
                    else if (NormalizeVersion(latest) > current)
                    {
                        _updateVersion = latest;
                        _updateMsiUrl = FindMsiAssetUrl(release);

                        set_UpdateStatus.Text = L("Upd_Available")
                            .Replace("{0}", latest.ToString()).Replace("{1}", current.ToString());
                        if (!string.IsNullOrEmpty(_updateMsiUrl))
                            set_InstallUpdate.Visibility = Visibility.Visible;
                        set_OpenRelease.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        set_UpdateStatus.Text = L("Upd_UpToDate").Replace("{0}", current.ToString());
                    }
                }
                catch (Exception)
                {
                    set_UpdateStatus.Text = L("Upd_ParseFailed");
                }
            };

            worker.RunWorkerAsync();
        }

        private void Set_OpenRelease_Click(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start(UpdatePage); }
            catch (Exception) { }
        }

        /// <summary>URL des ersten .msi-Assets im Release-JSON (bevorzugt AMM5_Setup.msi), sonst null.</summary>
        private static string FindMsiAssetUrl(Dictionary<string, object> release)
        {
            try
            {
                object assetsObj;
                if (release == null || !release.TryGetValue("assets", out assetsObj))
                    return null;

                System.Collections.IEnumerable assets = assetsObj as System.Collections.IEnumerable;
                if (assets == null)
                    return null;

                string firstMsi = null;
                foreach (object item in assets)
                {
                    Dictionary<string, object> asset = item as Dictionary<string, object>;
                    if (asset == null) continue;

                    string name = asset.ContainsKey("name") ? Convert.ToString(asset["name"]) : "";
                    string url = asset.ContainsKey("browser_download_url") ? Convert.ToString(asset["browser_download_url"]) : "";
                    if (string.IsNullOrEmpty(url) || !name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (name.Equals("AMM5_Setup.msi", StringComparison.OrdinalIgnoreCase))
                        return url;
                    if (firstMsi == null)
                        firstMsi = url;
                }
                return firstMsi;
            }
            catch (Exception) { return null; }
        }

        private void Set_InstallUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_updateMsiUrl))
                return;

            set_InstallUpdate.IsEnabled = false;
            set_UpdateStatus.Text = L("Upd_Downloading").Replace("{0}", "0");

            string target = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "AMM5_Setup_" + (_updateVersion != null ? _updateVersion.ToString() : "update") + ".msi");

            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
            System.Net.WebClient client = new System.Net.WebClient();
            client.Headers.Add(System.Net.HttpRequestHeader.UserAgent, "AnnoModificationManager5");

            client.DownloadProgressChanged += delegate(object s, System.Net.DownloadProgressChangedEventArgs args)
            {
                set_UpdateStatus.Text = L("Upd_Downloading").Replace("{0}", args.ProgressPercentage.ToString());
            };

            client.DownloadFileCompleted += delegate(object s, System.ComponentModel.AsyncCompletedEventArgs args)
            {
                client.Dispose();

                if (args.Error != null)
                {
                    set_UpdateStatus.Text = L("Upd_DownloadFailed") + " " + args.Error.Message;
                    set_InstallUpdate.IsEnabled = true;
                    return;
                }

                try
                {
                    // Start the MSI (MajorUpgrade replaces the installed version) and quit
                    // so no files of this instance are locked during the upgrade.
                    set_UpdateStatus.Text = L("Upd_StartingInstall");
                    System.Diagnostics.Process.Start("msiexec.exe", "/i \"" + target + "\"");
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    set_UpdateStatus.Text = L("Upd_InstallFailed") + " " + ex.Message;
                    set_InstallUpdate.IsEnabled = true;
                }
            };

            try
            {
                client.DownloadFileAsync(new Uri(_updateMsiUrl), target);
            }
            catch (Exception ex)
            {
                client.Dispose();
                set_UpdateStatus.Text = L("Upd_DownloadFailed") + " " + ex.Message;
                set_InstallUpdate.IsEnabled = true;
            }
        }

        /// <summary>First version number (e.g. 5.0.1) found in a release name/tag, else null.</summary>
        private static Version ExtractVersion(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            System.Text.RegularExpressions.Match m =
                System.Text.RegularExpressions.Regex.Match(text, @"\d+(\.\d+){1,3}");
            if (!m.Success)
                return null;
            Version v;
            return Version.TryParse(m.Value, out v) ? v : null;
        }

        /// <summary>Pad missing Build/Revision with 0 so 5.0.1 == 5.0.1.0 in comparisons.</summary>
        private static Version NormalizeVersion(Version v)
        {
            return new Version(v.Major, Math.Max(0, v.Minor), Math.Max(0, v.Build), Math.Max(0, v.Revision));
        }
        #endregion

        private void nav_About_Click(object sender, RoutedEventArgs e) { ShowView(view_About, nav_About, L("Title_About")); }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;
            _filterCategory = b.Tag as string;
            txt_Search.Text = "";
            RefreshList();
            ShowView(view_Mods, nav_Mods, L("Title_Mods"));
        }

        private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string q = (txt_Search.Text ?? "").Trim();
            lbl_SearchHint.Visibility = string.IsNullOrEmpty(q) ? Visibility.Visible : Visibility.Collapsed;
            RefreshList();
            if (!string.IsNullOrEmpty(q) && view_Mods.Visibility != Visibility.Visible)
                ShowView(view_Mods, nav_Mods, L("Title_Mods"));
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            RunActivationDialog(false);
        }

        private void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            RunActivationDialog(true);
        }

        private void RunActivationDialog(bool deactivate)
        {
            Modification mod = SelectedMod();
            if (mod == null) { d_Desc.Text = L("PleaseSelectMod"); return; }

            try
            {
                EnsureActivationResponses();

                bool ok;
                if (deactivate)
                {
                    DeactivationDialog dlg = new DeactivationDialog();
                    dlg.LoadModification(mod);
                    ok = ShowDialogModal(dlg);
                }
                else
                {
                    ActivationDialog dlg = new ActivationDialog();
                    dlg.LoadModification(mod);
                    ok = ShowDialogModal(dlg);
                }

                if (ok)
                {
                    EnsureActivationResponses();
                    if (MainWindow.CurrentMainWindow != null)
                        MainWindow.CurrentMainWindow.UpdateActivationResponses();
                    UpdateDetail(mod);
                    UpdateApplyButton();
                }
            }
            catch (Exception ex)
            {
                d_Desc.Text = L("Error") + " " + ex.Message;
            }
        }

        private static void EnsureActivationResponses()
        {
            foreach (Modification m in ModificationHandler.Modifications)
            {
                try { ModificationHandler.ActivationResponses[m] = m.CheckActivation(); }
                catch (Exception) { }
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainWindow.CurrentMainWindow != null)
                    MainWindow.CurrentMainWindow.ApplyPendingChanges();
            }
            catch (Exception ex) { d_Desc.Text = L("Error") + " " + ex.Message; }
            UpdateApplyButton();
        }

        private void UpdateApplyButton()
        {
            bool pending = false;
            try { pending = Modification.AMMRDA.Pending; } catch (Exception) { }
            btn_Apply.IsEnabled = pending;
            btn_Apply.ToolTip = pending
                ? L("ApplyTooltip_Pending")
                : L("ApplyTooltip_None");
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            if (mod == null) { d_Desc.Text = L("PleaseSelectMod"); return; }
            try
            {
                EnsureActivationResponses();
                DeleteDialog dlg = new DeleteDialog();
                dlg.LoadModification(mod);
                if (ShowDialogModal(dlg))
                {
                    _selected = null;
                    if (MainWindow.CurrentMainWindow != null)
                        MainWindow.CurrentMainWindow.ReloadModifications(true);
                }
            }
            catch (Exception ex) { d_Desc.Text = L("Error") + " " + ex.Message; }
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            if (mod == null) return;
            try
            {
                EnsureActivationResponses();
                ModificationStatusInformationDialog dlg = new ModificationStatusInformationDialog();
                dlg.SetModification(mod);
                ShowDialogModal(dlg);
            }
            catch (Exception) { }
        }

        private void Website_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            string url = mod != null ? mod.Info.Website : null;
            if (string.IsNullOrEmpty(url))
                url = "https://www.nexusmods.com/anno1404historyedition/mods/";
            Open(url);
        }

        private void AddMod_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Mod-Paket (*.zip)|*.zip";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() != true)
                return;

            bool any = false;
            foreach (string file in dlg.FileNames)
            {
                try
                {
                    if (ModificationHandler.Instance.AddModification(file))
                        any = true;
                }
                catch (Exception) { }
            }

            if (any && MainWindow.CurrentMainWindow != null)
                MainWindow.CurrentMainWindow.ReloadModifications(true);
        }

        private void Nexus_Click(object sender, RoutedEventArgs e)
        {
            try { ShowDialogModal(new Nexus.NexusBrowseWindow()); } catch (Exception) { }
            Refresh_Click(sender, e);
        }

        private void GitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadPackagesWindow w = new DownloadPackagesWindow();
                ShowDialogModal(w);
                if (w.HasDownloaded && MainWindow.CurrentMainWindow != null)
                    MainWindow.CurrentMainWindow.ReloadModifications(true);
            }
            catch (Exception) { }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentMainWindow != null)
                MainWindow.CurrentMainWindow.ReloadModifications(true);
        }

        private void LinkGitHub_Click(object sender, RoutedEventArgs e) { Open("https://github.com/C1yHAX/Anno-1404-Modification-Manager-v5"); }
        private void LinkNexus_Click(object sender, RoutedEventArgs e) { Open("https://www.nexusmods.com/anno1404historyedition/mods/"); }

        private static void Open(string url)
        {
            try { System.Diagnostics.Process.Start(url); } catch (Exception) { }
        }

        private void Min_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            btn_Max.Content = WindowState == WindowState.Maximized ? "❐" : "□";
        }

        private void Close_Click(object sender, RoutedEventArgs e) { Close(); }
    }

    public class CategoryCard
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string CountText { get; set; }
        public string Glyph { get; set; }
        public string IconPath { get; set; }
    }

    public class ModRow
    {
        public string Name { get; set; }
        public string Meta { get; set; }
        public string Category { get; set; }
        public Modification Source { get; set; }
    }
}
