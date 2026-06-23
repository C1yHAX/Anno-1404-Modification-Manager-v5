using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager4.UserInterface.Misc;
using AnnoModificationManager4.Language.DictionarySystem;
using AnnoModificationManager4.Misc;
using System.Windows;
using System.IO;

namespace AnnoModificationManager4.Components
{
    public class AnnoDirectoryHandler
    {
        private static string LastDirectory = "";

        public static string GetCurrent()
        {
            if (!string.IsNullOrEmpty(LastDirectory))
                return LastDirectory;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.OverwrittenAnnoDirectory))
            {
                LastDirectory = Properties.Settings.Default.OverwrittenAnnoDirectory;
                return LastDirectory;
            }

            try
            {
                Microsoft.Win32.RegistryKey localMachine = Microsoft.Win32.Registry.LocalMachine;
                if (IntPtr.Size == 4)
                {
                    localMachine = localMachine.OpenSubKey(@"SOFTWARE\Ubisoft\Anno 1404\GameUpdate");
                }
                else
                {
                    localMachine = localMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Ubisoft\Anno 1404\GameUpdate");
                }
                LastDirectory = localMachine.GetValue("installdir").ToString().Trim('\\');

                return LastDirectory;
            }
            catch (Exception)
            {
            }

            //if (LastDirectory == "")
            //{
            //    MessageWindow.Show(LanguageDictionary.Get("Initialization", "AnnoFolder_NoFoundMessage"));

            //    OpenFileDialog dlg = new OpenFileDialog();
            //    dlg.Filter = "Anno 1404 Executable|Anno4.exe";

            //    if (dlg.ShowDialog() == true)
            //    {
            //        LastDirectory = Path.GetDirectoryName(dlg.FileName).Trim('\\');
            //    }
            //    else
            //    {
            //        Application.Current.Shutdown();
            //    }
            //}

            return "";
        }
    }
}
