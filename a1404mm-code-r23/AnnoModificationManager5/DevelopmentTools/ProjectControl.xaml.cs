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
using System.IO;
using AnnoModificationManager5.ModificationTypes;
using AnnoModificationManager5.UserInterface.Misc;
using DevelopmentTools.Editors.XmlModule;
using DevelopmentTools.Editors.ListModule;
using DevelopmentTools.Editors.ModificationInfo;
using DevelopmentTools.Tools.Global;
using DevelopmentTools.Editors.UserdefinedValue;
using DevelopmentTools.Editors.FileSystem;
using AnnoModificationManager5.Misc;
using System.Diagnostics;
using DevelopmentTools.PluginSystem;
using DevelopmentTools.Editors.ModuleManager;

namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for Project.xaml
    /// </summary>
    public partial class ProjectControl : UserControl
    {
        public ModificationInfoEditor ModificationInfoEditor = new ModificationInfoEditor();
        public FileSystemEditor_Main FileSystemEditor_AnnoFolder = new FileSystemEditor_Main();
        public FileSystemEditor_Main FileSystemEditor_AppDataFolder = new FileSystemEditor_Main();
        public ModuleManager_Main ModuleManager = new ModuleManager_Main();
        public XmlModuleEditor_Main XmlModuleEditor = new XmlModuleEditor_Main();
        public ListModuleEditor_Main ListModuleEditor = new ListModuleEditor_Main();
        public UserdefinedValueEditor_Main UserdefinedValueEditor = new UserdefinedValueEditor_Main();
        //DevelopmentTools.Misc.TestUserControl tst = new Misc.TestUserControl();

        public ProjectControl()
        {
            this.InitializeComponent();
            Loaded += new RoutedEventHandler(ProjectControl_Loaded);
            //Focus();
        }

        void ProjectControl_Loaded(object sender, RoutedEventArgs e)
        {
            EditorList_ModificationInfoEditor.IsSelected = true;

            try
            {
                string t = MainWindow.CurrentMainWindow.Title;
                int idx = t.IndexOf(" - ");
                string name = idx >= 0 ? t.Substring(idx + 3) : t;
                if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(0, name.Length - 4);
                // Strip a trailing Nexus-style version suffix like "-14-1-0-1673039526".
                name = System.Text.RegularExpressions.Regex.Replace(name, @"-\d+(-\d+){2,}$", "").Trim();
                lbl_ProjectName.Text = name;
                bc_Project.Text = name;
            }
            catch (Exception) { }
        }

        public void Refresh()
        {
            ModificationInfoEditor.Refresh();
            XmlModuleEditor.Refresh();
            ListModuleEditor.Refresh();
            UserdefinedValueEditor.Refresh();

            FileSystemEditor_AnnoFolder.RootFolder = Modification.Development_CurrentModification.Folder + "\\Files\\Anno1404";
            FileSystemEditor_AnnoFolder.Refresh();

            FileSystemEditor_AppDataFolder.RootFolder = Modification.Development_CurrentModification.Folder + "\\Files\\AppData";
            FileSystemEditor_AppDataFolder.Refresh();

            RefreshPlugins();
        }

        private void RefreshPlugins()
        {
            //Plugins
            Menu_Plugins.Items.Clear();
            foreach (IDevelopmentPlugin plugin in PluginHandler.Plugins)
            {
                MenuItem item = new MenuItem();
                item.Icon = new Image()
                {
                    Source = plugin.Icon != null ? plugin.Icon :
                       BitmapImageExtension.Load("pack://application:,,,/Images/Icons/brick.png"),
                    Width = 16,
                    Height = 16
                };
                item.Header = new TextBlock() { Text = plugin.Name };

                item.Click += (sender, e) => plugin.RunPlugin();

                Menu_Plugins.Items.Add(item);
            }

            if (Menu_Plugins.Items.Count == 0)
                Menu_Plugins.Visibility = System.Windows.Visibility.Collapsed;
            else
                Menu_Plugins.Visibility = System.Windows.Visibility.Visible;
        }

        private void EditorList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            string section = "";
            if (EditorList.SelectedItem == EditorList_ModificationInfoEditor)
            {
                CurrentEditor.Content = ModificationInfoEditor;
                section = "Project Settings";
            }
            else if (EditorList.SelectedItem == EditorList_FileSystem_Anno)
            {
                CurrentEditor.Content = FileSystemEditor_AnnoFolder;
                section = "Files  ›  Anno Directory";
            }
            else if (EditorList.SelectedItem == EditorList_FileSystem_AppData)
            {
                CurrentEditor.Content = FileSystemEditor_AppDataFolder;
                section = "Files  ›  Application Data";
            }
            else if (EditorList.SelectedItem == EditorList_Modulemanager)
            {
                CurrentEditor.Content = ModuleManager;
                section = "Module Manager";

                //Refresh everytime, the manager is opened
                ModuleManager.Refresh();
            }
            else if (EditorList.SelectedItem == EditorList_XmlModuleEditor)
            {
                CurrentEditor.Content = XmlModuleEditor;
                section = "XML Modules";
            }
            else if (EditorList.SelectedItem == EditorList_ListModuleEditor)
            {
                CurrentEditor.Content = ListModuleEditor;
                section = "List Modules";
            }
            else if (EditorList.SelectedItem == EditorList_UserdefinedValueEditor)
            {
                CurrentEditor.Content = UserdefinedValueEditor;
                section = "Userdefined Values";
            }

            if (bc_Section != null && section.Length != 0)
                bc_Section.Text = section;

            var pres = (Control)CurrentEditor.Content;
            pres.UpdateLayout();
            pres.Focus();
        }

        #region Default Save
        private void Project_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Modification.Development_CurrentModification.SaveFile();
                try
                {
                    MainWindow.CurrentMainWindow.Title = "Development Tools Version 5 - "
                                + Path.GetFileName(Modification.Development_CurrentModification.File);
                }
                catch (Exception)
                {
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
            }
        }

        private void Project_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Modification.Development_CurrentModification.SaveFile_As();
                try
                {
                    MainWindow.CurrentMainWindow.Title = "Development Tools Version 5 - "
                                + Path.GetFileName(Modification.Development_CurrentModification.File);
                }
                catch (Exception)
                {
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
            }
        }

        private void Project_SaveCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Modification.Development_CurrentModification.SaveFile_Copy();
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
            }
        }
        #endregion

        private void Project_State_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Modification.Development_CurrentModification.SaveFolder();
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
            }
        }

        private void Project_State_Load_Click(object sender, RoutedEventArgs e)
        {
            //try
            {
                EditorList_ModificationInfoEditor.IsSelected = true;

                Modification.Development_CurrentModification.OpenFolder(
                    Modification.Development_CurrentModification.Folder);
                Refresh();
            }
            //catch (Exception ex)
            //{               
            //    MessageWindow.Show(ex.Message);
            //}
        }

        private void Project_Close_Click(object sender, RoutedEventArgs e)
        {
            if (MessageWindow.Show("Do you really want to close this project?", MessageWindow.MessageWindowType.YesNo)
                    == MessageBoxResult.No)
            {
                return;
            }

            Project.Close();
            MainWindow.CurrentMainWindow.MainWindow_Loaded(null, null);
        }

        public void Project_Test_Click(object sender, RoutedEventArgs e)
        {
            (new TestTool()).Show();
        }

        private void Project_Patch_Click(object sender, RoutedEventArgs e)
        {
            (new PatchTool()).ShowDialog();
        }

        private void Application_Exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentMainWindow.Close();
        }

        private void Tools_OpenPublisher_Click(object sender, RoutedEventArgs e)
        {
            Tools.PackagePublisher.PackagePublisher pub = new Tools.PackagePublisher.PackagePublisher();
            pub.PreviousContent = MainWindow.CurrentMainWindow.Content;
            MainWindow.CurrentMainWindow.Content = pub;
        }

        //private void Project_RemoveUnusedOriginalFiles_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MessageWindow.Show("All files in the folder 'OriginalFiles', which are not used by Modifiers will be deleted",
        //         MessageWindow.MessageWindowType.OKCancel) == MessageBoxResult.OK)
        //    {
        //        MessageWindow.Show(StringExtension.PutTogether(Project.Development_CurrentProject.Modification.RemoveUnusedOriginalFiles(), '\n'));
        //    }
        //}

        private void Project_OpenProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Project.Development_CurrentProject.Modification.Folder);
        }

        private void Tools_OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.PreviousContent = MainWindow.CurrentMainWindow.Content;
            MainWindow.CurrentMainWindow.Content = settings;
        }

        private void Project_Publish_Click(object sender, RoutedEventArgs e)
        {
            Tools.PackagePublisher.PackagePublisher pub =
                new Tools.PackagePublisher.PackagePublisher();
            pub.AutoAddCurrentProject = true;
            pub.PreviousContent = MainWindow.CurrentMainWindow.Content;
            MainWindow.CurrentMainWindow.Content = pub;
        }

        private void Help_ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpView help = new HelpView();
            help.PreviousContent = MainWindow.CurrentMainWindow.Content;
            MainWindow.CurrentMainWindow.Content = help;
        }

        private void Help_ShowInfo_Click(object sender, RoutedEventArgs e)
        {
            (new InfoDialog()).ShowDialog();
        }

        private void Project_Open_Click(object sender, RoutedEventArgs e)
        {
            if (MessageWindow.Show("Do you really want to close this project?", MessageWindow.MessageWindowType.YesNo)
                  == MessageBoxResult.No)
            {
                return;
            }

            Project.Close();
            MainWindow.CurrentMainWindow.MainWindow_Loaded(null, null);

            {
                OpenFileDialog OpenFile = new OpenFileDialog()
      {
          Filter = "Modification Project|*.zip"
      };

                if (OpenFile.ShowDialog() == true)
                {
                    Project project = new Project();
                    project.OpenFile(OpenFile.FileName);

                    MainWindow.CurrentMainWindow.Content = project.ProjectControl;
                    MainWindow.CurrentMainWindow.Activate();
                    if (!Properties.Settings.Default.StartPage_RecentFiles.Contains(OpenFile.FileName))
                    {
                        Properties.Settings.Default.StartPage_RecentFiles += ";" + OpenFile.FileName;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }
    }
}