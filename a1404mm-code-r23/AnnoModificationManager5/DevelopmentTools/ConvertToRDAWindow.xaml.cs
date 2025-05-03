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
using AnnoModificationManager5.ModificationTypes;
using System.IO;
using AnnoModificationManager5.Misc;
using RDAExplorer;
using System.Threading;
using System.ComponentModel;
using AnnoModificationManager5.Components;
using xmlmod = AnnoModificationManager5.ModificationTypes.XmlModule;
using listmod = AnnoModificationManager5.ModificationTypes.ListModule;

namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for ConvertToRDAWindow.xaml
    /// </summary>
    public partial class ConvertToRDAWindow : Window
    {
        public Modification _ToConvert;
        public string _ConvertedProjectFolder;
        private List<string> _RDAFiles = new List<string>();
        private bool _RDAAutoAssing = false;

        public ConvertToRDAWindow()
        {
            InitializeComponent();
        }

        private void b_OK_Click(object sender, RoutedEventArgs e)
        {
            Convert();
        }

        private void Convert()
        {
            //Check rda dir
            if (!SettingsWindow.CheckRDAWorkingDir())
            {
                MessageBox.Show("Operation canceled.");
                DialogResult = false;
                return;
            }

            //Open rdas
            OpenRDAs();

            #region Convert modification
            #region Prepare
            string srcdir = _ToConvert.Folder;
            string tmpdir = _ToConvert.Folder + "_RDA" + (chk_SupportAddon.IsChecked == true ? "_ADDON" : ":");

            _ConvertedProjectFolder = tmpdir;

            Directory.CreateDirectory(tmpdir);
            #endregion
            #region Convert xmlm, lm
            #region Prepare operation, find assignments
            Dictionary<string, string> _RDAAssignments = new Dictionary<string, string>();

            foreach (var file in _ToConvert.CollectedFiles_List_List)
            {
                string fx = file.ToLower();

                if (fx.StartsWith("%anno%") || !fx.Contains('%'))
                {
                    if (fx.StartsWith("%anno%"))
                        fx = fx.Remove(0, 7);
                    var foundrda = GetRDAFolderFor(fx);

                    if (string.IsNullOrEmpty(foundrda))
                    {
                        DialogResult = false;
                        return;
                    }

                    _RDAAssignments.Add(file, foundrda);
                }
            }

            foreach (var file in _ToConvert.CollectedFiles_Xml_List)
            {
                string fx = file.ToLower();

                if (fx.StartsWith("%anno%") || !fx.Contains('%'))
                {
                    if (fx.StartsWith("%anno%"))
                        fx = fx.Remove(0, 7);
                    var foundrda = GetRDAFolderFor(fx);

                    if (string.IsNullOrEmpty(foundrda))
                    {
                        DialogResult = false;
                        return;
                    }

                    _RDAAssignments.Add(file, foundrda);
                }
            }
            #endregion
            foreach (var module in _ToConvert.XmlModules)
            {
                var list = module.Get();
                int toeditcount = list.Count;

                for (int i = 0; i < toeditcount; i++)
                {
                    string found;

                    xmlmod.XMLModifiers.IXMLModifier mod = list[i];

                    string fx = mod.File.ToLower();
                    if (fx.StartsWith("%anno%"))
                        fx = fx.Remove(0, 7);

                    _RDAAssignments.TryGetValue(mod.File, out found);

                    if (!string.IsNullOrEmpty(found))
                    {
                        mod.File = found + ";" + fx;
                    }
                }
            }

            foreach (var module in _ToConvert.ListModules)
            {
                var list = module.Get();
                int toeditcount = list.Count;

                for (int i = 0; i < toeditcount; i++)
                {
                    string found;

                    listmod.ListModifiers.IListModifier mod = list[i];

                    string fx = mod.File.ToLower();
                    if (fx.StartsWith("%anno%"))
                        fx = fx.Remove(0, 7);

                    _RDAAssignments.TryGetValue(mod.File, out found);

                    if (!string.IsNullOrEmpty(found))
                    {
                        mod.File = found + ";" + fx;
                    }
                }
            }
            #endregion
            #region Save to folder
            //_ToConvert.Info.RDAIgnoreNonExistingRDAMods = chb_IgnoreNonExistingRDAs.IsChecked.Value;
            _ToConvert.Info.SupportsAMM4RDA = true;
            _ToConvert.SaveFolder(tmpdir);
            #endregion
            #region Convert files
            //Copy images
            DirectoryExtension.copyDirectory(srcdir + "\\Images", tmpdir + "\\Images");

            //Copy appdata
            DirectoryExtension.copyDirectory(srcdir + "\\Files\\AppData", tmpdir + "\\Files\\AppData");

            //Copy Anno1404
            foreach (string srcfile in Directory.GetFiles(srcdir + "\\Files\\Anno1404", "*", SearchOption.AllDirectories))
            {
                string rootedsrcfile = srcfile.Replace(srcdir + "\\Files\\Anno1404", "");
                string foundrda = GetRDAFolderFor(rootedsrcfile);

                if (string.IsNullOrEmpty(foundrda))
                {
                    DialogResult = false;
                    return;
                }

                string dst = tmpdir + "\\Files\\Anno1404\\" + foundrda + "\\" + rootedsrcfile.Trim('\\', '/');
                if (!Directory.Exists(Path.GetDirectoryName(dst)))
                    Directory.CreateDirectory(Path.GetDirectoryName(dst));

                File.Copy(srcfile, dst, true);
            }
            #endregion
            #endregion

            DialogResult = true;
        }

        private void OpenRDAs()
        {
            txb_Output.Text += "\nReading RDAs in working directory ...";

            _RDAFiles = new List<string>();
            _RDAFiles.AddRange(Directory.GetFiles(Properties.Settings.Default.RDAWorkingDir + "\\maindata", "*.rda", SearchOption.TopDirectoryOnly));

            if (AnnoVersionHandler.IsAddonCompatible(_ToConvert.Info))
            {
                var it = Directory.GetFiles(Properties.Settings.Default.RDAWorkingDir + "\\addon", "*.rda", SearchOption.TopDirectoryOnly);
                _RDAFiles.AddRange(it);
            }

            _RDAFiles = RDAManager.OrderRDAs(_RDAFiles, (chk_SupportAddon.IsChecked == true ? 1 : -1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">Rooted</param>
        /// <returns></returns>
        private List<string> GetRDAFilesFor(string file, bool allowmultiple)
        {
            file = file.ToLower().Trim('\\', '/').Replace("/", "\\");

            List<string> output = new List<string>();

            //Look for file in RDAs
            if (!allowmultiple)
            {
                RDAReader lookup = Modification.RDAManager.LookForFileIn(file, _RDAFiles);
                if (lookup != null)
                    output.Add(Path.GetFileName(Path.GetDirectoryName(lookup.FileName)).Trim('\\') + "\\" + Path.GetFileName(lookup.FileName));
            }
            else
            {
                foreach (var lookup in Modification.RDAManager.LookForFilesIn(file, _RDAFiles))
                {
                    if (lookup != null)
                        output.Add(Path.GetFileName(Path.GetDirectoryName(lookup.FileName)).Trim('\\') + "\\" + Path.GetFileName(lookup.FileName));
                }
            }

            if (output.Count == 0)
            {
                if (!_RDAAutoAssing)
                {
                    //Ask the user
                    ConvertToRDASelectManuallyWindow dlg = new ConvertToRDASelectManuallyWindow();
                    dlg.Prepare(_RDAFiles, file);

                    if (dlg.ShowDialog() == true)
                    {
                        output.Add(dlg.SelectedRDA);
                        _RDAAutoAssing = dlg.EnableAutoAssign;
                    }
                }
                else
                {
                    if (file.ToLower().StartsWith("data"))
                    {
                        output.Add("maindata");
                    }
                    else
                    {
                        output.Add("addon");
                    }
                }
            }

            return output;
        }

        private string GetRDAFolderFor(string file)
        {
            var list = GetRDAFilesFor(file, false);

            if (list.Count == 0)
                return string.Empty;

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].Split('\\')[0];
            }

            return list[0];
        }
    }
}
