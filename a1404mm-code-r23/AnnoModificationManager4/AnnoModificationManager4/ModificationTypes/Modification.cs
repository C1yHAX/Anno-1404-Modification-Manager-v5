using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers;
using xmlm = AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers;
using System.IO;
using AnnoModificationManager4.ModificationTypes.Userdefined;
using AnnoModificationManager4.ModificationTypes.XmlModule;
using System.Xml;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.ModificationTypes.ListModule;
using AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers;
using Ionic.Zip;
using System.Diagnostics;
using AnnoModificationManager4.ModificationTypes.TaskSystem;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoModificationManager4.Components;
using System.ComponentModel;
using AnnoModificationManager4.Components.ModificationHelpers;
using RDAExplorer;
using wf = System.Windows.Forms;
using AnnoModificationManager4.RDA;

namespace AnnoModificationManager4.ModificationTypes
{

    public class Modification
    {
        public static RDAManager RDAManager = new RDAManager();
        public static AMMRDAManager AMMRDA = new AMMRDAManager();
        public static string RDAPath
        {
            get
            {
                if (Modification.Development_CurrentModification == null)
                {
                    return AnnoDirectoryHandler.GetCurrent();
                }
                else
                {
                    return Modification.Development_RDADirectory.Trim('\\');
                }
            }
        }

        public static string RDABackupFolder
        {
            get
            {
                return Properties.Settings.Default.RDABackupDir;
            }
        }

        public delegate void MessageEventHandler(string Message);
        public event MessageEventHandler OnMessage;

        public string Folder;
        public string File;

        public static wf.SaveFileDialog File_Save_Dialog = new wf.SaveFileDialog()
        {
            Filter = "Modification Project|*.zip"
        };

        public ModificationUICollector UICollector { get; set; }
        public ModificationUtils ModificationUtils { get; set; }

        #region Development Vars
        public bool IsDevelopment = false;
        public static Modification Development_CurrentModification;
        public static string Development_RDADirectory;
        #endregion
        #region Modification Parts
        public ModificationInfo Info = new ModificationInfo();

        public List<UserdefinedValue> UserdefinedValues = new List<UserdefinedValue>();
        public List<UserdefinedValueGroup> UserdefinedValueGroups = new List<UserdefinedValueGroup>();

        public List<XmlModuleList> XmlModules = new List<XmlModuleList>();
        public List<ListModuleList> ListModules = new List<ListModuleList>();

        public List<TaskList> Tasks = new List<TaskList>();
        #endregion

        //Modiferies per ListFile
        #region List



        #endregion

        #region Count
        public int Files_Anno_Count
        {
            get
            {
                return Directory.GetFiles(Folder + "\\Files\\Anno1404", "*", SearchOption.AllDirectories).Length;
            }
        }

        public int Files_AppData_Count
        {
            get
            {
                return Directory.GetFiles(Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories).Length;
            }
        }

        public int XmlModules_Count
        {
            get
            {
                int count = 0;
                foreach (XmlModuleList xml in XmlModules)
                    count += xml.Get().Count;

                return count;
            }
        }

        public int ListModules_Count
        {
            get
            {
                int count = 0;
                foreach (ListModuleList xml in ListModules)
                    count += xml.Get().Count;

                return count;
            }
        }

        public int UValues_Count
        {
            get
            {
                return UserdefinedValues.Count;
            }
        }
        #endregion

        [Obsolete]
        public List<string> GetOriginalFiles
        {
            get
            {
                if (!Info.SupportsAMM4RDA)
                {
                    return Directory.GetFiles(Folder + "\\OriginalFiles").ToList();
                }
                throw new NotSupportedException("GetOriginalFiles is not supported by AMM4.2");
            }
        }

        /// <summary>
        /// All Filesnames of the originalFiles
        /// Not used anymore!
        /// </summary>
        [Obsolete]
        public List<string> GetOriginalFileNames
        {
            get
            {
                List<string> o = new List<string>();
                foreach (string f in GetOriginalFiles)
                {
                    o.Add(Path.GetFileName(f));
                }

                return o;
            }
        }

        public Modification(bool IsDevelopment)
        {
            this.IsDevelopment = IsDevelopment;

            if (IsDevelopment)
                Development_CurrentModification = this;

            UICollector = new ModificationUICollector(this);
            ModificationUtils = new ModificationUtils(this);
        }

        public Modification()
        {
            UICollector = new ModificationUICollector(this);
            ModificationUtils = new ModificationUtils(this);
        }

