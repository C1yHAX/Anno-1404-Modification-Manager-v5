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

        public StartPage()
        {
            InitializeComponent();            
        }

        void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = new ContextMenu();
            foreach (string file in Properties.Settings.Default.StartPage_RecentFiles.Split(';'))
            {
                try
                {
                    if (File.Exists(file))
                    {
                        MenuItem item = new MenuItem();
                        item.Header = new TextBlock() { Text = file };
                        item.Click += new RoutedEventHandler(RecentFile_Click);
                        item.Icon = new Image()
                        {
                            Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white.png"))
                        };

                        menu.Items.Add(item);
                    }
                }
                catch (Exception) { }
            }

            Project_RecentProjects.DropDown = menu;
        }

        void Open(string FileName)
        {
            Project project = new Project();
            project.OpenFile(FileName);

            MainWindow.CurrentMainWindow.Content = project.ProjectControl;
            MainWindow.CurrentMainWindow.Activate();
        }

        void RecentFile_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Open(((TextBlock)item.Header).Text);
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
                Open(OpenFile.FileName);
                if (!Properties.Settings.Default.StartPage_RecentFiles.Contains(OpenFile.FileName))
                {
                    Properties.Settings.Default.StartPage_RecentFiles += ";" + OpenFile.FileName;
                    Properties.Settings.Default.Save();
                }
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
            p.ShowDialog();
        }       

        private void ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(DirectoryExtension.GetApplicationFolder() + "\\Help\\DevHelp.chm");
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            (new SettingsWindow()).ShowDialog();
            MainWindow.CurrentMainWindow.MainWindow_Loaded(null, null);
        }         
    }
}
