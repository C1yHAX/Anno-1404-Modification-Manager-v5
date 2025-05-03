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
using AnnoModificationManager4.ModificationTypes;
using System.ComponentModel;
using AnnoModificationManager4.Misc;
using RDAExplorer;
using System.Threading;
using AnnoModificationManager4.UserInterface.Misc;

namespace AnnoModificationManager4.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for ApplyChangesWindow.xaml
    /// </summary>
    public partial class ApplyChangesWindow : Window
    {
        private bool _IsRunning = false;
        private BackgroundWorker wrk;
        private RDAWriter _LastRDAWriter;

        public ApplyChangesWindow()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            prg_AllOperations.Maximum = Modification.AMMRDA._RequestTypes.Count;
            button_OK.IsEnabled = false;
            button_Cancel.IsEnabled = false;
            this.Closing += new CancelEventHandler(ApplyChangesWindow_Closing);
            _IsRunning = true;

            wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            wrk.DoWork += new DoWorkEventHandler(wrk_DoWork);
            wrk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(wrk_RunWorkerCompleted);
            wrk.ProgressChanged += new ProgressChangedEventHandler(wrk_ProgressChanged);

            wrk.RunWorkerAsync();
        }

        void ApplyChangesWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = _IsRunning;
        }

        void wrk_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lbl_Status.Dispatch(l => l.Content = _LastRDAWriter.UI_LastMessage);
            prg_CurrentOperation.Dispatch(prg => prg.Value = e.ProgressPercentage);
        }

        void wrk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _IsRunning = false;
            Modification.AMMRDA.Clear();
            DialogResult = true;
        }

        void wrk_DoWork(object sender, DoWorkEventArgs e)
        {
            //return;
            Dictionary<string, string> _archivecouples = new Dictionary<string, string>();

            try
            {
                foreach (var rdafile in Modification.AMMRDA._RequestTypes.Keys)
                {
                    lbl_Status.Dispatch(l => l.Content = "Creating archive \"" + Path.GetFileName(rdafile.FileName) + "\"");
                    prg_CurrentOperation.Dispatch(p => p.Value = 0);

                    //Set filename
                    string dstfile = FileExtension.Unify(rdafile.FileName);

                    //Create new archive
                    var folder = RDAFolder.GenerateFrom(rdafile.rdaFileEntries);
                    RDAWriter wr = new RDAWriter(folder);
                    _LastRDAWriter = wr;
                    wr.Write(dstfile, false, wrk);

                    //Add couple
                    _archivecouples.Add(rdafile.FileName, dstfile);

                    //Remove from list
                    lv_RDAFilesPending.Dispatch(l => l.Items.RemoveAt(0));

                    //Progress
                    prg_AllOperations.Dispatch(p => p.Value++);
                }
            }
            catch (Exception ex)
            {
                this.Dispatch(() => MessageWindow.Show(ex.Message));

                lbl_Status.Dispatch(l => l.Content = "Rollback ...");
                foreach (var couple in _archivecouples)
                {
                    File.Delete(couple.Value);
                }

                return;
            }

            lbl_Status.Dispatch(l => l.Content = "Copying modified archives ...");

            {
                Dictionary<string, string> _backuparchivecouples = new Dictionary<string, string>();

                //Clear
                RDAManagerExtension.Clear();
                //Free RDA handlers
                Modification.RDAManager.DisposeAll();

                try
                {
                    foreach (var couple in _archivecouples)
                    {
                        lbl_Status.Dispatch(l => l.Content = "Backuping old archive ... " + Path.GetFileName(couple.Key));

                        string newfilenameforold = FileExtension.Unify(couple.Key);

                        File.Move(couple.Key, newfilenameforold);

                        //Add to rollback
                        _backuparchivecouples.Add(newfilenameforold, couple.Key);

                        lbl_Status.Dispatch(l => l.Content = "Waiting ... " + Path.GetFileName(couple.Key));

                        while (File.Exists(couple.Key))
                        {
                            Thread.Sleep(100);
                        }

                        lbl_Status.Dispatch(l => l.Content = "Copying new archive ... " + Path.GetFileName(couple.Value));
                        File.Move(couple.Value, couple.Key);
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatch(() => MessageWindow.Show(ex.Message));

                    lbl_Status.Dispatch(l => l.Content = "Rollback ...");
                    foreach (var couple in _backuparchivecouples)
                    {
                        if (File.Exists(couple.Value))
                            File.Delete(couple.Value);

                        while (File.Exists(couple.Value))
                        {
                            Thread.Sleep(100);
                        }

                        File.Move(couple.Key, couple.Value);
                    }

                    return;
                }

                lbl_Status.Dispatch(l => l.Content = "Deleting backup files ...");

                foreach (var couple in _backuparchivecouples)
                {
                    if (File.Exists(couple.Key))
                        File.Delete(couple.Key);
                }
            }

            lbl_Status.Dispatch(l => l.Content = "Finished.");
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (!_IsRunning)
                DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in Modification.AMMRDA._RequestTypes.Keys)
            {
                lv_RDAFilesPending.Items.Add(Path.GetFileName(Path.GetDirectoryName(item.FileName)) + "\\" + Path.GetFileName(item.FileName));
            }
        }
    }
}
