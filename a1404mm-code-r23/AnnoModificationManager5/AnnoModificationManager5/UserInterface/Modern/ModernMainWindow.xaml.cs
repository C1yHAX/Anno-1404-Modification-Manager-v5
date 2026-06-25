using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AnnoModificationManager5.Components;
using AnnoModificationManager5.ModificationTypes;

namespace AnnoModificationManager5.UserInterface.Modern
{
    public partial class ModernMainWindow : Window
    {
        private readonly List<ModRow> _rows = new List<ModRow>();

        public ModernMainWindow()
        {
            InitializeComponent();
            Loaded += delegate { Populate(); SetActiveNav(nav_Overview); };
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

            if (_rows.Count > 0)
            {
                ModRow rep = _rows[0];
                lbl_ModName.Text = rep.Name;
                lbl_ModMeta.Text = Safe(rep.Source.UICollector.VersionString) + "   •   Erstellt von " + Safe(rep.Source.UICollector.Author);
            }
            else
            {
                lbl_ModName.Text = "Keine Mods installiert";
                lbl_ModMeta.Text = "Lade Mods über den Browser oder Nexus.";
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
            if (c.Contains("verschieden") || c.Contains("sonstige")) return "📦";
            if (c.Contains("anno")) return "⚓";
            return "🏰";
        }

        private static string SubtitleFor(string category)
        {
            string c = (category ?? "").ToLowerInvariant();
            if (c.Contains("venedig") || c.Contains("venice")) return "Venedig-Erweiterung";
            if (c.Contains("i.a.a.m") || c.Contains("iaam")) return "I.A.A.M.-Erweiterung";
            if (c.Contains("verschieden") || c.Contains("sonstige")) return "Verschiedene Versionen";
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
            SetActiveNav(nav_Settings);
            try { new MainUI.SettingsDialog().ShowDialog(); }
            catch (Exception) { }
            SetActiveNav(nav_Overview);
        }

        private void nav_About_Click(object sender, RoutedEventArgs e)
        {
            SetActiveNav(nav_About);
            try { new MainUI.AboutDialog().ShowDialog(); }
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

        private void Updates_Click(object sender, RoutedEventArgs e)
        {
            lbl_ModMeta.Text = "Keine Updates gefunden.";
        }

        private void Activate_Click(object sender, RoutedEventArgs e) { Hint("Aktivieren/Anwenden erfolgt in der klassischen Ansicht."); }
        private void Info_Click(object sender, RoutedEventArgs e) { nav_Mods_Click(this, null); }
        private void Uninstall_Click(object sender, RoutedEventArgs e) { Hint("Deinstallieren erfolgt in der klassischen Ansicht."); }

        private void Website_Click(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("https://www.nexusmods.com/anno1404historyedition/mods/"); }
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
