using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.ModificationTypes;
using System.IO;
using Ionic.Zip;
using System.Threading;
using oldamm = AnnoModificationManager;
using System.Runtime.Serialization.Formatters.Binary;
using AnnoModificationManager4.ModificationTypes.Userdefined;
using AnnoModificationManager4.ModificationTypes.XmlModule;
using System.Xml;
using AnnoModificationManager4.ModificationTypes.TaskSystem;
using AnnoModificationManager4.ModificationTypes.ListModule;

namespace DevelopmentTools.Tools.Converter
{
    /// <summary>
    /// Interaction logic for ConverterTool.xaml
    /// </summary>
    public partial class ConverterTool : UserControl
    {
        private OpenFileDialog OpenOldProject = new OpenFileDialog()
        {
            Filter = "Modification Package|*.zip"
        };
        private SaveFileDialog SaveNewProject = new SaveFileDialog()
        {
            Filter = "Modification Project|*.zip"
        };

        Project newProject;
        Modification projectModification;
        string oldProjectFolder;
        string oldProjectZipFile;
        string newProjectZipFile;

        public ConverterTool()
        {
            InitializeComponent();
        }

        private void Field_OldProject_Open_Click(object sender, RoutedEventArgs e)
        {
            if (OpenOldProject.ShowDialog() == true)
            {
                Field_OldProject.Text = OpenOldProject.FileName;
                oldProjectZipFile = OpenOldProject.FileName;
            }
        }

        private void Field_NewProject_Open_Click(object sender, RoutedEventArgs e)
        {
            if (SaveNewProject.ShowDialog() == true)
            {
                Field_NewProject.Text = SaveNewProject.FileName;
                newProjectZipFile = Field_NewProject.Text;
            }
        }

        private void Field_Status_Append(string i)
        {
            Field_Status.Dispatch(p =>
                {
                    p.AppendText(i + "\n");
                    p.ScrollToEnd();
                });            
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentMainWindow.Content = new DevelopmentTools.StartPage();
        }

