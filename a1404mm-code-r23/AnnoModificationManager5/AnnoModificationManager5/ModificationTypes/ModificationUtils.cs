using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using xmlm = AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers;
using listm = AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers;
using AnnoModificationManager5.ModificationTypes.ListModule;
using System.IO;

namespace AnnoModificationManager5.ModificationTypes
{

    public class ModificationUtils
    {
        public Modification CurrentModification;

        //XML
        public Dictionary<XmlNode, List<xmlm.IXMLModifier>> Xml_AddModifiers = new Dictionary<XmlNode, List<xmlm.IXMLModifier>>();
        public Dictionary<XmlNode, List<xmlm.IXMLModifier>> Xml_RemoveModifiers = new Dictionary<XmlNode, List<xmlm.IXMLModifier>>();
        public Dictionary<XmlNode, List<xmlm.IXMLModifier>> Xml_EditModifiers = new Dictionary<XmlNode, List<xmlm.IXMLModifier>>();

        public List<xmlm.IXMLModifier> Xml_AllModififers
        {
            get
            {
                List<xmlm.IXMLModifier> mods = new List<xmlm.IXMLModifier>();
                foreach (XmlModule.XmlModuleList list in CurrentModification.XmlModules)
                    mods.AddRange(list.Get());

                return mods;
            }
        }

        //List
        public Dictionary<ListFile, List<listm.IListModifier>> List_AddGroupModifiers = new Dictionary<ListFile, List<listm.IListModifier>>();
        public Dictionary<ListFile, List<listm.IListModifier>> List_AddModifiers = new Dictionary<ListFile, List<listm.IListModifier>>();
        public Dictionary<ListFile, List<listm.IListModifier>> List_RemoveModifiers = new Dictionary<ListFile, List<listm.IListModifier>>();
        public Dictionary<ListFile, List<listm.IListModifier>> List_EditModifiers = new Dictionary<ListFile, List<listm.IListModifier>>();

        public List<listm.IListModifier> List_AllModififers
        {
            get
            {
                List<listm.IListModifier> mods = new List<listm.IListModifier>();
                foreach (ListModule.ListModuleList list in CurrentModification.ListModules)
                    mods.AddRange(list.Get());

                return mods;
            }
        }

        //Files
        /// <summary>
        /// Search directory
        /// </summary>
        public List<string> Files_Anno
        {
            get
            {
                return Directory.GetFiles(CurrentModification.Folder + "\\Files\\Anno1404", "*", SearchOption.AllDirectories).ToList();
            }
        }
        /// <summary>
        /// Search directory
        /// </summary>
        public List<string> Files_AppData
        {
            get
            {
                return Directory.GetFiles(CurrentModification.Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories).ToList();
            }
        }
        /// <summary>
        /// Search directory
        /// </summary>
        public List<string> Files_GetLockedFiles_InProject
        {
            get
            {
                List<string> n = new List<string>();

                foreach (Userdefined.UserdefinedValue val in CurrentModification.UserdefinedValues.FindAll(uv =>
                {
                    return uv.Type == Userdefined.UserdefinedValue.UserdefinedValueType.FilesEnabled
                        && uv.Current == "False";
                }))
                {
                    foreach (string file in val.Files)
                    {
                        n.Add(file.Replace("%Anno%", CurrentModification.Folder + "\\Files\\Anno1404")
                            .Replace("%AppData%", CurrentModification.Folder + "\\Files\\AppData"));
                    }
                }

                return n;
            }
        }

        public ModificationUtils(Modification mod)
        {
            CurrentModification = mod;
        }

        #region XML
        public Dictionary<XmlNode, List<xmlm.IXMLModifier>> Xml_NodeModifiers_SelectOf(xmlm.IXMLModifier Modifier)
        {
            if (Modifier is AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers.AddModifier)
                return Xml_AddModifiers;
            if (Modifier is AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers.RemoveModifier)
                return Xml_RemoveModifiers;
            if (Modifier is AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers.EditModifier)
                return Xml_EditModifiers;
            return null;
        }

