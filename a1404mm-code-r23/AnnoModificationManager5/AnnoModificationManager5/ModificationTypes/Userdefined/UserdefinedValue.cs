using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AnnoModificationManager5.Language;
using System.Reflection;
using AnnoModificationManager5.Misc;
using win = System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using PixelLab.Wpf;
using System.Windows;

namespace AnnoModificationManager5.ModificationTypes.Userdefined
{
    
    public class UserdefinedValue
    {
        public Modification Parent;

        public string Name { get; set; }
        public string Group;
        public int Index = 0;       

        //Save into Current!
        public string Current { get; set; }

        public Label Label_Name = new Label() { Name = "Name" };
        public Label Label_Description = new Label() { Name = "Description" };

        //UI
        public string UI_Name
        {
            get
            {
                return !string.IsNullOrEmpty(Label_Name.Get) ? Label_Name.Get : Name;
            }
        }

        public string UI_Group
        {
            get
            {
                UserdefinedValueGroup gr = Parent.UserdefinedValueGroups.Find(g => g.InternalName == Group);
                if (gr != null && !string.IsNullOrEmpty(gr.Label_Name.Get))
                    return gr.Label_Name.Get;
                return Group;
            }
        }

        public string UI_Description
        {
            get
            {
                return Label_Description.Get;
            }
        }

        public ImageSource UI_Icon
        {
            get
            {
                if (Type == UserdefinedValueType.TextEdit)
                    return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/text_signature.png"));
                if (Type == UserdefinedValueType.Numeric)
                    return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/timeline_marker.png"));
                if (Type == UserdefinedValueType.ComboBox)
                    return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/text_list_bullets.png"));
                if (Type == UserdefinedValueType.ModifierEnabled | Type == UserdefinedValueType.FilesEnabled)
                    return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/tick.png"));
                return null;
            }
        }

        public object UI_Editor
        {
            get
            {
                if (Type == UserdefinedValueType.TextEdit)
                {
                    win.TextBox box = new win.TextBox();
                    box.TextWrapping = System.Windows.TextWrapping.NoWrap;
                    box.Text = Current;
                    box.TextChanged += delegate(object sender, win.TextChangedEventArgs e)
                    {
                        Current = box.Text;
                    };

                    return box;
                }
                else if (Type == UserdefinedValueType.Numeric)
                {
                    NumericUpDown num = new NumericUpDown();
                    num.Minimum = Numeric_Min;
                    num.Maximum = Numeric_Max;
                    num.Value = int.Parse(Current);
                    num.ValueChanged += delegate(object sender, System.Windows.RoutedPropertyChangedEventArgs<decimal> e)
                    {
                        Current = num.Value.ToString();
                    };

                    return num;
                }
                else if (Type == UserdefinedValueType.ComboBox)
                {
                    win.ComboBox box = new win.ComboBox();
                    box.Background = Brushes.White;
                    box.ItemsSource = ComboBoxItems;
                    box.DisplayMemberPath = "GetText";
                    box.SelectedItem = ComboBoxItems.Find(cm => cm.Value == Current);
                    if (box.SelectedItem == null)
                        box.SelectedIndex = 0;

                    box.SelectionChanged += delegate(object sender, win.SelectionChangedEventArgs e)
                    {
                        Current = (box.SelectedItem as UserdefinedValue_ComboBoxItem).Value;
                    };

                    return box;
                }
                else if (Type == UserdefinedValueType.ModifierEnabled | Type == UserdefinedValueType.FilesEnabled)
                {
                    win.CheckBox check = new win.CheckBox();
                    check.Content = Language.DictionarySystem.LanguageDictionary.Get("UserInterface", "Activated");

                    try
                    {
                        check.IsChecked = bool.Parse(Current);
                    }
                    catch (Exception)
                    {
                        check.IsChecked = true;
                        Current = "True";
                    }

                    check.Checked += delegate(object sender, RoutedEventArgs e)
                    {
                        Current = "True";
                    };
                    check.Unchecked += delegate(object sender, RoutedEventArgs e)
                    {
                        Current = "False";
                    };

                    return check;
                }
                return null;
            }
        }      

        //GetTypeString for UValueEditor[dev]
        public string GetTypeString
        {
            get
            {
                return Type.ToString();
            }
        }
 
        //Multiple Types
        public enum UserdefinedValueType
        {
            TextEdit,
            Numeric,
            ComboBox,
            ModifierEnabled,
            FilesEnabled
        }
        public UserdefinedValueType Type = UserdefinedValueType.TextEdit;

        //ComboBox
        public List<UserdefinedValue_ComboBoxItem> ComboBoxItems = new List<UserdefinedValue_ComboBoxItem>();

        //Numeric
        public int Numeric_Min = int.MinValue;
        public int Numeric_Max = int.MaxValue;

