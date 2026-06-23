using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace AnnoModificationManager4.Misc
{
    public static class XExtension
    {
        public static string Text(this XElement element)
        {
            if (element.Nodes().OfType<XText>().Count() != 0)
            {
                return element.Nodes().OfType<XText>().First().Value;
            }

            return "";
        }        

        public static string InnerXml(this XElement element)
        {
            XmlReader reader = element.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }

        public static string FormattetInnerXml(this XElement element)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var el in element.Nodes()) sb.AppendLine(el.ToString());
            return sb.ToString();
        }

        public static void SetXml(this XElement element, string InnerXml)
        {
            element.ReplaceNodes(XElement.Parse(InnerXml));
        }
        
        public static bool IsOrContainsParent(XElement nd, XElement tosearch)
        {
            if (nd == tosearch)
                return true;
            if (tosearch.Parent != null)
                return IsOrContainsParent(nd, tosearch.Parent);
            return false;
        }

        public static string IndentString(string xml)
        {
            try
            {
                XElement element = XElement.Parse("<Root>" + xml + "</Root>");
                return element.FormattetInnerXml();
            }
            catch (Exception)
            {
                return xml.Replace("><", ">\r\n<").Trim(new char[] { '\r', '\n' });
            }
        }
    }
}
