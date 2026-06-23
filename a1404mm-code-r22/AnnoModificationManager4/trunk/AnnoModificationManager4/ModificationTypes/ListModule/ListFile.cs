using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers;
using AnnoModificationManager4.Misc;
using RDAExplorer;

namespace AnnoModificationManager4.ModificationTypes.ListModule
{
    public class ListFile
    {
        public static Encoding ListFileEncoding = new UnicodeEncoding(false, true);
        public string FileName;

        public RDAFile RDAFile
        {
            get
            {
                RDAReader rd;
                RDAFile rda = RDAManagerExtension.GetRDAFileFromPath(FileName, out rd);

                return rda;
            }
        }

        public RDAReader RDAReader
        {
            get
            {
                RDAReader rd;
                RDAFile rda = RDAManagerExtension.GetRDAFileFromPath(FileName, out rd);

                return rd;
            }
        }

        public Dictionary<string, List<StringBuilder>> ListEntries = new Dictionary<string, List<StringBuilder>>();

        /// <summary>
        /// Automatically add Groups
        /// </summary>
        public Dictionary<string, List<StringBuilder>> ListEntries_ReadOnly
        {
            get
            {
                if (Modification.Development_CurrentModification == null)
                {
                    return ListEntries;
                }
                else
                {
                    Dictionary<string, List<StringBuilder>> output = new Dictionary<string, List<StringBuilder>>();
                    foreach (KeyValuePair<string, List<StringBuilder>> str in ListEntries)
                    {
                        output.Add(str.Key, str.Value);
                    }

                    if (Modification.Development_CurrentModification.ModificationUtils.List_AddGroupModifiers.ContainsKey(this))
                    {
                        foreach (AddGroupModifier mod in Modification.Development_CurrentModification.ModificationUtils.List_AddGroupModifiers[this])
                        {
                            if (!output.ContainsKey(mod.ElementGroup))
                                output.Add(mod.ElementGroup, new List<StringBuilder>());
                        }
                    }
                    return output;
                }
            }
        }

        public bool Changed;

        public List<StringBuilder> AllListEntries
        {
            get
            {
                List<StringBuilder> o = new List<StringBuilder>();
                foreach (KeyValuePair<string, List<StringBuilder>> b in ListEntries)
                {
                    o.AddRange(b.Value);
                }

                return o;
            }
        }

        public ListFile()
        {
            ListEntries.Add("<No Group>", new List<StringBuilder>());
        }

        public ListFile(string file)
        {
            this.FileName = file;
            ListEntries.Add("<No Group>", new List<StringBuilder>());

            ReadContent();
            //ListFileCollector.CollectedFiles.Add(file, this);
        }

        public void ReadData(string content)
        {
            List<StringBuilder> current = ListEntries["<No Group>"];

            foreach (string ix in content.Split('\n'))
            {
                string i = ix.Replace("\r", "");

                if (!string.IsNullOrEmpty(i))
                {
                    if (i[0] == '#')
                    {
                        if (string.IsNullOrEmpty(i.Replace("#", "").Replace("-", "").Replace(" ", "")))
                            continue;

                        string grname = i.Trim(new char[] { '#', ' ' });

                        if (!ListEntries.ContainsKey(grname))
                        {
                            List<StringBuilder> list = new List<StringBuilder>();
                            ListEntries.Add(i.Trim(new char[] { '#', ' ' }), list);

                            current = list;
                        }
                        else
                        {
                            current = ListEntries[grname];
                        }
                    }
                    else
                    {
                        current.Add(new StringBuilder(i));
                    }
                }
            }
        }

        /// <summary>
        /// Can read RDAPATH or FILE
        /// </summary>
        /// <param name="file"></param>
        public void ReadContent(string file)
        {
            if (!file.Contains(";"))
                ReadData(File.ReadAllText(file));
            else
            {
                ReadData(RDAManagerExtension.ReadTextOfRDA(FileName, ListFileEncoding));
            }
        }

        public void ReadContent()
        {
            ReadContent(FileName);
        }

        public void WriteContent()
        {
            WriteContent(FileName);
        }

        public void WriteContent(string FileName)
        {
            #region Write data
            StringBuilder content = new StringBuilder();
            foreach (KeyValuePair<string, List<StringBuilder>> entry in ListEntries)
            {
                if (entry.Key != "<No Group>")
                {
                    content.AppendLine("# " + entry.Key);
                    content.AppendLine("#------------------");
                    content.AppendLine();
                }

                foreach (StringBuilder v in entry.Value)
                {
                    content.AppendLine(v.ToString());
                }

                content.AppendLine();
            }
            #endregion

            if (!FileName.Contains(";"))
            {
                if (!Directory.Exists(Path.GetDirectoryName(FileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(FileName));

                using (StreamWriter w = new StreamWriter(FileName))
                {
                    w.Write(content.ToString());
                }
            }
            else
            {
#if RDA_SAVEATHP
                RDAReader rd = RDAManagerExtension.GetHPReader(FileName.Split(';')[0]);
                RDAFile gotfile = Modification.RDAManager.GetFile(rd, FileName.Split(';')[1]);

                if (gotfile == null)
                {
                    gotfile = new RDAFile();
                    gotfile.FileName = FileName.Split(';')[1].Trim('\\');
                    rd.rdaFileEntries.Add(gotfile);

                    //Remove to force update
                    RDAManagerExtension.RemoveRDAFileAssignment(FileName);
                }
#else

                //Write to RDA
                RDAReader rd;
                RDAFile gotfile = RDAManagerExtension.GetRDAFileFromPath(FileName, out rd);

                if (gotfile == null)
                {
                    gotfile = new RDAFile();
                    gotfile.FileName = FileName.Split(';')[1].Trim('\\');
                    rd.rdaFileEntries.Add(gotfile);
                }

                //var byt = ListFileEncoding.GetBytes(content.ToString());
                //byt. ListFileEncoding.GetPreamble();

                //gotfile.SetData();
#endif

                MemoryStream stream = new MemoryStream();
                using (StreamWriter w = new StreamWriter(stream, ListFileEncoding))
                {
                    w.Write(content.ToString());
                }

                gotfile.SetData(stream.ToArray());
                stream.Close();

                Modification.AMMRDA.CommitChange(rd, RDA.AMMRDAManager.RDARequestType.SaveChanges);
            }

            Console.WriteLine("'" + FileName + "' written.");
        }

        // "11=Steinstraße"
        // "streets 10 - 39/11=Steinstraße"
        // "streets 10 - 39//11=Steinstraße/straße"
        // "*10 - 39/11=Steinstraße"
        // "*Steinstraße"
        public List<StringBuilder> Select(string group, string entry)
        {
            List<StringBuilder> output = new List<StringBuilder>();

            if (string.IsNullOrEmpty(group))
            {
                foreach (KeyValuePair<string, List<StringBuilder>> et in ListEntries)
                {
                    foreach (StringBuilder bd in et.Value)
                    {
                        if (bd.ToString() == entry || string.IsNullOrEmpty(entry))
                            output.Add(bd);
                    }
                }
            }
            else
            {
                foreach (StringBuilder bd in ListEntries[group])
                {
                    if (bd.ToString() == entry || string.IsNullOrEmpty(entry))
                        output.Add(bd);
                }
            }

            return output;
        }
    }
}
