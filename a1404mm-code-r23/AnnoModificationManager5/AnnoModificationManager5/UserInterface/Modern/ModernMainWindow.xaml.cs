using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        public ModernMainWindow()
        {
            InitializeComponent();
            Current = this;
            Loaded += delegate { Populate(); SetActiveNav(nav_Overview); };
            Closed += delegate { Current = null; };
        }

        public void RefreshData()
        {
            if (Dispatcher.CheckAccess())
                Populate();
            else
                Dispatcher.Invoke((Action)Populate);
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
            lst_Mods.ItemsSource = null;
            lst_Mods.ItemsSource = _rows;

            var groups = _rows
                .GroupBy(r => string.IsNullOrEmpty(r.Category) ? "Sonstige" : r.Category)
                .OrderByDescending(g => g.Count());

            List<CategoryCard> cards = new List<CategoryCard>();
            foreach (var g in groups)
            {
                cards.Add(new CategoryCard
                {
                    Title = g.Key,
                    Subtitle = SubtitleFor(g.Key),
                    Glyph = GlyphFor(g.Key),
                    CountText = g.Count() + (g.Count() == 1 ? " Mod" : " Mods")
                });
            }
            ic_Categories.ItemsSource = cards;

            if (_selected == null && _rows.Count > 0)
                _selected = _rows[0].Source;

            ShowCard(_selected);
        }

        private void ShowCard(Modification mod)
        {
            if (mod == null)
            {
                lbl_ModName.Text = "Keine Mods installiert";
                lbl_ModMeta.Text = "Lade Mods über Nexus oder den Browser.";
                return;
            }
            lbl_ModName.Text = Safe(mod.UICollector.Name);
            lbl_ModMeta.Text = Safe(mod.UICollector.VersionString) + "   •   Erstellt von " + Safe(mod.UICollector.Author);
        }

        private Modification SelectedMod()
        {
            ModRow row = lst_Mods.SelectedItem as ModRow;
            if (row != null)
                return row.Source;
            return _selected;
        }

        private void lst_Mods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModRow row = lst_Mods.SelectedItem as ModRow;
            if (row != null)
            {
                _selected = row.Source;
                ShowCard(_selected);
            }
        }

        private static string Safe(string s)
        {
            return string.IsNullOrEmpty(s) ? "" : s;
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
            if (c.Contains("venedig") || c.Contains("venice")) return "Venedig-Erweiterung";
            if (c.Contains("i.a.a.m") || c.Contains("iaam")) return "I.A.A.M.-Erweiterung";
            return "Mods & Erweiterungen";
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

        private void ShowView(bool overview)
        {
            view_Overview.Visibility = overview ? Visibility.Visible : Visibility.Collapsed;
            view_Mods.Visibility = overview ? Visibility.Collapsed : Visibility.Visible;
        }

        private void nav_Overview_Click(object sender, RoutedEventArgs e)
        {
            SetActiveNav(nav_Overview);
            ShowView(true);
            lbl_Title.Text = "Übersicht";
        }

        private void nav_Mods_Click(object sender, RoutedEventArgs e)
        {
            SetActiveNav(nav_Mods);
            ShowView(false);
            lbl_Title.Text = "Mods";
        }

        private void nav_Categories_Click(object sender, RoutedEventArgs e)
        {
            SetActiveNav(nav_Categories);
            ShowView(true);
            lbl_Title.Text = "Kategorien";
        }

        private void nav_Settings_Click(object sender, RoutedEventArgs e)
        {
            try { new SettingsDialog().ShowDialog(); }
            catch (Exception) { }
            SetActiveNav(nav_Overview);
        }

        private void nav_About_Click(object sender, RoutedEventArgs e)
        {
            try { new AboutDialog().ShowDialog(); }
            catch (Exception) { }
            SetActiveNav(nav_Overview);
        }

        private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string q = (txt_Search.Text ?? "").Trim();
            lbl_SearchHint.Visibility = string.IsNullOrEmpty(q) ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrEmpty(q))
                lst_Mods.ItemsSource = _rows;
            else
                lst_Mods.ItemsSource = _rows.Where(r =>
                    (r.Name ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.Category ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (!string.IsNullOrEmpty(q) && view_Overview.Visibility == Visibility.Visible)
                nav_Mods_Click(this, null);
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            if (mod == null) { Hint("Bitte zuerst eine Mod auswählen (links → Mods)."); return; }

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
                }
            }
            catch (Exception ex)
            {
                Hint("Fehler: " + ex.Message);
            }
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            if (mod == null) { Hint("Bitte zuerst eine Mod auswählen (links → Mods)."); return; }

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
            catch (Exception ex)
            {
                Hint("Fehler: " + ex.Message);
            }
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            Modification mod = SelectedMod();
            if (mod == null) { nav_Mods_Click(this, null); return; }
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
            try { System.Diagnostics.Process.Start(url); }
            catch (Exception) { }
        }

        private void Updates_Click(object sender, RoutedEventArgs e)
        {
            try { new Nexus.NexusBrowseWindow().ShowDialog(); RefreshData(); }
            catch (Exception) { }
        }

        private void Hint(string text)
        {
            lbl_ModMeta.Text = text;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Min_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
        private void Close_Click(object sender, RoutedEventArgs e) { Close(); }
    }

    public class CategoryCard
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string CountText { get; set; }
        public string Glyph { get; set; }
    }

    public class ModRow
    {
        public string Name { get; set; }
        public string Meta { get; set; }
        public string Category { get; set; }
        public Modification Source { get; set; }
    }
}