        //userdefinedValues
        public void UserdefinedValueGroups_Check()
        {
            if (UserdefinedValueGroups.Count == 0)
            {
                UserdefinedValueGroups.Add(new UserdefinedValueGroup()
                {
                    InternalName = "No Group",
                    Label_Name = new Language.Label() { German = "", English = "", Name = "Name" }
                });
            }
        }

        //Messaging System
        public void SendMessage(string Message)
        {
            if (OnMessage != null)
                OnMessage(Message);
        }

        #region All Xml + List Files
        public List<string> CollectedFiles_Xml_List
        {
            get
            {
                List<string> files = new List<string>();
                foreach (XmlModuleList l in XmlModules)
                {
                    foreach (IXMLModifier mod in l.Get())
                    {
                        if (!files.Contains(mod.File))
                        {
                            files.Add(mod.File);
                        }
                    }
                }

                return files;
            }
        }

        /// <summary>
        /// Converts back to Default Formatting! # -> %
        /// </summary>
        [Obsolete]
        public List<string> CollectedFiles_Xml_List_Folder
        {
            get
            {
                List<string> files = new List<string>();
                foreach (string f in GetOriginalFiles)
                {
                    if (Path.GetExtension(f).ToLower() == ".xml")
                        files.Add(Path.GetFileName(f).Replace("#", "%").Replace("_", "\\"));
                }

                return files;
            }
        }

        /// <summary>
        /// Converts back to Default Formatting! # -> %
        /// </summary>
        [Obsolete]
        public List<string> CollectedFiles_List_List_Folder
        {
            get
            {
                List<string> files = new List<string>();
                foreach (string f in GetOriginalFiles)
                {
                    if (Path.GetExtension(f).ToLower() != ".xml")
                        files.Add(Path.GetFileName(f).Replace("#", "%").Replace("_", "\\"));
                }

                return files;
            }
        }

        public List<string> CollectedFiles_List_List
        {
            get
            {
                List<string> files = new List<string>();
                foreach (ListModuleList l in ListModules)
                {
                    foreach (IListModifier mod in l.Get())
                    {
                        if (!files.Contains(mod.File))
                        {
                            files.Add(mod.File);
                        }
                    }
                }

                return files;
            }
        }
        #endregion
        #region Folder
        public void OpenFolder(string folder)
        {
            //Clean
            UserdefinedValueGroups.Clear();
            UserdefinedValues.Clear();
            XmlModules.Clear();
            ModificationUtils.Xml_AddModifiers.Clear();
            ModificationUtils.Xml_EditModifiers.Clear();
            ModificationUtils.Xml_RemoveModifiers.Clear();
            ListModules.Clear();
            ModificationUtils.List_AddModifiers.Clear();
            ModificationUtils.List_EditModifiers.Clear();
            ModificationUtils.List_RemoveModifiers.Clear();

            folder = folder.Trim(new char[] { '/', '\\' });
            Folder = folder;

            //Load Infos
            Info = ModificationInfo.FromXml(folder + "\\config.xml");

            //Create not existing folders
            //Create File Folders, if not existing
            if (!Directory.Exists(folder + "\\Files\\Anno1404"))
                Directory.CreateDirectory(folder + "\\Files\\Anno1404");
            if (!Directory.Exists(folder + "\\Files\\AppData"))
                Directory.CreateDirectory(folder + "\\Files\\AppData");

            //Create OriginalFilesFolders if not existing
            //if (!Directory.Exists(folder + "\\OriginalFiles"))
            //    Directory.CreateDirectory(folder + "\\OriginalFiles");
            if (!Directory.Exists(folder + "\\Images"))
                Directory.CreateDirectory(folder + "\\Images");

            //Load Userdefined Values
            if (Directory.Exists(folder + "\\UserdefinedValues"))
            {
                //if (System.IO.File.Exists(folder + "\\UserdefinedValues\\UserdefinedValueGroups.xml"))
                UserdefinedValueGroups.AddRange(UserdefinedValueGroup.FromXml(folder + "\\UserdefinedValues\\UserdefinedValueGroups.xml"));
                if (System.IO.File.Exists(folder + "\\UserdefinedValues\\UserdefinedValues.xml"))
                    UserdefinedValues.AddRange(UserdefinedValue.FromXml(folder + "\\UserdefinedValues\\UserdefinedValues.xml", this));

                //Create not existing Groups
                foreach (UserdefinedValue val in UserdefinedValues)
                {
                    if (UserdefinedValueGroups.Find(gr => gr.InternalName == val.Group) == null)
                    {
                        UserdefinedValueGroups.Add(new UserdefinedValueGroup()
                        {
                            InternalName = val.Group,
                            Label_Name = new Language.Label()
                            {
                                German = val.Group,
                                English = val.Group
                            }
                        });
                    }
                }
            }

            //Load XMLModules
            if (Directory.Exists(folder + "\\XmlModules"))
            {
                foreach (string file in Directory.GetFiles(folder + "\\XmlModules"))
                {
                    XmlModuleList list = new XmlModuleList();
                    list.Parent = this;

                    list.Load(file);
                    XmlModules.Add(list);
                }
                //Order
                XmlModules = XmlModules.OrderBy(xml => xml.Index).ToList();
            }

            //Load ListModules
            if (Directory.Exists(folder + "\\ListModules"))
            {
                foreach (string file in Directory.GetFiles(folder + "\\ListModules"))
                {
                    ListModuleList list = new ListModuleList();
                    list.Parent = this;

                    list.Load(file);
                    ListModules.Add(list);
                }
                //Order
                ListModules = ListModules.OrderBy(l => l.Index).ToList();
            }

            //Load Tasks (Dev only)
            if (Directory.Exists(folder + "\\Tasks") && IsDevelopment)
            {
                foreach (string file in Directory.GetFiles(folder + "\\Tasks"))
                {
                    TaskList task = TaskList.LoadFromXml(file);
                    Tasks.Add(task);
                }
            }
        }

