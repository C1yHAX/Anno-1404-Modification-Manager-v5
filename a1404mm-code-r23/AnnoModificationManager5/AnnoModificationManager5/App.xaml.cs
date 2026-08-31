using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using AnnoModificationManager5.UserInterface.Startup;
using System.Diagnostics;
using AnnoModificationManager5.UserInterface.Misc;
using System.IO;
using AnnoModificationManager5.Components;

namespace AnnoModificationManager5
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static System.Windows.SplashScreen Splash;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private static void ApplyDarkTitleBar(object sender, RoutedEventArgs e)
        {
            try
            {
                Window w = sender as Window;
                if (w == null)
                    return;

                // Implicit Window styles only match the exact type, not subclasses, so apply the
                // dark borderless chrome here to every dialog window (except the windows with their own look).
                if (w.Style == null
                    && !(w is UserInterface.Modern.ModernMainWindow)
                    && !(w is MainWindow))
                {
                    System.Windows.Style chrome = Current.TryFindResource("DarkWindowChrome") as System.Windows.Style;
                    if (chrome != null)
                        w.Style = chrome;
                }

                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(w).Handle;
                if (hwnd == IntPtr.Zero)
                    return;
                int on = 1;
                if (DwmSetWindowAttribute(hwnd, 20, ref on, sizeof(int)) != 0)
                    DwmSetWindowAttribute(hwnd, 19, ref on, sizeof(int));
            }
            catch (Exception) { }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(ApplyDarkTitleBar));

            Nexus.NxmProtocolHandler.Register();

            if (e.Args != null && e.Args.Length > 0 && e.Args[0].StartsWith("nxm://", StringComparison.OrdinalIgnoreCase))
            {
                Nexus.NexusDownloadHandler.HandleNxm(e.Args[0]);
                Shutdown();
                return;
            }

            try { Splash = new System.Windows.SplashScreen("Images/Background/splash.png"); Splash.Show(false); }
            catch (Exception) { }

            // Migrate the settings of a previous version - but never overwrite values this
            // version already has. Upgrade() replaces the current user.config with the old
            // one, which wiped the RDABackupDir that StartupDialogBackup had just saved:
            // the backup check then failed again on every start and the backup dialog
            // reappeared forever (endless "auto or manual backup?" loop).
            if (!AnnoModificationManager5.Properties.Settings.Default.StartupShown
                && string.IsNullOrEmpty(AnnoModificationManager5.Properties.Settings.Default.RDABackupDir))
                AnnoModificationManager5.Properties.Settings.Default.Upgrade();

            //Only One Manager Instance
            if (Process.GetProcessesByName("AnnoModificationManager5").Length == 2)
            {
                MessageWindow.Show("Only one instance is allowed!");
                Process.GetCurrentProcess().Kill();
            }

            //Load Language
            Language.DictionarySystem.LanguageDictionary.Load();

            //Startup Dialog
            if (!AnnoModificationManager5.Properties.Settings.Default.StartupShown)
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
                    if (AnnoVersionHandler.GetCurrent().ToString() != AnnoModificationManager5.Properties.Settings.Default.LastAnnoVersion)
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
