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

            // 2. - 5. Automatische Erkennung
            string detected = AutoDetect();
            if (!string.IsNullOrEmpty(detected))
            {
                LastDirectory = detected;
                return LastDirectory;
            }


            // 6. Manuelle Auswahl per Dialog
            MessageWindow.Show(LanguageDictionary.Get("Initialization", "AnnoFolder_NoFoundMessage"));

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // The second filter entry is deliberately permissive: some stores ship the game
            // with an executable name we do not know yet, and without it such an install
            // could never be selected at all.
            dlg.Filter = LanguageDictionary.Get("Initialization", "AnnoFolder_SelectExeFilter")
                + "|Anno4.exe;Anno1404.exe;Addon.exe;Anno1404Addon.exe|"
                + LanguageDictionary.Get("Initialization", "AnnoFolder_SelectExeFilterAll") + "|*.exe";
            dlg.Title = LanguageDictionary.Get("Initialization", "AnnoFolder_SelectExeTitle");

            if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.FileName))
            {
                string dir = Path.GetDirectoryName(dlg.FileName);
                if (!string.IsNullOrEmpty(dir))
                {
                    dir = dir.Trim('\\');

                    // Sanity check: without maindata this is not an Anno 1404 folder.
                    if (!Directory.Exists(Path.Combine(dir, "maindata")))
                    {
                        MessageWindow.Show(LanguageDictionary.Get("Initialization", "AnnoFolder_NoMaindata"));
                        ShutdownBecauseNoGame();
                        return "";
                    }

                    LastDirectory = dir;

                    // Remember the choice. Previously only the in-memory value was set, so
                    // the "Anno not found" dialog came back on every single start.
                    try
                    {
                        Properties.Settings.Default.OverwrittenAnnoDirectory = dir;
                        Properties.Settings.Default.Save();
                    }
                    catch (Exception) { }

                    return LastDirectory;
                }
            }

            ShutdownBecauseNoGame();
            return "";
        }

        private static string FindUbisoftAnnoInstall()
        {
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
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Locates the game without asking the user: classic registry key, Ubisoft Connect,
        /// Steam libraries and the well known default paths. Returns null when nothing was
        /// found. Shows no dialogs and has no side effects, so callers can use it to check
        /// whether automatic detection works at all.
        /// </summary>
        public static string AutoDetect()
        {
            // Registry: Klassische Version
            try
            {
                RegistryKey localMachine = Registry.LocalMachine;
                RegistryKey annoKey = null;

                if (IntPtr.Size == 4)
                    annoKey = localMachine.OpenSubKey(@"SOFTWARE\Ubisoft\Anno 1404\GameUpdate");
                else
                    annoKey = localMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Ubisoft\Anno 1404\GameUpdate");

                if (annoKey != null)
                {
                    object regValue = annoKey.GetValue("installdir");
                    if (regValue != null)
                    {
                        // Only accept the path when it really exists - the old code cached it
                        // unconditionally, which poisoned the lookup for the rest of the run.
                        string dir = regValue.ToString().Trim('\\');
                        if (Directory.Exists(dir))
                            return dir;
                    }
                }
            }
            catch (Exception)
            {
                // Registry nicht gefunden oder kein Zugriff
            }

            // Registry: Ubisoft Connect
            try
            {
                string found = FindUbisoftAnnoInstall();
                if (!string.IsNullOrEmpty(found))
                    return found;
            }
            catch (Exception)
            {
                // Registry nicht gefunden oder kein Zugriff
            }

            // Steam-Installationen (nicht in den Ubisoft-Registry-Schluesseln enthalten)
            try
            {
                string steamfound = FindSteamAnnoInstall();
                if (!string.IsNullOrEmpty(steamfound))
                    return steamfound;
            }
            catch (Exception)
            {
                // Steam nicht installiert oder kein Zugriff
            }

            // Typische Standardpfade der History Edition
            foreach (string path in HistoryEditionPaths)
            {
                try
                {
                    if (Directory.Exists(path))
                        return path;
                }
                catch (Exception) { }
            }

            return null;
        }


        /// <summary>
        /// Ends the application when no game directory could be determined. Shutdown() alone
        /// is not enough: the caller keeps running and the next access to the (already shut
        /// down) Application surfaced as "The Application object is being shut down.".
        /// </summary>
        private static void ShutdownBecauseNoGame()
        {
            try
            {
                if (Application.Current != null)
                    Application.Current.Shutdown();
            }
            catch (Exception) { }

            Environment.Exit(0);
        }

        /// <summary>
        /// Steam installations are not listed in the Ubisoft registry keys, so a Steam copy
        /// of the History Edition was never detected. Read the Steam root from the registry,
        /// collect every library folder and look for Anno 1404 in each library.
        /// </summary>
        private static string FindSteamAnnoInstall()
        {
            foreach (string library in GetSteamLibraryFolders())
            {
                string common = Path.Combine(library, "steamapps", "common");

                if (!Directory.Exists(common))
                    continue;

                foreach (string dir in Directory.GetDirectories(common))
                {
                    try
                    {
                        if (DirectoryContainsAnno1404(dir))
                            return dir.Trim('\\');
                    }
                    catch (Exception) { }
                }
            }

            return null;
        }

        /// <summary>Steam root plus every extra library folder from libraryfolders.vdf.</summary>
        private static List<string> GetSteamLibraryFolders()
        {
            List<string> libraries = new List<string>();
            string steamRoot = null;

            string[] machineKeys = new string[]
            {
                @"SOFTWARE\Wow6432Node\Valve\Steam",
                @"SOFTWARE\Valve\Steam"
            };

            foreach (string keyPath in machineKeys)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        object value = key == null ? null : key.GetValue("InstallPath");
                        if (value != null && !string.IsNullOrEmpty(value.ToString()))
                        {
                            steamRoot = value.ToString();
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }

            if (string.IsNullOrEmpty(steamRoot))
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                    {
                        object value = key == null ? null : key.GetValue("SteamPath");
                        if (value != null)
                            steamRoot = value.ToString();
                    }
                }
                catch (Exception) { }
            }

            if (string.IsNullOrEmpty(steamRoot))
                return libraries;

            steamRoot = steamRoot.Replace('/', '\\').Trim('\\');
            libraries.Add(steamRoot);

            // Extra libraries are listed as "path" entries inside libraryfolders.vdf.
            try
            {
                string vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");

                if (File.Exists(vdf))
                {
                    foreach (string line in File.ReadAllLines(vdf))
                    {
                        string trimmed = line.Trim();

                        if (!trimmed.StartsWith("\"path\"", StringComparison.OrdinalIgnoreCase))
                            continue;

                        int open = trimmed.IndexOf('"', 6);
                        int close = trimmed.LastIndexOf('"');

                        if (open < 0 || close <= open)
                            continue;

                        string path = trimmed.Substring(open + 1, close - open - 1)
                            .Replace("\\\\", "\\").Replace('/', '\\').Trim('\\');

                        if (!string.IsNullOrEmpty(path) &&
                            !libraries.Contains(path, StringComparer.OrdinalIgnoreCase))
                            libraries.Add(path);
                    }
                }
            }
            catch (Exception) { }

            return libraries;
        }


        private static bool DirectoryContainsAnno1404(string dir)
        {
            return File.Exists(Path.Combine(dir, "Anno1404.exe"))
                || File.Exists(Path.Combine(dir, "Anno1404Addon.exe"))
                || File.Exists(Path.Combine(dir, "Anno4.exe"))
                || File.Exists(Path.Combine(dir, "Addon.exe"));
        }
    }
}
