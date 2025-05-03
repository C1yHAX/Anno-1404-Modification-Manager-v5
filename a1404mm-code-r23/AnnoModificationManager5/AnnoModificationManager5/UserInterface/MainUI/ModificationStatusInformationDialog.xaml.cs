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
using AnnoModificationManager5.ModificationTypes;
using AnnoModificationManager5.Components;
using AnnoModificationManager5.Language.DictionarySystem;

namespace AnnoModificationManager5.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for ModificationStatusInformationDialog.xaml
    /// </summary>
    public partial class ModificationStatusInformationDialog : Window
    {
        public Modification CurrentModification;
        public ModificationActivationResponse CurrentActivationResponse;

        public ModificationStatusInformationDialog()
        {
            InitializeComponent();
        }

        public void SetModification(Modification mod)
        {
            CurrentModification = mod;
            CurrentActivationResponse = ModificationHandler.ActivationResponses[mod];
            field_ActivationLog.Text = CurrentActivationResponse.Log.ToString();

            //Fill into information
            field_Name.Text = mod.UICollector.Name;
            field_Version.Text = mod.UICollector.VersionString;

            #region Files_Anno
            if (CurrentActivationResponse.FileModuleAnnoCount != 0)
            {
                field_Files_Anno.Text = LanguageDictionary.Get("ActivationDialog", "Field_Files_Anno")
                    .Replace("{0}", CurrentModification.Files_Anno_Count.ToString());
                field_Files_Anno_Progress.Maximum = CurrentActivationResponse.FileModuleAnnoCount;
                field_Files_Anno_Progress.Value = CurrentActivationResponse.FileModuleAnnoActive;

                field_Files_Anno_Status.Text =
                    CurrentActivationResponse.FileModuleAnnoActive + "/" + CurrentActivationResponse.FileModuleAnnoCount;
            }
            else
                (field_Files_Anno.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;
            #endregion
            #region Files_AppData
            if (CurrentActivationResponse.FileModuleAppDataCount != 0)
            {
                field_Files_Anno.Text = LanguageDictionary.Get("ActivationDialog", "Field_Files_AppData")
                    .Replace("{0}", CurrentModification.Files_AppData_Count.ToString());
                field_Files_AppData_Progress.Maximum = CurrentActivationResponse.FileModuleAppDataCount;
                field_Files_AppData_Progress.Value = CurrentActivationResponse.FileModuleAppDataActive;

                field_Files_AppData_Status.Text =
                    CurrentActivationResponse.FileModuleAppDataActive + "/" + CurrentActivationResponse.FileModuleAppDataCount;
            }
            else
                (field_Files_AppData.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;
            #endregion
            #region XmlModules
            if (CurrentActivationResponse.XmlModuleCount != 0)
            {
                field_XmlModules.Text = LanguageDictionary.Get("ActivationDialog", "Field_XmlModules")
                    .Replace("{0}", mod.XmlModules_Count.ToString());
                field_XmlModules_Progress.Maximum = CurrentActivationResponse.XmlModuleCount;
                field_XmlModules_Progress.Value = CurrentActivationResponse.XmlModuleActive;

                field_XmlModules_Status.Text =
                    CurrentActivationResponse.XmlModuleActive + "/" + CurrentActivationResponse.XmlModuleCount;
            }
            else
                (field_XmlModules.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;
            #endregion
            #region ListModules
            if (CurrentActivationResponse.ListModuleCount != 0)
            {
                field_ListModules.Text = LanguageDictionary.Get("ActivationDialog", "Field_ListModules")
                    .Replace("{0}", mod.ListModules_Count.ToString());
                field_ListModules_Progress.Maximum = CurrentActivationResponse.ListModuleCount;
                field_ListModules_Progress.Value = CurrentActivationResponse.ListModuleActive;

                field_ListModules_Status.Text =
                    CurrentActivationResponse.ListModuleActive + "/" + CurrentActivationResponse.ListModuleCount;
            }
            else
                (field_ListModules.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;
            #endregion
            #region UValues
            if (CurrentModification.UserdefinedValues.Count != 0)
            {
                field_UValues.Text = LanguageDictionary.Get("ActivationDialog", "Field_UValues")
                    .Replace("{0}", mod.UValues_Count.ToString());
            }
            else
            {
                (field_UValues.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;               
            }
            #endregion
        }
    }
}
