using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Windows;
using System.Windows.Input;

namespace RDAExplorerGUI.Controls
{
    public class MultiSelectTreeView : TreeView
    {
        public bool AutoRecursive
        {
            get { return (bool)GetValue(AutoRecursiveProperty); }
            set { SetValue(AutoRecursiveProperty, value); }
        }

        public static readonly DependencyProperty AutoRecursiveProperty =
            DependencyProperty.Register("AutoRecursive", typeof(bool), typeof(MultiSelectTreeView), new UIPropertyMetadata(false));

        public bool DisableContextMenu
        {
            get { return (bool)GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisableContextMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisableContextMenuProperty =
            DependencyProperty.Register("DisableContextMenu", typeof(bool), typeof(MultiSelectTreeView), new UIPropertyMetadata(false));


        public List<object> SelectedItems = new List<object>();
        public List<object> AllItems
        {
            get
            {
                List<object> items = new List<object>();

                foreach (object it in Items)
                {
                    items.AddRange(GetRecursiveItems(it));
                }

                return items;
            }
        }

        public MultiSelectTreeView()
        {
            SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(MultiSelectTreeView_SelectedItemChanged);
        }

        private List<object> GetRecursiveItems(object item)
        {
            List<object> items = new List<object>();
            items.Add(item);

            if (item as TreeViewItem != null)
            {
                foreach (object it in (item as TreeViewItem).Items)
                {
                    items.AddRange(GetRecursiveItems(it));
                }
            }

            return items;
        }

        public void UpdateSelectedItems()
        {
            foreach (object item in AllItems)
            {
                if (item as TreeViewItem != null)
                {
                    TreeViewItem it = (TreeViewItem)item;
                    //it.FontWeight = SelectedItems.Contains(item) ? FontWeights.Bold : FontWeights.Normal;
                    it.Style = SelectedItems.Contains(item) ? App.Current.Resources["TreeViewItemStyle_Selected"] as Style : null;
                }
            }
        }

        public void SelectItem(object item)
        {
            if (!SelectedItems.Contains(item))
                SelectedItems.Add(item);

            if (item as TreeViewItem != null && AutoRecursive)
            {
                foreach (object i in (item as TreeViewItem).Items)
                {
                    SelectItem(i);
                }
            }
        }

        void MultiSelectTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem != null)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    SelectItem(SelectedItem);
                }
                else
                {
                    SelectedItems.Clear();
                    SelectItem(SelectedItem);
                }
            }
            else
            {
                SelectedItems.Clear();
            }

            UpdateSelectedItems();
        }
    }
}
