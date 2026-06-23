using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using AnnoModificationManager4.ModificationTypes.ListModule;
using AnnoModificationManager4.ModificationTypes.Userdefined;
using AnnoModificationManager4.Misc;


namespace AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers
{

    public class RemoveModifier : IListModifier
    {
        public ListModuleList Parent
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

        public ListFile TemporaryListFile
        {
            get;
            set;
        }

        public ListFile ListFile
        {
            get
            {
                if (TemporaryListFile != null)
                    return TemporaryListFile;
                return ListFileCollector.Request(File);
            }
        }

        public List<ListUserdefinedValue> UserdefinedValues
        {
            get;
            set;
        }

        public object ModuleList_ToHeader
        {
            get
            {
                return IListTools.GenerateHeader(this, "Remove");
            }
        }

        public bool IsActive
        {
            get
            {
                //Look for UserdefinedValues, which can disable the modifier
                foreach (ListUserdefinedValue val in UserdefinedValues)
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

        public string ElementGroup { get; set; }
        public string ElementValue { get; set; }

        public RemoveModifier()
        {
            Group = "General";
            UserdefinedValues = new List<ListUserdefinedValue>();
        }

        public static RemoveModifier FromXML(ListModuleList parent, XmlNode node2, string GlobalFile)
        {
            RemoveModifier mod = new RemoveModifier();
            mod.Parent = parent;
            mod.File = node2.Attributes["File"] != null ? node2.Attributes["File"].Value : GlobalFile;
            //mod.Selector = node2.Attributes["Selector"].Value;
            mod.ElementGroup = node2.Attributes["ElementGroup"] != null ? node2.Attributes["ElementGroup"].Value : "<No Group>";
            mod.ElementValue = node2.Attributes["ElementValue"].Value;

            mod.Group = node2.Attributes["Group"] != null ? node2.Attributes["Group"].Value : "General";
            mod.Index = int.Parse(node2.Attributes["Index"].Value);

            mod.UserdefinedValues = ListUserdefinedValue.FromXml(node2, mod);

            mod.CheckIntegrity();

            return mod;
        }

        public XmlNode ToXML(XmlDocument doc)
        {
            XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Remove", null);

            #region Default
            /*newNode.Attributes.Append(doc.CreateAttribute("Selector"));
            newNode.Attributes["Selector"].Value = Selector;*/

            newNode.Attributes.Append(doc.CreateAttribute("File"));
            newNode.Attributes["File"].Value = File;

            newNode.Attributes.Append(doc.CreateAttribute("Group"));
            newNode.Attributes["Group"].Value = Group;

            //Userdefined Values
            newNode.AppendChild(ListUserdefinedValue.ToXml(UserdefinedValues, doc));
            #endregion

            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "ElementGroup", ElementGroup));
            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "ElementValue", ElementValue));

            newNode.Attributes.Append(XmlExtension.CreateAttribute(doc, "Index", Index.ToString()));

            return newNode;
        }

        public void CheckIntegrity()
        {
            if (string.IsNullOrEmpty(File) |
                string.IsNullOrEmpty(ElementGroup) |
                string.IsNullOrEmpty(ElementValue))
                throw new Exception(GetType().ToString() + ": IntegrityError");
        }

        public void Activate()
        {
            if (ListFile == null)
                return;

            string __TagName = ListUserdefinedValue.Append(ElementGroup, UserdefinedValues);
            string __InnerXml = ListUserdefinedValue.Append(ElementValue, UserdefinedValues);

            ListFile.ListEntries[__TagName].Remove(ListFile.ListEntries[__TagName].Find(d => d.ToString() == __InnerXml));
        }

        public void Deactivate()
        {
            if (ListFile == null)
                return;

            string __TagName = ListUserdefinedValue.Append(ElementGroup, UserdefinedValues);
            string __InnerXml = ListUserdefinedValue.Append(ElementValue, UserdefinedValues);

            if (!ListFile.ListEntries.ContainsKey(__TagName))
                ListFile.ListEntries.Add(__TagName, new List<StringBuilder>());
            ListFile.ListEntries[__TagName].Add(new StringBuilder(__InnerXml));
        }

        public bool Validitate()
        {
            if (ListFile == null)
                return false;
            return ListFile.AllListEntries.Find(sd => sd.ToString() == ListUserdefinedValue.Append(ElementValue, UserdefinedValues)) == null;
        }

        public bool ValiditateUserdefinedValueAppend()
        {
            string __TagName = ListUserdefinedValue.Append(ElementGroup, UserdefinedValues);
            string __Value = !string.IsNullOrEmpty(ElementValue) ? ListUserdefinedValue.Append(ElementValue, UserdefinedValues) : "";

            string i = string.Concat(__TagName, __Value);
            return !(i.Contains("{") | i.Contains("}"));
        }

        public IListModifier Clone()
        {
            return new RemoveModifier()
            {
                Parent = this.Parent,
                File = this.File,
                Group = this.Group,
                ElementGroup = this.ElementGroup,
                ElementValue = this.ElementValue,
                Index = this.Index,
                UserdefinedValues = this.UserdefinedValues
            };
        }
    }
}
