using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using AnnoModificationManager4.ModificationTypes;

namespace AnnoModificationManager4.Components
{
    public class AnnoVersionHandler
    {
        private static AnnoVersion LastAnnoVersion = AnnoVersion.None;

        public enum AnnoVersion
        {
            None,
            Retail,
            Patch1,
            Patch2,
            Patch3,
            Addon1,
            Addon1_Patch1,
            IAAM
        }

        public static readonly List<string> AnnoVersionList = new List<string>()
        {
            "None",
            "Retail",
            "Patch1",
            "Patch2",
            "Patch3",
            "Addon1",
            "Addon1_Patch1",
            "IAAM"
        };

        /// <summary>
        /// Current Version is addon
        /// </summary>
        /// <returns></returns>
        public static bool IsAddon()
        {
            var v = GetCurrent();

            return v.ToString().Contains("Addon1");
        }

        /// <summary>
        /// Modification is supporting addon directly (without "all")
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool IsAddon(ModificationInfo info)
        {
            return info.AnnoVersions.Any(w => w.ToLower().Contains("addon"));
        }


        public static bool IsAddonCompatible(ModificationInfo info)
        {
            if (info.AnnoVersions.Contains("All"))
                return true;
            return info.AnnoVersions.Any(w => w.ToLower().Contains("addon"));
        }


        public static AnnoVersion GetCurrentViaFilesize()
        {
            try
            {
                string annoexe = AnnoDirectoryHandler.GetCurrent() + "\\Anno4.exe";
                string addonexe = AnnoDirectoryHandler.GetCurrent() + "\\Addon.exe";

                if (File.Exists(addonexe))
                {
                    switch ((new FileInfo(addonexe)).Length)
                    {
                        case 16380504:
                            return AnnoVersion.Addon1;
                        case 16385440:
                            return AnnoVersion.Addon1_Patch1;
                    }
                }

                switch ((new FileInfo(annoexe)).Length)
                {
                    case 14708672:
                        return AnnoVersion.Retail;
                    case 15000944:
                        return AnnoVersion.Patch1;
                    case 14943648:
                        return AnnoVersion.Patch2;
                    case 14951840:
                        return AnnoVersion.Patch3;
                }
            }
            catch (Exception) { }

            return AnnoVersion.None;
        }

        public static AnnoVersion GetCurrent()
        {
            if (LastAnnoVersion != AnnoVersion.None)
                return LastAnnoVersion;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.OverwrittenAnnoVersion))
            {
                LastAnnoVersion = (AnnoVersion)Enum.Parse(typeof(AnnoVersion), Properties.Settings.Default.OverwrittenAnnoVersion);
                return LastAnnoVersion;
            }

            return GetCurrentViaFilesize();
        }

        public static bool IsCompatible(List<string> input)
        {
            if (input.Contains("All"))
                return true;
            if (input.Contains("Patch2") && GetCurrent() == AnnoVersion.Addon1)
                return true;
            if (input.Contains("Patch3") && GetCurrent() == AnnoVersion.Addon1_Patch1)
                return true;
            if (input.Contains("Addon1_Patch1") && GetCurrent() == AnnoVersion.IAAM)
                return true;
            return input.Contains(GetCurrent().ToString());
        }
    }
}
