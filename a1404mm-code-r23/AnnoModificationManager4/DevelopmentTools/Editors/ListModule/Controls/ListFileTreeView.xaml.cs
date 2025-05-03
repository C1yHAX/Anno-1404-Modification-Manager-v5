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
using AnnoModificationManager4.ModificationTypes.ListModule;
using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.Misc;
using DevelopmentTools.Misc;
using Borgstrup.DocBase.Client.Controls;
using AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers;
using AnnoModificationManager4.UserInterface.Misc;

namespace DevelopmentTools.Editors.ListModule.Controls
{
    /// <summary>
    /// Interaction logic for ListFileTreeView.xaml
    /// </summary>
    public partial class ListFileTreeView : UserControl
    {
        public new ListModuleEditor_ModuleCreator Parent;
        public Modification Modification;

        public ListFile ListFile;
        public Exception ListFileException;

        public string Selector;

        public ListFileTreeView()
        {
            InitializeComponent();
        }

        public void SetListFile(string fileName)
        {
            fileName = fileName.FormatDevelopmentFolders();

            try
            {
                ListFile = ListFileCollector.Request(fileName);

                ListFileException = null;
            }
            catch (Exception ex)
            {
                ListFile = null;
                ListFileException = ex;
            }
        }

        public void Select(string group, string entry)
        {
            try
            {
                ListNodeMapper.Generate(this);
                Parent.XmlNode_AddGroup.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                if (Parent != null)
                {
                    Parent.XmlNode_AddGroup.Visibility = System.Windows.Visibility.Hidden;
                    Parent.Message.Text = ex.Message.Replace("\r", "").Replace("\n", "");
                }
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Parent.Message.Text = "";
            Parent.XmlNode_Add.Visibility = Visibility.Collapsed;
            Parent.XmlNode_Remove.Visibility = Visibility.Collapsed;
            Parent.XmlNode_Edit.Visibility = Visibility.Collapsed;
            Parent.XmlNode_Select.Visibility = Visibility.Collapsed;

            if (treeView.SelectedItem != null)
            {
                TreeViewItem item = treeView.SelectedItem as TreeViewItem;

                if (item.Header is EditableTextBlock)
                {
                    Parent.XmlNode_Add.Visibility = Visibility.Visible;
                }
                else
                {
                    ListNodeMapper map = item.Header as ListNodeMapper;

                    if (map.Modifier == null)
                    {
                        Parent.XmlNode_Remove.Visibility = Visibility.Visible;
                        Parent.XmlNode_Edit.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (map.Modifier is RemoveModifier)
                            Parent.XmlNode_Select.Visibility = Visibility.Collapsed;
                        else
                            Parent.XmlNode_Select.Visibility = System.Windows.Visibility.Visible;

                        //Select Modifier in ModuleList
                        Parent.Parent.ModuleList_SelectModifier(map.Modifier);
                    }
                }
            }
        }

        private void treeView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (treeView.SelectedItem != null)
            {
                if ((treeView.SelectedItem as TreeViewItem).Header is EditableTextBlock)
                {
                    if (((treeView.SelectedItem as TreeViewItem).Header as EditableTextBlock).IsInEditMode)
                        ((treeView.SelectedItem as TreeViewItem).Header as EditableTextBlock).IsInEditMode = false;
                }
            }
        }

        #region 4020 Events
        private void XmlNode_AddGroup_Click(object sender, RoutedEventArgs e)
        {
            Parent.XmlNode_AddGroup_Click(sender, e);
        }

        private void XmlNode_Add_Click(object sender, RoutedEventArgs e)
        {
            Parent.XmlNode_Add_Click(sender, e);
        }

        private void XmlNode_Edit_Click(object sender, RoutedEventArgs e)
        {
            Parent.XmlNode_Edit_Click(sender, e);
        }

        private void XmlNode_Remove_Click(object sender, RoutedEventArgs e)
        {
            Parent.XmlNode_Remove_Click(sender, e);
        }

        private void treeView_ContextMenu_ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                treeView.ExpandAll();
            }
            catch (Exception) { }
        }
        #endregion
    }
}
