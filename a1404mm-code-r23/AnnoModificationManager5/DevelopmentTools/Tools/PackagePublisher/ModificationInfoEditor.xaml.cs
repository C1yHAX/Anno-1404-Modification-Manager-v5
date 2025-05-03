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
using AnnoModificationManager5.Misc;
using mod = AnnoModificationManager5.ModificationTypes;
using AnnoModificationManager5.UserInterface.Misc;
using Ionic.Zip;

namespace DevelopmentTools.Tools.PackagePublisher
{
    /// <summary>
    /// Interaction logic for ModificationInfoEditor.xaml
    /// </summary>
    public partial class ModificationInfoEditor : Window
    {       
        public ModificationInfo CurrentInfo;

        private OpenFileDialog OpenProject = new OpenFileDialog()
        {
            Filter = "Modification Project|*.zip"
        };

        public ModificationInfoEditor()
        {
            InitializeComponent();
        }

        public void SetCurrentInfo(ModificationInfo mod, bool clearlinks)
        {
            CurrentInfo = mod;
            Field_Identifier.Text = mod.GetIdentificationString;
            Field_Project.Text = "Loaded.";
            if (clearlinks)
            {
                Field_Image.Text = mod.ImageUrl;
                Field_Download.Text = mod.DownloadUrl;
            }
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentInfo != null)
            {
                CurrentInfo.DownloadUrl = Field_Download.Text;
                CurrentInfo.ImageUrl = Field_Image.Text;
                CurrentInfo.Date = DateTime.Now;
                DialogResult = true;
            }
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Field_Project_Open_Click(object sender, RoutedEventArgs e)
        {
            if (OpenProject.ShowDialog() == true)
            {
                try
                {
                    string xml = "";

                    ZipFile zip = new ZipFile(OpenProject.FileName);
                    using (MemoryStream memory = new MemoryStream())
                    {
                        zip["config.xml"].Extract(memory);
                        memory.Position = 0;

                        using (StreamReader w = new StreamReader(memory))
                        {
                            xml = w.ReadToEnd();
                        }
                    }
                    zip.Dispose();
                    zip = null;

                    SetCurrentInfo(ModificationInfo.FromModification(AnnoModificationManager5.ModificationTypes.ModificationInfo.FromXmlData(xml)), false);
                }
                catch (Exception ex)
                {
                    MessageWindow.Show(ex.Message);

                    CurrentInfo = null;
                    Field_Identifier.Text = "";
                    Field_Project.Text = "";
                }
            }
        }

        private void Field_Download_DropDownOpened(object sender, EventArgs e)
        {
            Field_Download.Items.Clear();
            foreach (ModificationInfo info in ModificationInfoConnector.Instance.AvailablePackages)
                Field_Download.Items.Add(new ComboBoxItem() { Content = info.DownloadUrl });
        }
    }
}
