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
using AnnoModificationManager5.Language.DictionarySystem;
using AnnoModificationManager5.Components.ModificationHelpers;
using System.Threading;
using AnnoModificationManager5.Components;
using AnnoModificationManager5.Misc;
using System.ComponentModel;

namespace AnnoModificationManager5.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for ActivationDialog.xaml
    /// </summary>
    public partial class ActivationDialog : Window
    {
        public Modification CurrentModification;
        private List<ConflictResponse> Conflicts = new List<ConflictResponse>();

        public ActivationDialog()
        {
            WindowExtension.Append(WindowExtension.ExtensionType.RemoveCloseButton, this);
            InitializeComponent();
        }

        public void LoadModification(Modification mod)
        {
            CurrentModification = mod;

            //Fill into information
            field_Name.Text = mod.UICollector.Name;
            field_Version.Text = mod.UICollector.VersionString;

            #region Files_Anno
            if (CurrentModification.Files_Anno_Count != 0)
            {
                field_Files_Anno.Text = LanguageDictionary.Get("ActivationDialog", "Field_Files_Anno")
                    .Replace("{0}", CurrentModification.Files_Anno_Count.ToString());
            }
            else
                (field_Files_Anno.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed; 
            #endregion
            #region Files_AppData
            if (CurrentModification.Files_AppData_Count != 0)
            {
                field_Files_Anno.Text = LanguageDictionary.Get("ActivationDialog", "Field_Files_AppData")
                    .Replace("{0}", CurrentModification.Files_AppData_Count.ToString());
            }
            else
                (field_Files_AppData.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;
            #endregion
            #region XmlModules
            if (CurrentModification.XmlModules_Count != 0)
            {
                field_XmlModules.Text = LanguageDictionary.Get("ActivationDialog", "Field_XmlModules")
                    .Replace("{0}", mod.XmlModules_Count.ToString());
            }
            else
                (field_XmlModules.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;
            #endregion
            #region ListModules
            if (CurrentModification.ListModules_Count != 0)
            {
                field_ListModules.Text = LanguageDictionary.Get("ActivationDialog", "Field_ListModules")
                    .Replace("{0}", mod.ListModules_Count.ToString());
            }
            else
                (field_ListModules.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;
            #endregion
            #region UValues
            if (CurrentModification.UserdefinedValues.Count != 0)
            {
                field_UValues.Text = LanguageDictionary.Get("ActivationDialog", "Field_UValues")
                    .Replace("{0}", mod.UValues_Count.ToString());

                CollectionViewSource src = new CollectionViewSource();
                src.GroupDescriptions.Add(new PropertyGroupDescription("UI_Group"));
                src.Source = mod.UserdefinedValues;
                list_UserdefinedValues.SetBinding(ListView.ItemsSourceProperty, new Binding() { Source = src });
                list_UserdefinedValues.Items.Refresh();
            }
            else
            {
                (field_UValues.Parent as UIElement).Visibility = System.Windows.Visibility.Collapsed;
                tabItem_UValues.Visibility = System.Windows.Visibility.Hidden;
            }
            #endregion

            //Load Conflicts
            Thread tr = new Thread(new ParameterizedThreadStart(delegate
                {
                    try
                    {
                        foreach (Modification modification in ModificationHandler.Modifications)
                        {
                            try
                            {
                                if (modification == mod)
                                    continue;
                                ModificationActivationResponse mar;
                                if (!ModificationHandler.ActivationResponses.TryGetValue(modification, out mar) || mar == null)
                                    continue;
                                if (mar.Result() == Enums.Modification_ActivationStatus.Deactivated)
                                    continue;
                                ConflictResponse response = ConflictResponse.Generate(mod, modification);
                                if (response != null && response.IsConflict())
                                    Conflicts.Add(response);
                            }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception) { }

                    Application.Current.Dispatch(app => ConflictsLoaded());
                }));
            tr.Start();
        }

        private void ConflictsLoaded()
        {
            button_OK.IsEnabled = true;
            button_Cancel.IsEnabled = true;
            conflictMods_Loading.Visibility = System.Windows.Visibility.Hidden;
            if (Conflicts.Count == 0)
            {
                conflictContainer.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                list_ConflictMods.ItemsSource = Conflicts;
                list_ConflictMods.Items.Refresh();
            }
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (tabItem_UValues.Visibility == System.Windows.Visibility.Visible)
            {
                if (tabControl.SelectedItem != tabItem_UValues)
                {
                    tabControl.SelectedItem = tabItem_UValues;
                    return;
                }
            }

            //Save UValues
            if (CurrentModification.UserdefinedValues.Count != 0)
            {
                ModificationTypes.Userdefined.UserdefinedValue.Save(CurrentModification.Folder + "\\UserdefinedValues\\UserdefinedValues.xml",
                    CurrentModification.UserdefinedValues);
            }

            button_OK.Visibility = System.Windows.Visibility.Hidden;
            button_Cancel.Visibility = System.Windows.Visibility.Hidden;
            features_CreateRestorePoint.Visibility = System.Windows.Visibility.Hidden;
            tabControl.SelectedItem = tabControl_Information;
            progress_Activation.Visibility = System.Windows.Visibility.Visible;

            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            wrk.DoWork += new DoWorkEventHandler(wrk_DoWork);
            wrk.ProgressChanged += new ProgressChangedEventHandler(wrk_ProgressChanged);
            wrk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(wrk_RunWorkerCompleted);
            wrk.RunWorkerAsync();
        }

        void wrk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            App.Current.Dispatch(app =>
                {
                    DialogResult = true;
                });
        }

        void wrk_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            App.Current.Dispatch(app =>
                {
                    progress_Activation.Value = e.ProgressPercentage;
                });
        }

        void wrk_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker wrk = sender as BackgroundWorker;

            //Create restore point, if whished
            
            if (App.Current.Dispatch(app => features_CreateRestorePoint.IsChecked.Value))
            {
                RestoreItem.CreateItem(LanguageDictionary.Get("ActivationDialog", "Feature_CreateRestorePoint_ItemName").
                    Replace("{0}", CurrentModification.UICollector.Name).Replace("{1}", CurrentModification.UICollector.VersionString));
            }

            if (Conflicts.Count != 0 && App.Current.Dispatch(app => conflict_DeactivateIncompatible.IsChecked.Value))
            {
                wrk.ReportProgress(0);
                int count = 0;

                foreach (ConflictResponse resp in Conflicts)
                {
                    count++;
                    wrk.ReportProgress(ProgressBarExtension.Calculate(count, Conflicts.Count));
                    resp.CurrentModification.Deactivate();
                }
            }

            wrk.ReportProgress(0);
            CurrentModification.Activate(wrk);
        }
    }
}
