using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AnnoModificationManager5.ModificationTypes.ListModule;
using AnnoModificationManager5.ModificationTypes.Userdefined;
using AnnoModificationManager5.Misc;


namespace AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers
{

    public class AddModifier : IListModifier
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
                return IListTools.GenerateHeader(this, "Add");
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

        //<TagName>
        //Value
        //</TagName>
        public string ElementGroup { get; set; }
        public string ElementValue { get; set; }

        public AddModifier()
        {
            Group = "General";
            UserdefinedValues = new List<ListUserdefinedValue>();
        }

        public static AddModifier FromXML(ListModuleList parent, XmlNode node2, string GlobalFile)
        {
            AddModifier mod = new AddModifier();
            mod.Parent = parent;
            mod.File = node2.Attributes["File"] != null ? node2.Attributes["File"].Value : GlobalFile;
            //mod.Selector = node2.Attributes["Selector"].Value;           
            mod.Index = int.Parse(node2.Attributes["Index"].Value);

            mod.ElementGroup = node2.Attributes["ElementGroup"] != null ? node2.Attributes["ElementGroup"].Value : "<No Group>";
            mod.ElementValue = node2.Attributes["ElementValue"].Value;

            mod.Group = node2.Attributes["Group"] != null ? node2.Attributes["Group"].Value : "General";

            mod.UserdefinedValues = ListUserdefinedValue.FromXml(node2, mod);

            mod.CheckIntegrity();
            return mod;
        }

        public XmlNode ToXML(XmlDocument doc)
        {
            XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "Add", null);

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
            if (string.IsNullOrEmpty(ElementValue) |
                string.IsNullOrEmpty(File) |
                string.IsNullOrEmpty(ElementGroup))
                throw new Exception(GetType().ToString() + ": IntegrityError");
        }

        public void Activate()
        {
            if (ListFile == null)
                return;

            string __TagName = ListUserdefinedValue.Append(ElementGroup, UserdefinedValues);
            string __Value = !string.IsNullOrEmpty(ElementValue) ? ListUserdefinedValue.Append(ElementValue, UserdefinedValues) : "";

            if (!ListFile.ListEntries.ContainsKey(__TagName))
                ListFile.ListEntries.Add(__TagName, new List<StringBuilder>());
            ListFile.ListEntries[__TagName].Add(new StringBuilder(__Value));
        }

        public void Deactivate()
        {
            if (ListFile == null)
                return;

            string __TagName = ListUserdefinedValue.Append(ElementGroup, UserdefinedValues);
            string __Value = !string.IsNullOrEmpty(ElementValue) ? ListUserdefinedValue.Append(ElementValue, UserdefinedValues) : "";
            ListFile.ListEntries[__TagName].Remove(ListFile.ListEntries[__TagName].Find(d => d.ToString() == __Value));
        }

        public bool Validitate()
        {
            if (ListFile == null)
                return false;

            return ListFile.AllListEntries.Find(sd => sd.ToString() == ListUserdefinedValue.Append(ElementValue, UserdefinedValues)) != null;
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
            return new AddModifier()
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
