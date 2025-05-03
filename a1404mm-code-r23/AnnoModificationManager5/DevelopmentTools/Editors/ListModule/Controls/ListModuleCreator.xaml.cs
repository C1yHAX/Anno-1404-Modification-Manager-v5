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
using AnnoModificationManager5.ModificationTypes.Userdefined;
using usd = AnnoModificationManager5.ModificationTypes.Userdefined;
using AnnoModificationManager5.ModificationTypes.ListModule.ListModifiers;
using DevelopmentTools.Tools.Global;

namespace DevelopmentTools.Editors.ListModule.Controls
{
    /// <summary>
    /// Interaction logic for ListModuleCreator.xaml
    /// </summary>
    public partial class ListModuleCreator : Window
    {
        public IListModifier uValueParent = null;

        /*public string NewValue = "";
        public List<ListUserdefinedValue> UserdefinedValues = new List<ListUserdefinedValue>();*/

        public ListModuleCreator()
        {
            InitializeComponent();
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(Field_NewValue.Text.Trim()))
            //    return;
            /*NewValue = Field_NewValue.Text;
            UserdefinedValues = UserdefinedValues_List.Items.OfType<ListUserdefinedValue>().ToList();*/

            DialogResult = true;
        }

        private void UserdefinedValues_List_Add_Click(object sender, RoutedEventArgs e)
        {
            ListUserdefinedValue val = new ListUserdefinedValue();
            val.Parent = uValueParent;
            val.Key = "{" + UserdefinedValues_List.Items.Count + "}";

            UserdefinedValues_List.Items.Add(val);

        }

        private void UserdefinedValues_List_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (UserdefinedValues_List.SelectedItem != null)
            {
                UserdefinedValues_List.Items.Remove(UserdefinedValues_List.SelectedItem);
            }
        }

        private void UserdefinedValue_ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox Sender = sender as ComboBox;
            Sender.Items.Clear();

            foreach (usd.UserdefinedValue val in Project.Development_CurrentProject.Modification.UserdefinedValues)
            {
                Sender.Items.Add(new ComboBoxItem() { Content = val.Name });
            }
        }

        public void Load(IListModifier mod)
        {
            uValueParent = mod;

            if (mod is AddModifier)
            {
                AddModifier m = (AddModifier)mod;
                Field_Group.Text = m.ElementGroup;
                Field_NewValue.Text = m.ElementValue;
            }
            else if (mod is AddGroupModifier)
            {
                g_NewValue.Visibility = System.Windows.Visibility.Collapsed;
                g_OldValue.Visibility = System.Windows.Visibility.Collapsed;
                g_Group.Visibility = System.Windows.Visibility.Visible;
                Field_Group.Text = ((AddGroupModifier)mod).ElementGroup;
            }
            else if (mod is EditModifier)
            {
                g_OldValue.Visibility = System.Windows.Visibility.Visible;

                Field_Group.Text = mod.ElementGroup;
                Field_OldValue.Text = mod.ElementValue;
                Field_NewValue.Text = (mod as EditModifier).NewValue;
            }
            else if (mod is RemoveModifier)
            {
                brd_UserDefValues.Visibility = System.Windows.Visibility.Collapsed;
                g_NewValue.Visibility = System.Windows.Visibility.Collapsed;
                Field_OldValue.Text = mod.ElementValue;
                Field_Group.Text = ((RemoveModifier)mod).ElementGroup;
            }

            foreach (var usd in mod.UserdefinedValues)
            {
                UserdefinedValues_List.Items.Add(usd);
            }

            Files_File.Text = mod.File;
        }

        public void PushData()
        {
            IListModifier mod = uValueParent;

            if (string.IsNullOrEmpty(Field_Group.Text.Trim()))
                Field_Group.Text = "<No Group>";

            if (mod is AddModifier)
            {
                AddModifier m = (AddModifier)mod;
                m.ElementGroup = Field_Group.Text.Trim();
                m.ElementValue = Field_NewValue.Text.Trim();
            }
            else if (mod is AddGroupModifier)
            {
                ((AddGroupModifier)mod).ElementGroup = Field_Group.Text.Trim();
            }
            else if (mod is EditModifier)
            {
                mod.ElementGroup = Field_Group.Text.Trim();
                mod.ElementValue = Field_OldValue.Text.Trim();
                ((EditModifier)mod).NewValue = Field_NewValue.Text.Trim();
            }
            else if (mod is RemoveModifier)
            {
                mod.ElementValue = Field_OldValue.Text.Trim();
                ((RemoveModifier)mod).ElementGroup = Field_Group.Text.Trim();
            }

            uValueParent.UserdefinedValues.Clear();
            foreach (usd.ListUserdefinedValue usd in UserdefinedValues_List.Items)
            {
                uValueParent.UserdefinedValues.Add(usd);
                usd.Parent = uValueParent;
            }

            mod.File = Files_File.Text.Trim();
        }

        private void Files_File_DropDownOpened(object sender, EventArgs e)
        {
            Files_File.ItemsSource = Project.Development_CurrentProject.Modification.CollectedFiles_List_List;
            Files_File.Items.Refresh();
        }

        private void Files_Files_Open_Click(object sender, RoutedEventArgs e)
        {
            FileBrowser browser = new FileBrowser();
            if (browser.ShowDialog() == true)
            {
                Files_File.Text = browser.ChoosenFile;
            }
        }
    }
}
