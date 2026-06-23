using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager4.Misc;

using System.IO;
using AnnoModificationManager4.Components;
using RDAExplorer;
using System.Xml;
using System.Diagnostics;

namespace AnnoModificationManager4.ModificationTypes.XmlModule
{
    public class XMLFileCollector
    {
        public static Dictionary<string, XMLFile> CollectedFiles = new Dictionary<string, XMLFile>();

        public static XMLFile Request(string unifiedfile)
        {
            return Request(unifiedfile, false);
        }

        public static XMLFile Request(string unifiedfile, bool forcenew)
        {
            if (unifiedfile.Contains(";"))
            {
                var cell = unifiedfile.Split(';');

                return Request(cell[0], cell[1]);
            }
            return Request("", unifiedfile, forcenew);
        }

        public static XMLFile Request(string rdafile, string file, bool forcenew)
        {
            string origfile = file;
            file = file.FormatGlobalFolders();
            string cmbfile = (!string.IsNullOrEmpty(rdafile) ? rdafile + ";" : "") + file;


            if (!forcenew && CollectedFiles.ContainsKey(cmbfile))
            {
                return CollectedFiles[cmbfile];
            }
            else
            {
                if (!string.IsNullOrEmpty(rdafile))
                {
                    try
                    {
                        XMLFile xm = new XMLFile();
                        xm.FileName = cmbfile;
                        xm.ReadContent();

                        if (!forcenew)
                            CollectedFiles.Add(cmbfile, xm);

                        return xm;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        return null;
                    }

                    //if (Modification.Development_CurrentModification == null)
                    //{
                    //    rdafile = AnnoDirectoryHandler.GetCurrent() + "\\maindata\\" + rdafile;
                    //}
                    //else
                    //{
                    //    rdafile = Modification.Development_RDADirectory.Trim('\\') + "\\" + rdafile;
                    //}

                    ////Get from RDA file
                    //RDAReader read = Modification.RDAManager.GetReader(rdafile);
                    //if (read != null)
                    //{
                    //    RDAFile foundfile = Modification.RDAManager.GetFile(read, file);

                    //    if (foundfile != null)
                    //    {
                    //        XMLFile xm = new XMLFile();
                    //        xm.FileName = cmbfile;

                    //        using (MemoryStream stream = new MemoryStream(foundfile.GetData()))
                    //        {
                    //            stream.Position = 0;

                    //            using (StreamReader r = new StreamReader(stream))
                    //            {
                    //                xm.ReadData(r.ReadToEnd());
                    //            }
                    //        }

                    //        if (!forcenew)
                    //            CollectedFiles.Add(cmbfile, xm);

                    //        return xm;
                    //    }
                    //    else
                    //    {
                    //        return null;
                    //    }
                    //}
                    //else
                    //{
                    //    return null;
                    //}
                }
                else
                {
                    if (File.Exists(file))
                    {
                        XMLFile x = new XMLFile(file);
                        //x.ReadContent();

                        if (!forcenew)
                            CollectedFiles.Add(cmbfile, x);

                        return x;
                    }
                    else
                    {
                        if (Modification.Development_CurrentModification != null)
                        {
                            string f = Modification.Development_CurrentModification.Folder + "\\OriginalFiles\\" + origfile.FormatProjectPath();
                            XMLFile x = new XMLFile(f);
                            //x.ReadContent();
                            if (!forcenew)
                                CollectedFiles.Add(cmbfile, x);

                            return x;
                        }
                        else
                            throw new NotSupportedException();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rdafile">RDA relative path</param>
        /// <param name="filename">relative path</param>
        /// <returns></returns>
        public static XMLFile Request(string rdafile, string file)
        {
            return Request(rdafile, file, false);
        }


        public static void CheckContentFiles()
        {
            //XML Files
            foreach (XMLFile xml in XMLFileCollector.CollectedFiles.Values)
            {
                if (xml.Changed)
                {
                    xml.WriteContent();
                }
            }
        }
    }
}
