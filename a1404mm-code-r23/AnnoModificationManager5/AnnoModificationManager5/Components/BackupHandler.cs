using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AnnoModificationManager5.Language.DictionarySystem;
using vbio = Microsoft.VisualBasic.FileIO;

namespace AnnoModificationManager5.Components
{
    public static class BackupHandler
    {
        public static bool IsValid(out string reason)
        {
            return IsValid(AnnoModificationManager5.Properties.Settings.Default.RDABackupDir, out reason);
        }

        /// <summary>
        /// Does the installed game actually have an addon folder that can be backed up?
        /// The History Edition may be detected as an addon version (Anno1404Addon.exe is
        /// present) without shipping a separate "addon" archive folder.
        /// </summary>
        private static bool GameHasAddonFolder()
        {
            try
            {
                return Directory.Exists(AnnoDirectoryHandler.GetCurrent().Trim('\\') + "\\addon");
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValid(string folder, out string reason)
        {
            string currdir = folder.Trim('\\');

            if (!string.IsNullOrEmpty(currdir) &&
                   Directory.Exists(currdir))
            {
                if (Directory.Exists(currdir + "\\maindata"))
                {
                    // Only demand an addon backup when the installation actually has an
                    // addon folder to copy. Otherwise CreateBackup has nothing to back up,
                    // while this check keeps rejecting its own result -> the backup dialog
                    // reappears on every start (endless loop).
                    if (AnnoVersionHandler.IsAddon() && !Directory.Exists(currdir + "\\addon") && GameHasAddonFolder())
                    {
                        reason = LanguageDictionary.Get("MainUI", "Backup_Invalid_NoAddon");
                        return false;
                    }

                    reason = "OK";
                    return true;
                }
                else
                {
                    reason = LanguageDictionary.Get("MainUI", "Backup_Invalid_NoMaindata");
                    return false;
                }
            }

            reason = LanguageDictionary.Get("MainUI", "Backup_Invalid_NoDirectory");
            return false;
        }

        public static void CreateBackup(string folder)
        {
            folder = folder.Trim('\\');
            string source = AnnoDirectoryHandler.GetCurrent().Trim('\\');

            if (Directory.Exists(folder))
                vbio.FileSystem.DeleteDirectory(folder, vbio.UIOption.AllDialogs, vbio.RecycleOption.SendToRecycleBin, vbio.UICancelOption.ThrowException);
            Directory.CreateDirectory(folder);

            vbio.FileSystem.CopyDirectory(source + "\\maindata", folder + "\\maindata", vbio.UIOption.AllDialogs, vbio.UICancelOption.ThrowException);

            // Back up the addon archives whenever they exist — independent of the detected
            // version. Backing up a folder that is not strictly needed is harmless; missing
            // it means the addon can never be restored.
            if (Directory.Exists(source + "\\addon"))
                vbio.FileSystem.CopyDirectory(source + "\\addon", folder + "\\addon", vbio.UIOption.AllDialogs, vbio.UICancelOption.ThrowException);
        }
    }
}
