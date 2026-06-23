using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDAExplorer
{
    public class UISettings
    {
        /// <summary>
        /// If using UI -> Disable
        /// </summary>
        public static bool EnableConsole = true;

        /// <summary>
        /// if(currentIndex % _value_) => UpdateProgress
        /// </summary>
        public static int Progress_UpdateFileCount = 250;
    }
}
