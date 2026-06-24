using System;
using System.Reflection;
using Microsoft.Win32;

namespace AnnoModificationManager5.Nexus
{
    public static class NxmProtocolHandler
    {
        private const string Root = @"Software\Classes\nxm";

        private static string ExePath
        {
            get { return Assembly.GetExecutingAssembly().Location; }
        }

        public static void Register()
        {
            try
            {
                using (RegistryKey root = Registry.CurrentUser.CreateSubKey(Root))
                {
                    if (root == null)
                        return;
                    root.SetValue("", "URL:Nexus Mods Protocol");
                    root.SetValue("URL Protocol", "");
                    using (RegistryKey command = root.CreateSubKey(@"shell\open\command"))
                        command.SetValue("", "\"" + ExePath + "\" \"%1\"");
                }
            }
            catch (Exception) { }
        }

        public static bool IsRegisteredToThisExe()
        {
            try
            {
                using (RegistryKey command = Registry.CurrentUser.OpenSubKey(Root + @"\shell\open\command"))
                {
                    if (command == null)
                        return false;
                    string value = command.GetValue("") as string;
                    return value != null && value.IndexOf(ExePath, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