        public void SaveFolder(string folder)
        {
            Folder = folder.Trim('\\');
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            //Save Modification Settings
            Info.ToXml(folder + "\\config.xml");

            //Save Userdefined Values
            #region Folder
            if (Directory.Exists(folder + "\\UserdefinedValues"))
                DirectoryExtension.CleanDirectory(folder + "\\UserdefinedValues");
            else
                Directory.CreateDirectory(folder + "\\UserdefinedValues");
            #endregion
            UserdefinedValueGroup.Save(folder + "\\UserdefinedValues\\UserdefinedValueGroups.xml", UserdefinedValueGroups);
            UserdefinedValue.Save(folder + "\\UserdefinedValues\\UserdefinedValues.xml", UserdefinedValues);

            //Save XMLModules
            #region Folder
            if (Directory.Exists(folder + "\\XmlModules"))
                DirectoryExtension.CleanDirectory(folder + "\\XmlModules");
            else
                Directory.CreateDirectory(folder + "\\XmlModules");
            #endregion
            foreach (XmlModuleList module in XmlModules)
            {
                module.Save(folder + "\\XmlModules\\" + module.Name + ".xml");
            }

            //Save ListModules
            #region Folder
            if (Directory.Exists(folder + "\\ListModules"))
                DirectoryExtension.CleanDirectory(folder + "\\ListModules");
            else
                Directory.CreateDirectory(folder + "\\ListModules");
            #endregion
            foreach (ListModuleList module in ListModules)
            {
                module.Save(folder + "\\ListModules\\" + module.Name + ".xml");
            }

            //Save Tasks
            if (IsDevelopment)
            {
                #region Folder
                if (Directory.Exists(folder + "\\Tasks"))
                    DirectoryExtension.CleanDirectory(folder + "\\Tasks");
                else
                    Directory.CreateDirectory(folder + "\\Tasks");
                #endregion

                foreach (TaskList task in Tasks)
                {
                    task.SaveToXml(folder + "\\Tasks\\" + task.Name + ".xml");
                }
            }

            ////Save Original Files (xml + list)
            //if (!Directory.Exists(folder + "\\OriginalFiles"))
            //    Directory.CreateDirectory(folder + "\\OriginalFiles");

            if (!Directory.Exists(folder + "\\Images"))
                Directory.CreateDirectory(folder + "\\Images");

            if (!Directory.Exists(folder + "\\Files\\Anno1404\\maindata"))
                Directory.CreateDirectory(folder + "\\Files\\Anno1404\\maindata");

            if (!Directory.Exists(folder + "\\Files\\Anno1404\\addon"))
                Directory.CreateDirectory(folder + "\\Files\\Anno1404\\addon");

            if (!Directory.Exists(folder + "\\Files\\AppData"))
                Directory.CreateDirectory(folder + "\\Files\\AppData");

            //CollectedFiles_Xml_List
            /*List<string> allfiles = new List<string>();
            foreach (string file in CollectedFiles_Xml_List)
            {
                if (!allfiles.Contains(file))
                    allfiles.Add(file);
            }
            foreach (string file in CollectedFiles_List_List)
            {
                if (!allfiles.Contains(file))
                    allfiles.Add(file);
            }*/
            GC.Collect();
        }

