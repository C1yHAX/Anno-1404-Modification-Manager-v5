using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace RDAExplorerGUI.Misc
{
    public class ControlExtension
    {
        public static object BuildImageTextblock(string file, string message)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            try
            {
                stack.Children.Add(new Image()
                   {
                       Source = BitmapImageExtension.Load((file)),
                       Stretch = System.Windows.Media.Stretch.None
                   });
            }
            catch (Exception)
            {
            }
            stack.Children.Add(new TextBlock()
            {
                Text = message,
                Margin = new Thickness(5, 0, 0, 0)
            });

            return stack;
        }

        public static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
    }
}
