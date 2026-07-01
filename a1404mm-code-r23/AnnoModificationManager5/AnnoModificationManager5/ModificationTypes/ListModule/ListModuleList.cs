using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using AnnoModificationManager5.ModificationTypes;
using AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers;
using AnnoModificationManager5.Misc;
using System.IO;

namespace AnnoModificationManager5.ModificationTypes.ListModule
{

    public class ListModuleList
    {
        private List<IListModifier> ListModifiers
            = new List<IListModifier>();
        public Modification Parent;
        public string Name = "Module " + Misc.RandomProvider.Random.Next(10000, 99999999);
        public int Index = 0;

        public void OrderByIndex()
        {
            ListModifiers = ListModifiers.OrderBy(mod => mod.Index).ToList();
        }

        public List<IListModifier> Get()
        {
            return ListModifiers;
        }

        public void Add(IListModifier mod)
        {
            ListModifiers.Add(mod);
            Parent.ModificationUtils.List_NodeModifiers_AddValue(mod);
        }

        public void Remove(IListModifier mod)
        {
            ListModifiers.Remove(mod);
            Parent.ModificationUtils.List_NodeModifiers_RemoveValue(mod);
        }

        public void Edit(IListModifier mod)
        {
            Remove(mod);
            Add(mod);

            OrderByIndex();
        }

        public void Activate()
        {
            foreach (IListModifier mod in ListModifiers)
            {
                if (mod.IsActive && !mod.Validitate())
                {
                    try
                    {
                        if (mod.ListFile == null)
                            continue; // Datei in dieser Anno-Version nicht vorhanden -> still überspringen

                        mod.Activate();

                        //Set Changed to true
                        mod.ListFile.Changed = true;
                    }
                    catch (Exception)
                    {
                        // still überspringen (z. B. Datei in dieser Installation nicht vorhanden)
                    }
                }
            }
        }

        public void Deactivate()
        {
            foreach (IListModifier mod in ListModifiers.Reverse<IListModifier>())
            {
                if (mod.IsActive && mod.Validitate())
                {
                    try
                    {
                        mod.Deactivate();

                        //Set Changed to true
                        mod.ListFile.Changed = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /*public AnnoModificationManager5.Misc.Enums.Modification_ActivationStatus Validitate()
        {
            List<bool> Stati = new List<bool>();
            foreach (IListModifier mod in ListModifiers)
            {
                if (mod.IsActive)
                    Stati.Add(mod.Validitate());
            }
        
            if (!Stati.Contains(false))
                return AnnoModificationManager5.Misc.Enums.Modification_ActivationStatus.Activated;
            if (!Stati.Contains(true))
                return AnnoModificationManager5.Misc.Enums.Modification_ActivationStatus.Deactivated;
            return AnnoModificationManager5.Misc.Enums.Modification_ActivationStatus.Partially;
        }*/

        public ModificationActivationResponse CheckActivation()
        {
            ModificationActivationResponse resp = new ModificationActivationResponse();

            int cindex = 0;
            foreach (IListModifier xml in ListModifiers)
            {
                string rdfilename;
                if (xml.ListFile != null)
                {
                    var rd = xml.ListFile.RDAReader;
                    rdfilename = rd != null ? Path.GetFileName(rd.FileName) : "<No File>";
                }
                else
                {
                    rdfilename = "<No File>";
                }

                if (xml.IsActive)
                {
                    resp.ListModuleCount++;

                    if (xml.Validitate())
                    {
                        resp.ListModuleActive++;
                        resp.Log.AppendLine("\t\tListModifier [" + cindex + "] " + " in file '" +
                            StringExtension.DeFormatGlobalFolders(xml.ListFile.FileName) + "' @" + rdfilename + " -> Activated.");
                    }
                    else
                    {
                        resp.Log.AppendLine("\t\tListModifier [" + cindex + "] " + " in file '" +
                            StringExtension.DeFormatGlobalFolders(xml.ListFile.FileName) + "' @" + rdfilename + " -> Deactivated.");
                    }
                }
                else
                    resp.Log.AppendLine("\t\tListModifier [" + cindex + "] " + " in file '" +
                            StringExtension.DeFormatGlobalFolders(xml.ListFile.FileName) + "' @" + rdfilename + " -> Not Active / xml.IsActive <= false");
                cindex++;
            }
            return resp;
        }

        public void Load(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            if (doc.FirstChild.Attributes != null)
            {
                //Load Name
                Name = doc.FirstChild.Attributes["Name"] != null ? doc.FirstChild.Attributes["Name"].Value : Name;
                Index = int.Parse(doc.FirstChild.Attributes["Index"].Value);
            }

            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                string GlobalFile = node.Attributes["File"] != null ? node.Attributes["File"].Value : null;

                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "Add":
                            ListModifiers.Add(AddModifier.FromXML(this, node2, GlobalFile));
                            break;
                        case "AddGroup":
                            ListModifiers.Add(AddGroupModifier.FromXML(this, node2, GlobalFile));
                            break;
                        case "Edit":
                            ListModifiers.Add(EditModifier.FromXML(this, node2, GlobalFile));
                            break;
                        case "Remove":
                            ListModifiers.Add(RemoveModifier.FromXML(this, node2, GlobalFile));
                            break;
                    }
                }
            }

            //Update ListModule_Nodes
            foreach (IListModifier mod in ListModifiers)
            {
                Parent.ModificationUtils.List_NodeModifiers_AddValue(mod);
            }
        }

        public void Save(string filename)
        {
            if (File.Exists(filename))
                filename = FileExtension.MakeFileUnique(filename);

            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("ListModules");

            //Save Name
            XmlAttribute name = doc.CreateAttribute("Name");
            name.Value = Name;
            root.Attributes.Append(name);
            root.Attributes.Append(XmlExtension.CreateAttribute(doc, "Index", Index.ToString()));

            var q = from n in ListModifiers
                    group n by n.Group into perfiles
                    select perfiles.ToList();
            foreach (List<IListModifier> nd in q)
            {
                XmlNode c = doc.CreateElement("ModuleList");
                foreach (IListModifier xml in nd)
                {
                    c.AppendChild(xml.ToXML(doc));
                }
                root.AppendChild(c);
            }
            doc.AppendChild(root);

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