        public void SaveFolder()
        {
            SaveFolder(Folder);
        }
        #endregion
        #region ZIP
        public void OpenFile(string File)
        {
            this.File = File;
            ZipFile zip = new ZipFile(File);

            //Create temporary Folder
            string folder = DirectoryExtension.UnifyDirectory(Path.GetTempPath().Trim('\\') +
                "\\DevelopmentTools4\\" +
                Path.GetFileNameWithoutExtension(File));

            Directory.CreateDirectory(folder);

            //Unpack
            zip.ExtractAll(folder);
            zip.Dispose();
            zip = null;

            //Open
            //Debug.WriteLine("Loading modification");
            //Stopwatch wtach = new Stopwatch();
            //wtach.Start();
            OpenFolder(folder);
            //wtach.Stop();
            //Debug.WriteLine(wtach.ElapsedMilliseconds.ToString());
        }

        public void SaveFile(string File, bool Copy)
        {
            //First save to Temp
            SaveFolder();

            if (System.IO.File.Exists(File))
            {
                System.IO.File.Delete(File);
            }

            ZipFile zip = new ZipFile();
            zip.AddDirectory(Folder, "");
            zip.Save(File);

            if (!Copy)
                this.File = File;

            GC.Collect();
        }

        public void SaveFile()
        {
            if (string.IsNullOrEmpty(File))
            {
                try
                {
                    File_Save_Dialog.FileName = Path.GetFileName(Folder);
                }
                catch (Exception)
                {
                }

                if (File_Save_Dialog.ShowDialog() == wf.DialogResult.OK)
                    File = File_Save_Dialog.FileName;
                else
                    return;
            }

            SaveFile(File, false);
        }

        public void SaveFile_As()
        {
            if (File_Save_Dialog.ShowDialog() == wf.DialogResult.OK)
            {
                SaveFile(File_Save_Dialog.FileName, false);
            }
        }

        public void SaveFile_Copy()
        {
            if (File_Save_Dialog.ShowDialog() == wf.DialogResult.OK)
            {
                SaveFile(File_Save_Dialog.FileName, true);
            }
        }
        #endregion
        #region Modification Operation
        public void Activate()
        {
            List<string> LockedFiles = ModificationUtils.Files_GetLockedFiles_InProject;

            //foreach (string file in Directory.GetFiles(Folder + "\\Files\\Anno1404", "*", SearchOption.AllDirectories))
            //{
            //    if (LockedFiles.Contains(file))
            //        continue;
            //    System.IO.File.Copy(file, file.Replace(Folder + "\\Files\\Anno1404", AnnoDirectoryHandler.GetCurrent()), true);
            //}

            #region File mods
            {
                foreach (var rdacfolder in Directory.GetDirectories(Folder + "\\Files\\Anno1404"))
                {
                    #region Validate
                    string rdacfoldername = Path.GetFileName(rdacfolder);

                    if (rdacfoldername != "maindata" && rdacfoldername != "addon")
                        continue;
                    #endregion

                    string annordafolder = RDAPath + "\\" + rdacfoldername;

                    foreach (var absfile in Directory.GetFiles(rdacfolder, "*", SearchOption.AllDirectories))
                    {
                        //Is Locked => continue
                        if (LockedFiles.Contains(absfile))
                            continue;

                        string rootedfile = absfile.Remove(0, rdacfolder.Length).Trim('\\');
                        RDAReader existingreader;

#if RDA_SAVEATHP
                        existingreader = RDAManagerExtension.GetHPReader(rdacfoldername);
                        var existing = RDAManager.GetFile(existingreader, rootedfile);
#else
                        var existing = RDAManagerExtension.GetRDAFileFromPath(rdacfoldername + ";" + rootedfile, out existingreader);
#endif

                        if (existing != null)
                        {
                            existing.SetData(System.IO.File.ReadAllBytes(absfile));
                            AMMRDA.CommitChange(existingreader, AMMRDAManager.RDARequestType.SaveChanges);
                        }
                        else
                        {
                            //Look for a suitable RDA file
                            string mostprioriterized = RDAManager.GetMostImportantFileInFolder(annordafolder);

#if RDA_SAVEATHP
                            RDAReader rd = existingreader;

                            //Remove to force update
                            RDAManagerExtension.RemoveRDAFileAssignment(rdacfoldername + ";" + rootedfile);
#else
                            RDAReader rd = RDAManager.GetReader(annordafolder + "\\" + mostprioriterized);
#endif


                            RDAFile file = new RDAFile();
                            file.FileName = rootedfile;
                            file.SetData(System.IO.File.ReadAllBytes(absfile));

                            rd.rdaFileEntries.Add(file);
                            AMMRDA.CommitChange(rd, AMMRDAManager.RDARequestType.SaveChanges);
                        }
                    }
                }
            }
            #endregion

            foreach (string file in Directory.GetFiles(Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories))
            {
                if (LockedFiles.Contains(file))
                    continue;
                System.IO.File.Copy(file, file.Replace(Folder + "\\Files\\AppData", DirectoryExtension.GetAppDataFolder()), true);
            }
            foreach (XmlModuleList mod in XmlModules.OrderBy(md => md.Index))
            {
                mod.Activate();
            }
            foreach (ListModuleList mod in ListModules.OrderBy(md => md.Index))
            {
                mod.Activate();
            }

            XMLFileCollector.CheckContentFiles();
            ListFileCollector.CheckContentFiles();
        }

