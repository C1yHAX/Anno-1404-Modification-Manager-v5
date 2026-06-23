using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoModificationManager4.Misc;

namespace DevelopmentTools.Editors.FileSystem
{
    public class FileItem
    {
        public string ItemPath { get; set; }
        public bool IsFolder { get; set; }
        public string Name
        {
            get
            {
                return Path.GetFileName(ItemPath);
            }
        }

        public ImageSource Icon
        {
            get
            {
                if (!IsFolder)
                {
                    switch (Path.GetExtension(ItemPath))
                    {
                        case ".png":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_picture.png"));
                        case ".bmp":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_picture.png"));
                        case ".tga":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_picture.png"));
                        case ".jpg":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_picture.png"));
                        case ".jpeg":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_picture.png"));
                        case ".dds":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_picture.png"));
                        case ".txt":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_text.png"));
                        case ".ini":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_gear.png"));
                        case ".xml":
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_code.png"));
                    }

                    return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white.png"));
                }
                else
                {
                    return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/folder.png"));
                }
            }
        }
    }
}
