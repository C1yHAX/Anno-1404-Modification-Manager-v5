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
using System.IO;

namespace AnnoModificationManager5.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for DeactivationDialog.xaml
    /// </summary>
    public partial class DeleteDialog : Window
    {
        public Modification CurrentModification;
        private List<ConflictResponse> Conflicts = new List<ConflictResponse>();

        public DeleteDialog()
        {
            InitializeComponent();
        }

        public void LoadModification(Modification mod)
        {
            CurrentModification = mod;

            field_Name.Text = mod.UICollector.Name;
            field_Version.Text = mod.UICollector.VersionString;

            //Load Conflicts
            Thread tr = new Thread(new ParameterizedThreadStart(delegate
            {
                try
                {
                    if (mod.CheckActivation().Result() != Enums.Modification_ActivationStatus.Deactivated)
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
                    }
                }
                catch (Exception) { }

                try
                {
                    //Only if is COMPATIBLE!!!
                    if (ModificationHandler.Instance.IsCompatible(mod))
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

            list_ConflictMods.ItemsSource = Conflicts;
            list_ConflictMods.Items.Refresh();

            if (Conflicts.Count == 0)
            {
                list_ConflictMods.Visibility = System.Windows.Visibility.Hidden;
                deleteMessage.Text = LanguageDictionary.Get("DeleteDialog", "DeleteOnlyMessage");
            }
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            button_OK.IsEnabled = false;
            button_Cancel.IsEnabled = false;

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

            wrk.ReportProgress(0);
            //Only if is COMPATIBLE!!!
            if (ModificationHandler.Instance.IsCompatible(CurrentModification) && ModificationHandler.ActivationResponses[CurrentModification].Result() != Enums.Modification_ActivationStatus.Deactivated)
            {
                CurrentModification.Deactivate(wrk);
            }
            Directory.Delete(CurrentModification.Folder, true);
        }
    }
}
