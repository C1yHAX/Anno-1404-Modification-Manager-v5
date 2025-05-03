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
using Borgstrup.DocBase.Client.Controls;
using DevelopmentTools.Misc;
using AnnoModificationManager5.ModificationTypes.XmlModule;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers;
using DevelopmentTools.Editors.XmlModule.ModuleEditors;
using AnnoModificationManager5.Misc;
using DevelopmentTools.Controls;
using AnnoModificationManager5.Controls;

namespace DevelopmentTools.Editors.XmlModule
{
    /// <summary>
    /// Interaction logic for XmlModuleEditor_Main.xaml
    /// </summary>
    public partial class XmlModuleEditor_Main : UserControl
    {
        public new Project Parent
        {
            get
            {
                return Project.Development_CurrentProject;
            }
        }
        public XmlModuleList CurrentModule;

        public XmlModuleEditor_Main()
        {
            InitializeComponent();

            Project.Development_CurrentProject.UserInterface_Editors.Add(GetType(), this);
        }

        public void Refresh()
        {
            FileList_Refresh_AutoSelect();
        }

        public XmlModuleList CurrentModuleList
        {
            get
            {
                return FileList.SelectedItem != null ?
                    (FileList.SelectedItem as ContentTreeViewItem).Content as XmlModuleList : null;
            }
        }

        #region FileListTreeView
        private void FileList_Refresh_AutoSelect()
        {
            FileList.Items.Clear();
            //Load from Modification
            foreach (XmlModuleList mod in Parent.Modification.XmlModules)
            {
                ContentTreeViewItem item = new ContentTreeViewItem();
                EditableTextBlock block = new EditableTextBlock();

                block.IsFile = true;

                block.OnEdited += new EventHandler(FileList_EditableTextBlock_Edited);
                block.Text = mod.Name;

                block.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_code.png"));

                item.Content = mod;

                item.Header = block;
                FileList.Items.Add(item);
            }

            if (FileList.Items.Count != 0)
            {
                (FileList.Items[0] as TreeViewItem).IsSelected = true;
                ModifierListEditor.Visibility = System.Windows.Visibility.Visible;
            }
            else
                ModifierListEditor.Visibility = System.Windows.Visibility.Hidden;
        }

        #region Add Remove Edit
        private void FileList_Add_Click(object sender, RoutedEventArgs e)
        {
            ContentTreeViewItem item = new ContentTreeViewItem();
            EditableTextBlock block = new EditableTextBlock();

            block.IsFile = true;

            block.OnEdited += new EventHandler(FileList_EditableTextBlock_Edited);
            block.Text = "XmlModuleList " + (FileList.Items.Count + 1);

            block.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_code.png"));

            XmlModuleList list = new XmlModuleList();
            list.Name = block.Text;
            list.Parent = Parent.Modification;
            Parent.Modification.XmlModules.Add(list);

            item.Content = list;

            item.Header = block;
            FileList.Items.Add(item);

            item.IsSelected = true;

            block.IsInEditMode = true;
            block.Focus();

            FileList_UpdateOrder();
        }

        void FileList_EditableTextBlock_Edited(object sender, EventArgs e)
        {
            ContentTreeViewItem tree = (FileList.SelectedItem as ContentTreeViewItem);
            (tree.Content as XmlModuleList).Name = (tree.Header as EditableTextBlock).Text;
        }

