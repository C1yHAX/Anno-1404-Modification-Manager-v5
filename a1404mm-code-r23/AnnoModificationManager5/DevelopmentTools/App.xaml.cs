using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private static void ApplyDarkTitleBar(object sender, RoutedEventArgs e)
        {
            try
            {
                Window w = sender as Window;
                if (w == null)
                    return;
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(w).Handle;
                if (hwnd == IntPtr.Zero)
                    return;
                int on = 1;
                if (DwmSetWindowAttribute(hwnd, 20, ref on, sizeof(int)) != 0)
                    DwmSetWindowAttribute(hwnd, 19, ref on, sizeof(int));

                // Force a dark caption (and light caption text) so the title bar does not
                // pick up the user's Windows accent colour (which can show up red).
                // DWMWA_CAPTION_COLOR = 35, DWMWA_TEXT_COLOR = 36 (Win11 22000+). COLORREF = 0x00BBGGRR.
                int captionColor = 0x002B1A0A;   // #0A1A2B
                int textColor = 0x00F6ECE3;      // #E3ECF6
                DwmSetWindowAttribute(hwnd, 35, ref captionColor, sizeof(int));
                DwmSetWindowAttribute(hwnd, 36, ref textColor, sizeof(int));
            }
            catch (Exception) { }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(ApplyDarkTitleBar));

            // Recolour the AvalonEdit XML highlighting for a dark background (VS-Code-like palette).
            ApplyDarkXmlHighlighting();

            //DevelopmentTools.Properties.Settings.Default.Upgrade();
            //Load all Plugins
            PluginSystem.PluginHandler.LoadPlugins();
        }

        private static void ApplyDarkXmlHighlighting()
        {
            try
            {
                IHighlightingDefinition xml = HighlightingManager.Instance.GetDefinition("XML");
                if (xml == null)
                    return;

                SetHighlightColor(xml, "Comment", 0x6A9955);
                SetHighlightColor(xml, "CData", 0x808080);
                SetHighlightColor(xml, "DocType", 0x808080);
                SetHighlightColor(xml, "XmlDeclaration", 0x808080);
                SetHighlightColor(xml, "XmlTag", 0x569CD6);
                SetHighlightColor(xml, "AttributeName", 0x9CDCFE);
                SetHighlightColor(xml, "AttributeValue", 0xCE9178);
                SetHighlightColor(xml, "Entity", 0xD7BA7D);
                SetHighlightColor(xml, "BrokenEntity", 0xD7BA7D);
            }
            catch (Exception) { }
        }

        private static void SetHighlightColor(IHighlightingDefinition def, string name, int rgb)
        {
            try
            {
                HighlightingColor c = def.GetNamedColor(name);
                if (c != null)
                    c.Foreground = new ThemeHighlightingBrush(
                        Color.FromRgb((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF)));
            }
            catch (Exception) { }
        }

        /// <summary>Public HighlightingBrush (SimpleHighlightingBrush is internal in AvalonEdit).</summary>
        private sealed class ThemeHighlightingBrush : HighlightingBrush
        {
            private readonly Brush _brush;
            public ThemeHighlightingBrush(Color c)
            {
                SolidColorBrush b = new SolidColorBrush(c);
                b.Freeze();
                _brush = b;
            }
            public override Brush GetBrush(ITextRunConstructionContext context) { return _brush; }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //Dispose all Plugins
            PluginSystem.PluginHandler.DisposePlugins();
        }
    }
}
