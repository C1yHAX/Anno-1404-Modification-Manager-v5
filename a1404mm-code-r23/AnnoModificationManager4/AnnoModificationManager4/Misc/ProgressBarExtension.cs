using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnoModificationManager4.Misc
{
    public class ProgressBarExtension
    {
        public static int Calculate(float current, float max)
        {
            return (int)(current / max * 100);
        }
    }
}
