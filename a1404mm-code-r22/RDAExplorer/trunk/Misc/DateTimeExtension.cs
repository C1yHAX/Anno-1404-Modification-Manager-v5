using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDAExplorer.Misc
{
    public class DateTimeExtension
    {
        public static DateTime FromTimeStamp(int timestamp)
        {
            DateTime t = new DateTime(1970, 1, 1);
            return t.AddSeconds(timestamp);
        }
    }
}