        //FilesEnabled
        public List<string> Files = new List<string>();

        public static List<UserdefinedValue> FromXml(string filename, Modification mod)
        {
            List<UserdefinedValue> list = new List<UserdefinedValue>();
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                if (node.Name == "UserdefinedValue")
                {
                    UserdefinedValue value = new UserdefinedValue();
                    value.Parent = mod;

                    //Read Attributes
                    value.Name = node.Attributes["Name"].Value;
                    value.Group = node.Attributes["Group"].Value;
                    value.Index = int.Parse(node.Attributes["Index"].Value);
                    value.Type = (UserdefinedValueType)Enum.Parse(typeof(UserdefinedValueType), node.Attributes["Type"].Value);

                    value.Current = node.Attributes["Current"] != null ? node.Attributes["Current"].Value : "";

                    //Read labels 
                    List<Label> lbs = Label.FromXml(node);
                    foreach (Label lb in lbs)
                    {
                        if (lb.Name == "Name")
                            value.Label_Name = lb;
                        else if (lb.Name == "Description")
                            value.Label_Description = lb;
                    }

                    //ComboBox
                    if (value.Type == UserdefinedValueType.ComboBox)
                    {
                        XmlNode Items = node["ComboBoxItems"];
                        foreach (XmlNode nd in Items.ChildNodes)
                        {
                            string cvalue = nd.Attributes["Value"].Value;

                            value.ComboBoxItems.Add(new UserdefinedValue_ComboBoxItem(Label.OneFromXml(nd["Label"]), cvalue));
                        }
                    }

                    //Numeric
                    if (value.Type == UserdefinedValueType.Numeric)
                    {
                        value.Numeric_Min = int.Parse(node.Attributes["Numeric_Min"].Value);
                        value.Numeric_Max = int.Parse(node.Attributes["Numeric_Max"].Value);
                    }

                    //FilesEnabled or ModifierEnabled
                    if (value.Type == UserdefinedValueType.FilesEnabled || value.Type== UserdefinedValueType.ModifierEnabled)
                    {
                        if (node.SelectSingleNode("Files") != null)
                        {
                            XmlNode Items = node["Files"];
                            foreach (XmlNode nd in Items.ChildNodes)
                            {
                                string cvalue = nd.Attributes["File"].Value;

                                value.Files.Add(cvalue);
                            }
                        }
                    }

                    list.Add(value);
                }
            }

            return list;
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "UserdefinedValue", null);

            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Name", Name));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Index", Index.ToString()));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Group", Group));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Type", Type.ToString()));

            if (!string.IsNullOrEmpty(Current))
            {
                XmlAttribute current = doc.CreateAttribute("Current");
                current.Value = Current;
                nd.Attributes.Append(current);
            }

            nd.AppendChild(Label.ToXml(new List<Label> { Label_Name, Label_Description }, doc));

            if (Type == UserdefinedValueType.ComboBox)
            {
                XmlNode comboboxes = doc.CreateNode(XmlNodeType.Element, "ComboBoxItems", null);
                foreach (UserdefinedValue_ComboBoxItem citem in ComboBoxItems)
                {
                    XmlNode nitem = doc.CreateNode(XmlNodeType.Element, "Item", null);
                    nitem.Attributes.Append(XmlExtension.CreateAttribute(doc, "Value", citem.Value));

                    nitem.AppendChild(citem.Name.ToXml(doc));
                    comboboxes.AppendChild(nitem);
                }

                nd.AppendChild(comboboxes);
            }

            if (Type == UserdefinedValueType.Numeric)
            {
                nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Numeric_Min", Numeric_Min.ToString()));
                nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Numeric_Max", Numeric_Max.ToString()));
            }

            if (Type == UserdefinedValueType.FilesEnabled || Type== UserdefinedValueType.ModifierEnabled)
            {
                XmlNode comboboxes = doc.CreateNode(XmlNodeType.Element, "Files", null);
                foreach (string file in Files)
                {
                    XmlNode nitem = doc.CreateNode(XmlNodeType.Element, "Item", null);
                    nitem.Attributes.Append(XmlExtension.CreateAttribute(doc, "File", file));                  
                    comboboxes.AppendChild(nitem);
                }
                nd.AppendChild(comboboxes);
            }

            return nd;
        }

        public static XmlNode ToXml(List<UserdefinedValue> labels, XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "UserdefinedValues", null);
            foreach (UserdefinedValue lb in labels)
            {
                nd.AppendChild(lb.ToXml(doc));
            }

            return nd;
        }

        public static void Save(string filename, List<UserdefinedValue> values)
        {
            XmlDocument doc = new XmlDocument();

            doc.AppendChild(UserdefinedValue.ToXml(values, doc));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CheckCharacters = false;
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.Entitize;
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(filename, settings);
            doc.Save(writer);

            writer.Close();
        }
    }
}