        public void Activate(BackgroundWorker wrk)
        {
            List<string> LockedFiles = ModificationUtils.Files_GetLockedFiles_InProject;

            float current = 0;
            float all = Directory.GetFiles(Folder + "\\Files\\Anno1404", "*", SearchOption.AllDirectories).Length +
                Directory.GetFiles(Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories).Length +
                XmlModules.Count +
                ListModules.Count;

            //Files
            #region File mods
            {
                foreach (var rdacfolder in Directory.GetDirectories(Folder + "\\Files\\Anno1404"))
                {
                    #region Validate
                    string rdacfoldername = Path.GetFileName(rdacfolder);

                    if (rdacfoldername != "maindata" && rdacfoldername != "addon")
                        continue;
                    #endregion

                    string annordafolder = RDAPath + "\\" + rdacfoldername;

                    foreach (var absfile in Directory.GetFiles(rdacfolder, "*", SearchOption.AllDirectories))
                    {
                        //Is Locked => continue
                        if (LockedFiles.Contains(absfile))
                            continue;

                        current++;
                        wrk.ReportProgress(ProgressBarExtension.Calculate(current, all));

                        string rootedfile = absfile.Remove(0, rdacfolder.Length).Trim('\\');
                        RDAReader existingreader;
#if RDA_SAVEATHP
                        existingreader = RDAManagerExtension.GetHPReader(rdacfoldername);
                        var existing = RDAManager.GetFile(existingreader, rootedfile);
#else
                        var existing = RDAManagerExtension.GetRDAFileFromPath(rdacfoldername + ";" + rootedfile, out existingreader);
#endif

                        if (existing != null)
                        {
                            existing.SetData(System.IO.File.ReadAllBytes(absfile));
                            AMMRDA.CommitChange(existingreader, AMMRDAManager.RDARequestType.SaveChanges);
                        }
                        else
                        {
                            //Look for a suitable RDA file
                            string mostprioriterized = RDAManager.GetMostImportantFileInFolder(annordafolder);

#if RDA_SAVEATHP
                            RDAReader rd = existingreader;

                            //Remove to force update
                            RDAManagerExtension.RemoveRDAFileAssignment(rdacfoldername + ";" + rootedfile);
#else
                            RDAReader rd = RDAManager.GetReader(annordafolder + "\\" + mostprioriterized);
#endif


                            RDAFile file = new RDAFile();
                            file.FileName = rootedfile;
                            file.SetData(System.IO.File.ReadAllBytes(absfile));

                            rd.rdaFileEntries.Add(file);
                            AMMRDA.CommitChange(rd, AMMRDAManager.RDARequestType.SaveChanges);
                        }
                    }
                }
            }
            #endregion
            foreach (string file in Directory.GetFiles(Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories))
            {
                current++;
                wrk.ReportProgress(ProgressBarExtension.Calculate(current, all));

                //Is locked => continue
                if (LockedFiles.Contains(file))
                    continue;

                string td = file.Replace(Folder + "\\Files\\AppData", DirectoryExtension.GetAppDataFolder());

                if (!Directory.Exists(Path.GetDirectoryName(td)))
                    Directory.CreateDirectory(Path.GetDirectoryName(td));

                System.IO.File.Copy(file, td, true);
            }
            //XmlModules
            foreach (XmlModuleList mod in XmlModules.OrderBy(md => md.Index))
            {
                current++;
                wrk.ReportProgress(ProgressBarExtension.Calculate(current, all));
                mod.Activate();
            }
            //ListModules
            foreach (ListModuleList mod in ListModules.OrderBy(md => md.Index))
            {
                current++;
                wrk.ReportProgress(ProgressBarExtension.Calculate(current, all));
                mod.Activate();
            }

            XMLFileCollector.CheckContentFiles();
            ListFileCollector.CheckContentFiles();
        }

