using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AnnoModificationManager5.ModificationTypes.Userdefined;
using AnnoModificationManager5.ModificationTypes.ListModule;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.IO;
using System.Windows.Media;
using AnnoModificationManager5.Misc;

namespace AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers
{

    public interface IListModifier
    {
        ListModuleList Parent { get; set; }
        ListFile ListFile { get; }
        ListFile TemporaryListFile { get; set; }

        string File { get; set; }
        string Group { get; set; }
        int Index { get; set; }

        string ElementGroup { get; set; }
        string ElementValue { get; set; }

        bool IsActive { get; }

        object ModuleList_ToHeader { get; }

        List<ListUserdefinedValue> UserdefinedValues { get; set; }

        XmlNode ToXML(XmlDocument doc);
        void Activate();
        void Deactivate();
        bool Validitate();
        bool ValiditateUserdefinedValueAppend();
        void CheckIntegrity();
        IListModifier Clone();
    }

    public class IListTools
    {
        public static object GenerateHeader(IListModifier mod, string type)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            Image img = new Image();

            if (type == "Add")
                img.Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/add.png"));
            else if (type == "Remove")
                img.Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/delete.png"));
            else if (type == "Edit")
                img.Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/pencil.png"));
            else if (type.Contains("Group"))
                img.Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/key_add.png"));

            img.Width = 14;
            img.Height = 14;
            img.Stretch = System.Windows.Media.Stretch.UniformToFill;

            stack.Children.Add(img);
            stack.Children.Add(new TextBlock() { Text = type, Margin = new Thickness(3, 0, 0, 0) });
            stack.Children.Add(new TextBlock() { Text = " @ " + Path.GetFileName(mod.File), Foreground = Brushes.DarkGray });

            return stack;
        }
    }
}
