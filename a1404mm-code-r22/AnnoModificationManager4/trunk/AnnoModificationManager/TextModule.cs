namespace AnnoModificationManager
{
    // using AnnoModificationManager.Properties;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    [Serializable]
    public class TextModule
    {        
        public string add_after = "";
        public string add_text = "";
        public Encoding currentencoding = Encoding.Default;
        public string destinationFile = "";
        public string encoding = "UTF";
        public int ModuleID = 0;
        public moduleTypes moduleType;
        public List<string> Nodelist = new List<string>();
        public string originalFileData = "";
        public string originalFilePath = "";
        public string replace_modified = "";
        public string replace_original = "";
        private bool? undoable = true;
        private bool? useoriginaldata = true;
    
        public static string getAllXMLIdentifiers(string p)
        {
            string str = "";
            string str2 = p;
            if (str2.Contains("<Name>"))
            {
                for (int i = 0; str2.Contains("<Name>"); i++)
                {
                    int index = str2.IndexOf("<Name>");
                    int num4 = str2.IndexOf("</Name>") - index;
                    str = str + str2.Substring(index + 6, num4 - 6) + ", ";
                    str2 = str2.Replace(str2.Substring(index, num4 + 7), "");
                }
            }
            if (str != "")
            {
                return str;
            }
            return "";
        }           

        public static string getXMLIdentifiers(string p)
        {
            string str = "";
            string str2 = p;
            if (str2.Contains("<Name>"))
            {
                int num = 0;
                while (str2.Contains("<Name>"))
                {
                    int index = str2.IndexOf("<Name>");
                    int num4 = str2.IndexOf("</Name>") - index;
                    str = str + str2.Substring(index + 6, num4 - 6) + ", ";
                    str2 = str2.Replace(str2.Substring(index, num4 + 7), "");
                    num++;
                    if (num == 3)
                    {
                        str = str + "...";
                        break;
                    }
                }
            }
            if (str != "")
            {
                return ("(" + str.TrimEnd(new char[] { ' ', ',' }) + ")");
            }
            return "";
        }

        public static TextModule Load(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (TextModule) formatter.Deserialize(reader.BaseStream);
            }
        }

        public static void Save(TextModule i, string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                new BinaryFormatter().Serialize(writer.BaseStream, i);
            }
        }

        public string Comment
        {
            get;
            set;
        }

        public Encoding getEncoding
        {
            get
            {
                if ((this.encoding == "UTF8") | (this.encoding == "UTF"))
                {
                    return Encoding.UTF8;
                }
                if (this.encoding == "UTF16LE")
                {
                    return Encoding.Unicode;
                }
                if (this.encoding == "ANSI")
                {
                    return Encoding.Default;
                }
                return Encoding.GetEncoding(int.Parse(this.encoding));
            }
        }

        public string getFilename
        {
            get
            {
                return this.destinationFile;
            }
        }

        public string getID
        {
            get
            {
                return this.ModuleID.ToString();
            }
        }       

        public int getIDInt
        {
            get
            {
                return this.ModuleID;
            }
        }

        public string GetName
        {
            get
            {
                return string.Concat(new object[] { "[", this.ModuleID, "] ", this.moduleType.ToString(), " -> ", this.destinationFile });
            }
        }

        public string GetTypeString
        {
            get
            {
                return this.moduleType.ToString();
            }
        }

        public bool Undoable
        {
            get
            {
                bool? undoable = this.undoable;
                return (!undoable.HasValue || undoable.GetValueOrDefault());
            }
            set
            {
                this.undoable = new bool?(value);
            }
        }

        public bool UseOriginalData
        {
            get
            {
                bool? useoriginaldata = this.useoriginaldata;
                return (!useoriginaldata.HasValue || useoriginaldata.GetValueOrDefault());
            }
            set
            {
                this.useoriginaldata = new bool?(value);
            }
        }

        public enum moduleTypes
        {
            Replace,
            AddText
        }
    }
}

