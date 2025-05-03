using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager5.Language;
using System.Xml;
using AnnoModificationManager5.Misc;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using wctrls = System.Windows.Controls;
using AnnoModificationManager5.Language.DictionarySystem;
using AnnoModificationManager5.Components;

namespace AnnoModificationManager5.DownloadService
{
    public class ModificationInfo
    {
        public Version Version = new Version(1, 0, 0, 0);
        public string VersionToString
        {
            get
            {
                return Version.ToString();
            }
        }

        public string VersionString
        {
            get
            {
                return "Version " + Version;
            }
        }

        public string InternalName { get; set; }
        public string InternalCategory { get; set; }
        public string Author { get; set; }
        public string Website { get; set; }
        public string Documentation { get; set; }
        public string DownloadUrl { get; set; }
        public string ImageUrl { get; set; }
        public bool SupportsAMM4RDA { get; set; }

        public int DownloadUrlFileSize { get; set; }

        public object GetImage
        {
            get
            {
                try
                {
                    wctrls.Image img = new wctrls.Image();
                    img.Source = BitmapImageExtension.Load((ImageUrl));
                    img.Stretch = Stretch.UniformToFill;

                    return img;
                }
                catch (Exception)
                {
                    /*wctrls.Border brd = new wctrls.Border();
                    brd.BorderBrush = Brushes.DarkGray;
                    brd.BorderThickness = new System.Windows.Thickness(1);
                    
                    brd.Child = new wctrls.TextBlock()
                    {
                        Text = "Error.",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center
                    };

                    return brd;*/
                    wctrls.Image img = new wctrls.Image();
                    img.Source = BitmapImageExtension.Load(("pack://application:,,,/Images/AlternativePreview.jpg"));
                    img.Stretch = Stretch.UniformToFill;
                    /*img.Width = 75;
                    img.Height = 50;*/

                    return img;
                }
            }
        }

        public List<string> AnnoVersions = new List<string>();
        public string AnnoVersionsPutTogether
        {
            get
            {
                return StringExtension.PutTogether(AnnoVersions, ',');
            }
        }

        public DateTime Date = DateTime.Now;
        public string DateToString
        {
            get
            {
                return Date.ToString();
            }
        }

        public List<string> Images = new List<string>();

        public Label Name = new Label() { Name = "Name", German = "Modifikation", English = "Modification" };
        public Label Description = new Label() { Name = "Description", German = "Beschreibung", English = "Description" };
        public Label Category = new Label() { Name = "Category", German = "Allgemein", English = "General" };

        public string UI_Name
        {
            get
            {
                return Name.Get;
            }
        }

        public string UI_AnnoVersions
        {
            get
            {
                if (AnnoVersions.Contains("All"))
                    return LanguageDictionary.Get("Modification", "AllVersions");
                else
                    return StringExtension.PutTogetherComma(AnnoVersions);
            }
        }

        public string UI_NameAnnoVersions
        {
            get
            {
                string i = Name.Get;

                if (AnnoVersions.Contains("All"))
                    i += " (" + LanguageDictionary.Get("Modification", "AllVersions") + ")";
                else
                    i += " (" + StringExtension.PutTogetherComma(AnnoVersions) + ")";

                return i;
            }
        }

        public string UI_Description
        {
            get
            {
                return Description.Get;
            }
        }

        public string UI_Category
        {
            get
            {
                return Category.Get;
            }
        }

        public string UI_FileSize
        {
            get
            {
                return DownloadUrlFileSize + " MB";
            }
        }

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

        public bool IsHidden
        {
            get
            {
                if (!SupportsAMM4RDA)
                    return true;

                return Properties.Settings.Default.PackageService_Hidden.Contains(GetIdentificationString);
            }
            set
            {
                PropertyList list = new PropertyList();
                list.Load(Properties.Settings.Default.PackageService_Hidden);

                if (value)
                    list.Items.Add(GetIdentificationString);
                else
                {
                    if (list.Items.Contains(GetIdentificationString))
                        list.Items.Remove(GetIdentificationString);
                }

                list.Items = list.Items.Distinct().ToList();
                Properties.Settings.Default.PackageService_Hidden = list.Save();
                Properties.Settings.Default.Save();
            }
        }

        public static ModificationInfo FromModification(ModificationTypes.ModificationInfo input)
        {
            ModificationInfo output = new ModificationInfo();
            output.InternalName = input.InternalName;
            output.InternalCategory = input.InternalCategory;
            output.Website = input.Website;
            output.Version = input.Version.Clone() as Version;
            output.Author = input.Author;
            output.Name = input.Name.Clone();
            output.Category = input.Category.Clone();
            output.Description = input.Description.Clone();
            output.Description = input.Description;
            output.AnnoVersions = input.AnnoVersions.ToList();
            output.Images = input.Images.ToList();
            output.SupportsAMM4RDA = input.SupportsAMM4RDA;


            return output;
        }

        public static ModificationInfo FromXml(string file)
        {
            string content = File.ReadAllText(file);
            return FromXmlData(content);
        }

        public static ModificationInfo FromXmlData(string data)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);

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

            //<DownloadUrl>Name</DownloadUrl>
            generate.DownloadUrl = XmlExtension.GetValueText(doc.FirstChild, "DownloadUrl");

            //<ImageUrl>Name</ImageUrl>
            generate.ImageUrl = XmlExtension.GetValueText(doc.FirstChild, "ImageUrl");

            //<Website>Name</Website>
            generate.Website = XmlExtension.GetValueText(doc.FirstChild, "Website");

            //<RDASupport>boolean</RDASupport>
            generate.SupportsAMM4RDA = (XmlExtension.GetValueText(doc.FirstChild, "RDASupport").ToLower() == "true");

            //<Documentation>File</Documentation>
            //generate.Documentation = XmlExtension.GetValue(doc.FirstChild, "Documentation");

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

            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "DownloadUrl", DownloadUrl));
            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "ImageUrl", ImageUrl));

            doc.FirstChild.AppendChild(XmlExtension.CreateElementText(doc, "RDASupport", SupportsAMM4RDA.ToString()));

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
