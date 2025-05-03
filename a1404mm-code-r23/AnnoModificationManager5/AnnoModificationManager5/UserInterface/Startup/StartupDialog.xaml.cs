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
using AnnoModificationManager5.Components;
using AnnoModificationManager5.Misc;
using System.IO;
using System.Diagnostics;

namespace AnnoModificationManager5.UserInterface.Startup
{
    /// <summary>
    /// Interaction logic for StartupDialog.xaml
    /// </summary>
    public partial class StartupDialog : Window
    {
        private OpenFileDialog openAnnoExecutable = new OpenFileDialog()
        {
            Filter = "Anno 1404|Anno4.exe"
        };

        public StartupDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            radio_AnnoDirectory_Auto.IsChecked = true;

            if (Properties.Settings.Default.Language == "German")
                radio_Language_German.IsChecked = true;
            else if (Properties.Settings.Default.Language == "English")
                radio_Language_English.IsChecked = true;
        }

        #region AnnoVersion
        private void RefreshAnnoVersion()
        {
            switch (AnnoVersionHandler.GetCurrentViaFilesize())
            {
                case AnnoVersionHandler.AnnoVersion.Patch1:
                    radio_Version_Patch1.IsChecked = true;
                    break;
                case AnnoVersionHandler.AnnoVersion.Patch2:
                    radio_Version_Patch2.IsChecked = true;
                    break;
                case AnnoVersionHandler.AnnoVersion.Patch3:
                    radio_Version_Patch3.IsChecked = true;
                    break;
                case AnnoVersionHandler.AnnoVersion.IAAM:
                    radio_Version_Mod_IAAM.IsChecked = true;
                    break;
                case AnnoVersionHandler.AnnoVersion.Addon1:
                    radio_Version_Addon1.IsChecked = true;
                    break;
                case AnnoVersionHandler.AnnoVersion.Addon1_Patch1:
                    radio_Version_Addon1_Patch1.IsChecked = true;
                    break;
                default:
                    radio_Version_Retail.IsChecked = true;
                    break;
            }

            RefreshAnnoVersion_MarkFileSize();
        }

        private void RefreshAnnoVersion_MarkFileSize()
        {
            radio_Version_Retail.FontWeight = FontWeights.Normal;
            radio_Version_Patch1.FontWeight = FontWeights.Normal;
            radio_Version_Patch2.FontWeight = FontWeights.Normal;
            radio_Version_Patch3.FontWeight = FontWeights.Normal;
            radio_Version_Mod_IAAM.FontWeight = FontWeights.Normal;
            radio_Version_Addon1.FontWeight = FontWeights.Normal;
            radio_Version_Addon1_Patch1.FontWeight = FontWeights.Normal;

            switch (AnnoVersionHandler.GetCurrentViaFilesize())
            {
                case AnnoVersionHandler.AnnoVersion.Patch1:
                    radio_Version_Patch1.FontWeight = FontWeights.Bold;
                    break;
                case AnnoVersionHandler.AnnoVersion.Patch2:
                    radio_Version_Patch2.FontWeight = FontWeights.Bold;
                    break;
                case AnnoVersionHandler.AnnoVersion.Patch3:
                    radio_Version_Patch3.FontWeight = FontWeights.Bold;
                    break;
                case AnnoVersionHandler.AnnoVersion.Addon1:
                    radio_Version_Addon1.FontWeight = FontWeights.Bold;
                    break;
                case AnnoVersionHandler.AnnoVersion.Addon1_Patch1:
                    radio_Version_Addon1_Patch1.FontWeight = FontWeights.Bold;
                    break;
                default:
                    radio_Version_Retail.FontWeight = FontWeights.Bold;
                    break;
            }
        }
        #endregion
        #region AnnoDirectory
        private void radio_AnnoDirectory_Auto_Checked(object sender, RoutedEventArgs e)
        {
            if (radio_AnnoDirectory_Auto.IsChecked == true)
            {
                field_AnnoDirectory.Text = AnnoDirectoryHandler.GetCurrent();
                RefreshAnnoVersion();
            }
        }

        private void radio_AnnoDirectory_Choose_Checked(object sender, RoutedEventArgs e)
        {
            if (radio_AnnoDirectory_Choose.IsChecked == true)
            {
                if (openAnnoExecutable.ShowDialog() == true)
                {
                    field_AnnoDirectory.Text = Path.GetDirectoryName(openAnnoExecutable.FileName);
                    RefreshAnnoVersion();
                }
                else
                    radio_AnnoDirectory_Auto.IsChecked = true;
            }
        }
        #endregion

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(field_AnnoDirectory.Text))
                return;
            if (!Directory.Exists(field_AnnoDirectory.Text))
                return;

            if (radio_AnnoDirectory_Choose.IsChecked == true)
                Properties.Settings.Default.OverwrittenAnnoDirectory = field_AnnoDirectory.Text;
            else
                Properties.Settings.Default.OverwrittenAnnoDirectory = "";

            AnnoModificationManager5.Components.AnnoVersionHandler.AnnoVersion checkedversion
                = AnnoVersionHandler.AnnoVersion.Retail;
            if (radio_Version_Retail.IsChecked == true)
                checkedversion = AnnoVersionHandler.AnnoVersion.Retail;
            else if (radio_Version_Patch1.IsChecked == true)
                checkedversion = AnnoVersionHandler.AnnoVersion.Patch1;
            else if (radio_Version_Patch2.IsChecked == true)
                checkedversion = AnnoVersionHandler.AnnoVersion.Patch2;
            else if (radio_Version_Patch3.IsChecked == true)
                checkedversion = AnnoVersionHandler.AnnoVersion.Patch3;
            else if (radio_Version_Mod_IAAM.IsChecked == true)
                checkedversion = AnnoVersionHandler.AnnoVersion.IAAM;
            else if (radio_Version_Addon1.IsChecked == true)
                checkedversion = AnnoVersionHandler.AnnoVersion.Addon1;
            else if (radio_Version_Addon1_Patch1.IsChecked == true)
                checkedversion = AnnoVersionHandler.AnnoVersion.Addon1_Patch1;

            if (!checkedversion.Equals(AnnoVersionHandler.GetCurrentViaFilesize()))
            {
                Properties.Settings.Default.OverwrittenAnnoVersion = checkedversion.ToString();
            }
            else
                Properties.Settings.Default.OverwrittenAnnoVersion = "";

            Properties.Settings.Default.StartupShown = true;
            Properties.Settings.Default.Save();

            ApplicationExtension.RestartManager();
        }

        private void radio_UpdateLanguage(object sender, RoutedEventArgs e)
        {
            bool restart = false;

            if (radio_Language_German.IsChecked == true && Properties.Settings.Default.Language != "German")
            {
                Properties.Settings.Default.Language = "German";
                restart = true;
            }
            else if (radio_Language_English.IsChecked == true && Properties.Settings.Default.Language != "English")
            {
                Properties.Settings.Default.Language = "English";
                restart = true;
            }

            if (restart)
            {
                Properties.Settings.Default.Save();

                ApplicationExtension.RestartManager();
            }
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void button_Help_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Language == "German")
            {
                Process.Start(DirectoryExtension.GetApplicationFolder() + "\\Help\\AMM4Help_Deutsch.chm");
            }
            else if (Properties.Settings.Default.Language == "English")
            {
                Process.Start(DirectoryExtension.GetApplicationFolder() + "\\Help\\AMM4Help_English.chm");
            }
        }
    }
}
