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
using AnnoModificationManager5.Controls;
using AnnoModificationManager5.Misc;

namespace AnnoModificationManager5.UserInterface.Misc
{
    /// <summary>
    /// Interaction logic for FolderBrowser.xaml
    /// </summary>
    public partial class FolderBrowser : Window
    {
        private string _RootFolder = "C:\\";
        public string RootFolder
        {
            get
            {
                return _RootFolder;
            }
            set
            {
                _RootFolder = value;
                RefreshList();
            }
        }

        public List<string> SelectedItems { get; set; }
        private List<ModifiedTreeViewItem> AllItems = new List<ModifiedTreeViewItem>();

        public bool Multiselect { get; set; }

        private bool _ShowFiles=false;
        public bool ShowFiles
        {
            get
            {
                return _ShowFiles;
            }
            set
            {
                _ShowFiles = value;
                RefreshList();
            }
        }

        public List<string> GetFiles
        {
            get
            {
                List<string> o = new List<string>();

                foreach (string f in SelectedItems)
                {
                    if (File.Exists(f))
                    {
                        o.Add(f);
                    }
                    else
                    {
                        o.AddRange(Directory.GetFiles(f, "*", SearchOption.AllDirectories));
                    }
                }

                return o.Distinct().ToList();
            }
        }

        public FolderBrowser()
        {
            SelectedItems = new List<string>();
            Multiselect = false;

            InitializeComponent();
        }

        public void RefreshList()
        {
            AllItems.Clear();
            folderList.Items.Clear();

            foreach (string folder in Directory.GetDirectories(_RootFolder, "*"))
            {
                ModifiedTreeViewItem it = GenerateItems(folder);
                folderList.Items.Add(it);
                AllItems.Add(it);
            }

            if (ShowFiles)
            {
                foreach (string folder in Directory.GetFiles(_RootFolder, "*"))
                {
                    ModifiedTreeViewItem it = new ModifiedTreeViewItem();
                    it.Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/page_white.png",
               Path.GetFileName(folder));
                    it.SemanticValue = folder;

                    folderList.Items.Add(it);
                    AllItems.Add(it);
                }
            }
        }

        private ModifiedTreeViewItem GenerateItems(string folder)
        {
            ModifiedTreeViewItem item = new ModifiedTreeViewItem();
            item.Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/folder.png",
                Path.GetFileName(folder));
            item.SemanticValue = folder;

            foreach (string f in Directory.GetDirectories(folder, "*"))
            {
                ModifiedTreeViewItem it = GenerateItems(f);
                item.Items.Add(it);
                AllItems.Add(it);
            }

            if (ShowFiles)
            {
                foreach (string f in Directory.GetFiles(folder, "*"))
                {
                    ModifiedTreeViewItem it = new ModifiedTreeViewItem();
                    it.Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/page_white.png",
               Path.GetFileName(f));
                    it.SemanticValue = f;

                    item.Items.Add(it);
                    AllItems.Add(it);
                }
            }

            return item;
        }

        private void UpdateSelections()
        {
            foreach (ModifiedTreeViewItem item in AllItems)
            {
                if (SelectedItems.Contains(item.SemanticValue))
                {
                    item.FontWeight = FontWeights.Bold;
                }
                else
                    item.FontWeight = FontWeights.Normal;
            }
        }

        private void AddFolderTreeViewItemToSelection(ModifiedTreeViewItem mod)
        {
            mod.FontWeight = FontWeights.Bold;
            SelectedItems.Add(mod.SemanticValue);

            foreach (ModifiedTreeViewItem itm in mod.Items)
            {
                AddFolderTreeViewItemToSelection(itm);
            }
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (folderList.SelectedItem != null)
            {
                DialogResult = true;
            }
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void folderList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (folderList.SelectedItem != null)
            {
                string sel = "";

                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (!SelectedItems.Contains((folderList.SelectedItem as ModifiedTreeViewItem).SemanticValue))
                    {
                        sel = ((folderList.SelectedItem as ModifiedTreeViewItem).SemanticValue);
                    }
                }
                else
                {
                    SelectedItems.Clear();
                    sel = ((folderList.SelectedItem as ModifiedTreeViewItem).SemanticValue);
                }

                //SelectedItems.Add(sel);
                AddFolderTreeViewItemToSelection(AllItems.Find(i => i.SemanticValue == sel));

                UpdateSelections();
            }
        }
    }
}
