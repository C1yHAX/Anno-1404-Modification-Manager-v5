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
using System.IO;
using AnnoModificationManager4.ModificationTypes;
using DevelopmentTools.Tools.Global.PatchTool_Classes;

using DevelopmentTools.Controls;
using AnnoModificationManager4.UserInterface.Misc;
using AnnoModificationManager4.Misc;
using Ionic.Zip;

namespace DevelopmentTools.Tools.Global
{
    /// <summary>
    /// Interaction logic for PatchTool.xaml
    /// </summary>
    public partial class PatchTool : Window
    {
        private SaveFileDialog SaveFileDialog_ProjectFile = new SaveFileDialog()
        {
            Filter = "Modification Project|*.zip"
        };

        public PatchTool()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(PatchTool_Loaded);
        }

        void PatchTool_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string file in Modification.Development_CurrentModification.CollectedFiles_Xml_List)
            {
                Files_List.Items.Add(new PatchCouple()
                {
                    DestinationFile = file
                });
            }
            foreach (string file in Modification.Development_CurrentModification.CollectedFiles_List_List)
            {
                Files_List.Items.Add(new PatchCouple()
                {
                    DestinationFile = file
                });
            }
        }

        private void Files_AssignNewSourceFile_Click(object sender, RoutedEventArgs e)
        {
            ContentButton Sender = sender as ContentButton;
            PatchCouple cp = Sender.Binding as PatchCouple;

            OpenFileDialog dlg = new OpenFileDialog()
            {
                Filter = "Matching File|" + Path.GetFileName(cp.DestinationFile) +
                "|Matching File (Type only)|*" + Path.GetExtension(cp.DestinationFile) +
                "|Any File|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                cp.NewSourceFile = dlg.FileName;
                Files_List.Items.Refresh();
            }
        }

        private void Files_DeleteNewSourceFile_Click(object sender, RoutedEventArgs e)
        {
            ContentButton Sender = sender as ContentButton;

            PatchCouple cp = Sender.Binding as PatchCouple;
            cp.NewSourceFile = "";

            Files_List.Items.Refresh();
        }

        private void Field_NewProject_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Modification.Development_CurrentModification.File))
                    SaveFileDialog_ProjectFile.InitialDirectory = Path.GetDirectoryName(Modification.Development_CurrentModification.File);
            }
            catch (Exception)
            {
            }

            if (SaveFileDialog_ProjectFile.ShowDialog() == true)
            {
                Field_NewProject.Text = SaveFileDialog_ProjectFile.FileName;
            }
        }

        private void button_Patch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Field_NewProject.Text))
                {
                    //First save
                    MessageBoxResult res = MessageWindow.Show("Do you want to save the project first?", MessageWindow.MessageWindowType.YesNoCancel);
                    if (res == MessageBoxResult.Yes)
                        Project.Development_CurrentProject.Modification.SaveFile();
                    else if (res == MessageBoxResult.No)
                        Project.Development_CurrentProject.Modification.SaveFolder();
                    else if (res == MessageBoxResult.Cancel)
                        return;

                    //Create Folder
                    string newFolder = Modification.Development_CurrentModification.Folder + "_Patched";
                    while (Directory.Exists(newFolder))
                    {
                        newFolder += RandomProvider.Random.Next(0, 9).ToString();
                    }

                    //Copy Folder
                    DirectoryExtension.copyDirectory(Modification.Development_CurrentModification.Folder, newFolder);

                    //Copy new SourceFiles
                    foreach (PatchCouple couple in Files_List.Items)
                    {
                        if (!string.IsNullOrEmpty(couple.NewSourceFile))
                        {
                            File.Copy(couple.NewSourceFile, newFolder + "\\OriginalFiles\\" + couple.DestinationFile.FormatProjectPath(), true);
                        }
                    }

                    //Save ZipFile
                    ZipFile zip = new ZipFile();
                    zip.AddDirectory(newFolder, "");

                    if (File.Exists(Field_NewProject.Text))
                        File.Delete(Field_NewProject.Text);

                    zip.Save(Field_NewProject.Text);
                    zip.Dispose();
                    zip = null;

                    GC.Collect();

                    //Clean
                    Directory.Delete(newFolder, true);

                    //Message
                    MessageWindow.Show("Patching successful.\nThe Development Tools will open the newly created project");

                    //Close old project
                    Project.Close();

                    //Open new Project
                    Project project = new Project();
                    project.OpenFile(Field_NewProject.Text);

                    MainWindow.CurrentMainWindow.Content = project.ProjectControl;
                    MainWindow.CurrentMainWindow.Activate();

                    //Run Test
                    project.ProjectControl.Project_Test_Click(null, null);

                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
            }
        }
    }
}
