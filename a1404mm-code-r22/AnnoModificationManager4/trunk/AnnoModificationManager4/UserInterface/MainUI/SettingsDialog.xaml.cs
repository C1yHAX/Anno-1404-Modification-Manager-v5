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
using AnnoModificationManager4.Components;
using AnnoModificationManager4.Misc;
using System.IO;
using AnnoModificationManager4.UserInterface.Misc;
using wf = System.Windows.Forms;
using AnnoModificationManager4.Language.DictionarySystem;

namespace AnnoModificationManager4.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private OpenFileDialog openAnnoExecutable = new OpenFileDialog()
        {
            Filter = "Anno 1404|Anno4.exe"
        };
        private wf.FolderBrowserDialog openDataFolder = new wf.FolderBrowserDialog();

        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //load data
            #region Language
            if (Properties.Settings.Default.Language == "German")
                radio_Language_German.IsChecked = true;
            else if (Properties.Settings.Default.Language == "English")
                radio_Language_English.IsChecked = true;
            #endregion
            #region Anno Directory
            field_AnnoDirectory.Text = AnnoDirectoryHandler.GetCurrent();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.OverwrittenAnnoDirectory))
            {
                radio_AnnoDirectory_Choose.IsChecked = true;
            }
            else
            {
                radio_AnnoDirectory_Auto.IsChecked = true;
            }
            #endregion
            #region Data Directory
            field_DataDirectory.Text = DirectoryExtension.GetAMM4ApplicationDataFolder();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.OverwrittenDataFolder))
            {
                radio_DataDirectory_Choose.IsChecked = true;
            }
            else
            {
                radio_DataDirectory_Auto.IsChecked = true;
            }
            #endregion
            #region Annoversion
            switch (AnnoVersionHandler.GetCurrent())
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
            #endregion
            #region ANNO backup
            txb_Backup_Folder.Text = Properties.Settings.Default.RDABackupDir;
            #endregion

            //At least add Handlers
            radio_AnnoDirectory_Auto.Checked += new RoutedEventHandler(radio_AnnoDirectory_Auto_Checked);
            //radio_AnnoDirectory_Auto.Unchecked += new RoutedEventHandler(radio_AnnoDirectory_Auto_Checked);
            radio_AnnoDirectory_Choose.Checked += new RoutedEventHandler(radio_AnnoDirectory_Choose_Checked);
            //radio_AnnoDirectory_Choose.Unchecked += new RoutedEventHandler(radio_AnnoDirectory_Choose_Checked);

            radio_DataDirectory_Auto.Checked += new RoutedEventHandler(radio_DataDirectory_Auto_Checked);
            radio_DataDirectory_Choose.Checked += new RoutedEventHandler(radio_DataDirectory_Choose_Checked);
        }

        #region DataDirectory
        void radio_DataDirectory_Choose_Checked(object sender, RoutedEventArgs e)
        {
            if (openDataFolder.ShowDialog() == wf.DialogResult.OK)
            {
                field_DataDirectory.Text = openDataFolder.SelectedPath;
            }
            else
                radio_DataDirectory_Auto.IsChecked = true;
        }

        void radio_DataDirectory_Auto_Checked(object sender, RoutedEventArgs e)
        {
            field_DataDirectory.Text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).
                Trim('\\') + "\\AnnoModificationManager4";
        }
        #endregion
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

            if (radio_Language_German.IsChecked == true)
                Properties.Settings.Default.Language = "German";
            else
                Properties.Settings.Default.Language = "English";

            if (radio_AnnoDirectory_Choose.IsChecked == true)
                Properties.Settings.Default.OverwrittenAnnoDirectory = field_AnnoDirectory.Text;
            else
                Properties.Settings.Default.OverwrittenAnnoDirectory = "";

            if (radio_DataDirectory_Choose.IsChecked == true)
                Properties.Settings.Default.OverwrittenDataFolder = field_DataDirectory.Text;
            else
                Properties.Settings.Default.OverwrittenDataFolder = "";

            AnnoModificationManager4.Components.AnnoVersionHandler.AnnoVersion checkedversion
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

            Properties.Settings.Default.RDABackupDir = txb_Backup_Folder.Text;

            Properties.Settings.Default.StartupShown = true;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();

            ApplicationExtension.RestartManager();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void txb_Backup_SetFolder_Click(object sender, RoutedEventArgs e)
        {
            wf.FolderBrowserDialog dlg = new wf.FolderBrowserDialog();
            if (dlg.ShowDialog() == wf.DialogResult.OK)
            {
                //string nondir = (AnnoDirectoryHandler.GetCurrent() + "\\maindata").ToLower();
                string seldir = dlg.SelectedPath.ToLower().Trim('\\');

                //if (nondir == seldir)
                //{
                //    MessageWindow.Show(LanguageDictionary.Get("MainUI", "RDAStart_WrongFolder"));
                //    return;
                //}

                //if (Directory.GetFiles(seldir, "*.rda").Length == 0)
                //{
                //    MessageWindow.Show(LanguageDictionary.Get("MainUI", "RDAStart_NoRDAs"));
                //    return;
                //}

                string msg;
                if (!BackupHandler.IsValid(seldir, out msg))
                {
                    MessageWindow.Show(msg);
                    return;
                }

                txb_Backup_Folder.Text = dlg.SelectedPath.Trim('\\');
            }
        }
    }
}
