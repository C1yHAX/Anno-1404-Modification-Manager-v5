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
using DevelopmentTools.Tools.Global;
using AnnoModificationManager5.Misc;
using System.IO;
using Borgstrup.DocBase.Client.Controls;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers;
using AnnoModificationManager5.ModificationTypes;
using DevelopmentTools.Editors.ListModule.Controls;
using AnnoModificationManager5.ModificationTypes.Userdefined;

namespace DevelopmentTools.Editors.ListModule
{
    /// <summary>
    /// Interaction logic for XmlModuleEditor_ModuleCreator.xaml
    /// </summary>
    public partial class ListModuleEditor_ModuleCreator : UserControl
    {
        public new ListModuleEditor_Main Parent
        {
            get
            {
                return Project.Development_CurrentProject.UserInterface_Editors[typeof(ListModuleEditor_Main)] as ListModuleEditor_Main;
            }
        }

        public ListModuleEditor_ModuleCreator()
        {
            InitializeComponent();

            try
            {
                Project.Development_CurrentProject.UserInterface_Editors.Add(GetType(), this);
                listFileTreeView.Parent = this;
            }
            catch (Exception)
            {
            }
        }

        #region Files

        private void Files_File_OpenSelector_Click(object sender, RoutedEventArgs e)
        {
            //FileBrowser browser = new FileBrowser();
            //if (browser.ShowDialog() == true)
            //{
            //    Files_SourceFile.Text = browser.ChoosenFile;

            //    //Source
            //    if (Files_SourceFile.Text.Contains("%Project%\\OriginalFiles\\"))
            //    {
            //        string fi = Path.GetFileName(Files_SourceFile.Text);
            //        fi = fi.Replace("_", "\\").Replace("#", "%");

            //        File_DestinationFile.Text = fi;
            //    }

            //    AssignSystem_FromSource_Click(null, null);
            //}

            FileBrowser browser = new FileBrowser();
            if (browser.ShowDialog() == true)
            {
                Files_File.Text = browser.ChoosenFile;
            }
        }

        private void Files_File_DropDownOpened(object sender, EventArgs e)
        {
            //Load all destination Files
            Files_File.ItemsSource = Project.Development_CurrentProject.Modification.CollectedFiles_List_List;
            Files_File.Items.Refresh();
        }

        //private void Files_DestinationFile_AutoAssign_Click(object sender, RoutedEventArgs e)
        //{
        //    //If file is already in Project
        //    if (File.Exists(Project.Development_CurrentProject.Modification.Folder + "\\OriginalFiles\\" + File_DestinationFile.Text.FormatProjectPath()))
        //    {
        //        Files_SourceFile.Text = "%Project%\\OriginalFiles\\" + File_DestinationFile.Text.FormatProjectPath();
        //    }
        //}
        #endregion
        #region Modifiers
        public void XmlNode_ModifierAdded_Select(IListModifier mod)
        {
            if (mod is AddGroupModifier)
            {
                foreach (TreeViewItem item in listFileTreeView.treeView.Items)
                {
                    if ((item.Header as EditableTextBlock).Text == mod.ElementGroup)
                    {
                        item.IsExpanded = true;
                        item.IsSelected = true;
                        //item.Focus();
                        return;
                    }
                }

            }
            else
            {
                foreach (TreeViewItem item in listFileTreeView.treeView.Items)
                {
                    foreach (TreeViewItem it in item.Items)
                    {
                        ListNodeMapper map = (ListNodeMapper)it.Header;
                        if (map.Modifier == mod)
                        {
                            listFileTreeView.treeView.ExpandTo(it);
                            it.IsSelected = true;
                            //it.Focus();
                            return;
                        }
                    }
                }
            }
        }

