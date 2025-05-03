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

namespace DevelopmentTools.Editors.UserdefinedValue
{
    /// <summary>
    /// Interaction logic for UserdefinedValueCategoryEditor.xaml
    /// </summary>
    public partial class UserdefinedValueCategoryEditor : Window
    {
        public UserdefinedValueGroup Modifier;
        private bool IsEdit = false;

        public UserdefinedValueCategoryEditor()
        {
            InitializeComponent();
        }

        public void Refresh(UserdefinedValueGroup gr, bool edit)
        {
            Modifier = gr;
            IsEdit = edit;

            Field_InternalName.Text = gr.InternalName;
            LabelEditor_Name.Label = gr.Label_Name;
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            Field_InternalName.Text= Field_InternalName.Text.Trim();

            if (string.IsNullOrEmpty(Field_InternalName.Text))
                return;
            if (!IsEdit && Project.Development_CurrentProject.Modification.UserdefinedValueGroups.Find(g => g.InternalName == Field_InternalName.Text) != null)
                return;

            if (IsEdit)
            {
                foreach (AnnoModificationManager5.ModificationTypes.Userdefined.UserdefinedValue val in Modifier.GetUserdefinedValues(
                    Project.Development_CurrentProject.Modification))
                {
                    val.Group = Field_InternalName.Text;
                }
            }

            Modifier.InternalName = Field_InternalName.Text;
            Modifier.Label_Name = LabelEditor_Name.Label;            

            DialogResult = true;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
