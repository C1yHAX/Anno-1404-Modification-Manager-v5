using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AnnoModificationManager5.ModificationTypes.Userdefined;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using AnnoModificationManager5.Misc;

namespace AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers
{

    public interface IXMLModifier
    {
        XmlModuleList Parent { get; set; }
        XMLFile XMLFile { get; }

        XMLFile TemporaryXMLFile { get; set; }

        string Selector { get; set; }
        string DeSelector { get; set; }
        string File { get; set; }
        string Group { get; set; }
        int Index { get; set; }

        bool IsActive { get; }

        object ModuleList_ToHeader { get; }

        List<XMLUserdefinedValue> UserdefinedValues { get; set; }

        XmlNode ToXML(XmlDocument doc);
        void Activate();
        void Deactivate();
        bool Validitate();
        bool ValiditateUserdefinedValueAppend();
        void CheckIntegrity();
        IXMLModifier Clone();
    }

    public class IXMLTools
    {
        public static object GenerateHeader(IXMLModifier mod, string type)
        {
            string sel = "";
            try
            {
                string[] s = XMLFile.Selector_ExtractPathAll(mod.Selector).Split('/');
                sel = "./" + s[s.Length - 1];

                if (mod is RemoveModifier)
                {
                    sel += "//" + (mod as RemoveModifier).TagName;
                }
                if (mod is AddModifier)
                {
                    sel += "//" + (mod as AddModifier).TagName;
                }
            }
            catch (Exception) { }

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            Image img = new Image();

            if (type == "Add")
                img.Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/add.png"));
            if (type == "Remove")
                img.Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/delete.png"));
            if (type == "Edit")
                img.Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/pencil.png"));

            img.Width = 14;
            img.Height = 14;
            img.Stretch = System.Windows.Media.Stretch.UniformToFill;

            stack.Children.Add(img);
            stack.Children.Add(new TextBlock() { Text = sel, Margin = new Thickness(3, 0, 0, 0) });
            stack.Children.Add(new TextBlock() { Text = " @ " + Path.GetFileName(mod.File), Foreground = Brushes.DarkGray });

            return stack;
        }
    }
}
