using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.IO;
using AnnoModificationManager5.Misc;
using System.Xml.Linq;
using System.Xml.XPath;
using RDAExplorer;

namespace AnnoModificationManager5.ModificationTypes.XmlModule
{
    public class XMLFile
    {
        public XmlDocument Content;
        public string FileName;
        public bool Changed = false;

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

        //XmlModuleEditor
        public List<XmlNode> XmlModuleEditor_ChangedNodes = new List<XmlNode>();

        public XMLFile()
        {
        }

        public XMLFile(string FileName)
        {
            this.FileName = FileName;

            //Add to Global Collector
            //XMLFileCollector.CollectedFiles.Add(FileName, this);
            ReadContent();
        }

        /// <summary>
        /// Fill content with default
        /// </summary>
        public void Default()
        {
            Content = new XmlDocument();
            Content.LoadXml("<Message>Couldn't find XML file.</Message>");
        }

        public void ReadData(string content)
        {
            Content = new XmlDocument();
            Content.LoadXml(content);
            Changed = false;
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
                ReadData(RDAManagerExtension.ReadTextOfRDA(FileName, Encoding.Default));
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
            #region Save to string
            StringBuilder output = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.CheckCharacters = false;
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.NewLineHandling = NewLineHandling.Entitize;

            XmlWriter writer = XmlWriter.Create(output, settings);

            Content.Save(writer);
            //Dispose
            writer.Close();

            Changed = false;
            #endregion

            if (!FileName.Contains(";"))
            {
                if (!Directory.Exists(Path.GetDirectoryName(FileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(FileName));

                using (StreamWriter w = new StreamWriter(FileName))
                {
                    w.Write(output.ToString());
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
#endif

                gotfile.SetData(Encoding.Default.GetBytes(output.ToString()));

                Modification.AMMRDA.CommitChange(rd, RDA.AMMRDAManager.RDARequestType.SaveChanges);
            }

            Console.WriteLine("'" + FileName + "' written.");
        }

        public static string Selector_ExtractAttribute(string input)
        {
            string output = "";
            int level = 0;

            foreach (char ch in input)
            {
                if (ch == ']')
                    level--;

                if (level > 0)
                    output += ch;

                if (ch == '[')
                    level++;
            }

            return output;
        }

        public static string Selector_Attribute_Format(string input)
        {
            string output = "";
            int level = 0;

            foreach (char ch in input)
            {
                if (ch == ']')
                    level--;

                if ((ch == '/' && level >= 1) | (ch == ';' && level > 1))
                {
                    output += ch == '/' ? '>' : '|';
                }
                else
                    output += ch;

                if (ch == '[')
                    level++;
            }

            return output;
        }

        public static string Selector_ExtractPath(string input)
        {
            string output = "";

            foreach (char ch in input)
            {
                if (ch == '[')
                    break;
                output += ch;
            }

            return output;
        }

        public static string Selector_ExtractPathAll(string input)
        {
            string output = "";
            int level = 0;

            foreach (char ch in input)
            {
                if (ch == '[')
                    level++;

                if (level == 0)
                    output += ch;

                if (ch == ']')
                    level--;
            }

            return output;
        }

        //public static List<XmlNode> SelectViaNodeName(XmlNode parent, string selectionstring)
        //{           
        //    List<XmlNode> currentNode = new List<XmlNode>() { parent };


        //    if (selectionstring.Contains("[") | selectionstring.Contains("]"))
        //    {
        //        throw new Exception("XMLFile -> SelectViaNodeName() -> Braces found!");
        //    }

        //    foreach (string sel in selectionstring.Split('/'))
        //    {
        //        //Backup old list
        //        List<XmlNode> cnodes = currentNode.ToList();
        //        //Clear old list
        //        currentNode.Clear();

        //        foreach (XmlNode n in cnodes)
        //        {
        //            currentNode.AddRange(n.ChildNodes.OfType<XmlNode>().ToList().FindAll(nd =>
        //                {
        //                    return nd.Name == sel;
        //                }));
        //        }
        //    }
        //    return currentNode;
        //}

        #region Selector over Xpath
        public List<XmlNode> Select(string selector)
        {
            //return Content.XPathSelectElements(selector).ToList();
            return Content.SelectNodes(selector).OfType<XmlNode>().ToList();
        }
        #endregion

        ///AssetList/Groups/Group[Name="PlayerBuildings"]/Groups/Group/Assets/Asset[BuildCost>ProductCost>Wood="2000"]
        #region Selector Algorythm 1
        //public List<XmlNode> Select(string selectionstring/*, [Optional, DefaultParameterValue(null)] List<XmlNode> nodes*/)
        //{
        //    List<XmlNode> currentNode = new List<XmlNode>();
        //    List<XmlNode> parentContent = new List<XmlNode>() { Content };

        //    //Operate with attributes
        //    // '/' to '>'
        //    //selectionstring = Regex.Replace(selectionstring, "(?<=[[]).*(?=[]])", match => { return match.Value.Replace("/", ">"); });
        //    //Replace Regex
        //    selectionstring = XMLFile.Selector_Attribute_Format(selectionstring);

        //    #region Check
        //    //Only one * is allowed at [0]
        //    //if (selectionstring.Contains("*") && selectionstring.LastIndexOf("*") != 0)
        //    //    throw new Exception("XMLFile -> Select() -> Only one * at [0] allowed!");
        //    //Disabled because asett[asset/value[xy=5]] is sllowed now[x]

        //    //Count Braces etc
        //    if (Regex.Matches(selectionstring, "[[]|[]]").Count % 2 != 0)
        //    {
        //        throw new Exception("XMLFile -> Select() -> Brace % Exception");
        //    }
        //    if (Regex.Matches(selectionstring, "[\"']").Count % 2 != 0)
        //    {
        //        throw new Exception("XMLFile -> Select() -> \"' % Exception");
        //    }
        //    #endregion

        //    #region Special
        //    if (selectionstring == "#[Root]")
        //    {
        //        return parentContent;
        //    }
        //    #endregion
        //    #region Special Prepare (Standard search)
        //    if (selectionstring.IndexOf("#[Root]") == 0)
        //    {
        //        selectionstring = selectionstring.Replace("#[Root]", Content.Name);
        //    }
        //    #endregion

        //    #region Select first nodes to search
        //    /*if (nodes != null)
        //    {
        //        parentContent = nodes;
        //    }*/

        //    //If * At [0] then use XPATH
        //    if (selectionstring[0] != '*')
        //        currentNode.AddRange(parentContent);
        //    else
        //    {
        //        string selc = Regex.Replace(selectionstring, "[[].*[]]", "");

        //        // *Assets/Asset
        //        if (selectionstring[1] != '/')
        //        {
        //            foreach (XmlNode p in parentContent)
        //            {
        //                currentNode.AddRange(p.SelectNodes("//" + selc.Split('/')[0].TrimStart('*')).OfType<XmlNode>());
        //            }
        //        }
        //        // */Asset
        //        else
        //        {
        //            foreach (XmlNode p in parentContent)
        //            {
        //                foreach (XmlNode nd in p.SelectNodes("//" + Regex.Replace(selc.Split('/')[1], "[[].*[]]", "")))
        //                {
        //                    if (!currentNode.Contains(nd.ParentNode))
        //                    {
        //                        currentNode.Add(nd.ParentNode);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    #endregion


        //    foreach (string sel in selectionstring.Split('/'))
        //    {
        //        if (sel[0] == '*' || sel == Content.Name)
        //            continue;

        //        string Selection = sel;

        //        //Get Attributes                
        //        List<string> attributes = XMLFile.Selector_ExtractAttribute(sel).Split(';').ToList();
        //        //Check attributes
        //        attributes.RemoveAll(str => string.IsNullOrEmpty(str));

        //        //Remove Attributes from Selection
        //        Selection = Selector_ExtractPath(sel);


        //        //Backup old list
        //        List<XmlNode> cnodes = currentNode.ToList();
        //        //Clear old list
        //        currentNode.Clear();

        //        #region Selection Code (Array type)
        //        int attr_index = 0;
        //        if (attributes.Count == 1 && int.TryParse(attributes[0], out attr_index))
        //        {
        //            List<XmlNode> f = new List<XmlNode>();
        //            foreach (XmlNode n in cnodes)
        //            {
        //                f.AddRange(n.ChildNodes.OfType<XmlNode>().ToList().FindAll(nd =>
        //                    {
        //                        return nd.Name == Selection;
        //                    }));
        //            }
        //            currentNode.Add(f[attr_index]);
        //        }
        //        #endregion
        //        else
        //        #region Selection Code (Key = Value type)
        //        {
        //            foreach (XmlNode n in cnodes)
        //            {
        //                currentNode.AddRange(n.ChildNodes.OfType<XmlNode>().ToList().FindAll(nd =>
        //                    {
        //                        bool x = nd.Name == Selection;

        //                        if (!x)
        //                            return false;

        //                        if (attributes.Count == 0)
        //                        {
        //                            return x;
        //                        }
        //                        else
        //                        {
        //                            foreach (string _attr in attributes)
        //                            {
        //                                string attr = _attr.Replace(">", "/").Replace("|", ";");

        //                                #region key = value type
        //                                string attr_nodename = attr.Split('=')[0].Trim().Replace(">", "/");
        //                                string attr_requestedvalue = Regex.Match(attr.Split('=')[1], "(?<=[\"']).*(?=[\"'])").Value;

        //                                //Item[Name='x']
        //                                if (!string.IsNullOrEmpty(attr_nodename))
        //                                {
        //                                    //Replace by extended selector
        //                                    //List<XmlNode> attrnodes = XMLFile.SelectViaNodeName(nd, attr_nodename);
        //                                    List<XmlNode> attrnodes = XMLFile.SelectViaNodeName(nd, attr_nodename);

        //                                    if (attrnodes.Count != 0)
        //                                    {
        //                                        foreach (XmlNode atn in attrnodes)
        //                                        {
        //                                            //x = y
        //                                            if (attr_requestedvalue[0] != '*')
        //                                            {
        //                                                if (atn.InnerText != attr_requestedvalue)
        //                                                {
        //                                                    x = false;
        //                                                    break;
        //                                                }
        //                                            }
        //                                            // x contains y
        //                                            else
        //                                            {
        //                                                if (!atn.InnerText.Contains(attr_requestedvalue.Remove(0, 1)))
        //                                                {
        //                                                    x = false;
        //                                                    break;
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                        x = false;
        //                                }
        //                                //Item[='x'] ^= Value/InnerText
        //                                else
        //                                {
        //                                    if (nd.InnerText != attr_requestedvalue)
        //                                    {
        //                                        x = false;
        //                                        break;
        //                                    }
        //                                }
        //                                #endregion

        //                                if (x == false)
        //                                    break;
        //                            }

        //                            /*if (x == true)
        //                                Console.Write("");*/

        //                            return x;
        //                        }
        //                    }));
        //            }
        //        }
        //        #endregion
        //    }
        //    return currentNode;
        //} 
        #endregion
    }
}
