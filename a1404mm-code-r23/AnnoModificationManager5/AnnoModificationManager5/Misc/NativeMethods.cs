using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnoModificationManager5.Misc
{
    public static class NativeMethods
    {

        /// Return Type: DWORD->unsigned int
        ///hFile: HANDLE->void*
        ///lpFileSizeHigh: LPDWORD->DWORD*
        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetFileSize")]
        public static extern uint GetFileSize([System.Runtime.InteropServices.InAttribute()] System.IntPtr hFile, System.IntPtr lpFileSizeHigh);

    }
}
