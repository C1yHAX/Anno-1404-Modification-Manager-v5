using System.Windows;
using System.Windows.Controls.Primitives;

namespace AnnoModificationManager5.UserInterface.Misc
{
    /// <summary>
    /// Attached property used by the global dark window chrome (ControlStyle_DarkGlobal.xaml)
    /// to wire its caption "close" button to the hosting window without code-behind.
    /// </summary>
    public static class ChromeButtons
    {
        public static readonly DependencyProperty IsCloseProperty =
            DependencyProperty.RegisterAttached(
                "IsClose", typeof(bool), typeof(ChromeButtons),
                new PropertyMetadata(false, OnIsCloseChanged));

        public static bool GetIsClose(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCloseProperty);
        }

        public static void SetIsClose(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCloseProperty, value);
        }

        private static void OnIsCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonBase b = d as ButtonBase;
            if (b == null)
                return;
            if ((bool)e.NewValue)
                b.Click += CloseClick;
            else
                b.Click -= CloseClick;
        }

        private static void CloseClick(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(sender as DependencyObject);
            if (w != null)
                w.Close();
        }
    }
}
