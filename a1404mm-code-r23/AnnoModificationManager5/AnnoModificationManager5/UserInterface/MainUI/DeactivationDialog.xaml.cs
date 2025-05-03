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
using AnnoModificationManager5.Components.ModificationHelpers;
using AnnoModificationManager5.Misc;
using System.Threading;
using AnnoModificationManager5.Components;
using System.ComponentModel;
using AnnoModificationManager5.Language.DictionarySystem;

namespace AnnoModificationManager5.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for DeactivationDialog.xaml
    /// </summary>
    public partial class DeactivationDialog : Window
    {
        public Modification CurrentModification;
        private List<ConflictResponse> Conflicts = new List<ConflictResponse>();

        public DeactivationDialog()
        {
            InitializeComponent();
        }

        public void LoadModification(Modification mod)
        {
            CurrentModification = mod;

            //Load Conflicts
            Thread tr = new Thread(new ParameterizedThreadStart(delegate
            {
                Conflicts.Add(new ConflictResponse()
                {
                    CurrentModification = mod,
                    ToCheck = mod,
                    ConflictFiles_Anno = mod.ModificationUtils.Files_Anno,
                    ConflictFiles_AppData = mod.ModificationUtils.Files_AppData,
                    ConflictXmlModules = mod.ModificationUtils.Xml_AllModififers,
                    ConflictListModules = mod.ModificationUtils.List_AllModififers
                });

                foreach (Modification modification in ModificationHandler.Modifications)
                {
                    if (modification == mod)
                        continue;
                    if (ModificationHandler.ActivationResponses[modification].Result() == Enums.Modification_ActivationStatus.Deactivated)
                        continue;
                    ConflictResponse response = ConflictResponse.Generate(mod, modification);
                    if (response.IsConflict())
                        Conflicts.Add(response);
                }

                Application.Current.Dispatch(app => ConflictsLoaded());
            }));
            tr.Start();
        }

        private void ConflictsLoaded()
        {
            button_OK.IsEnabled = true;
            button_Cancel.IsEnabled = true;
            conflictMods_Loading.Visibility = System.Windows.Visibility.Hidden;

            list_ConflictMods.ItemsSource = Conflicts;
            list_ConflictMods.Items.Refresh();
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            button_Cancel.IsEnabled = false;
            button_OK.IsEnabled = false;

            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            wrk.DoWork += new DoWorkEventHandler(wrk_DoWork);
            wrk.ProgressChanged += new ProgressChangedEventHandler(wrk_ProgressChanged);
            wrk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(wrk_RunWorkerCompleted);
            wrk.RunWorkerAsync();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
                RestoreItem.CreateItem(LanguageDictionary.Get("DeactivationDialog", "Feature_CreateRestorePoint_ItemName").
                    Replace("{0}", CurrentModification.UICollector.Name).Replace("{1}", CurrentModification.UICollector.VersionString));
            }

            wrk.ReportProgress(0);
            CurrentModification.Deactivate(wrk);
        }
    }
}
