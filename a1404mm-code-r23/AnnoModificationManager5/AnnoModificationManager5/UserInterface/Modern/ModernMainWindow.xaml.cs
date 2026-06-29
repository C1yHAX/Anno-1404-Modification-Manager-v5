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
                .GroupBy(r => string.IsNullOrEmpty(r.Category) ? "Sonstige" : r.Category)
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
                : "Kategorie: " + _filterCategory + "  (" + list.Count + ")";
        }

        private static string CatKey(string category)
        {
            return string.IsNullOrEmpty(category) ? "Sonstige" : category;
        }

        private void ShowOverviewCard()
        {
            if (_selected == null)
            {
                lbl_ModName.Text = "Keine Mods installiert";
                lbl_ModMeta.Text = "Lade Mods über Nexus oder den GitHub-Browser.";
                return;
            }
            lbl_ModName.Text = Safe(_selected.UICollector.Name);
            lbl_ModMeta.Text = Safe(_selected.UICollector.VersionString) + "   •   Erstellt von " + Safe(_selected.UICollector.Author);
        }

        private void UpdateDetail(Modification mod)
        {
            if (mod == null)
            {
                d_Name.Text = "Keine Mod ausgewählt";
                d_Meta.Text = "";
                d_Desc.Text = "Wähle links eine Mod aus.";
                d_StatusChip.Visibility = Visibility.Collapsed;
                btn_Activate.Content = "✓  Aktivieren";
                return;
            }

            d_Name.Text = Safe(mod.UICollector.Name);
            d_Meta.Text = Safe(mod.UICollector.VersionString) + "   ·   " + Safe(mod.UICollector.Author);
            try { d_Desc.Text = mod.Info.Description.Get; } catch (Exception) { d_Desc.Text = ""; }

            d_StatusChip.Visibility = Visibility.Visible;
            try
            {
                switch (mod.CheckActivation().Result())
                {
                    case Enums.Modification_ActivationStatus.Activated:
                        d_Status.Text = "Aktiv";
                        btn_Activate.Content = "✕  Deaktivieren";
                        break;
                    case Enums.Modification_ActivationStatus.Partially:
                        d_Status.Text = "Teilweise aktiv";
                        btn_Activate.Content = "✓  Aktivieren";
                        break;
                    default:
                        d_Status.Text = "Inaktiv";
                        btn_Activate.Content = "✓  Aktivieren";
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
            if (c.Contains("venedig") || c.Contains("venice")) return "Venedig-Erweiterung";
            if (c.Contains("i.a.a.m") || c.Contains("iaam")) return "I.A.A.M.-Erweiterung";
            return "Mods & Erweiterungen";
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
            Button[] all = { nav_Overview, nav_Mods, nav_Categories, nav_Settings, nav_About };
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
            view.Visibility = Visibility.Visible;
            SetActiveNav(nav);
            lbl_Title.Text = title;
        }

        private void nav_Overview_Click(object sender, RoutedEventArgs e) { ShowView(view_Overview, nav_Overview, "Übersicht"); }

        private void nav_Mods_Click(object sender, RoutedEventArgs e)
        {
            _filterCategory = null;
            RefreshList();
            ShowView(view_Mods, nav_Mods, "Mods");
        }

        private void nav_Categories_Click(object sender, RoutedEventArgs e) { ShowView(view_Categories, nav_Categories, "Kategorien"); }

        private void nav_Settings_Click(object sender, RoutedEventArgs e)
        {
            try { new SettingsDialog().ShowDialog(); } catch (Exception) { }
            SetActiveNav(view_Overview.Visibility == Visibility.Visible ? nav_Overview :
                         view_Mods.Visibility == Visibility.Visible ? nav_Mods :
                         view_Categories.Visibility == Visibility.Visible ? nav_Categories : nav_About);
        }

        private void nav_About_Click(object sender, RoutedEventArgs e) { ShowView(view_About, nav_About, "Über"); }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;
            _filterCategory = b.Tag as string;
            txt_Search.Text = "";
            RefreshList();
            ShowView(view_Mods, nav_Mods, "Mods");
        }

        private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string q = (txt_Search.Text ?? "").Trim();
            lbl_SearchHint.Visibility = string.IsNullOrEmpty(q) ? Visibility.Visible : Visibility.Collapsed;
            RefreshList();
            if (!string.IsNullOrEmpty(q) && view_Mods.Visibility != Visibility.Visible)
                ShowView(view_Mods, nav_Mods, "Mods");
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            if (mod == null) { d_Desc.Text = "Bitte zuerst eine Mod auswählen."; return; }

            try
            {
                bool ok;
                if (mod.CheckActivation().Result() == Enums.Modification_ActivationStatus.Activated)
                {
                    DeactivationDialog dlg = new DeactivationDialog();
                    dlg.LoadModification(mod);
                    ok = dlg.ShowDialog() == true;
                }
                else
                {
                    ActivationDialog dlg = new ActivationDialog();
                    dlg.LoadModification(mod);
                    ok = dlg.ShowDialog() == true;
                }

                if (ok)
                {
                    if (MainWindow.CurrentMainWindow != null)
                        MainWindow.CurrentMainWindow.UpdateActivationResponses();
                    new RDAChangesButton().ApplyChanges(true);
                    UpdateDetail(mod);
                }
            }
            catch (Exception ex)
            {
                d_Desc.Text = "Fehler: " + ex.Message;
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            if (mod == null) { d_Desc.Text = "Bitte zuerst eine Mod auswählen."; return; }
            try
            {
                DeleteDialog dlg = new DeleteDialog();
                dlg.LoadModification(mod);
                if (dlg.ShowDialog() == true)
                {
                    _selected = null;
                    if (MainWindow.CurrentMainWindow != null)
                        MainWindow.CurrentMainWindow.ReloadModifications(true);
                }
            }
            catch (Exception ex) { d_Desc.Text = "Fehler: " + ex.Message; }
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            if (mod == null) return;
            try
            {
                ModificationStatusInformationDialog dlg = new ModificationStatusInformationDialog();
                dlg.SetModification(mod);
                dlg.ShowDialog();
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

        private void Nexus_Click(object sender, RoutedEventArgs e)
        {
            try { new Nexus.NexusBrowseWindow().ShowDialog(); } catch (Exception) { }
            Refresh_Click(sender, e);
        }

        private void GitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadPackagesWindow w = new DownloadPackagesWindow();
                w.ShowDialog();
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
