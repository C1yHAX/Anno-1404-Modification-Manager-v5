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
using AnnoModificationManager5.ModificationTypes;
using System.Text.RegularExpressions;
using System.IO;

using System.Diagnostics;
using AnnoModificationManager5.Misc;

namespace DevelopmentTools.Editors.ModificationInfo
{
    /// <summary>
    /// Interaction logic for ModificationInfoEditor.xaml
    /// </summary>
    public partial class ModificationInfoEditor : UserControl
    {
        public new Project Parent;
        private OpenFileDialog openDocumentation = new OpenFileDialog();
        private OpenFileDialog openImage = new OpenFileDialog()
        {
            Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp",
            Multiselect = true
        };
        private bool ModificationLoaded = false;

        public ModificationInfoEditor()
        {
            InitializeComponent();

            Project.Development_CurrentProject.UserInterface_Editors.Add(GetType(), this);
        }

        public void Refresh()
        {
            ModificationLoaded = false;
            Modification mod = Modification.Development_CurrentModification;

            Field_Author.Text = mod.Info.Author;
            Field_Category.Text = mod.Info.InternalCategory;
            Field_Documentation.Text = mod.Info.Documentation;
            Field_InternalName.Text = mod.Info.InternalName;
            Field_Website.Text = mod.Info.Website;

            Field_Version_Major.Text = mod.Info.Version.Major.ToString();
            Field_Version_Minor.Text = mod.Info.Version.Minor.ToString();
            Field_Version_Build.Text = mod.Info.Version.Build.ToString();
            Field_Version_Revision.Text = mod.Info.Version.Revision.ToString();

            //chb_IgnoreNonExistingRDAs.IsChecked = mod.Info.RDAIgnoreNonExistingRDAMods;

            LabelEditor_Category.Label = mod.Info.Category;
            LabelEditor_Description.Label = mod.Info.Description;
            LabelEditor_Name.Label = mod.Info.Name;

            AnnoVersions_All.IsChecked = mod.Info.AnnoVersions.Contains("All");
            AnnoVersions_Retail.IsChecked = mod.Info.AnnoVersions.Contains("Retail");
            AnnoVersions_Patch1.IsChecked = mod.Info.AnnoVersions.Contains("Patch1");
            AnnoVersions_Patch2.IsChecked = mod.Info.AnnoVersions.Contains("Patch2");
            AnnoVersions_Patch3.IsChecked = mod.Info.AnnoVersions.Contains("Patch3");
            AnnoVersions_Addon1.IsChecked = mod.Info.AnnoVersions.Contains("Addon1");
            AnnoVersions_Addon1_Patch1.IsChecked = mod.Info.AnnoVersions.Contains("Addon1_Patch1");
            AnnoVersions_IAAM.IsChecked = mod.Info.AnnoVersions.Contains("IAAM");

            Images.Items.Clear();
            foreach (string image in mod.Info.Images)
            {
                ListViewItem item = new ListViewItem();
                item.Content = new Image()
                {
                    Width = 80,
                    Height = 80,
                    Source = BitmapImageExtension.Load(mod.Folder + "\\Images\\" + image)
                };

                Images.Items.Add(item);
            }

            Images_Clean();

            ModificationLoaded = true;
        }

        #region Fields
        private void Field_UpdateModification(object sender, TextChangedEventArgs e)
        {
            if (ModificationLoaded)
            {
                Modification mod = Modification.Development_CurrentModification;
                mod.Info.InternalName = Field_InternalName.Text;
                mod.Info.Author = Field_Author.Text;
                mod.Info.InternalCategory = Field_Category.Text;
                //mod.Info.Documentation = Field_Documentation.Text;                
                mod.Info.Website = Field_Website.Text;
            }
        }

        private void Field_Documentation_Open_Click(object sender, RoutedEventArgs e)
        {
            if (openDocumentation.ShowDialog() == true)
            {
                Field_Documentation_Remove_Click(null, null);
                Field_Documentation.Text = Path.GetFileName(openDocumentation.FileName);
                Modification.Development_CurrentModification.Info.Documentation = openDocumentation.FileName;
                File.Copy(openDocumentation.FileName, Modification.Development_CurrentModification.Folder + "\\" + Field_Documentation.Text);
            }
        }

