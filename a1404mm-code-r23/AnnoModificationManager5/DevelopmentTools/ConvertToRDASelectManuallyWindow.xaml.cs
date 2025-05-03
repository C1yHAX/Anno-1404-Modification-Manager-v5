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
using RDAExplorer;

namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for ConvertToRDASelectManuallyWindow.xaml
    /// </summary>
    public partial class ConvertToRDASelectManuallyWindow : Window
    {
        public string SelectedRDA;
        public bool EnableAutoAssign = false;

        public ConvertToRDASelectManuallyWindow()
        {
            InitializeComponent();
        }

        public void Prepare(List<string> files, string tolookup)
        {
            txb_Message.Text = "Couldn't find a matching rda folder for \n\"" + tolookup + "\"\nSelect the RDA file manually.";
            //foreach (var i in files)
            //{
            //    cmb_RDAs.Items.Add(Path.GetFileName(i));
            //}

            if (tolookup.ToLower().StartsWith("data"))
            {
                cmb_RDAs.SelectedIndex = 0;
            }
            else
            {
                cmb_RDAs.SelectedIndex = 1;
            }
        }

        private void b_OK_Click(object sender, RoutedEventArgs e)
        {
            SelectedRDA = cmb_RDAs.Text;
            EnableAutoAssign = chb_EnableAutoAssign.IsChecked.Value;
            DialogResult = true;
        }
    }
}
