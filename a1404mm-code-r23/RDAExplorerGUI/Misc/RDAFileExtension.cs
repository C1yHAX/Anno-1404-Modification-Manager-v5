using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AnnoModificationManager5.Misc;
using RDAExplorer;

namespace RDAExplorerGUI.Misc
{
    public static class RDAFileExtension
    {
        public static RDAFileTreeViewItem ToTreeViewItem(this RDAFile file)
        {
            #region Generate
            #region FileIcon
            string extension = Path.GetExtension(file.FileName).ToLower();
            string Icon = "pack://application:,,,/Images/Icons/page_white.png";

            if (extension == ".xml")
                Icon = "pack://application:,,,/Images/Icons/page_white_code.png";

            else if (extension == ".txt" || extension == ".ini" || extension == ".cfg")
                Icon = "pack://application:,,,/Images/Icons/page_white_text.png";

            else if (extension == ".jpg" || extension == ".bmp" || extension == ".png" ||
                extension == ".dds")
                Icon = "pack://application:,,,/Images/Icons/page_white_picture.png";

            else if (extension == ".mp3" || extension == ".wav" || extension == ".wma")
                Icon = "pack://application:,,,/Images/Icons/sound.png";
            #endregion

            RDAFileTreeViewItem newitem = new RDAFileTreeViewItem();
            newitem.Header = ControlExtension.BuildImageTextblock(Icon, Path.GetFileName(file.FileName));
            newitem.SemanticValue = "<File>";
            newitem.File = file;
            #endregion

            return newitem;
        }

        public static void SetFile(this RDAFile rdafile, string file, bool deleteOldFileInTemp)
        {
            rdafile.SetFile(file);

            if (deleteOldFileInTemp)
            {
                //Delete old file in temp
                if (System.IO.File.Exists(DirectoryExtension.GetTempWorkingDirectory() + "\\" + rdafile.FileName))
                {
                    try
                    {
                        File.Delete(DirectoryExtension.GetTempWorkingDirectory() + "\\" + rdafile.FileName);
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}
