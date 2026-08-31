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
using System.Diagnostics;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.Components;
using System.IO;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.Language.DictionarySystem;
using wf = System.Windows.Forms;

namespace AnnoModificationManager5.UserInterface.Startup
{
    /// <summary>
    /// Interaction logic for StartupDialogBackup.xaml
    /// </summary>
    public partial class StartupDialogBackup : Window
    {
        public StartupDialogBackup()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (r_AutoBackup.IsChecked == true)
            {
                //string srcdir = AnnoDirectoryHandler.GetCurrent() + "\\maindata";
                string dstdir = AnnoDirectoryHandler.GetCurrent() + "\\ModificationManager.backup";

                try
                {
                    //if (Directory.Exists(dstdir))
                    //{
                    //    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(
                    //        dstdir,
                    //        Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                    //        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    //}
                    //Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(
                    //    srcdir,
                    //    dstdir,
                    //    Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                    //    Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);

                    BackupHandler.CreateBackup(dstdir);

                    // Never restart into this same dialog: if the freshly created backup
                    // would not pass the startup check, say why instead of asking again
                    // on every start.
                    string checkmsg;
                    if (!BackupHandler.IsValid(dstdir, out checkmsg))
                    {
                        MessageWindow.Show(LanguageDictionary.Get("MainUI", "RDAStart_AutoBackupFailed")
                            + "\n\n" + checkmsg);
                        return;
                    }

                    Properties.Settings.Default.RDABackupDir = dstdir;
                    Properties.Settings.Default.LastAnnoVersion = AnnoVersionHandler.GetCurrent().ToString();
                    // The startup configuration is complete once a backup is set up. Without
                    // this, App keeps calling Settings.Upgrade() on every start, which
                    // overwrites the RDABackupDir saved here -> endless backup dialog.
                    Properties.Settings.Default.StartupShown = true;
                    Properties.Settings.Default.Save();
                    ApplicationExtension.RestartManager();
                }
                catch (Exception ex)
                {
                    MessageWindow.Show(LanguageDictionary.Get("MainUI", "RDAStart_AutoBackupFailed") + "\n\n" + ex.Message);
                }
            }
            else
            {
                wf.FolderBrowserDialog dlg = new wf.FolderBrowserDialog();

                if (!String.IsNullOrEmpty(Properties.Settings.Default.RDABackupDir) && Directory.Exists(Properties.Settings.Default.RDABackupDir))
                {
                    dlg.SelectedPath = Properties.Settings.Default.RDABackupDir;
                }

                if (dlg.ShowDialog() == wf.DialogResult.OK)
                {
                    //string nondir = (AnnoDirectoryHandler.GetCurrent() + "\\maindata").ToLower();
                    string seldir = dlg.SelectedPath.ToLower().Trim('\\');

                    //if (nondir == seldir)
                    //{
                    //    MessageWindow.Show(LanguageDictionary.Get("MainUI", "RDAStart_WrongFolder"));
                    //    return;
                    //}

                    string msg;
                    if (!BackupHandler.IsValid(seldir, out msg))
                    {
                        MessageWindow.Show(msg);
                        return;
                    }

                    Properties.Settings.Default.RDABackupDir = dlg.SelectedPath.Trim('\\');
                    Properties.Settings.Default.LastAnnoVersion = AnnoVersionHandler.GetCurrent().ToString();
                    Properties.Settings.Default.StartupShown = true;
                    Properties.Settings.Default.Save();
                    ApplicationExtension.RestartManager();
                }
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

        public void SetMessageToAnnoVersionChanged()
        {
            lbl_Message.Text = LanguageDictionary.Get("MainUI", "RDAStart_Message_AnnoVersionChanged");
        }
    }
}
