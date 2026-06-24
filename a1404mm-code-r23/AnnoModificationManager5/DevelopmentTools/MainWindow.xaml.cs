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
using DevelopmentTools.Editors.XmlModule.ModuleEditors;
using System.IO;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.UserInterface.Misc;
using System.Reflection;
using RDAExplorer;

namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow CurrentMainWindow;

        public MainWindow()
        {
            CurrentMainWindow = this;
            InitializeComponent();

            Title = "Development Tools Version " + Assembly.GetExecutingAssembly().GetName().Version;

            #region Load settings
            //Properties.Settings.Default.Upgrade();

            Width = Properties.Settings.Default.Window_Width;
            Height = Properties.Settings.Default.Window_Height;
            Left = Properties.Settings.Default.Window_X;
            Top = Properties.Settings.Default.Window_Y;
            WindowState = Properties.Settings.Default.Window_IsMaximized ? WindowState.Maximized : WindowState.Normal;
            #endregion

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Project.Development_CurrentProject != null)
            {
                if (MessageWindow.Show("Do you really want to close the Development Tools?", MessageWindow.MessageWindowType.YesNo)
                    == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            //Close current Project
            Project.Close();

            #region save settings
            if (WindowState == System.Windows.WindowState.Normal)
            {
                Properties.Settings.Default.Window_Width = Width;
                Properties.Settings.Default.Window_Height = Height;
                Properties.Settings.Default.Window_X = Left;
                Properties.Settings.Default.Window_Y = Top;
            }
            Properties.Settings.Default.Window_IsMaximized = WindowState == WindowState.Maximized;

            Properties.Settings.Default.Save();
            #endregion
        }

        public void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Title = "Development Tools Version " + Assembly.GetExecutingAssembly().GetName().Version;

            StartPage page = new StartPage();
            Content = page;
            UpdateLayout();
        }
    }
}