        public void Deactivate()
        {
            List<string> LockedFiles = ModificationUtils.Files_GetLockedFiles_InProject;

            #region File mods
            {
                foreach (var rdacfolder in Directory.GetDirectories(Folder + "\\Files\\Anno1404"))
                {
                    #region Validate
                    string rdacfoldername = Path.GetFileName(rdacfolder);

                    if (rdacfoldername != "maindata" && rdacfoldername != "addon")
                        continue;
                    #endregion

                    foreach (var absfile in Directory.GetFiles(rdacfolder, "*", SearchOption.AllDirectories))
                    {
                        //Is Locked => continue
                        if (LockedFiles.Contains(absfile))
                            continue;

                        string rootedfile = absfile.Remove(0, rdacfolder.Length).Trim('\\');
                        RDAReader existingreader;
                        var existing = RDAManagerExtension.GetRDAFileFromPath(rdacfoldername + ";" + rootedfile, out existingreader);


                        if (existing != null)
                        {
                            RDAReader backupreader;
                            var backup = RDAManagerExtension.GetRDAFileFromPath(rdacfoldername + ";" + rootedfile, Properties.Settings.Default.RDABackupDir, out backupreader);

                            if (backup != null)
                            {
                                existing.SetData(backup.GetData());
                                AMMRDA.CommitChange(existingreader, AMMRDAManager.RDARequestType.SaveChanges);
                            }
                            else
                            {
                                RDAManagerExtension.RemoveRDAFromReader(existing, existingreader);
                            }
                        }
                    }
                }
            }
            #endregion
            foreach (string file in Directory.GetFiles(Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories))
            {
                //Is Locked => continue
                if (LockedFiles.Contains(file))
                    continue;

                string td = file.Replace(Folder + "\\Files\\AppData", DirectoryExtension.GetAppDataFolder());
                if (System.IO.File.Exists(td))
                    System.IO.File.Delete(td);
            }
            foreach (XmlModuleList mod in XmlModules.OrderByDescending(md => md.Index))
            {
                mod.Deactivate();
            }
            foreach (ListModuleList mod in ListModules.OrderByDescending(md => md.Index))
            {
                mod.Deactivate();
            }

            XMLFileCollector.CheckContentFiles();
            ListFileCollector.CheckContentFiles();
        }

        public void Deactivate(BackgroundWorker wrk)
        {
            List<string> LockedFiles = ModificationUtils.Files_GetLockedFiles_InProject;

            int current = 0;
            int all = Directory.GetFiles(Folder + "\\Files\\Anno1404", "*", SearchOption.AllDirectories).Length +
                Directory.GetFiles(Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories).Length +
                XmlModules.Count +
                ListModules.Count;

            #region File mods
            {
                foreach (var rdacfolder in Directory.GetDirectories(Folder + "\\Files\\Anno1404"))
                {
                    #region Validate
                    string rdacfoldername = Path.GetFileName(rdacfolder);

                    if (rdacfoldername != "maindata" && rdacfoldername != "addon")
                        continue;
                    #endregion

                    foreach (var absfile in Directory.GetFiles(rdacfolder, "*", SearchOption.AllDirectories))
                    {
                        //Is Locked => continue
                        if (LockedFiles.Contains(absfile))
                            continue;

                        current++;
                        wrk.ReportProgress(ProgressBarExtension.Calculate(current, all));

                        string rootedfile = absfile.Remove(0, rdacfolder.Length).Trim('\\');
                        RDAReader existingreader;
                        var existing = RDAManagerExtension.GetRDAFileFromPath(rdacfoldername + ";" + rootedfile, out existingreader);


                        if (existing != null)
                        {
                            RDAReader backupreader;
                            var backup = RDAManagerExtension.GetRDAFileFromPath(rdacfoldername + ";" + rootedfile, Properties.Settings.Default.RDABackupDir, out backupreader);

                            if (backup != null)
                            {
                                existing.SetData(backup.GetData());
                                AMMRDA.CommitChange(existingreader, AMMRDAManager.RDARequestType.SaveChanges);
                            }
                            else
                            {
                                RDAManagerExtension.RemoveRDAFromReader(existing, existingreader);
                            }
                        }
                    }
                }
            }
            #endregion
            foreach (string file in Directory.GetFiles(Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories))
            {
                current++;
                wrk.ReportProgress(ProgressBarExtension.Calculate(current, all));

                //Is Locked => continue
                if (LockedFiles.Contains(file))
                    continue;

                string td = file.Replace(Folder + "\\Files\\AppData", DirectoryExtension.GetAppDataFolder());
                if (System.IO.File.Exists(td))
                {
                    System.IO.File.Delete(td);
                }
            }
            foreach (XmlModuleList mod in XmlModules.OrderByDescending(md => md.Index))
            {
                current++;
                wrk.ReportProgress(ProgressBarExtension.Calculate(current, all));

                mod.Deactivate();
            }
            foreach (ListModuleList mod in ListModules.OrderByDescending(md => md.Index))
            {
                current++;
                wrk.ReportProgress(ProgressBarExtension.Calculate(current, all));

                mod.Deactivate();
            }

            XMLFileCollector.CheckContentFiles();
            ListFileCollector.CheckContentFiles();
        }

