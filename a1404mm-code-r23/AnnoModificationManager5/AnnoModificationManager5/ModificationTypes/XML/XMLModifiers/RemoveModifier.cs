using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.ModificationTypes.Userdefined;


namespace AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers
{

    public class RemoveModifier : IXMLModifier
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
                return XMLFileCollector.Request(File);
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

        public List<AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue> UserdefinedValues
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
                return IXMLTools.GenerateHeader(this, "Remove");
            }
        }

        public string InnerXml = "";
        public string TagName = "";
        /// <summary>
        /// -1 = Add at last
        /// </summary>
        public int InsertBeforeIndex = -1;

        public RemoveModifier()
        {
            Group = "General";
            UserdefinedValues = new List<Userdefined.XMLUserdefinedValue>();
        }

        public static RemoveModifier FromXML(XmlModuleList parent, XmlNode node2, string GlobalFile)
        {
            RemoveModifier mod = new RemoveModifier();
            mod.Parent = parent;
            mod.File = node2.Attributes["File"] != null ? node2.Attributes["File"].Value : GlobalFile;
            mod.DeSelector = node2.Attributes["Deselector"] != null ? node2.Attributes["Deselector"].Value : "";
            mod.InnerXml = node2.Attributes["RemovedInnerXml"].Value;
            mod.Selector = node2.Attributes["Selector"].Value;
            mod.TagName = node2.Attributes["TagName"].Value;
            mod.Group = node2.Attributes["Group"] != null ? node2.Attributes["Group"].Value : "General";
            mod.Index = int.Parse(node2.Attributes["Index"].Value);

            mod.InsertBeforeIndex = node2.Attributes["InsertBeforeIndex"] != null ? int.Parse(node2.Attributes["InsertBeforeIndex"].Value) : -1;

            mod.UserdefinedValues = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.FromXml(node2, mod);

            mod.CheckIntegrity();

            return mod;
        }

        public XmlNode ToXML(XmlDocument doc)
        {
            XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Remove", null);

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
            newNode.AppendChild(AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.ToXml(UserdefinedValues, doc));
            #endregion

            newNode.Attributes.Append(doc.CreateAttribute("TagName"));
            newNode.Attributes["TagName"].Value = TagName;

            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "RemovedInnerXml", InnerXml));
            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "Index", Index.ToString()));
            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "InsertBeforeIndex", InsertBeforeIndex.ToString()));

            return newNode;
        }

        public void CheckIntegrity()
        {
            if (string.IsNullOrEmpty(Selector) |
                string.IsNullOrEmpty(File))
                throw new Exception(GetType().ToString() + ": IntegrityError");
        }

        public void Activate()
        {
            if (XMLFile == null)
                return;

            string Selector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            foreach (XmlNode xml in XMLFile.Select(Selector))
            {
                xml.ParentNode.RemoveChild(xml);
            }
        }

        public void Deactivate()
        {
            if (XMLFile == null)
                return;

            string __TagName = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(TagName, UserdefinedValues);
            string __InnerXml = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(InnerXml, UserdefinedValues);
            string Selector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            foreach (XmlNode xml in XMLFile.Select(DeSelector))
            {
                XmlNode node = xml.OwnerDocument.CreateNode(XmlNodeType.Element, __TagName, null);
                node.InnerXml = __InnerXml;

                //Index
                if (InsertBeforeIndex < 0)
                {
                    xml.AppendChild(node);
                }
                else
                {
                    CodeExtension.TC(() => xml.InsertBefore(node, xml.ChildNodes[InsertBeforeIndex]),
                        (ex) => xml.AppendChild(node));
                }
            }
        }

        public bool Validitate()
        {
            if (XMLFile == null)
                return false;

            string Selector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            return XMLFile.Select(Selector).Count == 0;
        }

        public bool ValiditateUserdefinedValueAppend()
        {
            string __TagName = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(TagName, UserdefinedValues);
            string __InnerXml = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(InnerXml, UserdefinedValues);
            string Selector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            string i = string.Concat(__TagName, __InnerXml, Selector, DeSelector);
            return !(i.Contains("{") | i.Contains("}"));
        }

        public IXMLModifier Clone()
        {
            return new RemoveModifier()
            {
                DeSelector = this.DeSelector,
                File = this.File,
                Group = this.Group,
                Index = this.Index,
                Parent = this.Parent,
                Selector = this.Selector,
                UserdefinedValues = this.UserdefinedValues,

                TagName = this.TagName,
                InnerXml = this.InnerXml,
                InsertBeforeIndex = this.InsertBeforeIndex
            };
        }
    }
}