        private void button_Patch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Field_NewProject.Text) | string.IsNullOrEmpty(Field_OldProject.Text))
                return;

            tabControl.SelectedIndex = 1;
            newProject = new Project();
            newProject.CreateProject();
            projectModification = newProject.Modification;
            projectModification.File = Field_NewProject.Text;
            button_Patch.Visibility = System.Windows.Visibility.Collapsed;
            button_Cancel.Visibility = System.Windows.Visibility.Collapsed;

            Thread thread = new Thread(new ThreadStart(Convert));
            thread.Start();
        }

        private void Convert()
        {
            try
            {
                Field_Status_Append("Preparing ...");
                ConvertPrepare();
                Field_Status_Append("Step 1: Converting config.ini to config.xml");
                ConvertModificationInfo();
                Field_Status_Append("Step 2: Converting Propriae Viles to Userdefined Values");
                ConvertUserdefinedValues();
                Field_Status_Append("Step 3: Converting raw files");
                ConvertFiles();
                ConvertModules();
                Field_Status_Append("Finished.");
                Field_Status_Append("Saving ...");
                projectModification.SaveFolder();
                projectModification.SaveFile(newProjectZipFile, false);

                newProject.ProjectControl.Dispatch(pr =>
                    {
                        pr.Refresh();
                    });
                MainWindow.CurrentMainWindow.Dispatch(wnd =>
                    {
                        wnd.Content = newProject.ProjectControl;
                    });

                GC.Collect();

                Directory.Delete(oldProjectFolder, true);               
            }
            catch (Exception ex)
            {
                Directory.Delete(oldProjectFolder, true);
                Project.Close();
                Application.Current.Dispatch(app =>
                    {
                        AnnoModificationManager4.UserInterface.Misc.MessageWindow.Show(ex.Message);
                        MainWindow.CurrentMainWindow.Content = new DevelopmentTools.StartPage();
                    });
            }
        }

        private void ConvertPrepare()
        {
            string folder = Path.GetTempPath().Trim('\\') + "\\DevelopmentTools4\\ConverterTool_OldModification";
            while (Directory.Exists(folder))
            {
                folder += RandomProvider.Random.Next(0, 9);
            }
            Directory.CreateDirectory(folder);

            ZipFile zip = new ZipFile(oldProjectZipFile);
            zip.ExtractAll(folder);

            oldProjectFolder = folder;
        }

        private void ConvertModificationInfo()
        {
            #region Config.ini
            foreach (string f in File.ReadAllLines(oldProjectFolder + "\\config.ini"))
            {
                if (string.IsNullOrEmpty(f))
                    continue;

                string type = f.Split('=')[0];
                string value = StringExtension.PutTogether(f.Split('=').ToList().GetRange(1, f.Split('=').Length - 1), '=');

                switch (type)
                {
                    case "Name":
                        projectModification.Info.InternalName = value;
                        projectModification.Info.Name.German = value;
                        break;
                    case "EName":
                        projectModification.Info.Name.English = value;
                        break;
                    case "Preview":
                        projectModification.Info.Images.Add(value);
                        File.Copy(oldProjectFolder + "\\" + value, projectModification.Folder + "\\Images\\" + value);
                        break;
                    case "Author":
                        projectModification.Info.Author = value;
                        break;
                    case "Website":
                        projectModification.Info.Website = value;
                        break;
                    case "Category":
                        projectModification.Info.InternalCategory = value;
                        projectModification.Info.Category.German = value;
                        break;
                    case "ECategory":
                        projectModification.Info.Category.English = value;
                        break;
                    case "Version":
                        projectModification.Info.Version = new Version(value);
                        projectModification.Info.Version=new Version(
                            projectModification.Info.Version.Major > 0 ? projectModification.Info.Version.Major : 1,
                            projectModification.Info.Version.Minor > 0 ? projectModification.Info.Version.Minor : 0,
                            projectModification.Info.Version.Build > 0 ? projectModification.Info.Version.Build : 0,
                                projectModification.Info.Version.Revision > 0 ? projectModification.Info.Version.Revision : 0);
                        break;
                    case "AnnoVersion":
                        switch (value)
                        {
                            case "<All Versions>":
                                projectModification.Info.AnnoVersions.Add("All");
                                break;
                            case "Addon":
                                projectModification.Info.AnnoVersions.Add("Addon1");
                                break;
                            default:
                                projectModification.Info.AnnoVersions.Add(value);
                                break;
                        }
                        break;
                }
            } 
            #endregion
            #region Descriptions
            if (File.Exists(oldProjectFolder + "\\description.txt"))
            {
                projectModification.Info.Description.German = File.ReadAllText(oldProjectFolder + "\\description.txt");
            }
            if (File.Exists(oldProjectFolder + "\\edescription.txt"))
            {
                projectModification.Info.Description.English = File.ReadAllText(oldProjectFolder + "\\edescription.txt");
            }
            #endregion
        }

        private void ConvertUserdefinedValues()
        {
            List<oldamm.PropriaVilis> pvs = new List<oldamm.PropriaVilis>();

            foreach (string file in Directory.GetFiles(oldProjectFolder, "*.pv"))
            {
                BinaryFormatter bin = new BinaryFormatter();
                

                using (StreamReader w = new StreamReader(file))
                {
                    pvs.AddRange(bin.Deserialize(w.BaseStream) as List<oldamm.PropriaVilis>);
                }
            }

            foreach (oldamm.PropriaVilis pv in pvs)
            {
                if (projectModification.UserdefinedValueGroups.Find(gr => gr.InternalName == pv.Category) == null)
                {
                    //Create Group, if not existing
                    projectModification.UserdefinedValueGroups.Add(new UserdefinedValueGroup()
                    {
                        InternalName = pv.Category,
                        Label_Name = new AnnoModificationManager4.Language.Label()
                        {
                            Name = "Name",
                            German = pv.Category,
                            English = pv.ECategory
                        }
                    });

                    //Convert
                    UserdefinedValue val = new UserdefinedValue();
                    val.Name = pv.Category + "_" + pv.Name;

                    val.Label_Name.German = pv.Name;
                    val.Label_Name.English = pv.EName;

                    val.Label_Description.German = pv.Explanation;
                    val.Label_Description.English = pv.EExplanation;

                    val.Current = pv.CurrentValue;

                    val.Group = pv.Category;

                    val.Index = pvs.IndexOf(pv);
                    
                    switch (pv.Validitation.PTypus)
                    {
                        case oldamm.ProbatioPropriaeVilis.Typus.String:
                            val.Type = UserdefinedValue.UserdefinedValueType.TextEdit;
                            break;
                        case oldamm.ProbatioPropriaeVilis.Typus.Numeric:
                            val.Type = UserdefinedValue.UserdefinedValueType.Numeric;
                            val.Numeric_Min = pv.Validitation.Minimum;
                            val.Numeric_Max = pv.Validitation.Maximum;
                            break;
                    }

                    if (pv.AviableItems != null && pv.AviableItems.Count != 0)
                    {
                        val.Type = UserdefinedValue.UserdefinedValueType.ComboBox;
                        foreach (oldamm.Tripair tri in pv.AviableItems)
                        {
                            val.ComboBoxItems.Add(new UserdefinedValue_ComboBoxItem()
                            {
                                Name = new AnnoModificationManager4.Language.Label()
                                {
                                    Name = "Element",
                                    German = tri.Name,
                                    English = tri.EName
                                },
                                Value = tri.Value
                            });
                        }
                    }

                    //Add to Modification
                    projectModification.UserdefinedValues.Add(val);
                }
            }
        }

        private void ConvertFiles()
        {
            //First copy files
            List<string> files = Directory.GetFiles(oldProjectFolder).ToList();
            files.RemoveAll(f =>
                {
                    if (Path.GetFileName(f) == "config.ini")
                        return true;
                    if (Path.GetFileName(f) == "description.txt")
                        return true;
                    if (Path.GetFileName(f) == "edescription.txt")
                        return true;
                    if (Path.GetExtension(f).ToLower() == ".pv")
                        return true;
                    if (Path.GetExtension(f).ToLower() == ".textmodule")
                        return true;
                    if (projectModification.Info.Images.Count != 0 && Path.GetFileName(f) == projectModification.Info.Images[0])
                        return true;
                    return false;
                });
            foreach (string f in files)
            {
                File.Copy(f, projectModification.Folder + "\\Files\\Anno1404\\" + Path.GetFileName(f));
            }

            //Copy directories
            foreach (string dir in Directory.GetDirectories(oldProjectFolder))
            {
                DirectoryExtension.copyDirectory(dir, projectModification.Folder + "\\Files\\Anno1404\\" + Path.GetFileName(dir));
            }
        }

        private void ConvertModules()
        {
            Field_Status_Append("Step 4: Converting TextModules (*.xml) to XmlModules");
            ConvertXmlModules();
            Field_Status_Append("Step 5: Converting TextModules (*.txt, *.ini, ...) to ListModules");
            ConvertListModules();
        }

        private void ConvertXmlModules()
        {
            int file_index = 0;
            //Dictionary<string, XMLFile> CollectedXmlFiles = new Dictionary<string, XMLFile>();

            foreach (string file in Directory.GetFiles(oldProjectFolder, "*.textmodule"))
            {
                Field_Status_Append("\t" + file);

                List<oldamm.TextModule> textmodules = new List<oldamm.TextModule>();

                //Load TM with != XML
                using (StreamReader w = new StreamReader(file))
                {
                    BinaryFormatter f = new BinaryFormatter();
                    textmodules.AddRange((f.Deserialize(w.BaseStream) as List<oldamm.TextModule>).
                        FindAll(tm => Path.GetExtension(tm.destinationFile).ToLower() == ".xml"));
                }

                if (textmodules.Count == 0)
                    continue;

                //Precreate XmlModuleList
                XmlModuleList xmlmoduleList = new XmlModuleList();
                xmlmoduleList.Index = file_index;
                xmlmoduleList.Name = Path.GetFileNameWithoutExtension(file);
                xmlmoduleList.Parent = projectModification;          
     
                //TaskList
                TaskList xmlTaskList = new TaskList();
                xmlTaskList.Name = "XmlModule " + xmlmoduleList.Name;                  

                #region Prepare
                foreach (oldamm.TextModule text in textmodules)
                {
                    //Copy OriginalFile / Convert destination file
                    string folder = text.destinationFile;
                    if (!folder.Contains(":"))
                    {
                        folder = ("%Anno%\\" + folder.Replace("/", "\\")).Replace("\\\\", "\\");
                    }
                    else
                    {
                        folder = folder.Replace("/", "\\").Replace("AppData:", "%AppData%");
                    }

                    text.destinationFile = folder; //Set as destinationFile!

                    //Now Copy, if not existing
                    string destinationfile = projectModification.Folder + "\\OriginalFiles\\" + folder.FormatProjectPath();
                    if (!File.Exists(destinationfile))
                    {
                        using (StreamWriter w = new StreamWriter(destinationfile, false, Encoding.Unicode))
                        {
                            w.Write(text.originalFileData);
                        }
                        Field_Status_Append("\t\t" + folder + " written");

                        /*XMLFile xml = new XMLFile(destinationfile);
                        CollectedXmlFiles.Add(folder, xml);*/
                    }

                    //remove originalFileData to save space
                    text.originalFileData = null;
                }
                #endregion
                #region Create
                foreach (oldamm.TextModule text in textmodules)
                {
                    Field_Status_Append("\t\t\tTextModule " + (textmodules.IndexOf(text) + 1) + "/" + textmodules.Count + ":");
                    Field_Status_Append("\t\t\t\tTask created.");

                    //Generate original and modified
                    string original = text.moduleType == oldamm.TextModule.moduleTypes.Replace ? text.replace_original : text.add_after;
                    string modified = text.moduleType == oldamm.TextModule.moduleTypes.Replace ? text.replace_modified : text.add_after + text.add_text;
                    original = XmlExtension.RemoveEmptys(original);
                    modified = XmlExtension.RemoveEmptys(modified);

                    Task newTask = new Task();
                    newTask.Type = Task.TaskType.TextModule;
                    newTask.Name = "Convert Textmodule (" + text.destinationFile + ")";
                    newTask.Original = XmlExtension.IndentString(original);
                    newTask.Modified = XmlExtension.IndentString(modified);

                    xmlTaskList.Tasks.Add(newTask);
                }
                #endregion

                projectModification.XmlModules.Add(xmlmoduleList);
                projectModification.Tasks.Add(xmlTaskList);
                file_index++;
            }
        }

        private void ConvertListModules()
        {
            int file_index = 0;
            //Dictionary<string, XMLFile> CollectedXmlFiles = new Dictionary<string, XMLFile>();

            foreach (string file in Directory.GetFiles(oldProjectFolder, "*.textmodule"))
            {
                Field_Status_Append("\t" + file);

                List<oldamm.TextModule> textmodules = new List<oldamm.TextModule>();

                //Load TM with != XML
                using (StreamReader w = new StreamReader(file))
                {
                    BinaryFormatter f = new BinaryFormatter();
                    textmodules.AddRange((f.Deserialize(w.BaseStream) as List<oldamm.TextModule>).
                        FindAll(tm => Path.GetExtension(tm.destinationFile).ToLower() != ".xml"));
                }

                if (textmodules.Count == 0)
                    continue;

                //Precreate XmlModuleList
                ListModuleList xmlmoduleList = new ListModuleList();
                xmlmoduleList.Index = file_index;
                xmlmoduleList.Name = Path.GetFileNameWithoutExtension(file);
                xmlmoduleList.Parent = projectModification;

                //TaskList
                TaskList xmlTaskList = new TaskList();
                xmlTaskList.Name = "ListModule " + xmlmoduleList.Name;
                

                #region Prepare
                foreach (oldamm.TextModule text in textmodules)
                {
                    //Copy OriginalFile / Convert destination file
                    string folder = text.destinationFile;
                    if (!folder.Contains(":"))
                    {
                        folder = ("%Anno%\\" + folder.Replace("/", "\\")).Replace("\\\\", "\\");
                    }
                    else
                    {
                        folder = folder.Replace("/", "\\").Replace("AppData:", "%AppData%");
                    }

                    text.destinationFile = folder; //Set as destinationFile!

                    //Now Copy, if not existing
                    string destinationfile = projectModification.Folder + "\\OriginalFiles\\" + folder.FormatProjectPath();
                    if (!File.Exists(destinationfile))
                    {
                        using (StreamWriter w = new StreamWriter(destinationfile, false, Encoding.Unicode))
                        {
                            w.Write(text.originalFileData);
                        }
                        Field_Status_Append("\t\t" + folder + " written");

                        /*XMLFile xml = new XMLFile(destinationfile);
                        CollectedXmlFiles.Add(folder, xml);*/
                    }

                    //remove originalFileData to save space
                    text.originalFileData = null;
                }
                #endregion
                #region Create
                foreach (oldamm.TextModule text in textmodules)
                {
                    Field_Status_Append("\t\t\tTextModule " + (textmodules.IndexOf(text) + 1) + "/" + textmodules.Count + ":");
                    Field_Status_Append("\t\t\t\tTask created.");

                    //Generate original and modified
                    string original = text.moduleType == oldamm.TextModule.moduleTypes.Replace ? text.replace_original : text.add_after;
                    string modified = text.moduleType == oldamm.TextModule.moduleTypes.Replace ? text.replace_modified : text.add_after + text.add_text;
                    original = XmlExtension.RemoveEmptys(original);
                    modified = XmlExtension.RemoveEmptys(modified);

                    Task newTask = new Task();
                    newTask.Type = Task.TaskType.TextModule;
                    newTask.Name = "Convert Textmodule (" + text.destinationFile + ")";
                    newTask.Original = XmlExtension.IndentString(original);
                    newTask.Modified = XmlExtension.IndentString(modified);

                    xmlTaskList.Tasks.Add(newTask);
                }
                #endregion

                projectModification.ListModules.Add(xmlmoduleList);
                projectModification.Tasks.Add(xmlTaskList);
                file_index++;
            }
        }        
    }
}
