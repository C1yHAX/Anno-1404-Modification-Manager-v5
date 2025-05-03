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
using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.Components;
using System.IO;
using AnnoModificationManager4.UserInterface.Misc;
using AnnoModificationManager4.Language.DictionarySystem;
using System.Diagnostics;
using AnnoModificationManager4.UserInterface.MainUI;
using AnnoModificationManager4.Misc;

namespace AnnoModificationManager4.MainControls
{
    /// <summary>
    /// Interaction logic for ModificationAnalyzerPanel.xaml
    /// </summary>
    public partial class ModificationAnalyzerPanel : UserControl
    {
        private Modification CurrentModification;

        public ModificationAnalyzerPanel()
        {
            InitializeComponent();
        }

        public void LoadModification(Modification mod)
        {
            CurrentModification = mod;

            //Image presenter
            imagePresenter.LoadImages(mod);

            //Description
            field_Description.Text = mod.Info.Description.Get;

            //Fields
            field_Name.Text = mod.Info.Name.Get;
            field_Version.Text = "Version " + mod.Info.Version;

            //Function buttons
            button_OpenWebsite.ToolTip = mod.Info.Website;
            button_OpenDocumentation.Visibility =
                !string.IsNullOrEmpty(mod.Info.Documentation) ? Visibility.Visible : Visibility.Collapsed;
            button_OpenDocumentation.ToolTip = mod.Info.Documentation;

            ////Activation status
            ModificationActivationResponse activationstatus = ModificationHandler.ActivationResponses[mod];
            switch (activationstatus.Result())
            {
                case Misc.Enums.Modification_ActivationStatus.Activated:
                    button_Activate.Visibility = System.Windows.Visibility.Collapsed;
                    button_Deactivate.Visibility = System.Windows.Visibility.Visible;
                    break;
                case Misc.Enums.Modification_ActivationStatus.Deactivated:
                    button_Activate.Visibility = System.Windows.Visibility.Visible;
                    button_Deactivate.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case Misc.Enums.Modification_ActivationStatus.Partially:
                    button_Activate.Visibility = System.Windows.Visibility.Visible;
                    button_Deactivate.Visibility = System.Windows.Visibility.Visible;
                    break;
            }

            //Compatibility
            if (!ModificationHandler.Instance.IsCompatible(mod))
            {
                button_Activate.Visibility = System.Windows.Visibility.Collapsed;
                button_Deactivate.Visibility = System.Windows.Visibility.Collapsed;
                button_IsIncompatible.Visibility = System.Windows.Visibility.Visible;

                field_Description.Text =
                    LanguageDictionary.Get("MainUI", "AnalyzerPanel_Incompatible")
                    .Replace("{0}", StringExtension.PutTogetherComma(mod.Info.AnnoVersions)) + "\r\n\r\n" + field_Description.Text;
            }
            else
            {
                button_IsIncompatible.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void button_Activate_Click(object sender, RoutedEventArgs e)
        {
            ActivationDialog dlg = new ActivationDialog();
            dlg.LoadModification(CurrentModification);
            if (dlg.ShowDialog() == true)
            {
                MainWindow.CurrentMainWindow.UpdateActivationResponses();
            }
        }

        private void button_Deactivate_Click(object sender, RoutedEventArgs e)
        {
            DeactivationDialog dlg = new DeactivationDialog();
            dlg.LoadModification(CurrentModification);
            if (dlg.ShowDialog() == true)
            {
                MainWindow.CurrentMainWindow.UpdateActivationResponses();
            }
        }

        private void button_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteDialog dlg = new DeleteDialog();
            dlg.LoadModification(CurrentModification);
            if (dlg.ShowDialog() == true)
            {
                MainWindow.CurrentMainWindow.ReloadModifications(true);
            }
        }

        private void button_OpenDocumentation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string extension = Path.GetExtension(CurrentModification.Info.Documentation).ToLower();
                if (extension == ".bat" || extension == ".exe" || extension == ".com" || extension == ".hta")
                {
                    if (MessageWindow.Show(LanguageDictionary.Get("MainUI", "AnalyzerPanel_OpenDocumentation_Warning"), MessageWindow.MessageWindowType.OKCancel)
                        == MessageBoxResult.Cancel)
                        return;
                }

                Process.Start(CurrentModification.Folder + "\\" + CurrentModification.Info.Documentation);
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
            }
        }

        private void button_OpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(CurrentModification.Info.Website);
        }

        private void button_ShowActivationInformation_Click(object sender, RoutedEventArgs e)
        {
            ModificationStatusInformationDialog dlg = new ModificationStatusInformationDialog();
            dlg.SetModification(CurrentModification);
            dlg.ShowDialog();
        }
    }
}
