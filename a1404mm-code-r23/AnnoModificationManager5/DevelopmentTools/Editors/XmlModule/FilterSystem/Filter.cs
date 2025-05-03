using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using AnnoModificationManager5.Misc;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace DevelopmentTools.Editors.XmlModule.FilterSystem
{
    public class Filter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Name = "";
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _DestinationFile = "";
        public string DestinationFile
        {
            get
            {
                return _DestinationFile;
            }
            set
            {
                _DestinationFile = value;
                OnPropertyChanged("DestinationFile");
            }
        }

        private string _Selector = "";
        public string Selector
        {
            get
            {
                return _Selector;
            }
            set
            {
                _Selector = value;
                OnPropertyChanged("Selector");
            }
        }

        private string _Deselector = "";
        public string Deselector
        {
            get
            {
                return _Deselector;
            }
            set
            {
                _Deselector = value;
                OnPropertyChanged("Deselector");
            }
        }

        public List<FilterTripel> FilterValues { get; set; }

        //PropertyChanged
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        //To ComboboxHeader
        public object ToComboboxHeader
        {
            get
            {
                if (Name.Contains("|"))
                {
                    StackPanel stack = new StackPanel();
                    stack.Orientation = Orientation.Horizontal;

                    stack.Children.Add(new TextBlock { Text = Name.Split('|')[0], Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)) });
                    stack.Children.Add(new TextBlock { Text = Name.Split('|')[1], Margin = new System.Windows.Thickness(5, 0, 0, 0) });

                    return stack;
                }
                else
                    return new TextBlock() { Text = Name };
            }
        }

        public Filter()
        {
            FilterValues = new List<FilterTripel>();
        }

        public string ReplaceString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            //standard replacement
            foreach (FilterTripel tripel in FilterValues)
            {
                if (!tripel.IsAttribute)
                {
                    input = input.Replace(tripel.Key, tripel.Value);
                }
            }

            //attribute system
            foreach (FilterTripel tripel in FilterValues)
            {
                if (tripel.IsAttribute)
                {
                    if (!string.IsNullOrEmpty(tripel.Value))
                    {
                        input = input.Replace(tripel.Key, tripel.Attribute.Replace("{value}", tripel.Value));
                    }
                    else
                    {
                        input = input.Replace(tripel.Key, "");
                    }
                }
            }
          
            //clean
            while (input.Contains("++") || input.Contains("+]") || input.Contains("[+") || input.Contains("[]"))
            {
                input = input.Replace("++", "+").Replace("+]", "]").Replace("[+", "[").Replace("[]", "");
            }
            while (input.Contains("||") || input.Contains("|]") || input.Contains("[|") || input.Contains("[]"))
            {
                input = input.Replace("||", "|").Replace("|]", "]").Replace("[|", "[").Replace("[]", "");
            }
            input = input.Replace("()", "");

            input =  input.Replace("+", " and ");
            input =  input.Replace("|", " or ");

            return input;
        }

        #region Static
		public static List<Filter> Filters = new List<Filter>();

        public static void LoadFilters()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                Filters.Clear();

                doc.Load(DirectoryExtension.GetApplicationFolder() +
                    "\\Editors\\XmlModule\\XmlModule_SelectorFilters.xml");
                foreach (XmlNode nd in doc.FirstChild.ChildNodes)
                {
                    Filters.Add(Filter.FromXml(nd));
                }
            }
            catch (Exception)
            {              
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Filter", null);

            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "Name", Name));
            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "DestinationFile", DestinationFile));
            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "Selector", Selector));
            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "Deselector", Deselector));

            XmlNode default_replace = doc.CreateNode(XmlNodeType.Element, "KeyValueReplacement", null);
            foreach (FilterTripel trp in FilterValues.FindAll(t => !t.IsAttribute))
            {
                XmlNode nd = doc.CreateNode(XmlNodeType.Element, "Value", null);
                nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Name", trp.Name));
                nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Key", trp.Key));
                default_replace.AppendChild(nd);
            }
            newNode.AppendChild(default_replace);

            XmlNode attributes = doc.CreateNode(XmlNodeType.Element, "SelectorAttribute", null);
            foreach (FilterTripel trp in FilterValues.FindAll(t => t.IsAttribute))
            {
                XmlNode nd = doc.CreateNode(XmlNodeType.Element, "Value", null);
                nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Name", trp.Name));
                nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Key", trp.Key));
                nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Attribute", trp.Attribute));
                attributes.AppendChild(nd);
            }
            newNode.AppendChild(attributes);

            return newNode;
        }

        public static Filter FromXml(XmlNode node)
        {
            Filter filter = new Filter();
            filter.Name = node.Attributes["Name"].Value;

            filter.DestinationFile = XmlExtension.GetValue(node, "DestinationFile");
            filter.Selector = XmlExtension.GetValue(node, "Selector");
            filter.Deselector = XmlExtension.GetValue(node, "Deselector");


            foreach (XmlNode nd in node["KeyValueReplacement"].ChildNodes)
            {
                filter.FilterValues.Add(
                    new FilterTripel() { Key = nd.Attributes["Key"].Value, Name = nd.Attributes["Name"].Value, IsAttribute = false });
            }

            foreach (XmlNode nd in node["SelectorAttribute"].ChildNodes)
            {
                filter.FilterValues.Add(
                    new FilterTripel() { Key = nd.Attributes["Key"].Value, Name = nd.Attributes["Name"].Value, Attribute = nd.Attributes["Attribute"].Value, IsAttribute = true });
            }

            return filter;
        }

        public static void ToXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateNode(XmlNodeType.Element, "SelectorFilters", null);
            root.Attributes.Append(XmlExtension.CreateAttribute(doc, "Type", "XmlModule"));

            foreach(Filter fil in Filters)
                root.AppendChild(fil.ToXml(doc));
            doc.AppendChild(root);

            doc.Save("Editors\\XmlModule\\XmlModule_SelectorFilters.xml");
        }
	    #endregion        
    }
}
