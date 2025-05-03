using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace AnnoModificationManager4.Misc
{
    public class RegistryExtension
    {
        public static bool SubKeyExist(string Subkey)
        {
            if (Registry.CurrentUser.OpenSubKey(Subkey) == null)
            {
                return false;
            }
            return true;
        }

        public static bool ValueExist(RegistryKey key, string Value)
        {
            try
            {
                if (key.GetValue(Value) == null)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