        public void XmlNode_Modifier_AddToStructures(IListModifier mod)
        {
            //Modification.Development_CurrentModification.CopySourceFileToFolder(Files_SourceFile.Text, mod.File);

            Parent.CurrentModule.Add(mod);
            Parent.ModuleList_XmlModuleCreator_Add(mod);

            Files_File.Text = mod.File;

            if (mod is AddModifier)
            {
                TreeViewItem ni = new TreeViewItem();
                ni.HeaderTemplate = (DataTemplate)listFileTreeView.Resources["TreeViewItemHeader_Xml"];

                ListNodeMapper nd = new ListNodeMapper();
                nd.NodeName = mod.ElementValue;
                nd.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/add.png"));
                nd.Modifier = mod;

                ni.Header = nd;
                (listFileTreeView.treeView.SelectedItem as TreeViewItem).Items.Add(ni);
            }
            else if (mod is AddGroupModifier)
            {
                TreeViewItem trv = new TreeViewItem();
                EditableTextBlock header = new EditableTextBlock();
                header.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/key.png"));
                header.Text = (mod as AddGroupModifier).ElementGroup;
                trv.Header = header;

                listFileTreeView.treeView.Items.Add(trv);
            }
            else
            {
                RefreshList_Click(null, null);
            }
            XmlNode_ModifierAdded_Select(mod);
        }

        public void XmlNode_Select_Click(object sender, RoutedEventArgs e)
        {
            IListModifier mod = ((listFileTreeView.treeView.SelectedItem as TreeViewItem).Header as ListNodeMapper).Modifier;
            if (mod is AddModifier)
            {
                ListModuleCreator creator = new ListModuleCreator();
                creator.uValueParent = mod;
                creator.Field_NewValue.Text = (mod as AddModifier).ElementValue;
                foreach (ListUserdefinedValue val in mod.UserdefinedValues)
                {
                    creator.UserdefinedValues_List.Items.Add(val);
                }

                if (creator.ShowDialog() == true)
                {
                    mod.UserdefinedValues = creator.UserdefinedValues_List.Items.OfType<ListUserdefinedValue>().ToList();
                    (mod as AddModifier).ElementValue = creator.Field_NewValue.Text.Trim();
                    Parent.CurrentModuleList.Edit(mod);

                    RefreshList_Click(null, null);
                    XmlNode_ModifierAdded_Select(mod);
                }
            }
            else if (mod is AddGroupModifier)
            {
                string text = MessageWindow.GetText("Edit the Group Name:", (mod as AddGroupModifier).ElementValue);
                if (text != null)
                {
                    (mod as AddGroupModifier).ElementValue = text;
                    Parent.CurrentModuleList.Edit(mod);

                    //Edit Modifiers
                    foreach (TreeViewItem item in (listFileTreeView.treeView.SelectedItem as TreeViewItem).Items)
                    {
                        ListNodeMapper map = item.Header as ListNodeMapper;
                        if (map.Modifier != null)
                        {
                            map.Modifier.ElementGroup = text;
                        }
                    }

                    RefreshList_Click(null, null);
                    XmlNode_ModifierAdded_Select(mod);
                }
            }
            else if (mod is EditModifier)
            {
                ListModuleCreator editcreator = new ListModuleCreator();
                editcreator.uValueParent = mod;
                editcreator.Field_NewValue.Text = (mod as EditModifier).NewValue;
                foreach (ListUserdefinedValue val in mod.UserdefinedValues)
                {
                    editcreator.UserdefinedValues_List.Items.Add(val);
                }

                if (editcreator.ShowDialog() == true)
                {
                    mod.UserdefinedValues = editcreator.UserdefinedValues_List.Items.OfType<ListUserdefinedValue>().ToList();
                    (mod as EditModifier).NewValue = editcreator.Field_NewValue.Text.Trim();
                    Parent.CurrentModuleList.Edit(mod);

                    RefreshList_Click(null, null);
                    XmlNode_ModifierAdded_Select(mod);
                }
            }
        }


        public void XmlNode_Add_Click(object sender, RoutedEventArgs e)
        {
            //No preventive mechanisms, because SelectedEvent takes care of this
            ListModuleCreator creator = new ListModuleCreator();
            creator.Field_NewValue.Text = "New Value";

            #region Prepare
            TreeViewItem group = listFileTreeView.treeView.SelectedItem as TreeViewItem;
            string groupname = (group.Header as EditableTextBlock).Text;

            AddModifier mod = new AddModifier();
            mod.File = Files_File.Text;
            mod.Group = Parent.CurrentGroup;
            mod.ElementGroup = groupname;
            #endregion

            creator.Load(mod);

            if (creator.ShowDialog() == true)
            {
                creator.PushData();

                XmlNode_Modifier_AddToStructures(mod);
            }
        }