        private void FileList_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItem != null)
            {
                ContentTreeViewItem tree = (FileList.SelectedItem as ContentTreeViewItem);
                if (MessageWindow.Show("\"" + (tree.Content as XmlModuleList).Name + "\" will be deleted permanently."
                    + "\nRemove it anyway?", MessageWindow.MessageWindowType.YesNo) == MessageBoxResult.Yes)
                {
                    Parent.Modification.XmlModules.Remove((tree.Content as XmlModuleList));
                    FileList.Items.Remove(tree);

                    FileList_Refresh_AutoSelect();
                    FileList_UpdateOrder();
                }
            }
        }

        private void FileList_Rename_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItem != null)
            {
                EditableTextBlock block = (FileList.SelectedItem as TreeViewItem).Header as EditableTextBlock;
                block.IsInEditMode = true;
            }
        } 
        #endregion
        #region Order
        private void FileList_Move_Up_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItem != null
                && FileList.Items.IndexOf(FileList.SelectedItem) != 0)
            {
                object item = FileList.SelectedItem;
                int currentindex = FileList.Items.IndexOf(FileList.SelectedItem);
                FileList.Items.Remove(item);

                FileList.Items.Insert(currentindex - 1, item);
                (item as TreeViewItem).IsSelected = true;

                FileList_UpdateOrder();
            }
        }

        private void FileList_Move_Down_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItem != null
               && FileList.Items.IndexOf(FileList.SelectedItem) != FileList.Items.Count - 1)
            {
                object item = FileList.SelectedItem;
                int currentindex = FileList.Items.IndexOf(FileList.SelectedItem);
                FileList.Items.Remove(item);

                FileList.Items.Insert(currentindex + 1, item);
                (item as TreeViewItem).IsSelected = true;

                FileList_UpdateOrder();
            }
        }

        public void FileList_UpdateOrder()
        {
            foreach (ContentTreeViewItem item in FileList.Items)
            {
                (item.Content as XmlModuleList).Index = FileList.Items.IndexOf(item);
            }

            Parent.Modification.XmlModules = Parent.Modification.XmlModules.OrderBy(m => m.Index).ToList();
        }
        #endregion

        public void FileList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ModifierListEditor.Visibility = FileList.SelectedItem != null ? Visibility.Visible : Visibility.Hidden;
            if (ModifierListEditor.Visibility != System.Windows.Visibility.Hidden)
            {
                CurrentModule = (FileList.SelectedItem as ContentTreeViewItem).Content as XmlModuleList;

                ModuleList_Refresh_AutoSelect();                
            }
        }

        private void FileList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FileList.SelectedItem != null)
            {
                EditableTextBlock block = (FileList.SelectedItem as TreeViewItem).Header as EditableTextBlock;
                if (block.IsInEditMode)
                {
                    block.IsInEditMode = false;
                    block.RaiseOnEdited();
                }
            }
        } 
        #endregion
        #region ModuleListTreeView
        #region Add Remove Edit
        private void ModuleList_Add_Click(object sender, RoutedEventArgs e)
        {
            ModifiedTreeViewItem item = new ModifiedTreeViewItem();
            item.SemanticValue = "Group";

            EditableTextBlock block = new EditableTextBlock();

            block.OnEdited += new EventHandler(ModuleList_EditableTextBlock_Edited);
            block.Text = "Group " + (ModuleList.Items.Count + 1);

            block.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/key.png"));           

            item.Header = block;
            ModuleList.Items.Add(item);

            item.IsSelected = true;

            block.IsInEditMode = true;
            block.Focus();

            ModuleList_UpdateOrder();
        }

        private void ModuleList_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;
                if (selected.SemanticValue == "Group")
                {
                    if (ModuleList.SelectedItem != null && MessageWindow.Show("The group will be deleted permanently."
                    + "\nRemove it anyway?", MessageWindow.MessageWindowType.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (ContentTreeViewItem item in (ModuleList.SelectedItem as TreeViewItem).Items)
                        {
                            CurrentModule.Remove((IXMLModifier)item.Content);
                        }

                        ModuleCreator.Selector_Selector_Refresh_Click(null, null);
                        ModuleList.Items.Remove(ModuleList.SelectedItem);
                        ModuleList_Refresh_AutoSelect();
                        ModuleList_UpdateOrder();
                    }
                }
                else
                {
                    ContentTreeViewItem item = (ModuleList.SelectedItem as ContentTreeViewItem);

                    CurrentModule.Remove(item.Content as IXMLModifier);

                    TreeViewItem parent = item.Parent as TreeViewItem;
                    parent.Items.Remove(item);

                    ModuleList_UpdateOrder();
                    ModuleCreator.Selector_Selector_Refresh_Click(null, null);
                }
            }
        }

        private void ModuleList_Rename_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null && ModuleList.Items.Contains(ModuleList.SelectedItem))
            {
                ((ModuleList.SelectedItem as TreeViewItem).Header as EditableTextBlock).IsInEditMode = true;
            }
        }

        void ModuleList_EditableTextBlock_Edited(object sender, EventArgs e)
        {
            foreach (ContentTreeViewItem item in (ModuleList.SelectedItem as TreeViewItem).Items)
            {
                (item.Content as IXMLModifier).Group = (sender as EditableTextBlock).Text;
            }
        }
        #endregion
        #region Order
        private void ModuleList_Move_Up_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;

                //Modifier
                if (selected.SemanticValue == "Modifier")
                {
                    TreeViewItem parent = (ModuleList.SelectedItem as ContentTreeViewItem).Parent as TreeViewItem;

                    if (parent.Items.IndexOf(ModuleList.SelectedItem) != 0)
                    {
                        object item = ModuleList.SelectedItem;
                        int currentindex = parent.Items.IndexOf(ModuleList.SelectedItem);
                        parent.Items.Remove(item);

                        parent.Items.Insert(currentindex - 1, item);
                        (item as TreeViewItem).IsSelected = true;
                    }
                }
                //Group
                else
                {
                    if (ModuleList.Items.IndexOf(ModuleList.SelectedItem) != 0)
                    {
                        object item = ModuleList.SelectedItem;
                        int currentindex = ModuleList.Items.IndexOf(ModuleList.SelectedItem);
                        ModuleList.Items.Remove(item);

                        ModuleList.Items.Insert(currentindex - 1, item);
                        (item as TreeViewItem).IsSelected = true;
                    }
                }

                ModuleList_UpdateOrder();
            }
        }

        private void ModuleList_Move_Down_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;

                //Modifier
                if (selected.SemanticValue == "Modifier")
                {
                    TreeViewItem parent = (ModuleList.SelectedItem as ContentTreeViewItem).Parent as TreeViewItem;

                    if (parent.Items.IndexOf(ModuleList.SelectedItem) != parent.Items.Count - 1)
                    {
                        object item = ModuleList.SelectedItem;
                        int currentindex = parent.Items.IndexOf(ModuleList.SelectedItem);
                        parent.Items.Remove(item);

                        parent.Items.Insert(currentindex + 1, item);
                        (item as TreeViewItem).IsSelected = true;
                    }
                }
                //Group
                else
                {
                    if (ModuleList.Items.IndexOf(ModuleList.SelectedItem) != ModuleList.Items.Count - 1)
                    {
                        object item = ModuleList.SelectedItem;
                        int currentindex = ModuleList.Items.IndexOf(ModuleList.SelectedItem);
                        ModuleList.Items.Remove(item);

                        ModuleList.Items.Insert(currentindex + 1, item);
                        (item as TreeViewItem).IsSelected = true;
                    }
                }

                ModuleList_UpdateOrder();
            }
        }

        private void ModuleList_Move_ToLastGroup_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;

                //Modifier
                if (selected.SemanticValue == "Modifier")
                {
                    int currentparentindex = ModuleList.Items.IndexOf(selected.Parent);
                    if (currentparentindex != 0)
                    {
                        ModifiedTreeViewItem oldgroup = (ModifiedTreeViewItem)selected.Parent;
                        ModifiedTreeViewItem newgroup = (ModifiedTreeViewItem)ModuleList.Items[currentparentindex - 1];

                        oldgroup.Items.Remove(selected);
                        newgroup.Items.Add(selected);

                        ((selected as ContentTreeViewItem).Content as IXMLModifier).Group =
                            (newgroup.Header as EditableTextBlock).Text;

                        ModuleList_UpdateOrder();
                        newgroup.IsExpanded = true;
                    }
                }
            }
        }

        private void ModuleList_Move_ToNextGroup_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;

                //Modifier
                if (selected.SemanticValue == "Modifier")
                {
                    int currentparentindex = ModuleList.Items.IndexOf(selected.Parent);
                    if (currentparentindex != ModuleList.Items.Count - 1)
                    {
                        ModifiedTreeViewItem oldgroup = (ModifiedTreeViewItem)selected.Parent;
                        ModifiedTreeViewItem newgroup = (ModifiedTreeViewItem)ModuleList.Items[currentparentindex + 1];

                        oldgroup.Items.Remove(selected);
                        newgroup.Items.Insert(0, selected);

                        ((selected as ContentTreeViewItem).Content as IXMLModifier).Group =
                            (newgroup.Header as EditableTextBlock).Text;

                        ModuleList_UpdateOrder();
                        newgroup.IsExpanded = true;
                    }
                }
            }
        }

        public void ModuleList_UpdateOrder()
        {
            List<IXMLModifier> AllModifiers = new List<IXMLModifier>();
            foreach (TreeViewItem pitem in ModuleList.Items)
            {
                foreach (ContentTreeViewItem citem in pitem.Items)
                {
                    AllModifiers.Add(citem.Content as IXMLModifier);
                }
            }

            for (int i = 0; i < AllModifiers.Count; i++)
            {
                AllModifiers[i].Index = i;
            }

            //Order in XmlModuleList
            CurrentModuleList.OrderByIndex();
        }
        #endregion     
        public void ModuleList_RefreshSelectedItem()
        {
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;
                if (selected.SemanticValue == "Modifier")
                {
                    (ModuleList.SelectedItem as ContentTreeViewItem).Header
                        = ((ModuleList.SelectedItem as ContentTreeViewItem).Content as IXMLModifier).ModuleList_ToHeader;
                }
            }
        }

        public void ModuleList_SelectModification(IXMLModifier mod)
        {
            foreach (TreeViewItem group in ModuleList.Items)
            {
                ContentTreeViewItem item = group.Items.OfType<ContentTreeViewItem>().ToList()
                    .Find(ci => ci.Content == mod);
                if (item != null)
                {
                    item.IsSelected = true;
                    return;
                }
            }
        }

        private void ModuleList_Refresh_AutoSelect()
        {
            ModuleList.Items.Clear();

            if (CurrentModule != null)
            {
                if (CurrentModule.Get().Count != 0)
                {
                    foreach (IXMLModifier mod in CurrentModule.Get())
                    {
                        ModifiedTreeViewItem toAdd = null;

                        //If Group is not existing, create Group
                        if (ModuleList.Items.OfType<ModifiedTreeViewItem>().ToList().Find(ti => (ti.Header as EditableTextBlock).Text == mod.Group) == null)
                        {
                            ModifiedTreeViewItem tv = new ModifiedTreeViewItem();
                            tv.SemanticValue = "Group";

                            EditableTextBlock block = new EditableTextBlock();
                            block.Text = mod.Group;
                            block.OnEdited += new EventHandler(ModuleList_EditableTextBlock_Edited);

                            block.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/key.png"));

                            tv.Header = block;
                            ModuleList.Items.Add(tv);

                            toAdd = tv;
                        }

                        if (toAdd == null)
                            toAdd = ModuleList.Items.OfType<ModifiedTreeViewItem>().ToList().Find(ti => (ti.Header as EditableTextBlock).Text == mod.Group);

                        ContentTreeViewItem item = new ContentTreeViewItem();
                        item.SemanticValue = "Modifier";
                        item.Content = mod;
                        item.Header = mod.ModuleList_ToHeader;

                        toAdd.Items.Add(item);
                    }
                }
                else
                {
                    ModifiedTreeViewItem tv = new ModifiedTreeViewItem();
                    tv.SemanticValue = "Group";

                    EditableTextBlock block = new EditableTextBlock();
                    block.Text = "General";

                    block.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/key.png"));

                    tv.Header = block;
                    ModuleList.Items.Add(tv);

                    tv.IsExpanded = true;
                }
                (ModuleList.Items[0] as TreeViewItem).IsSelected = true;
            }
        }

        public void ModuleList_XmlModuleCreator_Add(IXMLModifier mod)
        {
            ContentTreeViewItem item = new ContentTreeViewItem();
            item.SemanticValue = "Modifier";
            item.Content = mod;
            item.Header = mod.ModuleList_ToHeader;

            (ModuleList.SelectedItem as TreeViewItem).Items.Add(item);
            ModuleList_UpdateOrder();
        }

        private void ModuleList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ModuleEditPanel.Content = null;
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;

                if (selected.SemanticValue == "Modifier") //Add @ xyz
                {
                    ModuleCreator.Visibility = System.Windows.Visibility.Collapsed;

                    IXMLModifier mod = (ModuleList.SelectedItem as ContentTreeViewItem).Content as IXMLModifier;

                    if (mod is EditModifier)
                    {
                        XmlEditEditor editor = new XmlEditEditor(mod as EditModifier);
                        editor.Parent = this;
                        editor.IsEdit = true;

                        ModuleEditPanel.Content = editor;
                    }
                    if (mod is AddModifier)
                    {
                        XmlAddEditor editor = new XmlAddEditor(mod as AddModifier);
                        editor.Parent = this;
                        editor.IsEdit = true;

                        ModuleEditPanel.Content = editor;
                    }
                    if (mod is RemoveModifier)
                    {
                        XmlRemoveEditor editor = new XmlRemoveEditor(mod as RemoveModifier);
                        editor.Parent = this;
                        editor.IsEdit = true;

                        ModuleEditPanel.Content = editor;
                    }
                }
                else
                {
                    ModuleCreator.Visibility = System.Windows.Visibility.Visible;
                    //ModuleCreator.Focus();

                    if (ModuleList.SelectedItem != null)
                        (ModuleList.SelectedItem as TreeViewItem).IsExpanded = true;
                }
            }
        }

        private void ModuleList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;
                if (selected.SemanticValue == "Group")
                {
                    EditableTextBlock block = (ModuleList.SelectedItem as TreeViewItem).Header as EditableTextBlock;
                    if (block.IsInEditMode)
                    {
                        block.IsInEditMode = false;
                        block.RaiseOnEdited();
                    }
                }
            }
        }
        #endregion        
    }
}
