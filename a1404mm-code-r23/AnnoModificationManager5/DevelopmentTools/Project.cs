using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager5.ModificationTypes;
using System.Windows.Controls;
using System.IO;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.ModificationTypes.XmlModule;
using AnnoModificationManager5.ModificationTypes.ListModule;
using DevelopmentTools.Tools.TaskSystem;
using System.Windows;

namespace DevelopmentTools
{
    public class Project
    {
        public static Project Development_CurrentProject;

        public Modification Modification;
        public ProjectControl ProjectControl;

        public Dictionary<Type, UserControl> UserInterface_Editors = new Dictionary<Type, UserControl>();

        public Project()
        {
            Development_CurrentProject = this;
            Modification.Development_RDADirectory = Properties.Settings.Default.RDAWorkingDir;
            PluginSystem.PluginHandler.UpdatePlugins();
        }

        public static void Close()
        {
            if (Development_CurrentProject != null)
            {
                string folder = Modification.Development_CurrentModification.Folder;

                Development_CurrentProject.ProjectControl = null;
                Development_CurrentProject = null;
                Modification.Development_CurrentModification = null;

                //Close TaskWindow
                if (TaskWindow.CurrentTaskWindow != null)
                {
                    TaskWindow.CurrentTaskWindow.Close();
                }
                TaskWindow.CurrentTaskWindow = null;

                //Destroy Files
                XMLFileCollector.CollectedFiles.Clear();
                ListFileCollector.CollectedFiles.Clear();

                //Remove Folder
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception)
                {
                }

                //Update Plugins
                PluginSystem.PluginHandler.UpdatePlugins();

                GC.Collect();
            }
        }

        public void OpenFile(string folder)
        {
            Modification = new Modification(true);
            Modification.IsDevelopment = true;

            Modification.OpenFile(folder);

            //Convert
            if (!Modification.Info.SupportsAMM4RDA)
            {
                ConvertToRDAWindow dlg = new ConvertToRDAWindow();
                dlg._ToConvert = Modification;

                if (dlg.ShowDialog() == true)
                {
                    OpenFolder(dlg._ConvertedProjectFolder);
                    return;
                }
                else
                {
                    MessageBox.Show("Cannot open project.");
                    Project.Close();
                    MainWindow.CurrentMainWindow.MainWindow_Loaded(null, null);
                    return;
                }
            }

            ProjectControl = new ProjectControl();
            ProjectControl.Refresh();

            MainWindow.CurrentMainWindow.Title = "Development Tools Version 4 - " + Path.GetFileName(folder);
        }

        public void OpenFolder(string folder)
        {
            Modification = new Modification(true);
            Modification.IsDevelopment = true;

            Modification.OpenFolder(folder);

            //Convert
            if (!Modification.Info.SupportsAMM4RDA)
            {
                ConvertToRDAWindow dlg = new ConvertToRDAWindow();
                dlg._ToConvert = Modification;

                if (dlg.ShowDialog() == true)
                {
                    OpenFolder(dlg._ConvertedProjectFolder);
                    return;
                }
                else
                {
                    MessageBox.Show("Cannot open project.");
                    Project.Close();
                    MainWindow.CurrentMainWindow.MainWindow_Loaded(null, null);
                    return;
                }
            }

            ProjectControl = new ProjectControl();
            ProjectControl.Refresh();

            MainWindow.CurrentMainWindow.Title = "Development Tools Version 4 - " + Path.GetFileName(folder);
        }

        public void CreateProject()
        {
            //Create temporary Folder
            string folder = DirectoryExtension.UnifyDirectory(Path.GetTempPath().Trim('\\') +
                "\\DevelopmentTools4\\NewProject");

            Directory.CreateDirectory(folder);

            Modification = new Modification(true);
            Modification.IsDevelopment = true;
            Modification.SaveFolder(folder);

            ProjectControl = new ProjectControl();
            ProjectControl.Refresh();

            MainWindow.CurrentMainWindow.Title = "Development Tools Version 4 - New Project";
        }
    }
}
