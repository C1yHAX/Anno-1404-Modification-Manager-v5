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

            // 3. Registry: Ubisoft Connect – alle installierten Spiele durchsuchen.
            //    Die Spiel-ID variiert je Installation (z. B. 16232, nicht fest 13504),
            //    daher nehmen wir den Eintrag, dessen Ordner eine Anno-1404-EXE enthält.
            try
            {
                string found = FindUbisoftAnnoInstall();
                if (!string.IsNullOrEmpty(found))
                {
                    LastDirectory = found;
                    return LastDirectory;
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

            if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.FileName))
            {
                string dir = Path.GetDirectoryName(dlg.FileName);
                if (!string.IsNullOrEmpty(dir))
                {
                    LastDirectory = dir.Trim('\\');
                    return LastDirectory;
                }
            }

            // Keine gültige Auswahl getroffen -> Anwendung beenden.
            Application.Current.Shutdown();
            return "";
        }

        /// <summary>
        /// Durchsucht die in Ubisoft Connect registrierten Spiele und liefert das
        /// Installationsverzeichnis zurück, das eine Anno-1404-EXE (Klassik oder
        /// History Edition) enthält. Die Spiel-ID ist nicht fest, daher Enumeration.
        /// </summary>
        private static string FindUbisoftAnnoInstall()
        {
            // 32-Bit-Prozess sieht Wow6432Node transparent unter SOFTWARE\...
            string installsPath = IntPtr.Size == 4
                ? @"SOFTWARE\Ubisoft\Launcher\Installs"
                : @"SOFTWARE\Wow6432Node\Ubisoft\Launcher\Installs";

            using (RegistryKey installs = Registry.LocalMachine.OpenSubKey(installsPath))
            {
                if (installs == null)
                    return null;

                foreach (string id in installs.GetSubKeyNames())
                {
                    try
                    {
                        using (RegistryKey game = installs.OpenSubKey(id))
                        {
                            object dirVal = game == null ? null : game.GetValue("InstallDir");
                            if (dirVal == null)
                                continue;

                            string dir = dirVal.ToString().Replace('/', '\\').Trim('\\');
                            if (Directory.Exists(dir) && DirectoryContainsAnno1404(dir))
                                return dir;
                        }
                    }
                    catch (Exception)
                    {
                        // Diesen Eintrag überspringen
                    }
                }
            }

            return null;
        }

        /// <summary>Prüft, ob ein Ordner eine Anno-1404-EXE (Klassik oder History Edition) enthält.</summary>
        private static bool DirectoryContainsAnno1404(string dir)
        {
            return File.Exists(Path.Combine(dir, "Anno1404.exe"))
                || File.Exists(Path.Combine(dir, "Anno1404Addon.exe"))
                || File.Exists(Path.Combine(dir, "Anno4.exe"))
                || File.Exists(Path.Combine(dir, "Addon.exe"));
        }
    }
}