        private void Field_Documentation_Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Field_Documentation.Text) &&
                    File.Exists(Modification.Development_CurrentModification.Folder + "\\" + Field_Documentation.Text))
                {
                    File.Delete(Modification.Development_CurrentModification.Folder + "\\" + Field_Documentation.Text);
                    Field_Documentation.Text = "";
                }
            }
            catch (Exception) { }
        }

        private void Field_Documentation_Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Field_Documentation.Text) &&
                    File.Exists(Modification.Development_CurrentModification.Folder + "\\" + Field_Documentation.Text))
                {
                    Process.Start(Modification.Development_CurrentModification.Folder + "\\" + Field_Documentation.Text);
                }
            }
            catch (Exception) { }
        }

        private void Field_Documentation_Create_Click(object sender, RoutedEventArgs e)
        {
            Field_Documentation_Remove_Click(null, null);
            RichTextBox tempTextbox = new RichTextBox();
            tempTextbox.AppendText("Anno 1404 Modification Manager Development Tools Version 5");

            using (StreamWriter w = new StreamWriter(Modification.Development_CurrentModification.Folder + "\\Documentation.rtf"))
            {
                TextRange range = new TextRange(tempTextbox.Document.ContentStart, tempTextbox.Document.ContentEnd);
                range.Save(w.BaseStream, DataFormats.Rtf);
            }

            Field_Documentation.Text = "Documentation.rtf";
            Modification.Development_CurrentModification.Info.Documentation = "Documentation.rtf";
        }
        #endregion
        #region Version
        private void Version_UpdateModification(object sender, TextChangedEventArgs e)
        {
            if (ModificationLoaded)
            {
                TextBox Sender = sender as TextBox;
                if (Regex.Match(Sender.Text, "[^0-9]").Success)
                {
                    string ntext = Regex.Replace(Sender.Text, "[^0-9]", "");
                    if (string.IsNullOrEmpty(ntext))
                        ntext = "0";
                    if (int.Parse(ntext) < 0)
                        ntext = "0";

                    Sender.Text = ntext;
                    return;
                }

                try
                {
                    Modification mod = Modification.Development_CurrentModification;
                    mod.Info.Version = new Version(
                        int.Parse(Field_Version_Major.Text),
                        int.Parse(Field_Version_Minor.Text),
                        int.Parse(Field_Version_Build.Text),
                        int.Parse(Field_Version_Revision.Text));
                }
                catch (Exception)
                {
                }
            }
        }
        #endregion
        #region AnnoVersion + RDA Support
        private void AnnoVersion_UpdateModification(object sender, RoutedEventArgs e)
        {
            if (ModificationLoaded)
            {
                Modification mod = Modification.Development_CurrentModification;
                mod.Info.AnnoVersions.Clear();

                if (AnnoVersions_All.IsChecked == true)
                    mod.Info.AnnoVersions.Add("All");

                if (AnnoVersions_Retail.IsChecked == true)
                    mod.Info.AnnoVersions.Add("Retail");
                if (AnnoVersions_Patch1.IsChecked == true)
                    mod.Info.AnnoVersions.Add("Patch1");
                if (AnnoVersions_Patch2.IsChecked == true)
                    mod.Info.AnnoVersions.Add("Patch2");
                if (AnnoVersions_Patch3.IsChecked == true)
                    mod.Info.AnnoVersions.Add("Patch3");
                if (AnnoVersions_IAAM.IsChecked == true)
                    mod.Info.AnnoVersions.Add("IAAM");

                if (AnnoVersions_Addon1.IsChecked == true)
                    mod.Info.AnnoVersions.Add("Addon1");
                if (AnnoVersions_Addon1_Patch1.IsChecked == true)
                    mod.Info.AnnoVersions.Add("Addon1_Patch1");

                //mod.Info.RDAIgnoreNonExistingRDAMods = chb_IgnoreNonExistingRDAs.IsChecked.Value;
            }
        }
        #endregion

        private void Images_Clean()
        {
            try
            {
                List<string> notexistingImages = new List<string>();
                foreach (string f in Directory.GetFiles(Modification.Development_CurrentModification.Folder + "\\Images"))
                {
                    if (!Modification.Development_CurrentModification.Info.Images.Contains(Path.GetFileName(f)))
                        notexistingImages.Add(Path.GetFileName(f));
                }

                foreach (string img in notexistingImages)
                {
                    try
                    {
                        File.Delete(Modification.Development_CurrentModification.Folder + "\\Images\\" + img);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void Images_Add_Click(object sender, RoutedEventArgs e)
        {
            if (openImage.ShowDialog() == true)
            {
                if (!Directory.Exists(Modification.Development_CurrentModification.Folder + "\\Images"))
                    Directory.CreateDirectory(Modification.Development_CurrentModification.Folder + "\\Images");

                foreach (string file in openImage.FileNames)
                {
                    string extension = Path.GetExtension(file);
                    string destination = Modification.Development_CurrentModification.Folder + "\\Images\\" + Path.GetFileNameWithoutExtension(file);
                    while (File.Exists(destination + extension))
                    {
                        destination += RandomProvider.Random.Next(0, 9).ToString();
                    }
                    destination += extension;

                    File.Copy(file, destination);
                    Modification.Development_CurrentModification.Info.Images.Add(Path.GetFileName(destination));

                    ListViewItem item = new ListViewItem();
                    item.Content = new Image()
                    {
                        Width = 80,
                        Height = 80,
                        Source = BitmapImageExtension.Load(destination)
                    };

                    Images.Items.Add(item);
                }
            }
            Images_Clean();
        }

        private void Images_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Images.SelectedItem != null)
            {
                Image content = (Images.SelectedItem as ListViewItem).Content as Image;

                string file = content.Source.ToString();

                Modification.Development_CurrentModification.Info.Images.Remove(Path.GetFileName(file));
                File.Delete(file.Replace("file:///", ""));

                Images.Items.Remove(Images.SelectedItem);
            }
        }
    }
}
