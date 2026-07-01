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
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.Components;
using Ionic.Zip;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.Language.DictionarySystem;
using AnnoModificationManager5.ModificationTypes;

namespace AnnoModificationManager5.UserInterface.MainUI
{
    /// <summary>
    /// Interaction logic for RestoreManager.xaml
    /// </summary>
    public partial class RestoreManager : UserControl
    {
        public bool HasRestored = false;

        public RestoreManager()
        {
            InitializeComponent();
        }

        /// <summary>Reload the restore point list (called on load and when the view is shown).</summary>
        public void ReloadItems()
        {
            list_Restore.ItemsSource = RestoreItem.GetItems();
            list_Restore.Items.Refresh();
        }

        private void button_Restore_Click(object sender, RoutedEventArgs e)
        {
            if (list_Restore.SelectedItem != null)
            {
                RestoreItem rest = list_Restore.SelectedItem as RestoreItem;

                if (rest.File != "<RDA>")
                {
                    if (Directory.Exists(AnnoDirectoryHandler.GetCurrent() + "\\data") && rest.DeleteData)
                        Directory.Delete(AnnoDirectoryHandler.GetCurrent() + "\\data", true);
                    if (Directory.Exists(AnnoDirectoryHandler.GetCurrent() + "\\addondata") && rest.DeleteAddon)
                        Directory.Delete(AnnoDirectoryHandler.GetCurrent() + "\\addondata", true);

                    if (!string.IsNullOrEmpty(rest.File))
                    {
                        ZipFile zip = new ZipFile(rest.File);
                        zip.ExtractAll(AnnoDirectoryHandler.GetCurrent(), ExtractExistingFileAction.OverwriteSilently);
                    }

                    //Setting HasRestored
                    HasRestored = true;
                    //Clean all List- and XmlFiles for full Reload
                    AnnoModificationManager5.ModificationTypes.XmlModule.XMLFileCollector.CollectedFiles.Clear();
                    AnnoModificationManager5.ModificationTypes.ListModule.ListFileCollector.CollectedFiles.Clear();

                    // Refresh activation status after a restore (previously done by the caller
                    // that opened this as a dialog; now embedded, so trigger it here).
                    try { MainWindow.CurrentMainWindow.UpdateActivationResponses(); }
                    catch (Exception) { }

                    MessageWindow.Show(LanguageDictionary.Get("RestoreManager", "RestoredMessage"));
                }
                else
                {
                    if (MessageWindow.Show(LanguageDictionary.Get("RestoreManager", "QuestionRDA"), MessageWindow.MessageWindowType.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Modification.RDAManager.DisposeAll();

                            if (Directory.Exists(Modification.RDAPath + "\\maindata"))
                            {
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(
                                                       Modification.RDAPath + "\\maindata",
                                                        Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                                         Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
                                                          Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);
                            }
                            if (Directory.Exists(Modification.RDAPath + "\\addon"))
                            {
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(
                                  Modification.RDAPath + "\\addon",
                                   Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
                                     Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);
                            }
                            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(
                                Properties.Settings.Default.RDABackupDir,
                                Modification.RDAPath,
                                Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,
                                Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);

                            ApplicationExtension.RestartManager();
                        }
                        catch (Exception ex)
                        {
                            MessageWindow.Show(ex.Message);
                        }
                    }
                }
            }
        }

        private void button_Add_Click(object sender, RoutedEventArgs e)
        {
            string name = MessageWindow.GetText(LanguageDictionary.Get("RestoreManager", "AddMessage"), "Restore 1");

            if (!string.IsNullOrEmpty(name))
            {
                foreach (char ch in Path.GetInvalidPathChars())
                    name = name.Replace(ch.ToString(), "");
                name = name.Replace("\\", "").Replace("/", "");

                RestoreItem.CreateItem(name);
                list_Restore.ItemsSource = RestoreItem.GetItems();
                list_Restore.Items.Refresh();
            }
        }

        private void button_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (list_Restore.SelectedItem != null &&
                !(list_Restore.SelectedItem as RestoreItem).IsLocked)
            {
                RestoreItem rest = list_Restore.SelectedItem as RestoreItem;
                File.Delete(rest.File);

                list_Restore.ItemsSource = RestoreItem.GetItems();
                list_Restore.Items.Refresh();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadItems();
        }
    }

    public class RestoreItem
    {
        public bool IsLocked { get; set; }
        public bool DeleteData { get; set; }
        public bool DeleteAddon { get; set; }

        public string File { get; set; }
        public string Name { get; set; }
        public string AnnoVersion { get; set; }
        public DateTime Date { get; set; }
        public string DateString
        {
            get
            {
                return Date.ToString();
            }
        }

