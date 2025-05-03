using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers;
using AnnoModificationManager4.Misc;
using System.IO;

namespace AnnoModificationManager4.ModificationTypes.XmlModule
{

    public class XmlModuleList
    {
        private List<IXMLModifier> XmlModifiers
            = new List<IXMLModifier>();
        public Modification Parent;
        public string Name = "Module " + Misc.RandomProvider.Random.Next(10000, 99999999);
        public int Index = 0;

        public void OrderByIndex()
        {
            XmlModifiers = XmlModifiers.OrderBy(mod => mod.Index).ToList();
        }

        public List<IXMLModifier> Get()
        {
            return XmlModifiers;
        }

        public void Add(IXMLModifier mod)
        {
            XmlModifiers.Add(mod);
            Parent.ModificationUtils.Xml_NodeModifiers_AddValue(mod);
        }

        public void Remove(IXMLModifier mod)
        {
            XmlModifiers.Remove(mod);
            Parent.ModificationUtils.Xml_NodeModifiers_RemoveValue(mod);
        }

        public void Edit(IXMLModifier mod)
        {
            Remove(mod);
            Add(mod);

            OrderByIndex();
        }

        public void Activate()
        {
            foreach (IXMLModifier mod in XmlModifiers)
            {
                if (mod.IsActive && !mod.Validitate())
                {
                    //try
                    {
                        mod.Activate();

                        //Set Changed to true
                        mod.XMLFile.Changed = true;
                    }
                    //catch (Exception)
                    //{
                    //}
                }
            }
        }

        public void Deactivate()
        {
            foreach (IXMLModifier mod in XmlModifiers.Reverse<IXMLModifier>())
            {
                if (mod.IsActive && mod.Validitate())
                {
                    try
                    {
                        mod.Deactivate();

                        //Set Changed to true
                        mod.XMLFile.Changed = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /*public AnnoModificationManager4.Misc.Enums.Modification_ActivationStatus Validitate()
        {
            List<bool> Stati = new List<bool>();
            foreach (IXMLModifier mod in XmlModifiers)
            {
                if (mod.IsActive)
                    Stati.Add(mod.Validitate());
            }
        
            if (!Stati.Contains(false))
                return AnnoModificationManager4.Misc.Enums.Modification_ActivationStatus.Activated;
            if (!Stati.Contains(true))
                return AnnoModificationManager4.Misc.Enums.Modification_ActivationStatus.Deactivated;
            return AnnoModificationManager4.Misc.Enums.Modification_ActivationStatus.Partially;
        }*/

        public ModificationActivationResponse CheckActivation()
        {
            ModificationActivationResponse resp = new ModificationActivationResponse();

            int cindex = 0;
            foreach (IXMLModifier xml in XmlModifiers)
            {
                string rdfilename;
                if (xml.XMLFile != null)
                {
                    var rd = xml.XMLFile.RDAReader;
                    rdfilename = rd != null ? Path.GetFileName(rd.FileName) : "<No File>";
                }
                else
                {
                    rdfilename = "<No File>";
                }

                if (xml.IsActive)
                {
                    resp.XmlModuleCount++;

                    if (xml.Validitate())
                    {
                        resp.XmlModuleActive++;
                        resp.Log.AppendLine("\t\tXmlModifier [" + cindex + "] " + " in file '" +
                            StringExtension.DeFormatGlobalFolders(xml.XMLFile.FileName) + "' @" + rdfilename + " -> Activated.");
                    }
                    else
                    {
                        resp.Log.AppendLine("\t\tXmlModifier [" + cindex + "] " +
                        " in file '" +
                            StringExtension.DeFormatGlobalFolders(xml.XMLFile.FileName) + "' @" + rdfilename + " -> Deactivated.");
                    }
                }
                else
                    resp.Log.AppendLine("\t\tXmlModifier [" + cindex + "] " + " in file '" +
                            StringExtension.DeFormatGlobalFolders(xml.XMLFile.FileName) + "' @" + rdfilename + " -> Not Active / xml.IsActive <= false");

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
                Name = doc.FirstChild.Attributes["Name"].Value;
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
                            XmlModifiers.Add(AddModifier.FromXML(this, node2, GlobalFile));
                            break;
                        case "Edit":
                            XmlModifiers.Add(EditModifier.FromXML(this, node2, GlobalFile));
                            break;
                        case "Remove":
                            XmlModifiers.Add(RemoveModifier.FromXML(this, node2, GlobalFile));
                            break;
                    }
                }
            }

            XmlModifiers = XmlModifiers.OrderBy(xml => xml.Index).ToList();

            //Update XmlModule_Nodes
            foreach (IXMLModifier mod in XmlModifiers)
            {
                Parent.ModificationUtils.Xml_NodeModifiers_AddValue(mod);
            }
        }

        public void Save(string filename)
        {
            if (File.Exists(filename))
                filename = FileExtension.MakeFileUnique(filename);

            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("XmlModules");

            //Save Name            
            root.Attributes.Append(XmlExtension.CreateAttribute(doc, "Name", Name));
            root.Attributes.Append(XmlExtension.CreateAttribute(doc, "Index", Index.ToString()));

            var q = from n in XmlModifiers
                    group n by n.Group into perfiles
                    select perfiles.ToList();
            foreach (List<IXMLModifier> nd in q)
            {
                XmlNode c = doc.CreateElement("ModuleList");
                foreach (IXMLModifier xml in nd)
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
