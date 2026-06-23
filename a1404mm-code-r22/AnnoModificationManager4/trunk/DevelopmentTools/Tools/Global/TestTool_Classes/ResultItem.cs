using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoModificationManager4.Misc;

namespace DevelopmentTools.Tools.Global.TestTool_Classes
{
    public class ResultItem
    {
        public ImageSource Icon { get; set; }
        public string Content { get; set; }

        public static ResultItem Generate(string message, string item)
        {
            ResultItem itm = new ResultItem();
            itm.Content = message;

            if (!string.IsNullOrEmpty(item))
                itm.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/" + item));

            return itm;
        }
    }
}