        public ImageSource GetIcon
        {
            get
            {
                if (IsLocked)
                {
                    return BitmapImageExtension.Load("pack://application:,,,/Images/Icons/folder_star_blue.png");
                }
                return BitmapImageExtension.Load("pack://application:,,,/Images/Icons/folder_star.png");
            }
        }

        public RestoreItem()
        {
            DeleteData = true;
            DeleteAddon = true;
        }

        public static List<RestoreItem> GetItems()
        {
            List<RestoreItem> items = new List<RestoreItem>();

            #region Locked Items
            //Add Restore maindata from backup
            items.Add(new RestoreItem()
            {
                File = "<RDA>",
                Date = DateTime.Now,
                AnnoVersion = "All",
                DeleteData = false,
                DeleteAddon = false,
                IsLocked = true,
                Name = LanguageDictionary.Get("RestoreManager", "Item_RestoreRDAs")
            });
            //Add Data Restore
            items.Add(new RestoreItem()
            {
                File = DirectoryExtension.GetApplicationFolder() + "\\data_restore.zip",
                Date = (new FileInfo("data_restore.zip")).LastWriteTime,
                AnnoVersion = "All",
                IsLocked = true,
                Name = LanguageDictionary.Get("RestoreManager", "Item_Original")
            });
            //Add Data Restore
            items.Add(new RestoreItem()
            {
                File = DirectoryExtension.GetApplicationFolder() + "\\data_restore.zip",
                Date = (new FileInfo("data_restore.zip")).LastWriteTime,
                AnnoVersion = "All",
                IsLocked = true,
                DeleteAddon = false,
                Name = LanguageDictionary.Get("RestoreManager", "Item_OriginalData")
            });
            //Add Remove Addon
            items.Add(new RestoreItem()
            {
                File = "",
                Date = (new FileInfo("data_restore.zip")).LastWriteTime,
                AnnoVersion = "All",
                DeleteData = false,
                DeleteAddon = true,
                IsLocked = true,
                Name = LanguageDictionary.Get("RestoreManager", "Item_DeleteAddondata")
            });
            //Add Restore Cursors
            items.Add(new RestoreItem()
            {
                File = DirectoryExtension.GetApplicationFolder() + "\\data_restore.zip",
                Date = (new FileInfo("data_restore.zip")).LastWriteTime,
                AnnoVersion = "All",
                DeleteData = false,
                DeleteAddon = false,
                IsLocked = true,
                Name = LanguageDictionary.Get("RestoreManager", "Item_RestoreCursors")
            });
            #endregion
            #region Restore Points
            if (Directory.Exists(DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\RestoreManager"))
            {
                foreach (string file in Directory.GetFiles(DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\RestoreManager"))
                {
                    if (Path.GetExtension(file).ToLower() == ".zip")
                    {
                        List<string> fileconnector = Path.GetFileNameWithoutExtension(file).Split('.').ToList();

                        RestoreItem item = new RestoreItem();
                        item.File = file;
                        item.Name = StringExtension.PutTogether(fileconnector.GetRange(0, fileconnector.Count - 1), '.');
                        item.AnnoVersion = fileconnector[fileconnector.Count - 1];
                        item.Date = (new FileInfo(file)).LastWriteTime;

                        items.Add(item);
                    }
                }
            }
            #endregion
            return items;
        }

        public static void CreateItem(string name)
        {
            //Prepare
            foreach (char ch in Path.GetInvalidPathChars())
                name = name.Replace(ch.ToString(), "");
            name = name.Replace("\\", "").Replace("/", "");

            if (string.IsNullOrEmpty(name))
                name = "Restore ";
            //Create
            if (!Directory.Exists(DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\RestoreManager"))
                Directory.CreateDirectory(DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\RestoreManager");
            string file = DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\RestoreManager\\" + name
                + "." + AnnoVersionHandler.GetCurrent() + ".zip";

            if (System.IO.File.Exists(file))
            {
                file = FileExtension.MakeFileUnique(file);
            }

            ZipFile zip = new ZipFile();
            if (Directory.Exists(AnnoDirectoryHandler.GetCurrent() + "\\data"))
                zip.AddDirectory(AnnoDirectoryHandler.GetCurrent() + "\\data", "data");
            if (Directory.Exists(AnnoDirectoryHandler.GetCurrent() + "\\addondata"))
                zip.AddDirectory(AnnoDirectoryHandler.GetCurrent() + "\\addondata", "addondata");
            zip.Save(file);
        }
    }
}
