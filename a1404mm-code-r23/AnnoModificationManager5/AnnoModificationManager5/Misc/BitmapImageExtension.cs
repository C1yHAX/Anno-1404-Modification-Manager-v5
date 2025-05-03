using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace AnnoModificationManager5.Misc
{
    public class BitmapImageExtension
    {
        private static Dictionary<string, BitmapImage> Cache =
            new Dictionary<string, BitmapImage>();

        /// <summary>
        /// With BitmapCacheOption.OnLoad
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static BitmapImage Load(string file)
        {
            if (Cache.ContainsKey(file))
            {
                return Cache[file];
            }

            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(file);
            img.EndInit();

            Cache.Add(file, img);
            return img;
        }
    }
}
