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

namespace AnnoModificationManager5.Language
{
    /// <summary>
    /// Interaction logic for LabelEditor.xaml
    /// </summary>
    public partial class LabelEditor_Big : UserControl
    {
        private Label _Label;
        public Label Label
        {
            get
            {
                return _Label;
            }
            set
            {
                _Label = value;

                groupBox.Header = value.Name;
                Field_German.Text = value.German;
                Field_English.Text = value.English;
            }
        }

        public LabelEditor_Big()
        {
            InitializeComponent();
        }

        private void TextChanged_UpdateBinding(object sender, TextChangedEventArgs e)
        {
            if (_Label != null)
            {
                string Name = (sender as TextBox).Name;
                if (Name == "Field_German")
                {
                    Label.German = (sender as TextBox).Text;
                }
                else if (Name == "Field_English")
                {
                    Label.English = (sender as TextBox).Text;
                }
            }
        }
    }
}
