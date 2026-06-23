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
using System.Windows.Shapes;
using AnnoModificationManager4.ModificationTypes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AnnoModificationManager4.Components;

namespace AnnoModificationManager4.MainControls.ModificationList
{
    /// <summary>
    /// Interaction logic for ModificationListControl.xaml
    /// </summary>
    public partial class ModificationListControl : UserControl
    {
        public event SelectionChangedEventHandler SelectionChanged;

        private object _SelectedItem;
        public object SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                _SelectedItem = value;

                list_Retail.SelectedItem = null;
                list_Addon.SelectedItem = null;
                list_AllVersions.SelectedItem = null;

                if (value != null)
                {
                    if (list_AllVersions.Items.Contains(value))
                        list_AllVersions.SelectedItem = value;
                    else if (list_Retail.Items.Contains(value))
                        list_Retail.SelectedItem = value;
                    else if (list_Addon.Items.Contains(value))
                        list_Addon.SelectedItem = value;
                }

                if (SelectionChanged != null)
                    SelectionChanged(this, null);
            }
        }

        public int SelectedIndex
        {
            get
            {
                if (SelectedItem != null)
                    return Items.IndexOf(SelectedItem);
                return -1;
            }
            set
            {
                SelectedItem = Items[value];
            }
        }
        public List<object> Items
        {
            get
            {
                List<object> items = new List<object>();
                items.AddRange(list_AllVersions.Items.OfType<object>());
                items.AddRange(list_Retail.Items.OfType<object>());
                items.AddRange(list_Addon.Items.OfType<object>());

                return items;
            }
        }

        public ModificationListControl()
        {
            InitializeComponent();
        }

        #region UI
        public void SetUIToLoading()
        {
            Expander_Loading.Visibility = System.Windows.Visibility.Visible;
            Expander_All.Visibility = System.Windows.Visibility.Collapsed;
            Expander_Retail.Visibility = System.Windows.Visibility.Collapsed;
            Expander_Addon.Visibility = System.Windows.Visibility.Collapsed;
            Expander_IAAM.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void SetUIToDefault()
        {
            Expander_Loading.Visibility = System.Windows.Visibility.Collapsed;
            Expander_All.Visibility = System.Windows.Visibility.Visible;
            Expander_Retail.Visibility = System.Windows.Visibility.Visible;
            Expander_Addon.Visibility = System.Windows.Visibility.Visible;
            Expander_IAAM.Visibility = System.Windows.Visibility.Visible;

            //Clear dev
            list_Loading.ItemsSource = null;
            list_Loading.Items.Refresh();
        }

        public void UpdateSelectionUI(MainWindow window)
        {
            object selected = SelectedItem;

            if (selected != null && Items.Contains(selected))
                SelectedItem = selected;
            else if (Items.Count != 0)
                SelectedIndex = 0;
            else
                window.analyzerPanel.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion
        #region Refresh
        public void Refresh(bool UpdateDescriptions)
        {
            SetUIToDefault();

            RefreshSection(list_AllVersions,
                ModificationHandler.Modifications.FindAll(m => m.UICollector.AnnoExecutableInteger == 2),
                UpdateDescriptions);
            RefreshSection(list_Retail,
                ModificationHandler.Modifications.FindAll(m => m.UICollector.AnnoExecutableInteger == 0),
                UpdateDescriptions);
            RefreshSection(list_Addon,
                ModificationHandler.Modifications.FindAll(m => m.UICollector.AnnoExecutableInteger == 1),
                UpdateDescriptions);
            RefreshSection(list_IAAM,
               ModificationHandler.Modifications.FindAll(m => m.UICollector.AnnoExecutableInteger == 3),
               UpdateDescriptions);
        }

        public void RefreshLoading()
        {
            list_Loading.ItemsSource = new ObservableCollection<Modification>(ModificationHandler.Modifications);
            list_Loading.Items.Refresh();
        }

        private void RefreshSection(ListView view, List<Modification> mod, bool UpdateDescriptions)
        {
            CollectionViewSource vsrc;
            GridViewColumn modificationList_Column_Category;
            GridViewColumn modificationList_Column_Author;

            #region Set vsrc and Column
            if (view == list_AllVersions)
            {
                vsrc = Resources["List_All_ViewSource"] as CollectionViewSource;
                modificationList_Column_Category = list_All_Column_Category;
                modificationList_Column_Author = list_All_Column_Author;
            }
            else if (view == list_Retail)
            {
                vsrc = Resources["List_Retail_ViewSource"] as CollectionViewSource;
                modificationList_Column_Category = list_Retail_Column_Category;
                modificationList_Column_Author = list_Retail_Column_Author;
            }
            else if (view == list_IAAM)
            {
                vsrc = Resources["List_IAAM_ViewSource"] as CollectionViewSource;
                modificationList_Column_Category = list_IAAM_Column_Category;
                modificationList_Column_Author = list_IAAM_Column_Author;
            }
            else
            {
                vsrc = Resources["List_Addon_ViewSource"] as CollectionViewSource;
                modificationList_Column_Category = list_Addon_Column_Category;
                modificationList_Column_Author = list_Addon_Column_Author;
            }
            #endregion
            #region Update
            vsrc.Source = null;
            vsrc.Source = new ObservableCollection<Modification>(mod);

            if (UpdateDescriptions)
            {
                vsrc.GroupDescriptions.Clear();
                vsrc.SortDescriptions.Clear();
                vsrc.GroupDescriptions.Add(new UserInterface.Group.ModificationGroupDescription());

                modificationList_Column_Category.Width = 120;
                modificationList_Column_Author.Width = 200;

                switch (Properties.Settings.Default.modificationList_SortProperty)
                {
                    case "Category":
                        vsrc.SortDescriptions.Add(new SortDescription("UICollector.Category", ListSortDirection.Ascending));
                        modificationList_Column_Category.Width = 0;
                        break;
                    case "Author":
                        vsrc.SortDescriptions.Add(new SortDescription("UICollector.Author", ListSortDirection.Ascending));
                        modificationList_Column_Author.Width = 0;
                        break;
                }

                //Add Name as Sort Descr
                vsrc.SortDescriptions.Add(new SortDescription("UICollector.Name", ListSortDirection.Ascending));
            }
            #endregion
        }
        #endregion
        #region Selection
        private void list_AllVersions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object sel = (sender as ListView).SelectedItem;
            if (sel != null && SelectedItem != sel)
                SelectedItem = sel;
        }

        private void list_Retail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object sel = (sender as ListView).SelectedItem;
            if (sel != null && SelectedItem != sel)
                SelectedItem = sel;
        }

        private void list_Addon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object sel = (sender as ListView).SelectedItem;
            if (sel != null && SelectedItem != sel)
                SelectedItem = sel;
        }

        private void list_IAAM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object sel = (sender as ListView).SelectedItem;
            if (sel != null && SelectedItem != sel)
                SelectedItem = sel;
        }
        #endregion


    }
}
