using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers;
using AnnoModificationManager4.ModificationTypes.Userdefined;

namespace AnnoModificationManager4.ModificationTypes.Userdefined
{
    
    public class XMLUserdefinedValue
    {
        public IXMLModifier Parent;

        public string UserdefinedValueName { get; set; }
        public string Math { get; set; }
        //public string OldValue;
        public string Key { get; set; }

        public XMLUserdefinedValue()
        {
            UserdefinedValueName = "";
            Math = "{value}";
            Key = "{}";
        }

        public bool Check()
        {
            UserdefinedValue uvalue = Parent.Parent.Parent.UserdefinedValues.Find(v => v.Name == UserdefinedValueName);
            return uvalue != null;                
        }

        public static string Append (string source, List<XMLUserdefinedValue> values)
        {
            foreach (XMLUserdefinedValue val in values)
            {
                UserdefinedValue uvalue = val.Parent.Parent.Parent.UserdefinedValues.Find(v => v.Name == val.UserdefinedValueName);

                if (uvalue != null)
                {
                    if (string.IsNullOrEmpty(val.Math) || !val.Math.Contains("{value}"))
                    {
                        source = source.Replace(val.Key, uvalue.Current);
                    }
                    //if (val.Math.Contains("{*}"))
                    //{
                    //    string i = val.Math.Replace("{value}", uvalue.Current).Replace("{*}", "");

                    //    try
                    //    {
                    //        MathParser.Parser p = new MathParser.Parser();
                    //        p.Parse(i);
                    //        source = source.Replace(val.Key, p.Result.ToString());
                    //    }
                    //    catch (Exception)
                    //    {
                    //        source = source.Replace(val.Key, i);
                    //    }
                    //}
                    else
                    {
                        string i = val.Math.Replace("{value}", uvalue.Current);
                        source = source.Replace(val.Key, i);
                    }
                }
                else
                {
                    Console.WriteLine("Couldn't find UserdefindedValue '" + val.UserdefinedValueName + "'");
                    val.Parent.Parent.Parent.SendMessage("Couldn't find UserdefindedValue '" + val.UserdefinedValueName + "'");
                }
            }
            return source;
        }

        public static List<XMLUserdefinedValue> FromXml(XmlNode node, IXMLModifier parent)
        {
            List<XMLUserdefinedValue> output = new List<XMLUserdefinedValue>();

            if (node.SelectSingleNode("UserdefinedValues") != null)
            {
                foreach (XmlNode nd in node.SelectSingleNode("UserdefinedValues").ChildNodes)
                {
                    if (nd.Name == "UserdefinedValue")
                    {
                        XMLUserdefinedValue val = new XMLUserdefinedValue();
                        val.Parent = parent;
                        val.UserdefinedValueName = nd.Attributes["UserdefinedValueName"].Value;

                        if (nd.Attributes["Math"] != null)
                            val.Math = nd.Attributes["Math"].Value;

                        //val.OldValue = nd.Attributes["OldValue"].Value;
                        val.Key = nd.Attributes["Key"].Value;

                        output.Add(val);
                    }
                }
            }

            return output;
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "UserdefinedValue", null);

            XmlAttribute uvname = doc.CreateAttribute("UserdefinedValueName");
            uvname.Value = UserdefinedValueName;
            nd.Attributes.Append(uvname);

            XmlAttribute math = doc.CreateAttribute("Math");
            math.Value = Math;
            nd.Attributes.Append(math);

            //XmlAttribute oldvalue = doc.CreateAttribute("OldValue");
            //uvname.Value = OldValue;
            //nd.Attributes.Append(oldvalue);

            XmlAttribute key = doc.CreateAttribute("Key");
            key.Value = Key;
            nd.Attributes.Append(key);

            return nd;
        }

        public static XmlNode ToXml(List<XMLUserdefinedValue> labels, XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "UserdefinedValues", null);
            foreach (XMLUserdefinedValue lb in labels)
            {
                nd.AppendChild(lb.ToXml(doc));
            }

            return nd;
        }        
    }
}
