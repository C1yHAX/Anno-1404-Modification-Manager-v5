using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.ModificationTypes.Userdefined;


namespace AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers
{

    public class AddModifier : IXMLModifier
    {
        public XmlModuleList Parent
        {
            get;
            set;
        }

        public string Selector
        {
            get;
            set;
        }

        public string File
        {
            get;
            set;
        }

        public string Group
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        public XMLFile XMLFile
        {
            get
            {
                if (TemporaryXMLFile != null)
                    return TemporaryXMLFile;
                //if (Modification.Development_CurrentModification == null)
                return XMLFileCollector.Request(File);
                //return XMLFileCollector.Request(Modification.Development_CurrentModification.Folder +
                //    "\\OriginalFiles\\" +
                //    File.FormatProjectPath());
            }
        }

        public XMLFile TemporaryXMLFile
        {
            get;
            set;
        }

        public string DeSelector
        {
            get;
            set;
        }

        public List<AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue> UserdefinedValues
        {
            get;
            set;
        }

        public bool IsActive
        {
            get
            {
                //Look for UserdefinedValues, which can disable the modifier
                foreach (XMLUserdefinedValue val in UserdefinedValues)
                {
                    UserdefinedValue v = Parent.Parent.UserdefinedValues.Find(z => z.Name == val.UserdefinedValueName);
                    if (v != null && v.Type == UserdefinedValue.UserdefinedValueType.ModifierEnabled)
                    {
                        if (v.Current == "False")
                            return false;
                    }
                }
                return true;
            }
        }

        public object ModuleList_ToHeader
        {
            get
            {
                return IXMLTools.GenerateHeader(this, "Add");
            }
        }

        //<TagName>
        //Value
        //</TagName>
        public string TagName = "";
        public string Value = "";

        /// <summary>
        /// -1 = Add at last
        /// </summary>
        public int InsertBeforeIndex = -1;

        public AddModifier()
        {
            Group = "General";
            UserdefinedValues = new List<Userdefined.XMLUserdefinedValue>();
        }

        public static AddModifier FromXML(XmlModuleList parent, XmlNode node2, string GlobalFile)
        {
            AddModifier mod = new AddModifier();
            mod.Parent = parent;
            mod.File = node2.Attributes["File"] != null ? node2.Attributes["File"].Value : GlobalFile;
            mod.DeSelector = node2.Attributes["Deselector"] != null ? node2.Attributes["Deselector"].Value : "";
            mod.Selector = node2.Attributes["Selector"].Value;
            mod.TagName = node2.Attributes["TagName"].Value;
            mod.Value = node2.Attributes["Value"] != null ? node2.Attributes["Value"].Value : "";
            mod.Group = node2.Attributes["Group"] != null ? node2.Attributes["Group"].Value : "General";
            mod.Index = int.Parse(node2.Attributes["Index"].Value);
            mod.InsertBeforeIndex = node2.Attributes["InsertBeforeIndex"] != null ? int.Parse(node2.Attributes["InsertBeforeIndex"].Value) : -1;

            mod.UserdefinedValues = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.FromXml(node2, mod);

            mod.CheckIntegrity();
            return mod;
        }

        public XmlNode ToXML(XmlDocument doc)
        {
            XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Add", null);

            #region Default
            newNode.Attributes.Append(doc.CreateAttribute("Selector"));
            newNode.Attributes["Selector"].Value = Selector;

            newNode.Attributes.Append(doc.CreateAttribute("Deselector"));
            newNode.Attributes["Deselector"].Value = DeSelector;

            newNode.Attributes.Append(doc.CreateAttribute("File"));
            newNode.Attributes["File"].Value = File;

            newNode.Attributes.Append(doc.CreateAttribute("Group"));
            newNode.Attributes["Group"].Value = Group;

            //Userdefined Values
            newNode.AppendChild(AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.ToXml(UserdefinedValues, doc));
            #endregion

            newNode.Attributes.Append(doc.CreateAttribute("TagName"));
            newNode.Attributes["TagName"].Value = TagName;

            if (!string.IsNullOrEmpty(Value))
            {
                newNode.Attributes.Append(doc.CreateAttribute("Value"));
                newNode.Attributes["Value"].Value = Value;
            }

            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "Index", Index.ToString()));
            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "InsertBeforeIndex", InsertBeforeIndex.ToString()));

            return newNode;
        }

        public void CheckIntegrity()
        {
            if (string.IsNullOrEmpty(Selector) |
                string.IsNullOrEmpty(File) |
                string.IsNullOrEmpty(TagName))
                throw new Exception(GetType().ToString() + ": IntegrityError");
        }

        public void Activate()
        {
            if (XMLFile == null)
                return;

            string Selector = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string __TagName = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.TagName, UserdefinedValues);
            string __Value = !string.IsNullOrEmpty(Value) ? AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(Value, UserdefinedValues) : "";

            foreach (XmlNode xml in XMLFile.Select(Selector))
            {
                /*if (xml.SelectSingleNode("/" + __TagName) != null)
                    throw new Exception("AddModifier -> " + __TagName + " already existing!");*/

                XmlNode element = xml.OwnerDocument.CreateNode(XmlNodeType.Element, __TagName, null);
                if (!string.IsNullOrEmpty(Value))
                {
                    element.InnerXml = __Value;
                }

                //Index
                if (InsertBeforeIndex < 0)
                {
                    xml.AppendChild(element);
                }
                else
                {
                    CodeExtension.TC(() => xml.InsertBefore(element, xml.ChildNodes[InsertBeforeIndex]),
                        (ex) => xml.AppendChild(element));
                }
            }
        }

        public void Deactivate()
        {
            if (XMLFile == null)
                return;

            string TagName = XMLUserdefinedValue.Append(this.TagName, UserdefinedValues);
            string Value = XMLUserdefinedValue.Append(this.Value, UserdefinedValues); ;
            string Selector = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            foreach (XmlNode xml in XMLFile.Select(string.IsNullOrEmpty(DeSelector) ? Selector : DeSelector))
            {
                List<XmlNode> cn = XmlExtension.FindNodeInChildren(xml, TagName, Value);
                foreach (XmlNode c in cn)
                {
                    xml.RemoveChild(c);
                }
            }
        }

        public bool Validitate()
        {
            if (XMLFile == null)
                return false;

            string TagName = XMLUserdefinedValue.Append(this.TagName, UserdefinedValues);
            string Value = XMLUserdefinedValue.Append(this.Value, UserdefinedValues);
            string Selector = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            List<XmlNode> selectednodes = XMLFile.Select(Selector);
            if (selectednodes.Count == 0)
                return false;

            foreach (XmlNode xml in selectednodes)
            {
                if (XmlExtension.FindNodeInChildren(xml, TagName, Value).Count == 0)
                    return false;
            }
            return true;
        }

        public bool ValiditateUserdefinedValueAppend()
        {
            string __TagName = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(TagName, UserdefinedValues);
            string __Value = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(Value, UserdefinedValues);
            string Selector = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager4.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            string i = string.Concat(__TagName, __Value, Selector, DeSelector);
            return !(i.Contains("{") | i.Contains("}"));
        }


        public IXMLModifier Clone()
        {
            return new AddModifier()
            {
                DeSelector = this.DeSelector,
                File = this.File,
                Group = this.Group,
                Index = this.Index,
                Parent = this.Parent,
                Selector = this.Selector,
                UserdefinedValues = this.UserdefinedValues,

                TagName = this.TagName,
                Value = this.Value,
                InsertBeforeIndex = this.InsertBeforeIndex
            };
        }
    }
}
