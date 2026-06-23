using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace AnnoModificationManager4.Misc
{
    public class DirectoryExtension
    {

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

        /// <summary>
        /// AppData/AMM4
        /// </summary>
        /// <returns></returns>
        public static string GetAMM4ApplicationDataFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).
                Trim('\\') + "\\AnnoModificationManager4";

            if (!string.IsNullOrEmpty(Properties.Settings.Default.OverwrittenDataFolder))
            {
                folder = Properties.Settings.Default.OverwrittenDataFolder;
            }

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        public static string GetAppDataFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Trim('\\');
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

        public static string UnifyDirectory(string path)
        {
            string op = path;
            while (Directory.Exists(op))
            {
                op = path + RandomProvider.Random.Next(11111111, 99999999);
            }
            return op;
        }
    }
}