        public ModificationActivationResponse CheckActivation()
        {
            List<string> LockedFiles = ModificationUtils.Files_GetLockedFiles_InProject;
            ModificationActivationResponse resp = new ModificationActivationResponse();
            resp.Log.AppendLine("ModificationActivationResponse <= Modification.CheckActivation()\r\n");

            #region List UserdefinedValues
            resp.Log.AppendLine("UserdefinedValues:");
            foreach (UserdefinedValueGroup gr in UserdefinedValueGroups)
            {
                resp.Log.AppendLine("\tGroup '" + gr.InternalName + "':");
                foreach (UserdefinedValue val in UserdefinedValues.FindAll(v => v.Group == gr.InternalName))
                {
                    resp.Log.AppendLine("\t\tUserdefined Value '" + val.UI_Name + "' of Type " + val.Type + " = " + val.Current);
                }
            }
            #endregion
            #region XmlModules
            resp.Log.AppendLine("\r\nXmlModules:");
            Debug.WriteLine("XmlMods");
            foreach (XmlModuleList xml in XmlModules)
            {
                //Stopwatch w = new Stopwatch();
                //w.Start();

                ModificationActivationResponse n = xml.CheckActivation();
                resp.XmlModuleActive += n.XmlModuleActive;
                resp.XmlModuleCount += n.XmlModuleCount;

                resp.Log.Append(n.Log.ToString());
                resp.Log.AppendLine("\tXmlModuleList '" + xml.Name + "' -> " + n.Result() + "\r\n");

                //w.Stop();
                //Debug.WriteLine(w.ElapsedMilliseconds);
            }
            #endregion
            #region ListModules
            resp.Log.AppendLine("\r\nListModules:");
            foreach (ListModuleList List in ListModules)
            {
                ModificationActivationResponse n = List.CheckActivation();
                resp.ListModuleActive += n.ListModuleActive;
                resp.ListModuleCount += n.ListModuleCount;

                resp.Log.Append(n.Log.ToString());
                resp.Log.AppendLine("\tListModuleList '" + List.Name + "' -> " + n.Result() + "\r\n");
            }
            #endregion
            #region Anno Files
            {
                resp.Log.AppendLine("\r\nFiles in Anno 1404 RDAs:");
                Debug.WriteLine("FileMods");

                foreach (var rdacfolder in Directory.GetDirectories(Folder + "\\Files\\Anno1404"))
                {
                    #region Validate
                    string rdacfoldername = Path.GetFileName(rdacfolder);

                    if (rdacfoldername != "maindata" && rdacfoldername != "addon")
                        continue;
                    #endregion

                    foreach (var absfile in Directory.GetFiles(rdacfolder, "*", SearchOption.AllDirectories))
                    {
                        string rootedfile = absfile.Remove(0, rdacfolder.Length).Trim('\\');
                        resp.Log.AppendLine("\t" + "Checking " + rootedfile + "...");

                        //Is Locked => continue
                        if (LockedFiles.Contains(absfile))
                        {
                            resp.Log.AppendLine("\t" + "Not active, ignoring");
                            continue;
                        }
                        //Stopwatch w = new Stopwatch();
                        // w.Start();

                        //Count
                        resp.FileModuleCount++;
                        resp.FileModuleAnnoCount++;


                        RDAReader existingreader;
                        var existing = RDAManagerExtension.GetRDAFileFromPath(rdacfoldername + ";" + rootedfile, out existingreader);

                        // w.Stop();

                        if (existing != null)
                        {
                            FileInfo fi = new FileInfo(absfile);

                            if (fi.Length == existing.UncompressedSize)
                            {
                                try
                                {
                                    int nipos = 0;

                                    if (FileExtension.FilesAreEqual(fi, existing.GetData(), ref nipos))
                                    {
                                        resp.FileModuleActive++;
                                        resp.FileModuleAnnoActive++;
                                        resp.Log.AppendLine("\t" + "Activated");
                                    }
                                    else
                                        resp.Log.AppendLine("\t" + "Binary not equal @ " + nipos + " of " + fi.Length);
                                }
                                catch (Exception wy)
                                {
                                    System.Windows.Forms.MessageBox.Show(wy.Message);
                                }


                            }
                            else
                                resp.Log.AppendLine("\t" + "Not the same size");
                        }
                        else
                        {
                            resp.Log.AppendLine("\t" + "Not existing");
                        }

                    }
                }
            }
            #endregion

            //foreach (string file in Directory.GetFiles(Folder + "\\Files\\Anno1404", "*", SearchOption.AllDirectories))
            //{
            //    //Is Locked => continue                
            //    if (LockedFiles.Contains(file))
            //    {
            //        resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\Anno1404", "%Anno%") + " -> Not Active / true <= LockedFiles.Contains(file)");
            //        continue;
            //    }

            //    resp.FileModuleCount++;
            //    resp.FileModuleAnnoCount++;

            //    string annofile = file.Replace(Folder + "\\Files\\Anno1404", AnnoDirectoryHandler.GetCurrent());

            //    if (System.IO.File.Exists(annofile))
            //    {
            //        FileInfo info = new FileInfo(annofile);
            //        FileInfo info2 = new FileInfo(file);

            //        if (info.Length == info2.Length)
            //        {
            //            resp.FileModuleActive++;
            //            resp.FileModuleAnnoActive++;
            //            resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\Anno1404", "%Anno%") + " -> Activated. Size OK");
            //        }
            //        else
            //            resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\Anno1404", "%Anno%") + " -> Deactivated. Size not OK.");
            //        //if (FileExtension.CompareFiles(annofile, file))
            //        //{
            //        //    resp.FileModuleActive++;
            //        //    resp.FileModuleAnnoActive++;
            //        //    resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\Anno1404", "%Anno%") + " -> Activated. MD5 Compare OK");
            //        //}
            //        //else
            //        //{
            //        //    resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\Anno1404", "%Anno%") + " -> Deactivated. MD5 Compare not OK.");
            //        //}
            //    }
            //    else
            //    {
            //        resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\Anno1404", "%Anno%") + " -> Deactivated. Not existing.");
            //    }
            //}
        #endregion
            #region AppData Files
            foreach (string file in Directory.GetFiles(Folder + "\\Files\\AppData", "*", SearchOption.AllDirectories))
            {
                //Is Locked => continue
                if (LockedFiles.Contains(file))
                {
                    resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\AppData", "%AppData%") + " -> Not Active / true <= LockedFiles.Contains(file)");
                    continue;
                }

                resp.FileModuleCount++;
                resp.FileModuleAppDataCount++;

                string annofile = file.Replace(Folder + "\\Files\\AppData", DirectoryExtension.GetAMM4ApplicationDataFolder());

                if (System.IO.File.Exists(annofile))
                {
                    FileInfo info = new FileInfo(annofile);
                    FileInfo info2 = new FileInfo(file);

                    if (info.Length == info2.Length)
                    {
                        resp.FileModuleActive++;
                        resp.FileModuleAppDataActive++;
                        resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\AppData", "%AppData%") + " -> Activated. Size OK");
                    }
                    else
                        resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\AppData", "%AppData%") + " -> Deactivated. Size not OK.");
                }
                else
                {
                    resp.Log.AppendLine("\t" + file.Replace(Folder + "\\Files\\AppData", "%AppData%") + " -> Deactivated. Not existing.");
                }
            }
            #endregion

            return resp;
        }

