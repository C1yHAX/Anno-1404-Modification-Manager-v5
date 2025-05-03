using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager4.Language;
using System.Xml;
using AnnoModificationManager4.Misc;
using System.IO;
using AnnoModificationManager4.Components;

namespace AnnoModificationManager4.ModificationTypes
{

    public class ModificationInfo
    {
        public Version Version = new Version(1, 0, 0, 0);
        public string InternalName = "Modification";
        public string InternalCategory = "General";
        public string Author = "Unknown";
        public string Website = "http://tilegame.bplaced.net/";
        public string Documentation = "";
        public bool SupportsAMM4RDA = true;
        //public bool RDAIgnoreNonExistingRDAMods = false;
        public List<string> AnnoVersions = new List<string>();
        public List<string> Images = new List<string>();

        public Label Name = new Label() { Name = "Name", German = "Modifikation", English = "Modification" };
        public Label Description = new Label() { Name = "Description", German = "Beschreibung", English = "Description" };
        public Label Category = new Label() { Name = "Category", German = "Allgemein", English = "General" };

        public string GetIdentificationString
        {
            get
            {
                return Author + "_" + InternalCategory + "_" + InternalName + "_" + Version.ToString() + "__" + StringExtension.PutTogether(AnnoVersions, '_');
            }
        }

        public string GetShortIdentificationString
        {
            get
            {
                string str = Author.Short(3) + "_" + InternalCategory.Short(3) + "_" + InternalName.Short(7) + Version.ToString().TrimEnd(new char[] { '0', '.' }) + "_";
                foreach (string anno in AnnoVersions)
                    str += AnnoVersionHandler.AnnoVersionList.IndexOf(anno);

                return str;
            }
        }

        public static ModificationInfo FromXml(string file)
        {
            string xml = File.ReadAllText(file);
            return FromXmlData(xml);
        }

        public static ModificationInfo FromXmlData(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            ModificationInfo generate = new ModificationInfo();

            // <Version Major="1" Minor="5" Build="50" Revision="1"
            XmlNode tag_version = doc.FirstChild["Version"];
            int version_major = int.Parse(tag_version.Attributes["Major"].Value);
            int version_minor = int.Parse(tag_version.Attributes["Minor"].Value);
            int version_build = int.Parse(tag_version.Attributes["Build"].Value);
            int version_revision = int.Parse(tag_version.Attributes["Revision"].Value);
            generate.Version = new Version(version_major, version_minor, version_build, version_revision);

            //<InternalName>Name</InternalName>
            generate.InternalName = XmlExtension.GetValueText(doc.FirstChild, "InternalName");

            //<InternalCategory>Name</InternalCategory>
            generate.InternalCategory = XmlExtension.GetValueText(doc.FirstChild, "InternalCategory");

            //<Author>Name</Author>
            generate.Author = XmlExtension.GetValueText(doc.FirstChild, "Author");

            //<Website>Name</Website>
            generate.Website = XmlExtension.GetValueText(doc.FirstChild, "Website");
            if (generate.Website == "http://forum.annozone.de/thread.php?threadid")
                generate.Website = "http://forum.annozone.de/";

            //<Documentation>File</Documentation>
            generate.Documentation = XmlExtension.GetValueText(doc.FirstChild, "Documentation");

            //<AnnoVersions><Version>Retail</Version></AnnoVersion>
            foreach (XmlNode node in doc.FirstChild["AnnoVersions"].ChildNodes)
            {
                generate.AnnoVersions.Add(node.InnerXml);
            }
            if (generate.AnnoVersions.Count == 0)
                generate.AnnoVersions.Add("All"); //Automatically add "All"

            foreach (XmlNode node in doc.FirstChild["Images"].ChildNodes)
            {
                generate.Images.Add((node.FirstChild as XmlText).Value);
            }

            //<RDASupport>boolean</RDASupport>
            generate.SupportsAMM4RDA = (XmlExtension.GetValueText(doc.FirstChild, "RDASupport").ToLower() == "true");

            //<RDAIgnoreNonExistingRDAMods>boolean</RDAIgnoreNonExistingRDAMods>
            //generate.RDAIgnoreNonExistingRDAMods = (XmlExtension.GetValueText(doc.FirstChild, "RDAIgnoreNonExistingRDAMods").ToLower() == "true");

            //<Labels>...</Labels>
            foreach (Label lbl in Label.FromXml(doc.FirstChild))
            {
                if (lbl.Name == "Name")
                    generate.Name = lbl;
                else if (lbl.Name == "Description")
                    generate.Description = lbl;
                else if (lbl.Name == "Category")
                    generate.Category = lbl;
            }

            return generate;
        }

        public void ToXml(string File)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateNode(XmlNodeType.Element, "ModificationInfo", null));

            #region Version
            XmlNode tag_version = doc.CreateNode(XmlNodeType.Element, "Version", null);

            XmlAttribute attr_version_major = doc.CreateAttribute("Major");
            attr_version_major.Value = Version.Major.ToString();
            tag_version.Attributes.Append(attr_version_major);

            XmlAttribute attr_version_Minor = doc.CreateAttribute("Minor");
            attr_version_Minor.Value = Version.Minor.ToString();
            tag_version.Attributes.Append(attr_version_Minor);

            XmlAttribute attr_version_Build = doc.CreateAttribute("Build");
            attr_version_Build.Value = Version.Build.ToString();
            tag_version.Attributes.Append(attr_version_Build);

            XmlAttribute attr_version_Revision = doc.CreateAttribute("Revision");
            attr_version_Revision.Value = Version.Revision.ToString();
            tag_version.Attributes.Append(attr_version_Revision);

            doc.FirstChild.AppendChild(tag_version);
            #endregion

            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "InternalName", InternalName));
            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "InternalCategory", InternalCategory));
            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "Author", Author));
            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "Website", Website));
            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "Documentation", Documentation));
            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "RDASupport", SupportsAMM4RDA.ToString()));
            //doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "RDAIgnoreNonExistingRDAMods", RDAIgnoreNonExistingRDAMods.ToString()));

            XmlNode tagAnnoVersions = doc.CreateNode(XmlNodeType.Element, "AnnoVersions", null);
            foreach (string version in AnnoVersions)
            {
                XmlNode vnode = doc.CreateNode(XmlNodeType.Element, "AnnoVersion", null);
                vnode.InnerXml = version;

                tagAnnoVersions.AppendChild(vnode);
            }
            doc.FirstChild.AppendChild(tagAnnoVersions);

            XmlNode tagImages = doc.CreateNode(XmlNodeType.Element, "Images", null);
            foreach (string version in Images)
            {
                XmlNode vnode = doc.CreateNode(XmlNodeType.Element, "Image", null);
                vnode.AppendChild(doc.CreateTextNode(version));

                tagImages.AppendChild(vnode);
            }
            doc.FirstChild.AppendChild(tagImages);

            doc.FirstChild.AppendChild(Label.ToXml(new List<Label>()
            {
                Name,
                Description,
                Category
            },
            doc));

            doc.Save(File);
        }
    }
}
