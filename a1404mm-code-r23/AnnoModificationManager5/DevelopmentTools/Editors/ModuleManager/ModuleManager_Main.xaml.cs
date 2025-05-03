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
using AnnoModificationManager5.Controls;
using AnnoModificationManager5.Misc;
using System.IO;
using System.Diagnostics;
using AnnoModificationManager5.UserInterface.Misc;
using DevelopmentTools.Tools.Global;
using RDAExplorer;
using AnnoModificationManager5.ModificationTypes;

namespace DevelopmentTools.Editors.ModuleManager
{
    /// <summary>
    /// Interaction logic for ModuleManager_Main.xaml
    /// </summary>
    public partial class ModuleManager_Main : UserControl
    {
        private OpenFileDialog openSourceFileDialog = new OpenFileDialog();
        List<ModuleManager_OriginalFile> originalFiles = new List<ModuleManager_OriginalFile>();

        public ModuleManager_Main()
        {
            InitializeComponent();

            Project.Development_CurrentProject.UserInterface_Editors.Add(GetType(), this);
        }

        public void Refresh()
        {
            originalFiles = ModuleManager_OriginalFile.Generate();
            list_OriginalFiles.ItemsSource = originalFiles;
            list_OriginalFiles.Items.Refresh();
        }

        private void button_originalFiles_SetSourceFile_Click(object sender, RoutedEventArgs e)
        {
            ModuleManager_OriginalFile file = ((sender as ContentButton).Binding as ModuleManager_OriginalFile);
            //openSourceFileDialog.Filter = "Matching file|" + Path.GetFileName(file.DestinationFile)
            //    + "|Matching file (type only)|*" + Path.GetExtension(file.DestinationFile)
            //    + "|Any file|*.*";

            //if (openSourceFileDialog.ShowDialog() == true)
            //{
            //    file.SetSourceFile(openSourceFileDialog.FileName);
            //    #region Refresh Module Editors
            //    Project.Development_CurrentProject.ProjectControl.
            //                XmlModuleEditor.FileList_SelectedItemChanged(null, null);
            //    Project.Development_CurrentProject.ProjectControl.
            //        ListModuleEditor.FileList_SelectedItemChanged(null, null); 
            //    #endregion
            //}

            FileBrowser browser = new FileBrowser();
            if (browser.ShowDialog() == true)
            {
                file.SetFile(browser.ChoosenFile);
                #region Refresh Module Editors
                Project.Development_CurrentProject.ProjectControl.
                            XmlModuleEditor.FileList_SelectedItemChanged(null, null);
                Project.Development_CurrentProject.ProjectControl.
                    ListModuleEditor.FileList_SelectedItemChanged(null, null);
                #endregion

                Refresh();
            }
        }

        private void button_originalFiles_ShowSourceFile_Click(object sender, RoutedEventArgs e)
        {
            ModuleManager_OriginalFile file = ((sender as ContentButton).Binding as ModuleManager_OriginalFile);

            //if (File.Exists(file.SourceFile))
            //{
            //    Process.Start(file.SourceFile);
            //}

            //RDAFile existing = RDAManagerExtension.GetRDAFileFromPath(file.File);

            //if (existing != null)
            //{
            //    string tempfile = Path.GetTempPath().Trim('\\') +
            //    "\\DevelopmentTools4\\Temp\\" + existing.FileName.Replace("/", "_").Replace("\\", "_");
            //    tempfile = FileExtension.MakeFileUnique(tempfile);

            //    if (!Directory.Exists(Path.GetDirectoryName(tempfile)))
            //        Directory.CreateDirectory(Path.GetDirectoryName(tempfile));

            //    using (FileStream fs = new FileStream(tempfile, FileMode.Create))
            //    {
            //        var data = existing.GetData();
            //        fs.Write(data, 0, data.Length);
            //    }

            //    Process.Start(tempfile);
            //}

            FileBrowser.Process_OpenFile(file.File);
        }

        //private void button_originalFiles_SetDestination_Click(object sender, RoutedEventArgs e)
        //{
        //    ModuleManager_OriginalFile file = ((sender as ContentButton).Binding as ModuleManager_OriginalFile);
        //    string newfile = MessageWindow.GetText("New destination file:", file.DestinationFile);

        //    if (!string.IsNullOrEmpty(newfile))
        //    {
        //        file.SetDestinationFile(newfile);
        //        list_OriginalFiles.ItemsSource = null;
        //        list_OriginalFiles.ItemsSource = originalFiles;
        //        list_OriginalFiles.Items.Refresh();
        //        #region Refresh Module Editors
        //        Project.Development_CurrentProject.ProjectControl.
        //                    XmlModuleEditor.FileList_SelectedItemChanged(null, null);
        //        Project.Development_CurrentProject.ProjectControl.
        //            ListModuleEditor.FileList_SelectedItemChanged(null, null);
        //        #endregion
        //    }
        //}
    }
}
