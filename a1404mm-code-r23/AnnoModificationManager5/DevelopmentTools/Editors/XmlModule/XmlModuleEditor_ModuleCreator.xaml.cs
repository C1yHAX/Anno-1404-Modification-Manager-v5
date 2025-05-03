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
using DevelopmentTools.Tools.Global;
using DevelopmentTools.Editors.XmlModule.FilterSystem;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.ModificationTypes;

namespace DevelopmentTools.Editors.XmlModule
{
    /// <summary>
    /// Interaction logic for XmlModuleEditor_ModuleCreator.xaml
    /// </summary>
    public partial class XmlModuleEditor_ModuleCreator : UserControl
    {
        public new XmlModuleEditor_Main Parent
        {
            get
            {
                return Project.Development_CurrentProject.UserInterface_Editors[typeof(XmlModuleEditor_Main)] as XmlModuleEditor_Main;
            }
        }

        public XmlModuleEditor_ModuleCreator()
        {
            InitializeComponent();
            try
            {
                Project.Development_CurrentProject.UserInterface_Editors.Add(GetType(), this);
                XmlEditor.Parent = this;
                Loaded += new RoutedEventHandler(XmlModuleEditor_ModuleCreator_Loaded);
            }
            catch (Exception)
            {
            }
        }

        void XmlModuleEditor_ModuleCreator_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadFilters();
            }
            catch (Exception)
            {
            }
        }

        #region Selector
        public void Selector_Selector_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Files_File.Text))
            {
                Xml_LastCount.Text = "Invalid Destination File!";
                return;
            }

            XmlEditor.SetXmlFile(Files_File.Text);
            XmlEditor.Select(Selectors_Selector.Text);
        }

        private void Selectors_Selector_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Selector_Selector_Refresh_Click(sender, new RoutedEventArgs());
            }
        }
        #endregion
        #region Files
        private void Files_File_OpenSelector_Click(object sender, RoutedEventArgs e)
        {
            FileBrowser browser = new FileBrowser();
            if (browser.ShowDialog() == true)
            {
                Files_File.Text = browser.ChoosenFile;
            }

            //throw new NotImplementedException();
        }

        //private void Files_DestinationFile_AutoAssign_Click(object sender, RoutedEventArgs e)
        //{
        //    Template_AutoAssign();
        //}

        private void Files_File_DropDownOpened(object sender, EventArgs e)
        {
            //Load all destination Files
            Files_File.ItemsSource = Project.Development_CurrentProject.Modification.CollectedFiles_Xml_List;
            Files_File.Items.Refresh();
        }
        #endregion
        #region Template
        public void LoadFilters()
        {
            Filter.LoadFilters();
            Template_ControlBox_Template.Items.Clear();

            foreach (Filter filter in Filter.Filters)
            {
                Template_ControlBox_Template.Items.Add(filter);
            }

            Template_ControlBox_Template.SelectedIndex = 0;
        }

        private void Template_ControlBox_Template_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Template_ControlBox_Template.SelectedItem == null)
                return;

            Template_ValueEditor.ItemsSource = (Template_ControlBox_Template.SelectedItem as Filter).FilterValues;
            Template_ValueEditor.Items.Refresh();

            if ((Template_ControlBox_Template.SelectedItem as Filter).FilterValues.Count == 0)
                Template_ValueEditor.Visibility = System.Windows.Visibility.Collapsed;
            else
                Template_ValueEditor.Visibility = System.Windows.Visibility.Visible;

            //Template_AutoAssign();
            Template_TextBox_TextChanged(sender, null);
        }

        private void Template_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filter current = (Template_ControlBox_Template.SelectedItem as Filter);

            #region Files

            if (!string.IsNullOrEmpty(current.DestinationFile))
                Files_File.Text = current.DestinationFile;

            //Template_AutoAssign();
            #endregion

            if (!string.IsNullOrEmpty(current.Selector))
                Selectors_Selector.Text = current.ReplaceString(current.Selector);
            if (!string.IsNullOrEmpty(current.Deselector))
                Selectors_Deselector.Text = current.ReplaceString(current.Deselector);
        }

        private void Template_OpenTemplateEditor_Click(object sender, RoutedEventArgs e)
        {
            FilterEditor filter = new FilterEditor();
            filter.Refresh();

            filter.ShowDialog();

            LoadFilters();
        }

        //public void Template_AutoAssign()
        //{
        //    AssignSystem_SourceFromDestination_Click(null, null);

        //    //If file is already in Project
        //    if (File.Exists(Project.Development_CurrentProject.Modification.Folder + "\\OriginalFiles\\" + File_DestinationFile.Text.FormatProjectPath()))
        //    {
        //        Files_SourceFile.Text = "%Project%\\OriginalFiles\\" + File_DestinationFile.Text.FormatProjectPath();
        //    }
        //}
        #endregion
        #region XML
        private void Xml_lastMessage_Inspect_Click(object sender, RoutedEventArgs e)
        {
            DevelopmentTools.Editors.XmlModule.Controls.XmlFileTreeViewInspector insp =
                new Controls.XmlFileTreeViewInspector(Parent.Parent.Modification);
            insp.File = XmlEditor.XmlFile;
            insp.Selector = XmlEditor.Selector + "/" + ((XmlEditor.treeView.SelectedItem as TreeViewItem).Header as
                DevelopmentTools.Editors.XmlModule.Controls.XmlNodeMapper).RelativePath;
            insp.ShowDialog();
        }
        public void XmlNode_Add_Click(object sender, RoutedEventArgs e)
        {
            XmlEditor.Add();
        }

        private void XmlNode_AddBeforeSelected_Click(object sender, RoutedEventArgs e)
        {
            XmlEditor.AddBeforeSelected();
        }

        public void XmlNode_Remove_Click(object sender, RoutedEventArgs e)
        {
            XmlEditor.Remove();
        }

        public void XmlNode_Edit_Click(object sender, RoutedEventArgs e)
        {
            XmlEditor.Edit();
        }

        private void XmlNode_Select_Click(object sender, RoutedEventArgs e)
        {
            XmlEditor.Select();
        }
        #endregion
        #region AssignSystem
        private void File_DestinationFile_DropDownClosed(object sender, EventArgs e)
        {
            //AssignSystem_SourceFromDestination_Click(null, null);
        }

        //private void AssignSystem_FromSource_Click(object sender, RoutedEventArgs e)
        //{
        //    if (AutoAssign.SourceFileAutoAssign.Assigns.ContainsKey(Files_SourceFile.Text))
        //    {
        //        File_DestinationFile.Text = AutoAssign.SourceFileAutoAssign.Assigns[Files_SourceFile.Text];
        //    }
        //}

        //private void AssignSystem_SourceFromDestination_Click(object sender, RoutedEventArgs e)
        //{
        //    if (File.Exists(Modification.Development_CurrentModification.Folder + "\\OriginalFiles\\" + File_DestinationFile.Text.FormatProjectPath()))
        //    {
        //        Files_SourceFile.Text = "%Project%\\OriginalFiles\\" + File_DestinationFile.Text.FormatProjectPath();
        //        return;
        //    }
        //    foreach (KeyValuePair<string, string> k in AutoAssign.SourceFileAutoAssign.Assigns)
        //    {
        //        if (k.Value == File_DestinationFile.Text)
        //        {
        //            Files_SourceFile.Text = k.Key;
        //            break;
        //        }
        //    }
        //}

        //private void AssignSystem_Create_Click(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(Files_SourceFile.Text) | string.IsNullOrEmpty(File_DestinationFile.Text))
        //        return;

        //    if (AutoAssign.SourceFileAutoAssign.Assigns.ContainsKey(Files_SourceFile.Text))
        //    {
        //        AutoAssign.SourceFileAutoAssign.Assigns.Remove(Files_SourceFile.Text);
        //    }

        //    AutoAssign.SourceFileAutoAssign.Assign(Files_SourceFile.Text, File_DestinationFile.Text);
        //}
        #endregion
    }
}
