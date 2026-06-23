using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.Language.DictionarySystem;
using AnnoModificationManager5.Misc;
using System.Windows;
using System.IO;
using Microsoft.Win32;

namespace AnnoModificationManager5.Components
{
    public class AnnoDirectoryHandler
    {
        private static string LastDirectory = "";

        // Typische Installationspfade für die History Edition
        private static readonly string[] HistoryEditionPaths = new[]
        {
                @"C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\games\Anno 1404 History Edition",
                @"C:\Program Files\Ubisoft\Ubisoft Game Launcher\games\Anno 1404 History Edition"
            };

        public static string GetCurrent()
        {
            if (!string.IsNullOrEmpty(LastDirectory))
                return LastDirectory;

            // 1. Benutzerdefinierter Pfad aus den Einstellungen
            if (!string.IsNullOrEmpty(Properties.Settings.Default.OverwrittenAnnoDirectory))
            {
                LastDirectory = Properties.Settings.Default.OverwrittenAnnoDirectory;
                return LastDirectory;
            }

            // 2. Registry: Klassische Version
            try
            {
                RegistryKey localMachine = Registry.LocalMachine;
                RegistryKey annoKey = null;
                if (IntPtr.Size == 4)
                {
                    annoKey = localMachine.OpenSubKey(@"SOFTWARE\Ubisoft\Anno 1404\GameUpdate");
                }
                else
                {
                    annoKey = localMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Ubisoft\Anno 1404\GameUpdate");
                }
                if (annoKey != null)
                {
                    object regValue = annoKey.GetValue("installdir");
                    if (regValue != null)
                    {
                        LastDirectory = regValue.ToString().Trim('\\');
                        if (Directory.Exists(LastDirectory))
                            return LastDirectory;
                    }
                }
            }
            catch (Exception)
            {
                // Registry nicht gefunden oder kein Zugriff
            }

            // 3. Registry: History Edition (Ubisoft Connect)
            try
            {
                RegistryKey localMachine = Registry.LocalMachine;
                RegistryKey historyKey = null;
                if (IntPtr.Size == 4)
                {
                    historyKey = localMachine.OpenSubKey(@"SOFTWARE\Ubisoft\Launcher\Installs\13504");
                }
                else
                {
                    historyKey = localMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Ubisoft\Launcher\Installs\13504");
                }
                if (historyKey != null)
                {
                    object regValue = historyKey.GetValue("InstallDir");
                    if (regValue != null)
                    {
                        LastDirectory = regValue.ToString().Trim('\\');
                        if (Directory.Exists(LastDirectory))
                            return LastDirectory;
                    }
                }
            }
            catch (Exception)
            {
                // Registry nicht gefunden oder kein Zugriff
            }

            // 4. Typische Standardpfade der History Edition prüfen
            foreach (var path in HistoryEditionPaths)
            {
                if (Directory.Exists(path))
                {
                    LastDirectory = path;
                    return LastDirectory;
                }
            }

            // 5. Manuelle Auswahl per Dialog
            MessageWindow.Show(LanguageDictionary.Get("Initialization", "AnnoFolder_NoFoundMessage"));

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = LanguageDictionary.Get("Initialization", "AnnoFolder_SelectExeFilter")
                + "|Anno4.exe;Anno1404.exe;Addon.exe;Anno1404Addon.exe";
            dlg.Title = LanguageDictionary.Get("Initialization", "AnnoFolder_SelectExeTitle");

            if (dlg.ShowDialog() == true)
            {
                LastDirectory = Path.GetDirectoryName(dlg.FileName).Trim('\\');
                return LastDirectory;
            }
            else
            {
                Application.Current.Shutdown();
            }

            return "";
        }
    }
}
