using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using AnnoModificationManager4.ModificationTypes.XmlModule;

namespace AnnoModificationManager4.Misc
{
    public static class XmlExtension
    {
        /// <summary>
        /// To search for conflicts -> Looks backwards till parent == null
        /// </summary>
        /// <param name="nd"></param>
        /// <param name="tosearch"></param>
        /// <returns></returns>
        public static bool IsOrContainsParent(XmlNode nd, XmlNode tosearch)
        {
            if (nd == tosearch)
                return true;
            if (tosearch.ParentNode != null)
                return IsOrContainsParent(nd, tosearch.ParentNode);
            return false;
        }

        public static List<string> GetChildNodeNames(this XmlNode nd)
        {
            List<string> i = new List<string>();
            foreach (XmlNode n in nd.ChildNodes)
            {
                if (!n.Name.Contains("#"))
                    i.Add(n.Name);
            }

            return i;
        }

        public static XmlAttribute CreateAttribute(XmlDocument doc, string Name, string Value)
        {
            XmlAttribute attr = doc.CreateAttribute(Name);
            attr.Value = Value;

            return attr;
        }

        public static XmlNode CreateElement(XmlDocument doc, string Name, string Value)
        {
            XmlNode attr = doc.CreateNode(XmlNodeType.Element, Name, null);
            attr.InnerXml = Value;

            return attr;
        }

        public static XmlNode CreateElementText(XmlDocument doc, string Name, string Value)
        {
            XmlNode attr = doc.CreateNode(XmlNodeType.Element, Name, null);
            attr.AppendChild(doc.CreateTextNode(Value));

            return attr;
        }

        /// <summary>
        /// Removes all \s[tag]\s and \r\n
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveEmptys(string input)
        {
            string newModifierValue = "";

            foreach (string i in input.Split('\n'))
            {
                newModifierValue += i.Trim();
            }

            return newModifierValue.Replace("\r", "").Replace("\n", "").Replace("\t", "");
        }

        public static string GetValue(XmlNode node, string Key)
        {
            var nd = node[Key];

            if (nd != null)
            {
                return nd.InnerXml;
            }
            else
            {
                var attrib = node.Attributes[Key];

                if (attrib != null)
                    return attrib.Value;
            }

            return "";
        }

        public static string GetValueText(XmlNode node, string Key)
        {
            var nd = node[Key];

            if (nd != null && nd.FirstChild is XmlText)
            {
                return ((XmlText)nd.FirstChild).Value;
            }

            return "";
        }

        public static List<XmlNode> FindNodeInChildren(XmlNode node, string TagName, string InnerXml)
        {
            List<XmlNode> found = node.ChildNodes.OfType<XmlNode>().ToList().FindAll(nd =>
                {
                    if (nd.Name == TagName)
                    {
                        if (nd.InnerXml.Equals(InnerXml))
                            return true;
                    }
                    return false;
                });

            return found;
        }

        public static string IndentString(string xml)
        {
            try
            {
                xml = xml.Replace("><", ">\r\n<");

                string indent = "";
                string output = "";

                foreach (string str in Regex.Split(xml, "\\r?\\n"))
                {
                    if (str.IndexOf("</") == 0)
                        indent = indent.Remove(0, 1);

                    output += indent + str + "\r\n";

                    if (str.IndexOf("<") == 0 && !str.Contains("/>") && !str.Contains("</"))
                        indent += "\t";
                }

                return output;
            }
            catch (Exception)
            {
                return xml;
            }
        }
    }
}
