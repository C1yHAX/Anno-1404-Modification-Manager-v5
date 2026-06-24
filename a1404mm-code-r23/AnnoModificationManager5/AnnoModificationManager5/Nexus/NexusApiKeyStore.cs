using System;
using System.IO;
using AnnoModificationManager5.Misc;

namespace AnnoModificationManager5.Nexus
{
    public static class NexusApiKeyStore
    {
        private static string KeyFile
        {
            get { return DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\NexusApiKey.txt"; }
        }

        public static string Get()
        {
            try
            {
                if (File.Exists(KeyFile))
                    return File.ReadAllText(KeyFile).Trim();
            }
            catch (Exception) { }
            return null;
        }

        public static void Set(string key)
        {
            try { File.WriteAllText(KeyFile, (key ?? "").Trim()); }
            catch (Exception) { }
        }

        public static bool HasKey
        {
            get { return !string.IsNullOrEmpty(Get()); }
        }
    }
}
