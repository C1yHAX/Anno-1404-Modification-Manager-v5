using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using vbio = Microsoft.VisualBasic.FileIO;

namespace AnnoModificationManager4.Components
{
    public static class BackupHandler
    {
        public static bool IsValid(out string reason)
        {
            return IsValid(AnnoModificationManager4.Properties.Settings.Default.RDABackupDir, out reason);
        }

        public static bool IsValid(string folder, out string reason)
        {
            string currdir = folder.Trim('\\');

            if (!string.IsNullOrEmpty(currdir) &&
                   Directory.Exists(currdir))
            {
                if (Directory.Exists(currdir + "\\maindata"))
                {
                    if (AnnoVersionHandler.IsAddon() && !Directory.Exists(currdir + "\\addon"))
                    {
                        reason = "addon subfolder not found.";
                        return false;
                    }

                    reason = "OK";
                    return true;
                }
                else
                {
                    reason = "maindata subfolder don't exist.";
                    return false;
                }
            }

            reason = "Directory don't exist.";
            return false;
        }

        public static void CreateBackup(string folder)
        {
            folder = folder.Trim('\\');
            if (Directory.Exists(folder))
                vbio.FileSystem.DeleteDirectory(folder, vbio.UIOption.AllDialogs, vbio.RecycleOption.SendToRecycleBin, vbio.UICancelOption.ThrowException);
            Directory.CreateDirectory(folder);

            vbio.FileSystem.CopyDirectory(AnnoDirectoryHandler.GetCurrent() + "\\maindata", folder + "\\maindata", vbio.UIOption.AllDialogs, vbio.UICancelOption.ThrowException);
            if (AnnoVersionHandler.IsAddon() && Directory.Exists(AnnoDirectoryHandler.GetCurrent() + "\\addon"))
                vbio.FileSystem.CopyDirectory(AnnoDirectoryHandler.GetCurrent() + "\\addon", folder + "\\addon", vbio.UIOption.AllDialogs, vbio.UICancelOption.ThrowException);
        }
    }
}
