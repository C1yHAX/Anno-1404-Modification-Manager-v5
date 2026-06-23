using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AnnoModificationManager4.Misc;

namespace AnnoModificationManager4.Language
{
    public class Label
    {
        public string Name { get; set; }
        public string German { get; set; }
        public string English { get; set; }

        public string Get
        {
            get
            {
                if (Properties.Settings.Default.Language == "English")
                {
                    return !string.IsNullOrEmpty(English) ? English : German;
                }
                return !string.IsNullOrEmpty(German) ? German : English;
            }
        }

        public Label Clone()
        {
            Label lb = new Label();
            lb.Name = Name;
            lb.German = German;
            lb.English = English;

            return lb;
        }

        #region Xml
        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "Label", null);

            XmlAttribute name = doc.CreateAttribute("Name");
            name.Value = Name;
            nd.Attributes.Append(name);

            XmlNode ger = doc.CreateNode(XmlNodeType.Element, "German", null);
            ger.AppendChild(doc.CreateTextNode(German != null ? German : ""));
            nd.AppendChild(ger);

            XmlNode eng = doc.CreateNode(XmlNodeType.Element, "English", null);
            eng.AppendChild(doc.CreateTextNode(English != null ? English : ""));
            nd.AppendChild(eng);

            return nd;
        }

        public static XmlNode ToXml(List<Label> labels, XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "Labels", null);
            foreach (Label lb in labels)
            {
                nd.AppendChild(lb.ToXml(doc));
            }

            return nd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nd">Parent Node With Labels-Child</param>
        /// <returns></returns>
        public static List<Label> FromXml(XmlNode nd)
        {
            List<Label> output = new List<Label>();
            if (nd.SelectSingleNode("Labels") != null)
            {
                foreach (XmlNode node in nd.SelectSingleNode("Labels").ChildNodes)
                {
                    if (node.Name == "Label")
                    {
                        Label lb = OneFromXml(node);
                        output.Add(lb);
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">Node.Name = Label</param>
        /// <returns></returns>
        public static Label OneFromXml(XmlNode node)
        {
            Label lb = new Label();

            lb.Name = node.Attributes["Name"].Value;

            var gernode = node.SelectSingleNode("German");
            var engnode = node.SelectSingleNode("English");

            if (gernode.FirstChild is XmlText)
            {
                lb.German = ((XmlText)gernode.FirstChild).Value;
            }

            if (engnode.FirstChild is XmlText)
            {
                lb.English = ((XmlText)engnode.FirstChild).Value;
            }

            return lb;
        }
        #endregion
    }
}