        [Obsolete]
        public void CopySourceFileToFolder(string source, string destination)
        {
            source = source.FormatDevelopmentFolders();

            if (!Directory.Exists(Folder + "\\OriginalFiles"))
                Directory.CreateDirectory(Folder + "\\OriginalFiles");

            string file = Folder + "\\OriginalFiles\\" +
                    destination.Replace("\\", "_").Replace("/", "_").Replace("%", "#");
            if (!System.IO.File.Exists(file))
            {
                System.IO.File.Copy(source, Folder + "\\OriginalFiles\\" +
                        destination.Replace("\\", "_").Replace("/", "_").Replace("%", "#"), true);
            }

        }

        [Obsolete]
        public List<string> RemoveUnusedOriginalFiles()
        {
            if (Modification.Development_CurrentModification == null)
                throw new NotSupportedException("Development only");

            List<string> UnusedFiles = new List<string>();
            List<string> UsedFiles = new List<string>();
            UsedFiles.AddRange(XMLFileCollector.CollectedFiles.Keys);
            UsedFiles.AddRange(ListFileCollector.CollectedFiles.Keys);

            List<string> dir = Directory.GetFiles(Folder + "\\OriginalFiles").ToList();
            foreach (string d in dir)
            {
                if (!UsedFiles.Contains(d))
                {
                    System.IO.File.Delete(d);
                    UnusedFiles.Add(d);
                }
            }

            return UnusedFiles;
        }

    }
}
