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
using AnnoModificationManager5.DownloadService;

namespace DevelopmentTools.Tools.PackagePublisher
{
    /// <summary>
    /// Interaction logic for PackagePublisher.xaml
    /// </summary>
    public partial class PackagePublisher : UserControl
    {
        public bool AutoAddCurrentProject = false;

        /// <summary>View that was shown before navigating here; restored on "Zurück".</summary>
        public object PreviousContent { get; set; }

        public PackagePublisher()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (PreviousContent != null)
                MainWindow.CurrentMainWindow.Content = PreviousContent;
            else
                MainWindow.CurrentMainWindow.MainWindow_Loaded(null, null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            button_Refresh_Click(null, null);

            if (AutoAddCurrentProject)
            {
                ModificationInfoEditor editor = new ModificationInfoEditor();
                editor.SetCurrentInfo(ModificationInfo.FromModification(Project.Development_CurrentProject.Modification.Info), true);

                if (editor.ShowDialog() == true)
                {
                    ModificationInfoConnector.Instance.AvailablePackages.Add(editor.CurrentInfo);
                    ModificationInfoConnector.Instance.Upload(editor.CurrentInfo);

                    button_Refresh_Click(null, null);
                }
            }
        }

        private void button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            ModificationInfoConnector.Instance.Refresh(null, true);

            Package_List.Items.Clear();
            foreach (ModificationInfo info in ModificationInfoConnector.Instance.AvailablePackages)
            {
                Package_List.Items.Add(info);
            }
        }

        private void button_Add_Click(object sender, RoutedEventArgs e)
        {
            ModificationInfoEditor editor = new ModificationInfoEditor();
            if (editor.ShowDialog() == true)
            {
                ModificationInfoConnector.Instance.AvailablePackages.Add(editor.CurrentInfo);
                ModificationInfoConnector.Instance.Upload(editor.CurrentInfo);

                button_Refresh_Click(null, null);
            }
        }

        private void button_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Package_List.SelectedItem != null)
            {
                ModificationInfo info = Package_List.SelectedItem as ModificationInfo;
                
                ModificationInfoConnector.Instance.Delete(info);
                ModificationInfoConnector.Instance.AvailablePackages.Remove(info);

                button_Refresh_Click(null, null);
            }
        }

        private void button_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Package_List.SelectedItem != null)
            {
                ModificationInfo info = Package_List.SelectedItem as ModificationInfo;
                ModificationInfoEditor editor = new ModificationInfoEditor();
                editor.SetCurrentInfo(info, true);

                if (editor.ShowDialog() == true)
                {
                    ModificationInfoConnector.Instance.AvailablePackages.Add(editor.CurrentInfo);
                    ModificationInfoConnector.Instance.Upload(editor.CurrentInfo);

                    button_Refresh_Click(null, null);
                }
            }
        }        
    }
}
