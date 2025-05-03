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
using DevelopmentTools.Misc;
using RDAExplorer;
using AnnoModificationManager5.ModificationTypes;

namespace DevelopmentTools.Tools.Global
{
    /// <summary>
    /// Interaction logic for RDAFileBrowser.xaml
    /// </summary>
    public partial class RDAFileBrowser : Window
    {


        ///////////
        //Property SelectedRDAContainer
        ///////////
        private string _SelectedRDAContainer;
        public string SelectedRDAContainer
        {
            get
            {
                return _SelectedRDAContainer;
            }
        }


        ///////////
        //Property SelectedRDAFile
        ///////////
        private List<RDAFile> _SelectedRDAFile;
        public List<RDAFile> SelectedRDAFiles
        {
            get
            {
                return _SelectedRDAFile;
            }
        }


        public RDAFileBrowser()
        {
            InitializeComponent();
        }

        private void b_OK_Click(object sender, RoutedEventArgs e)
        {
            _SelectedRDAFile = rdaView.SelectedFiles;
            _SelectedRDAContainer = ((ContentTreeViewItem)rdaFiles.SelectedItem).Header.ToString().Split('\\')[0];

            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsWindow.CheckRDAWorkingDir())
            {
                foreach (var file in Modification.RDAManager.ListRDAsInFolder(Properties.Settings.Default.RDAWorkingDir))
                {
                    ContentTreeViewItem item = new ContentTreeViewItem();
                    item.Header = file;
                    item.Content = Properties.Settings.Default.RDAWorkingDir + "\\" + file;

                    rdaFiles.Items.Add(item);
                }
            }
        }

        private void rdaView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                e.Handled = true;
        }

        private void rdaFiles_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (rdaFiles.SelectedItem != null)
            {
                try
                {
                    RDAReader rd = Modification.RDAManager.GetReader(((ContentTreeViewItem)rdaFiles.SelectedItem).Content.ToString());
                    rdaView.RDAFolder = rd.rdaFolder;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
