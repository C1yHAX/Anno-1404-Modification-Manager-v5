using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AnnoModificationManager4.Misc
{
    public class ApplicationExtension
    {
        public static void RestartManager()
        {
            System.Threading.Thread.Sleep(500);
            Process.Start(DirectoryExtension.GetApplicationFolder() + "\\AnnoModificationManager4.exe");
            Process.GetCurrentProcess().Kill();
        }
    }
}
