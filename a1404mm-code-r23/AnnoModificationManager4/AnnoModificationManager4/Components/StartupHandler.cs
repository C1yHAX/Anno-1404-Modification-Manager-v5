using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using AnnoModificationManager4.Misc;
using System.Reflection;

namespace AnnoModificationManager4.Components
{
    public class StartupHandler
    {
        public static void Run()
        {
            try
            {
                AddToRegistry();
            }
            catch (Exception)
            {
            }
            try
            {
                AddToUserCount();
            }
            catch (Exception)
            {
            }
        }

        private static void AddToRegistry()
        {
            if (!RegistryExtension.SubKeyExist(@"SOFTWARE\AnnoModificationManager"))
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AnnoModificationManager");

                key.SetValue("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                key.SetValue("Directory", Environment.CurrentDirectory);
            }
            else
            {
                RegistryKey key2 = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AnnoModificationManager", true);

                string version = "";
                try
                {
                    version = key2.GetValue("Version").ToString();
                }
                catch (Exception) { }

                string directory = "";
                try
                {
                    directory = key2.GetValue("Directory").ToString();
                }
                catch (Exception) { }

                if (version != Assembly.GetExecutingAssembly().GetName().Version.ToString())
                {
                    key2.SetValue("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                }
                if (directory != Environment.CurrentDirectory)
                {
                    key2.SetValue("Directory", Environment.CurrentDirectory);
                }
            }
        }

        private static void AddToUserCount()
        {
            if (!Properties.Settings.Default.Web_UserCount)
            {
                try
                {
                    TimeoutWebClient client = new TimeoutWebClient();
                    client.DownloadData("http://tilegame.bplaced.net/AMM4Admin/CountUser.php?Version=" +
                        Assembly.GetExecutingAssembly().GetName().Version);
                    Properties.Settings.Default.Web_UserCount = true;
                    Properties.Settings.Default.Save();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
