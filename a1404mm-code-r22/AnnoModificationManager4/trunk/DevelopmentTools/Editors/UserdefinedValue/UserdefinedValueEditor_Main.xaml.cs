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
using AnnoModificationManager4.ModificationTypes.Userdefined;
using udv = AnnoModificationManager4.ModificationTypes.Userdefined;
using DevelopmentTools.Misc;
using Borgstrup.DocBase.Client.Controls;
using AnnoModificationManager4.UserInterface.Misc;
using AnnoModificationManager4.Misc;

namespace DevelopmentTools.Editors.UserdefinedValue
{
    /// <summary>
    /// Interaction logic for UserdefinedValueEditor_Main.xaml
    /// </summary>
    public partial class UserdefinedValueEditor_Main : UserControl
    {
        public UserdefinedValueEditor_Main()
        {
            InitializeComponent();
            Project.Development_CurrentProject.UserInterface_Editors.Add(GetType(), this);
        }

        public void Refresh()
        {
            ModuleList.Items.Clear();

            //Add groups
            Project.Development_CurrentProject.Modification.UserdefinedValueGroups_Check();
            foreach (UserdefinedValueGroup gr in Project.Development_CurrentProject.Modification.UserdefinedValueGroups)
            {
                ContentTreeViewItem it = new ContentTreeViewItem();
                it.Content = gr;
                it.Header = new EditableTextBlock()
                {
                    Text = gr.InternalName,
                    Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/key.png"))
                };

                ModuleList.Items.Add(it);
            }

            //Add UserdefinedValues
            foreach (udv.UserdefinedValue val in Project.Development_CurrentProject.Modification.UserdefinedValues)
            {
                TreeViewItem it = new TreeViewItem();
                it.HeaderTemplate = Resources["DataTemplate_TreeView_UValue"] as DataTemplate;
                it.Header = val;

                ModuleList.Items.OfType<ContentTreeViewItem>().ToList().Find(ci =>
                    {
                        return (ci.Content as UserdefinedValueGroup).InternalName == val.Group;
                    })
                    .Items.Add(it);
            }

            (ModuleList.Items[0] as TreeViewItem).IsSelected = true;
        }

        public void SelectUserdefinedValue(udv.UserdefinedValue val)
        {
            foreach (TreeViewItem item in ModuleList.Items)
            {
                foreach (TreeViewItem itm in item.Items)
                {
                    if (itm.Header == val)
                    {
                        itm.IsSelected = true;
                        ModuleList.ExpandTo(itm);

                        return;
                    }
                }
            }
        }

        public void ModuleList_UpdateOrder()
        {
            int currentIndex = 0;

            foreach (ContentTreeViewItem item in ModuleList.Items)
            {
                UserdefinedValueGroup gr = item.Content as UserdefinedValueGroup;
                Project.Development_CurrentProject.Modification.UserdefinedValueGroups.Remove(gr);
                Project.Development_CurrentProject.Modification.UserdefinedValueGroups.Add(gr);

                foreach (TreeViewItem itm in item.Items)
                {
                    udv.UserdefinedValue val = itm.Header as udv.UserdefinedValue;
                    val.Index = currentIndex;
                    currentIndex++;
                }
            }

            Project.Development_CurrentProject.Modification.UserdefinedValues =
            Project.Development_CurrentProject.Modification.UserdefinedValues.OrderBy(uv => uv.Index).ToList();
        }

        private void ModuleList_Add_Click(object sender, RoutedEventArgs e)
        {
            UserdefinedValueGroup gr = new UserdefinedValueGroup();
            gr.InternalName = "New Group";
            gr.Label_Name = new AnnoModificationManager4.Language.Label()
            {
                German = "",
                English = "",
                Name = "Name"
            };

            UserdefinedValueCategoryEditor cat = new UserdefinedValueCategoryEditor();
            cat.Refresh(gr, false);

            if (cat.ShowDialog() == true)
            {
                Project.Development_CurrentProject.Modification.UserdefinedValueGroups.Add(gr);                
                Refresh();
                ModuleList_UpdateOrder();
            }
        }

