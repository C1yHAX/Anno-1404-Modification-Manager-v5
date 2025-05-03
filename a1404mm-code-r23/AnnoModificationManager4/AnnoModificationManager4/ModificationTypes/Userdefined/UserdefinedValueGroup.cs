using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AnnoModificationManager4.Language;
using System.Reflection;
using AnnoModificationManager4.Misc;

namespace AnnoModificationManager4.ModificationTypes.Userdefined
{
    
    public class UserdefinedValueGroup
    {      
        public Label Label_Name;
        public string InternalName;

        public List<UserdefinedValue> GetUserdefinedValues(Modification mod)
        {
            return mod.UserdefinedValues.FindAll(m => m.Group == InternalName);
        }

        public static List<UserdefinedValueGroup> FromXml(string filename)
        {
            List<UserdefinedValueGroup> list = new List<UserdefinedValueGroup>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);

                foreach (XmlNode node in doc.FirstChild.ChildNodes)
                {
                    if (node.Name == "Group")
                    {
                        UserdefinedValueGroup value = new UserdefinedValueGroup();
                        //Read Attributes
                        value.InternalName = node.Attributes["InternalName"].Value;

                        //Read labels 
                        List<Label> lbs = Label.FromXml(node);
                        value.Label_Name = lbs[0];

                        list.Add(value);
                    }
                }
            }
            catch (Exception)
            {              
            }

            if (list.Count == 0)
            {
                list.Add(new UserdefinedValueGroup()
                {
                    InternalName = "No Group",
                    Label_Name = new Label() { German = "", English = "", Name = "Name" }
                });
            }

            return list;
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "Group", null);

            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "InternalName", InternalName));
            nd.AppendChild(Label.ToXml(new List<Label> { Label_Name }, doc));
          
            return nd;
        }

        public static XmlNode ToXml(List<UserdefinedValueGroup> labels, XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "Groups", null);
            foreach (UserdefinedValueGroup lb in labels)
            {
                nd.AppendChild(lb.ToXml(doc));
            }

            return nd;
        }

        public static void Save(string filename, List<UserdefinedValueGroup> values)
        {
            XmlDocument doc = new XmlDocument();

            doc.AppendChild(UserdefinedValueGroup.ToXml(values, doc));

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
