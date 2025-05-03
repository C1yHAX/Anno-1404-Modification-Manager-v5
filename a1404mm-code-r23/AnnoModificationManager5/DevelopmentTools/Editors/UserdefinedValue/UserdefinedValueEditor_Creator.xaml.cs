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
using System.Text.RegularExpressions;
using usd=AnnoModificationManager5.ModificationTypes.Userdefined;
using lng=AnnoModificationManager5.Language;
using AnnoModificationManager5.ModificationTypes.Userdefined;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.UserInterface.Misc;

namespace DevelopmentTools.Editors.UserdefinedValue
{
    /// <summary>
    /// Interaction logic for UserdefinedValueEditor_Creator.xaml
    /// </summary>
    public partial class UserdefinedValueEditor_Creator : UserControl
    {       
        public UserdefinedValueEditor_Creator()
        {
            if (Project.Development_CurrentProject != null)
                Project.Development_CurrentProject.UserInterface_Editors.Add(GetType(), this);

            InitializeComponent();

            Field_Numeric_Min.Text = int.MinValue.ToString();
            Field_Numeric_Max.Text = int.MaxValue.ToString();

            Loaded += new RoutedEventHandler(UserdefinedValueEditor_Creator_Loaded);
        }

        void UserdefinedValueEditor_Creator_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Refresh(new usd.UserdefinedValue()
                   {
                       Label_Name = new lng.Label() { Name = "Name", English = "", German = "" },
                       Label_Description = new lng.Label() { Name = "Description", English = "", German = "" },
                       Name = "",
                       Group = "No Group"
                   });
            }
            catch (Exception)
            {               
            }
        }

        public usd.UserdefinedValue GetCurrentValue()
        {
            usd.UserdefinedValue val = new usd.UserdefinedValue();
            val.Name = Field_InternalName.Text.Trim();
            val.Group = Field_Group.Text;
            val.Current = Field_CurrentValue.Text.Trim();
            val.Label_Name = Labeleditor_Name.Label.Clone();
            val.Label_Description = Labeleditor_Description.Label.Clone();

            if (RadioButton_ValueType_Text.IsChecked == true)
            {
                val.Type = usd.UserdefinedValue.UserdefinedValueType.TextEdit;
            }
            else if (RadioButton_ValueType_Numeric.IsChecked == true)
            {
                val.Type = usd.UserdefinedValue.UserdefinedValueType.Numeric;
                val.Numeric_Min = int.Parse(Field_Numeric_Min.Text);
                val.Numeric_Max = int.Parse(Field_Numeric_Max.Text);
            }
            else if (RadioButton_ValueType_ComboBox.IsChecked == true)
            {
                val.Type = usd.UserdefinedValue.UserdefinedValueType.ComboBox;
                foreach (UserdefinedValue_ComboBoxItem itm in Field_Combobox_List.Items)
                {
                    UserdefinedValue_ComboBoxItem nit = new UserdefinedValue_ComboBoxItem(itm.Name.Clone(), itm.Value);
                    val.ComboBoxItems.Add(nit);
                }
            }
            else if (RadioButton_ValueType_EnableDisable.IsChecked == true)
            {
                val.Type = usd.UserdefinedValue.UserdefinedValueType.ModifierEnabled;

                foreach (UserdefinedValue_FileItem itm in Field_Files_List.Items)
                {
                    val.Files.Add(itm.File);
                }
                
                try
                {
                    val.Current = bool.Parse(val.Current).ToString();
                }
                catch (Exception)
                {
                    val.Current = "True";
                }
            }
            else if (RadioButton_ValueType_EnableDisableFileModifiers.IsChecked == true)
            {
                val.Type = usd.UserdefinedValue.UserdefinedValueType.FilesEnabled;
                foreach (UserdefinedValue_FileItem itm in Field_Files_List.Items)
                {
                    val.Files.Add(itm.File);
                }

                try
                {
                    val.Current = bool.Parse(val.Current).ToString();
                }
                catch (Exception)
                {
                    val.Current = "True";
                }
            }

            return val;
        }

        public void Refresh(usd.UserdefinedValue val)
        {
            Field_InternalName.Text = val.Name;
            Field_Group.Text = val.Group;
            Field_CurrentValue.Text = val.Current;
            Labeleditor_Name.Label = val.Label_Name.Clone();
            Labeleditor_Description.Label = val.Label_Description.Clone();

            Field_Combobox_List.Items.Clear();

            switch (val.Type)
            {
                case usd.UserdefinedValue.UserdefinedValueType.TextEdit:
                    RadioButton_ValueType_Text.IsChecked = true;
                    break;
                case usd.UserdefinedValue.UserdefinedValueType.Numeric:
                    RadioButton_ValueType_Numeric.IsChecked = true;
                    Field_Numeric_Min.Text = val.Numeric_Min.ToString();
                    Field_Numeric_Max.Text = val.Numeric_Max.ToString();
                    break;
                case usd.UserdefinedValue.UserdefinedValueType.ModifierEnabled:
                    RadioButton_ValueType_EnableDisable.IsChecked = true;

                    Field_Files_List.Items.Clear();

                    foreach (string i in val.Files)
                        Field_Files_List.Items.Add(new UserdefinedValue_FileItem(i));
                    break;
                case usd.UserdefinedValue.UserdefinedValueType.ComboBox:
                    RadioButton_ValueType_ComboBox.IsChecked = true;
                    Field_Combobox_List.Items.Clear();

                    foreach (UserdefinedValue_ComboBoxItem i in val.ComboBoxItems)
                    {
                        Field_Combobox_List.Items.Add(new UserdefinedValue_ComboBoxItem(i.Name.Clone(), i.Value));
                    }
                    break;
                case usd.UserdefinedValue.UserdefinedValueType.FilesEnabled:
                    RadioButton_ValueType_EnableDisableFileModifiers.IsChecked = true;
                    Field_Files_List.Items.Clear();

                    foreach (string i in val.Files)
                        Field_Files_List.Items.Add(new UserdefinedValue_FileItem(i));
                    break;
            }
        }

        private void UserdefinedValueType_Changed(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                Grid_ValueType_ComboBox.Visibility = System.Windows.Visibility.Collapsed;
                Grid_ValueType_Numeric.Visibility = System.Windows.Visibility.Collapsed;
                Grid_ValueType_EnableDisableFiles.Visibility = System.Windows.Visibility.Collapsed;

                if (RadioButton_ValueType_Numeric.IsChecked == true)
                    Grid_ValueType_Numeric.Visibility = System.Windows.Visibility.Visible;
                else if (RadioButton_ValueType_ComboBox.IsChecked == true)
                    Grid_ValueType_ComboBox.Visibility = System.Windows.Visibility.Visible;
                if (RadioButton_ValueType_EnableDisableFileModifiers.IsChecked == true
                    || RadioButton_ValueType_EnableDisable.IsChecked == true)
                    Grid_ValueType_EnableDisableFiles.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Textbox_Numeric_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox Sender = sender as TextBox;
            if (Regex.Match(Sender.Text, "[^0-9-]").Success || Sender.Text.LastIndexOf('-') > 0)
            {
                string ntext = Regex.Replace(Sender.Text, "[^0-9-]", "");
                if (Sender.Text.Contains("-"))
                {
                    ntext = "-" + ntext.Replace("-", "");
                }
                if (string.IsNullOrEmpty(ntext))
                    ntext = "0";

                Sender.Text = ntext;               
            }
        }

        private void Textbox_UpdateSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                System.Windows.Controls.Primitives.TextBoxBase Sender = sender as System.Windows.Controls.Primitives.TextBoxBase;
                Sender.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            else if (sender is ComboBox)
            {
                ComboBox Sender = sender as ComboBox;
                Sender.GetBindingExpression(ComboBox.TextProperty).UpdateSource();
            }
        }

        private void Combobox_List_Add_Click(object sender, RoutedEventArgs e)
        {
            Field_Combobox_List.Items.Add(new UserdefinedValue_ComboBoxItem(new lng.Label() { Name = "Element" }, ""));
        }

        private void Combobox_List_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Field_Combobox_List.SelectedItem != null)
            {
                Field_Combobox_List.Items.Remove(Field_Combobox_List.SelectedItem);
            }
        }

        private void Combobox_List_MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (Field_Combobox_List.SelectedItem != null && Field_Combobox_List.SelectedIndex != 0)
            {
                object tr = Field_Combobox_List.SelectedItem;
                int tri = Field_Combobox_List.SelectedIndex;

                Field_Combobox_List.Items.Remove(tr);
                Field_Combobox_List.Items.Insert(tri - 1, tr);
                Field_Combobox_List.SelectedIndex = tri - 1;
                Field_Combobox_List.ScrollIntoView(tr);
            }
        }

        private void Combobox_List_MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (Field_Combobox_List.SelectedItem != null 
                && Field_Combobox_List.SelectedIndex != Field_Combobox_List.Items.Count - 1)
            {
                object tr = Field_Combobox_List.SelectedItem;
                int tri = Field_Combobox_List.SelectedIndex;

                Field_Combobox_List.Items.Remove(tr);
                Field_Combobox_List.Items.Insert(tri + 1, tr);
                Field_Combobox_List.SelectedIndex = tri + 1;
                Field_Combobox_List.ScrollIntoView(tr);
            }
        }

        private void button_add_Click(object sender, RoutedEventArgs e)
        {
            usd.UserdefinedValue val = GetCurrentValue();
            Project.Development_CurrentProject.Modification.UserdefinedValues.Add(val);

            UserdefinedValueEditor_Main parent = Project.Development_CurrentProject.UserInterface_Editors[typeof(UserdefinedValueEditor_Main)] as UserdefinedValueEditor_Main;
            parent.Refresh();
            parent.SelectUserdefinedValue(val);
            parent.ModuleList_UpdateOrder();
        }

        private void button_replace_Click(object sender, RoutedEventArgs e)
        {
            UserdefinedValueEditor_Main parent = Project.Development_CurrentProject.UserInterface_Editors[typeof(UserdefinedValueEditor_Main)] as UserdefinedValueEditor_Main;
            if (parent.ModuleList.SelectedItem != null && (parent.ModuleList.SelectedItem as TreeViewItem).Header is usd.UserdefinedValue)
            {
                usd.UserdefinedValue old = (parent.ModuleList.SelectedItem as TreeViewItem).Header as usd.UserdefinedValue;
                usd.UserdefinedValue New = GetCurrentValue();

                if (New == null)
                    return;

                //get index
                int idx = Project.Development_CurrentProject.Modification.UserdefinedValues.IndexOf(old);

                //Set
                Project.Development_CurrentProject.Modification.UserdefinedValues[idx] = New;

                //Finish
                parent.Refresh();
                parent.SelectUserdefinedValue(New);
                parent.ModuleList_UpdateOrder();
            }
        }       

        private void EnableDisableFiles_ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            cmb.Items.Clear();

            foreach (string File in Project.Development_CurrentProject.Modification.ModificationUtils.Files_Anno)
            {
                cmb.Items.Add(new UserdefinedValue_FileItem(File.Replace(Project.Development_CurrentProject.Modification.Folder + "\\Files\\Anno1404", "%Anno%")));
            }
            foreach (string File in Project.Development_CurrentProject.Modification.ModificationUtils.Files_AppData)
            {
                cmb.Items.Add(new UserdefinedValue_FileItem(File.Replace(Project.Development_CurrentProject.Modification.Folder + "\\Files\\AppData", "%AppData%")));
            }
        }

        private void Files_List_Add_Click(object sender, RoutedEventArgs e)
        {
            Field_Files_List.Items.Add(new UserdefinedValue_FileItem(""));
        }

        private void Files_List_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Field_Files_List.SelectedItem != null)
            {
                int si = Field_Files_List.SelectedIndex;
                Field_Files_List.Items.Remove(Field_Files_List.SelectedItem);

                if (si != 0 && Field_Files_List.Items.Count != 0)
                {
                    Field_Files_List.SelectedIndex = si - 1;
                }
            }
        }

        private void Files_List_AddFolderRecursive_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser dlg = new FolderBrowser();
            dlg.RootFolder = Project.Development_CurrentProject.Modification.Folder + "\\Files";
            dlg.ShowFiles = true;

            if (dlg.ShowDialog() == true)
            {
                foreach (string file in dlg.GetFiles)
                {
                    string File=file.Replace(Project.Development_CurrentProject.Modification.Folder + "\\Files\\Anno1404", "%Anno%")
                        .Replace(Project.Development_CurrentProject.Modification.Folder + "\\Files\\AppData", "%AppData%");

                    if (Field_Files_List.Items.OfType<UserdefinedValue_FileItem>().ToList().Find(f => f.File == File) == null)
                        Field_Files_List.Items.Add(new UserdefinedValue_FileItem(File));
                }
            }
        }
    }
}