        public void Xml_NodeModifiers_AddValue(xmlm.IXMLModifier Modifier)
        {
            string Selector = Modifier.Selector;
            //if (Modifier is AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers.RemoveModifier)
            //{
            //    Selector += "/" +
            //        (Modifier as AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers.RemoveModifier).TagName;
            //}

            Dictionary<XmlNode, List<xmlm.IXMLModifier>> XmlModule_NodeModifiers = Xml_NodeModifiers_SelectOf(Modifier);

            var xfile = Modifier.XMLFile;

            if (xfile != null)
            {
                foreach (XmlNode nd in xfile.Select(Selector))
                {
                    if (XmlModule_NodeModifiers.ContainsKey(nd))
                    {
                        XmlModule_NodeModifiers[nd].Add(Modifier);
                    }
                    else
                    {
                        XmlModule_NodeModifiers.Add(nd, new List<xmlm.IXMLModifier>() { Modifier });
                    }
                }
            }
        }

        public void Xml_NodeModifiers_RemoveValue(xmlm.IXMLModifier Modifier)
        {
            Dictionary<XmlNode, List<xmlm.IXMLModifier>> XmlModule_NodeModifiers = Xml_NodeModifiers_SelectOf(Modifier);

            foreach (KeyValuePair<XmlNode, List<xmlm.IXMLModifier>> md in XmlModule_NodeModifiers)
            {
                if (md.Value.Contains(Modifier))
                {
                    md.Value.Remove(Modifier);
                }
            }

            List<XmlNode> keys = XmlModule_NodeModifiers.Keys.ToList();
            foreach (XmlNode nd in keys)
            {
                if (XmlModule_NodeModifiers.ContainsKey(nd) && XmlModule_NodeModifiers[nd].Count == 0)
                    XmlModule_NodeModifiers.Remove(nd);
            }
        }
        #endregion
        #region List
        public Dictionary<ListFile, List<listm.IListModifier>> List_NodeModifiers_SelectOf(listm.IListModifier Modifier)
        {
            if (Modifier is AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers.AddGroupModifier)
                return List_AddGroupModifiers;
            if (Modifier is AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers.AddModifier)
                return List_AddModifiers;
            if (Modifier is AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers.RemoveModifier)
                return List_RemoveModifiers;
            if (Modifier is AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers.EditModifier)
                return List_EditModifiers;
            return null;
        }

        public void List_NodeModifiers_AddValue(listm.IListModifier Modifier)
        {
            Dictionary<ListFile, List<listm.IListModifier>> ListModule_NodeModifiers = List_NodeModifiers_SelectOf(Modifier);

            var xfile = Modifier.ListFile;

            if (xfile != null)
            {
                if (ListModule_NodeModifiers.ContainsKey(xfile))
                {
                    ListModule_NodeModifiers[xfile].Add(Modifier);
                }
                else
                {
                    ListModule_NodeModifiers.Add(xfile, new List<listm.IListModifier>() { Modifier });
                }
            }
        }

        public void List_NodeModifiers_RemoveValue(listm.IListModifier Modifier)
        {
            if (Modifier.ListFile != null)
            {
                Dictionary<ListFile, List<listm.IListModifier>> ListModule_NodeModifiers = List_NodeModifiers_SelectOf(Modifier);

                if (ListModule_NodeModifiers.ContainsKey(Modifier.ListFile))
                {
                    ListModule_NodeModifiers[Modifier.ListFile].Remove(Modifier);
                    if (ListModule_NodeModifiers[Modifier.ListFile].Count == 0)
                        ListModule_NodeModifiers.Remove(Modifier.ListFile);
                }
            }
        }
        #endregion

        //Global refresh
        public void RefreshModifierDictionaries()
        {
            Xml_RemoveModifiers.Clear();
            Xml_EditModifiers.Clear();
            Xml_AddModifiers.Clear();
            List_RemoveModifiers.Clear();
            List_EditModifiers.Clear();
            List_AddModifiers.Clear();

            foreach (xmlm.IXMLModifier mod in Xml_AllModififers)
                Xml_NodeModifiers_AddValue(mod);
            foreach (listm.IListModifier mod in List_AllModififers)
                List_NodeModifiers_AddValue(mod);
        }
    }
}