        private void ModuleList_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                if (MessageWindow.Show("The selection will be deleted permanently.\nRemove it anyway?", MessageWindow.MessageWindowType.YesNo)
                    == MessageBoxResult.Yes)
                {
                    //group
                    if (ModuleList.Items.Contains(ModuleList.SelectedItem))
                    {
                        foreach (TreeViewItem itm in (ModuleList.SelectedItem as TreeViewItem).Items)
                        {
                            udv.UserdefinedValue val = itm.Header as udv.UserdefinedValue;
                            Project.Development_CurrentProject.Modification.UserdefinedValues.Remove(val);
                        }
                        Project.Development_CurrentProject.Modification.UserdefinedValueGroups.Remove(
                            (ModuleList.SelectedItem as ContentTreeViewItem).Content as UserdefinedValueGroup);

                        Refresh();
                    }
                    //uvalue
                    else
                    {
                        ContentTreeViewItem groupitem = (ModuleList.SelectedItem as TreeViewItem).Parent as ContentTreeViewItem;
                        udv.UserdefinedValue val = (ModuleList.SelectedItem as TreeViewItem).Header as udv.UserdefinedValue;

                        Project.Development_CurrentProject.Modification.UserdefinedValues.Remove(val);
                        groupitem.Items.Remove(ModuleList.SelectedItem);

                        if (groupitem.Items.Count != 0)
                            (groupitem.Items[0] as TreeViewItem).IsSelected = true;
                        else
                            Refresh();
                    }

                    ModuleList_UpdateOrder();
                }
            }
        }

        private void ModuleList_Rename_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                if (ModuleList.Items.Contains(ModuleList.SelectedItem))
                {
                    UserdefinedValueGroup gr = (ModuleList.SelectedItem as ContentTreeViewItem).Content as UserdefinedValueGroup;
                    UserdefinedValueCategoryEditor cat = new UserdefinedValueCategoryEditor();
                    cat.Refresh(gr, true);

                    if (cat.ShowDialog() == true)
                    {                        
                        Refresh();
                    }
                }
            }
        }

        private void ModuleList_Move_Up_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleList.SelectedItem != null)
            {
                //Modifier
                if (ModuleList.SelectedItem as ContentTreeViewItem == null)
                {
                    TreeViewItem parent = (ModuleList.SelectedItem as TreeViewItem).Parent as TreeViewItem;

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
                //Modifier
                if (ModuleList.SelectedItem as ContentTreeViewItem == null)
                {
                    TreeViewItem parent = (ModuleList.SelectedItem as TreeViewItem).Parent as TreeViewItem;

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
                TreeViewItem selected = ModuleList.SelectedItem as TreeViewItem;

                //Modifier
                if (ModuleList.SelectedItem as ContentTreeViewItem == null)
                {
                    int currentparentindex = ModuleList.Items.IndexOf(selected.Parent);
                    if (currentparentindex != 0)
                    {
                        TreeViewItem oldgroup = (TreeViewItem)selected.Parent;
                        TreeViewItem newgroup = (TreeViewItem)ModuleList.Items[currentparentindex - 1];

                        oldgroup.Items.Remove(selected);
                        newgroup.Items.Add(selected);

                        ((selected as TreeViewItem).Header as udv.UserdefinedValue).Group =
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
                TreeViewItem selected = ModuleList.SelectedItem as TreeViewItem;

                //Modifier
                if (ModuleList.SelectedItem as ContentTreeViewItem == null)
                {
                    int currentparentindex = ModuleList.Items.IndexOf(selected.Parent);
                    if (currentparentindex != ModuleList.Items.Count - 1)
                    {
                        TreeViewItem oldgroup = (TreeViewItem)selected.Parent;
                        TreeViewItem newgroup = (TreeViewItem)ModuleList.Items[currentparentindex + 1];

                        oldgroup.Items.Remove(selected);
                        newgroup.Items.Insert(0, selected);

                        ((selected as TreeViewItem).Header as udv.UserdefinedValue).Group =
                            (newgroup.Header as EditableTextBlock).Text;

                        ModuleList_UpdateOrder();
                        newgroup.IsExpanded = true;
                    }
                }
            }
        }

        private void ModuleList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ModuleList.SelectedItem != null)
            {
                if (ModuleList.Items.Contains(ModuleList.SelectedItem))
                {
                    ModuleCreator.Field_Group.Text = ((ModuleList.SelectedItem as TreeViewItem).Header as EditableTextBlock).Text;
                }
                else
                {
                    ModuleCreator.Refresh((ModuleList.SelectedItem as TreeViewItem).Header as udv.UserdefinedValue);
                }
            }
        }
    }
}
