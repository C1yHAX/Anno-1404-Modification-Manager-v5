using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using AnnoModificationManager4.UserInterface.Startup;
using System.Diagnostics;
using AnnoModificationManager4.UserInterface.Misc;
using System.IO;
using AnnoModificationManager4.Components;

namespace AnnoModificationManager4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!AnnoModificationManager4.Properties.Settings.Default.StartupShown)
                AnnoModificationManager4.Properties.Settings.Default.Upgrade();

            //Only One Manager Instance
            if (Process.GetProcessesByName("AnnoModificationManager4").Length == 2)
            {
                MessageWindow.Show("Only one instance is allowed!");
                Process.GetCurrentProcess().Kill();
            }

            //Load Language
            Language.DictionarySystem.LanguageDictionary.Load();

            //Startup Dialog
            if (!AnnoModificationManager4.Properties.Settings.Default.StartupShown)
            {
                (new StartupDialog()).ShowDialog();
            }

            //Backup startup
            {
                string msg;

                if (!BackupHandler.IsValid(out msg))
                {
                    StartupDialogBackup sbackup = new StartupDialogBackup();
                    sbackup.ShowDialog();
                }
                else
                {
                    if (AnnoVersionHandler.GetCurrent().ToString() != AnnoModificationManager4.Properties.Settings.Default.LastAnnoVersion)
                    {
                        StartupDialogBackup sbackup = new StartupDialogBackup();
                        sbackup.SetMessageToAnnoVersionChanged();
                        sbackup.ShowDialog();
                    }
                }
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
    }
}
