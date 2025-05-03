using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnoModificationManager4.Misc
{
    public static class StringBuilderExtension
    {
        public static void Set(this StringBuilder builder, string set)
        {
            if (builder.Length != 0)
            {
                builder.Remove(0, builder.Length);
            }

            builder.Insert(0, set);
        }
    }
}
