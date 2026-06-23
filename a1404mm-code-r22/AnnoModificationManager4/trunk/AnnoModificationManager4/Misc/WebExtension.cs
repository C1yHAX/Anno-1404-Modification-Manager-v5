using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace AnnoModificationManager4.Misc
{
    public class WebExtension
    {
        #region Win32
        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);    
        #endregion

        public static int GetFileSize(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    System.Net.WebRequest req = System.Net.HttpWebRequest.Create(url);
                    req.Timeout = 500;
                    req.Method = "HEAD";
                    System.Net.WebResponse resp = req.GetResponse();
                    int ContentLength;
                    if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                    {
                        return ContentLength;
                    }
                }
                catch (Exception)
                {
                }
            }
            return -1;
        }

        public static bool ConnectionExists()
        {
            int description = 0;
            return InternetGetConnectedState(out description, 0);
        }
    }
}
