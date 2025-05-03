using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using RDAExplorer.Misc;

namespace RDAExplorerGUI.Misc
{
    public class DirectoryExtension
    {
        private static string TempWorkingDirectory = "";

        public static void copyDirectory(string Src, string Dst)
        {
            String[] Files;

            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                Dst += Path.DirectorySeparatorChar;
            if (!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            Files = Directory.GetFileSystemEntries(Src);
            foreach (string Element in Files)
            {
                // Sub directories

                if (Directory.Exists(Element))
                    copyDirectory(Element, Dst + Path.GetFileName(Element));
                // Files in directory

                else
                    File.Copy(Element, Dst + Path.GetFileName(Element), true);
            }
        }

        public static string GetApplicationFolder()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName).Trim('\\');
        }

        public static string GetAppDataFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Trim('\\');
        }

        public static string GetTempWorkingDirectory()
        {
            if (!string.IsNullOrEmpty(TempWorkingDirectory))
                return TempWorkingDirectory;

            string path = Path.GetTempPath().Trim('\\') + "\\RDAExplorer\\Instance";
            path = StringExtension.MakeUnique(path, "", p => Directory.Exists(p));

            Directory.CreateDirectory(path);
            TempWorkingDirectory = path;

            return path;
        }

        public static void CleanDirectory(string dir)
        {
            string[] contentDirs = Directory.GetDirectories(dir);
            string[] contentFiles = Directory.GetFiles(dir);

            foreach (string d in contentDirs)
            {
                Directory.Delete(d, true);
            }
            foreach (string d in contentFiles)
            {
                File.Delete(d);
            }
        }
    }
}
