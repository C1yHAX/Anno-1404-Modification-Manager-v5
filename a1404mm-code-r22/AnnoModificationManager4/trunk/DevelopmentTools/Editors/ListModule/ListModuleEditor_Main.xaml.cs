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
using AnnoModificationManager4.ModificationTypes.ListModule;
using AnnoModificationManager4.UserInterface.Misc;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers;
using DevelopmentTools.Controls;
using AnnoModificationManager4.Controls;

namespace DevelopmentTools.Editors.ListModule
{
    /// <summary>
    /// Interaction logic for XmlModuleEditor_Main.xaml
    /// </summary>
    public partial class ListModuleEditor_Main : UserControl
    {
        public new Project Parent
        {
            get
            {
                return Project.Development_CurrentProject;
            }
        }
        public ListModuleList CurrentModule;
        public string CurrentGroup;
        public TreeViewItem CurrentGroupItem;

        public ListModuleEditor_Main()
        {
            InitializeComponent();

            Project.Development_CurrentProject.UserInterface_Editors.Add(GetType(), this);
        }

        public void Refresh()
        {
            FileList_Refresh_AutoSelect();
        }

        public ListModuleList CurrentModuleList
        {
            get
            {
                return FileList.SelectedItem != null ?
                    (FileList.SelectedItem as ContentTreeViewItem).Content as ListModuleList : null;
            }
        }

        #region FileListTreeView
        private void FileList_Refresh_AutoSelect()
        {
            FileList.Items.Clear();
            //Load from Modification
            foreach (ListModuleList mod in Parent.Modification.ListModules)
            {
                ContentTreeViewItem item = new ContentTreeViewItem();
                EditableTextBlock block = new EditableTextBlock();

                block.IsFile = true;

                block.OnEdited += new EventHandler(FileList_EditableTextBlock_Edited);
                block.Text = mod.Name;

                block.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_text.png"));

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
            block.Text = "ListModuleList " + (FileList.Items.Count + 1);

            block.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_text.png"));

            ListModuleList list = new ListModuleList();
            list.Name = block.Text;
            list.Parent = Parent.Modification;
            Parent.Modification.ListModules.Add(list);

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
            (tree.Content as ListModuleList).Name = (tree.Header as EditableTextBlock).Text;
        }

        private void FileList_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.SelectedItem != null)
            {
                ContentTreeViewItem tree = (FileList.SelectedItem as ContentTreeViewItem);
                if (MessageWindow.Show("\"" + (tree.Content as ListModuleList).Name + "\" will be deleted permanently."
                    + "\nRemove it anyway?", MessageWindow.MessageWindowType.YesNo) == MessageBoxResult.Yes)
                {
                    Parent.Modification.ListModules.Remove((tree.Content as ListModuleList));
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
                (item.Content as ListModuleList).Index = FileList.Items.IndexOf(item);
            }

            Parent.Modification.XmlModules = Parent.Modification.XmlModules.OrderBy(m => m.Index).ToList();
        }
        #endregion

        public void FileList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ModifierListEditor.Visibility = FileList.SelectedItem != null ? Visibility.Visible : Visibility.Hidden;
            if (ModifierListEditor.Visibility != System.Windows.Visibility.Hidden)
            {
                CurrentModule = (FileList.SelectedItem as ContentTreeViewItem).Content as ListModuleList;

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
        #region helpers
        public void ModuleList_SelectModifier(IListModifier mod)
        {
            foreach (TreeViewItem item in ModuleList.Items)
            {
                foreach (ContentTreeViewItem itm in item.Items)
                {
                    if (itm.Content == mod)
                    {
                        ModuleList.ExpandTo(itm);
                        itm.IsSelected = true;
                        return;
                    }
                }
            }
        }
        #endregion
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
                            CurrentModule.Remove((IListModifier)item.Content);
                        }