        public void XmlNode_Remove_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem group = (listFileTreeView.treeView.SelectedItem as TreeViewItem).Parent as TreeViewItem;
            string groupname = (group.Header as EditableTextBlock).Text;

            RemoveModifier mod = new RemoveModifier();
            mod.File = Files_File.Text;
            mod.Group = Parent.CurrentGroup;
            mod.ElementGroup = groupname;
            mod.ElementValue = ((listFileTreeView.treeView.SelectedItem as TreeViewItem).Header as ListNodeMapper).NodeName;

            XmlNode_Modifier_AddToStructures(mod);
        }

        public void XmlNode_Edit_Click(object sender, RoutedEventArgs e)
        {
            //No preventive mechanisms, because SelectedEvent takes care of this

            ListModuleCreator creator = new ListModuleCreator();
            creator.Field_NewValue.Text = ((listFileTreeView.treeView.SelectedItem as TreeViewItem).Header as ListNodeMapper).NodeName;

            #region Prepare
            TreeViewItem group = (listFileTreeView.treeView.SelectedItem as TreeViewItem).Parent as TreeViewItem;
            string groupname = (group.Header as EditableTextBlock).Text;

            EditModifier mod = new EditModifier();
            mod.File = Files_File.Text;
            mod.Group = Parent.CurrentGroup;
            mod.ElementGroup = groupname;
            mod.ElementValue = ((listFileTreeView.treeView.SelectedItem as TreeViewItem).Header as ListNodeMapper).NodeName;
            #endregion

            creator.Load(mod);
            if (creator.ShowDialog() == true)
            {
                creator.PushData();
                XmlNode_Modifier_AddToStructures(mod);
            }
        }

        public void XmlNode_AddGroup_Click(object sender, RoutedEventArgs e)
        {
            string gname = MessageWindow.GetText("Group Name", "New Group");
            if (gname != null)
            {
                AddGroupModifier mod = new AddGroupModifier();
                mod.File = Files_File.Text;
                mod.Group = Parent.CurrentGroup;
                mod.ElementGroup = gname;

                XmlNode_Modifier_AddToStructures(mod);
            }
        }
        #endregion

        public void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            listFileTreeView.SetListFile(Files_File.Text);
            listFileTreeView.Select(Selector_Group.Text, Selector_Element.Text);
        }

        #region AssignSystem
        //private void AssignSystem_FromSource_Click(object sender, RoutedEventArgs e)
        //{
        //    if (AutoAssign.SourceFileAutoAssign.Assigns.ContainsKey(Files_SourceFile.Text))
        //    {
        //        File_DestinationFile.Text = AutoAssign.SourceFileAutoAssign.Assigns[Files_SourceFile.Text];
        //    }
        //}

        ////private void File_DestinationFile_DropDownClosed(object sender, EventArgs e)
        ////{
        ////    AssignSystem_SourceFromDestination_Click(null, null);
        ////}

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

        #region Version 4002 Events
        public void treeView_ContextMenu_Refresh()
        {
            listFileTreeView.
            treeView_ContextMenu_AddGroup.Visibility = XmlNode_AddGroup.Visibility;
            listFileTreeView.
            treeView_ContextMenu_Add.Visibility = XmlNode_Add.Visibility;
            listFileTreeView.
            treeView_ContextMenu_Edit.Visibility = XmlNode_Edit.Visibility;
            listFileTreeView.
            treeView_ContextMenu_Remove.Visibility = XmlNode_Remove.Visibility;
        }

        public void XmlNode_AddGroup_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            treeView_ContextMenu_Refresh();
        }

        public void XmlNode_Add_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            treeView_ContextMenu_Refresh();
        }

        public void XmlNode_Remove_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            treeView_ContextMenu_Refresh();
        }

        public void XmlNode_Edit_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            treeView_ContextMenu_Refresh();
        }
        #endregion
    }
}
