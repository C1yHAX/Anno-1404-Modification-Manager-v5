using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.ModificationTypes.Userdefined;


namespace AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers
{

    public class EditModifier : IXMLModifier
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

        public object ModuleList_ToHeader
        {
            get
            {
                return IXMLTools.GenerateHeader(this, "Edit");
            }
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

                //If Old==New => Not active
                if (XMLUserdefinedValue.Append(OldValue, UserdefinedValues) ==
                    XMLUserdefinedValue.Append(NewValue, UserdefinedValues))
                    return false;

                return true;
            }
        }

        public string OldValue = "";
        public string NewValue = "";

        public EditModifier()
        {
            UserdefinedValues = new List<Userdefined.XMLUserdefinedValue>();
            Group = "General";
        }

        public static EditModifier FromXML(XmlModuleList parent, XmlNode node2, string GlobalFile)
        {
            EditModifier mod = new EditModifier();
            mod.Parent = parent;
            mod.File = node2.Attributes["File"] != null ? node2.Attributes["File"].Value : GlobalFile;
            mod.DeSelector = node2.Attributes["Deselector"] != null ? node2.Attributes["Deselector"].Value : "";
            mod.Selector = node2.Attributes["Selector"].Value;
            mod.OldValue = node2.Attributes["OldValue"].Value;
            mod.NewValue = node2.Attributes["NewValue"].Value;
            mod.Group = node2.Attributes["Group"] != null ? node2.Attributes["Group"].Value : "General";
            mod.Index = int.Parse(node2.Attributes["Index"].Value);

            mod.UserdefinedValues = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.FromXml(node2, mod);

            mod.CheckIntegrity();

            return mod;
        }

        public XmlNode ToXML(XmlDocument doc)
        {
            XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Edit", null);

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

            newNode.Attributes.Append(doc.CreateAttribute("OldValue"));
            newNode.Attributes["OldValue"].Value = OldValue;

            if (!string.IsNullOrEmpty(NewValue))
            {
                newNode.Attributes.Append(doc.CreateAttribute("NewValue"));
                newNode.Attributes["NewValue"].Value = NewValue;
            }

            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "Index", Index.ToString()));

            return newNode;
        }

        public void CheckIntegrity()
        {
            if (string.IsNullOrEmpty(Selector) |
                string.IsNullOrEmpty(File) /*|
                string.IsNullOrEmpty(OldValue)*/)
                throw new Exception(GetType().ToString() + ": IntegrityError");
        }

        public void Activate()
        {
            if (XMLFile == null)
                return;

            string Selector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);
            string __NewValue = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(NewValue, UserdefinedValues);

            foreach (XmlNode xml in XMLFile.Select(Selector))
            {
                xml.InnerXml = __NewValue;
            }
        }

        public void Deactivate()
        {
            if (XMLFile == null)
                return;

            string Selector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);
            string __OldValue = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(OldValue, UserdefinedValues);

            foreach (XmlNode xml in XMLFile.Select(string.IsNullOrEmpty(DeSelector) ? Selector : DeSelector))
            {
                xml.InnerXml = __OldValue;
            }
        }

        public bool Validitate()
        {
            if (XMLFile == null)
                return false;

            string NewValue = XMLUserdefinedValue.Append(this.NewValue, UserdefinedValues);
            string Selector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            List<XmlNode> selectednodes = XMLFile.Select(Selector);
            if (selectednodes.Count == 0)
                return false;

            foreach (XmlNode xml in selectednodes)
            {
                if (xml.InnerXml != NewValue)
                    return false;
            }
            return true;
        }

        public bool ValiditateUserdefinedValueAppend()
        {
            string NewValue = XMLUserdefinedValue.Append(this.NewValue, UserdefinedValues);
            string Selector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.Selector, UserdefinedValues);
            string DeSelector = AnnoModificationManager5.ModificationTypes.Userdefined.XMLUserdefinedValue.Append(this.DeSelector, UserdefinedValues);

            string i = string.Concat(NewValue, Selector, DeSelector);
            return !(i.Contains("{") | i.Contains("}"));
        }

        public IXMLModifier Clone()
        {
            return new EditModifier()
            {
                DeSelector = this.DeSelector,
                File = this.File,
                Group = this.Group,
                Index = this.Index,
                Parent = this.Parent,
                Selector = this.Selector,
                UserdefinedValues = this.UserdefinedValues,

                OldValue = this.OldValue,
                NewValue = this.NewValue
            };
        }
    }
}