                        ModuleCreator.RefreshList_Click(null, null);
                        ModuleList.Items.Remove(ModuleList.SelectedItem);
                        ModuleList_Refresh_AutoSelect();
                        ModuleList_UpdateOrder();
                    }
                }
                else
                {
                    ContentTreeViewItem item = (ModuleList.SelectedItem as ContentTreeViewItem);

                    CurrentModule.Remove(item.Content as IListModifier);

                    TreeViewItem parent = item.Parent as TreeViewItem;

                    int nitemindex = parent.Items.IndexOf(item) - 1;
                    parent.Items.Remove(item);

                    ModuleCreator.RefreshList_Click(null, null);
                    ModuleList_UpdateOrder();
                }
            }
        }

        private void ModuleList_Rename_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                if (ModuleList.Items.Contains(ModuleList.SelectedItem))
                    ((ModuleList.SelectedItem as TreeViewItem).Header as EditableTextBlock).IsInEditMode = true;
                else
                {
                    //Edit modifier
                    IListModifier mod = (ModuleList.SelectedItem as ContentTreeViewItem).Content as IListModifier;

                    Controls.ListModuleCreator dlg = new Controls.ListModuleCreator();
                    dlg.Load(mod);

                    if (dlg.ShowDialog() == true)
                    {
                        dlg.PushData();

                        ModuleList_Refresh_AutoSelect();
                        ModuleList_SelectModifier(mod);
                        ModuleCreator.RefreshList_Click(null, null);
                    }
                }
            }
        }

        void ModuleList_EditableTextBlock_Edited(object sender, EventArgs e)
        {
            foreach (ContentTreeViewItem item in (ModuleList.SelectedItem as TreeViewItem).Items)
            {
                (item.Content as IListModifier).Group = (sender as EditableTextBlock).Text;
            }
            ModuleList_SelectedItemChanged(null, null);
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

                        ((selected as ContentTreeViewItem).Content as IListModifier).Group =
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

                        ((selected as ContentTreeViewItem).Content as IListModifier).Group =
                            (newgroup.Header as EditableTextBlock).Text;

                        ModuleList_UpdateOrder();
                        newgroup.IsExpanded = true;
                    }
                }
            }
        }

        public void ModuleList_UpdateOrder()
        {
            List<IListModifier> AllModifiers = new List<IListModifier>();
            foreach (TreeViewItem pitem in ModuleList.Items)
            {
                foreach (ContentTreeViewItem citem in pitem.Items)
                {
                    AllModifiers.Add(citem.Content as IListModifier);
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

        private void ModuleList_Refresh_AutoSelect()
        {
            ModuleList.Items.Clear();

            if (CurrentModule != null)
            {
                if (CurrentModule.Get().Count != 0)
                {
                    foreach (IListModifier mod in CurrentModule.Get())
                    {
                        ModifiedTreeViewItem toAdd = null;

                        #region Create Group, if not existing
                        //If Group is not existing, create Group
                        if (ModuleList.Items.OfType<TreeViewItem>().ToList().Find(ti => (ti.Header as EditableTextBlock).Text == mod.Group) == null)
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
                        #endregion

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

        public void ModuleList_XmlModuleCreator_Add(IListModifier mod)
        {
            ContentTreeViewItem item = new ContentTreeViewItem();
            item.SemanticValue = "Modifier";
            item.Content = mod;
            item.Header = mod.ModuleList_ToHeader;

            CurrentGroupItem.Items.Add(item);
            ModuleList_UpdateOrder();
        }

        private void ModuleList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ModuleList.SelectedItem != null)
            {
                ModifiedTreeViewItem selected = ModuleList.SelectedItem as ModifiedTreeViewItem;

                if (selected.SemanticValue == "Group")
                {
                    CurrentGroup = (selected.Header as EditableTextBlock).Text;
                    CurrentGroupItem = selected;
                }
                else
                {
                    CurrentGroup = ((selected.Parent as TreeViewItem).Header as EditableTextBlock).Text;
                    CurrentGroupItem = selected.Parent as TreeViewItem;

                    //Select in ModuleEditor
                    IListModifier mod = (selected as ContentTreeViewItem).Content as IListModifier;
                    ModuleCreator.Files_File.Text = mod.File;
                    ModuleCreator.XmlNode_ModifierAdded_Select(mod);
                }
            }
        }

        private void ModuleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                try
                {
                    ModuleCreator.listFileTreeView.treeView.ExpandAll();
                }
                catch (Exception) { }
            }
        }

        private void ModuleList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                if ((ModuleList.SelectedItem as ModifiedTreeViewItem).SemanticValue == "Group")
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
