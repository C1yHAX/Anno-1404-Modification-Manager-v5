using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using AnnoModificationManager5.Language.DictionarySystem;

namespace AnnoModificationManager5.UserInterface.Group
{
    public class PipeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!value.ToString().Contains("|"))
                return value;

            string[] sect = value.ToString().Split('|');

            StackPanel stack = new StackPanel()
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };
            stack.Children.Add(new TextBlock()
            {
                Text = sect[1],
                FontWeight = FontWeights.Bold,
                FontSize = 12
            });
            stack.Children.Add(new TextBlock()
            {
                Text = LanguageDictionary.Get("UserInterface", "For") + " " + sect[0],
                Margin = new System.Windows.Thickness(5, 0, 0, 0),
                Foreground = Brushes.Gray,
                FontSize = 12
            });       

            return stack;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
