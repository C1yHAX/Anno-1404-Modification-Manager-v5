using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using AnnoModificationManager5.Misc;

namespace AnnoModificationManager5.Language.DictionarySystem
{
    public class LanguageDictionary
    {
        public static Dictionary<string, Dictionary<string, Label>> CollectedData
            = new Dictionary<string, Dictionary<string, Label>>();
        private static bool Loaded = false;

        static LanguageDictionary()
        {
            Load();
        }

        public static string Get(string dict, string key)
        {
            try
            {
                return CollectedData[dict][key].Get;
            }
            catch (Exception)
            {
                return "<Dummy>";
            }
        }

        public static void Add(string file)
        {
            Dictionary<string, Label> data = new Dictionary<string, Label>();

            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            foreach (XmlNode nd in doc.FirstChild)
            {
                if (nd.Name == "Label")
                {
                    Label lb = new Label();
                    lb.Name = nd.Attributes["Key"].Value;
                    lb.German = nd.Attributes["German"].Value.Replace("\\n", "\n");
                    lb.English = nd.Attributes["English"].Value.Replace("\\n", "\n");

                    data.Add(lb.Name, lb);
                }
            }

            CollectedData.Add(Path.GetFileNameWithoutExtension(file), data);
        }

        public static void Load()
        {
            try
            {
                if (Loaded)
                    return;

                CollectedData.Clear();
                foreach (string file in Directory.GetFiles(DirectoryExtension.GetApplicationFolder() + "\\Language\\Dictionaries"))
                {
                    if (Path.GetExtension(file).ToLower() == ".xml")
                    {
                        Add(file);
                    }
                }

                Loaded = true;
            }
            catch (Exception)
            {               
            }
        }
    }
}
