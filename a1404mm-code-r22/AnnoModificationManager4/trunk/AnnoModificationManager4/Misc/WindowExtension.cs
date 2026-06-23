using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;

namespace AnnoModificationManager4.Misc
{
    public class WindowExtension
    {
        #region Win32
        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        private static extern IntPtr GetSystemMenu(IntPtr hwnd, int revert);


        [DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
        private static extern int GetMenuItemCount(IntPtr hmenu);


        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        private static extern int RemoveMenu(IntPtr hmenu, int npos, int wflags);


        [DllImport("user32.dll", EntryPoint = "DrawMenuBar")]
        private static extern int DrawMenuBar(IntPtr hwnd);


        private const int MF_BYPOSITION = 0x0400;
        private const int MF_DISABLED = 0x0002; 
        #endregion

        public enum ExtensionType
        {
            RemoveCloseButton
        }

        public Window currentWindow;
        public ExtensionType extensionType= ExtensionType.RemoveCloseButton;

        private WindowExtension(ExtensionType type, Window win)
        {
            currentWindow = win;
            extensionType = type;

            switch (type)
            {
                case ExtensionType.RemoveCloseButton:
                    win.SourceInitialized += new EventHandler(Window_SourceInitialized);
                    break;
            }
        }       

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(currentWindow);
            IntPtr windowHandle = helper.Handle; //Get the handle of this window 

            IntPtr hmenu = GetSystemMenu(windowHandle, 0);
            int cnt = GetMenuItemCount(hmenu);

            //remove the button
            RemoveMenu(hmenu, cnt - 1, MF_DISABLED | MF_BYPOSITION);
            //remove the extra menu line
            RemoveMenu(hmenu, cnt - 2, MF_DISABLED | MF_BYPOSITION);
            DrawMenuBar(windowHandle); //Redraw the menu bar
        }

        ///
        public static void Append(ExtensionType type, Window win)
        {
            WindowExtension ext = new WindowExtension(type, win);
        }
    }
}
