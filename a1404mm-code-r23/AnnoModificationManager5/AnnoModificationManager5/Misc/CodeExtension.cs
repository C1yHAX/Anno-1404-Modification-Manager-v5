using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnoModificationManager5.Misc
{
    public class CodeExtension
    {
        public static void TC(Action t)
        {
            try
            {
                t();
            }
            catch (Exception)
            {
            }
        }

        public static void TC(Action t, Action<Exception> catched)
        {
            try
            {
                t();
            }
            catch (Exception ex)
            {
                catched(ex);
            }
        }
    }
}
