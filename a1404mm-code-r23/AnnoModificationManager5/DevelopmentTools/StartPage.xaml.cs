using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using AnnoModificationManager5.Misc;
using System.Diagnostics;

namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        OpenFileDialog OpenFile = new OpenFileDialog()
        {
            Filter = "Modification Project|*.zip"
        };

        public class RecentItem
        {
            public string Name { get; set; }
            public string PathText { get; set; }
            public string DateText { get; set; }
            public string File { get; set; }
        }

        public StartPage()
        {
            InitializeComponent();
        }

        void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecent();
        }

        private void LoadRecent()
        {
            List<RecentItem> items = new List<RecentItem>();
            try
            {
                string[] files = Properties.Settings.Default.StartPage_RecentFiles.Split(';');
                for (int i = files.Length - 1; i >= 0; i--)   // newest entries last -> show newest first
                {
                    string f = files[i];
                    if (string.IsNullOrEmpty(f) || string.IsNullOrEmpty(f.Trim()))
                        continue;
                    try
                    {
                        if (!System.IO.File.Exists(f))
                            continue;
                        FileInfo fi = new FileInfo(f);
                        items.Add(new RecentItem()
                        {
                            Name = Path.GetFileNameWithoutExtension(f),
                            PathText = f,
                            DateText = fi.LastWriteTime.ToString("dd.MM.yyyy HH:mm"),
                            File = f
                        });
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }

            ic_Recent.ItemsSource = items;
            lbl_NoRecent.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        void Open(string FileName)
        {
            Project project = new Project();
            project.OpenFile(FileName);

            MainWindow.CurrentMainWindow.Content = project.ProjectControl;
            MainWindow.CurrentMainWindow.Activate();
        }

        private void RecentCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string file = (sender as Button).Tag as string;
                if (!string.IsNullOrEmpty(file))
                    Open(file);
            }
            catch (Exception ex)
            {
                AnnoModificationManager5.UserInterface.Misc.MessageWindow.Show("Projekt konnte nicht geöffnet werden: " + ex.Message);
            }
        }

        private void Project_NewProject_Click(object sender, RoutedEventArgs e)
        {
            Project project = new Project();
            project.CreateProject();

            MainWindow.CurrentMainWindow.Content = project.ProjectControl;
        }

        private void Project_OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (OpenFile.ShowDialog() == true)
            {
                if (!Properties.Settings.Default.StartPage_RecentFiles.Contains(OpenFile.FileName))
                {
                    Properties.Settings.Default.StartPage_RecentFiles += ";" + OpenFile.FileName;
                    Properties.Settings.Default.Save();
                }
                Open(OpenFile.FileName);
            }
        }

        private void Project_ConvertProject_Click(object sender, RoutedEventArgs e)
        {
            DevelopmentTools.Tools.Converter.ConverterTool tool =
                new Tools.Converter.ConverterTool();
            MainWindow.CurrentMainWindow.Content = tool;
        }

        private void Project_StartPublisher_Click(object sender, RoutedEventArgs e)
        {
            DevelopmentTools.Tools.PackagePublisher.PackagePublisher p =
                new Tools.PackagePublisher.PackagePublisher();
            p.PreviousContent = MainWindow.CurrentMainWindow.Content;
            MainWindow.CurrentMainWindow.Content = p;
        }

        private void ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpView help = new HelpView();
            help.PreviousContent = MainWindow.CurrentMainWindow.Content;
            MainWindow.CurrentMainWindow.Content = help;
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.PreviousContent = MainWindow.CurrentMainWindow.Content;
            MainWindow.CurrentMainWindow.Content = settings;
        }

        private void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            AnnoModificationManager5.UserInterface.Misc.MessageWindow.Show("Du verwendest die aktuelle Version (5.0.0.0).");
        }
    }
}
