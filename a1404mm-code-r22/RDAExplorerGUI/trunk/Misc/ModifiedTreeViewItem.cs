using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using ammmisc = AnnoModificationManager4.Misc;
using System.Threading;
using RDAExplorerGUI;
using RDAExplorerGUI.Controls;
using RDAExplorerGUI.Misc;

namespace RDAExplorerGUI.Controls
{
    public class ModifiedTreeViewItem : TreeViewItem
    {
        public string SemanticValue
        {
            get { return (string)GetValue(SemanticValueProperty); }
            set { SetValue(SemanticValueProperty, value); }
        }
        public static readonly DependencyProperty SemanticValueProperty =
            DependencyProperty.Register("SemanticValue", typeof(string), typeof(ModifiedTreeViewItem), new UIPropertyMetadata(""));

        public bool SelectOnRightClick
        {
            get { return (bool)GetValue(SelectOnRightClickProperty); }
            set { SetValue(SelectOnRightClickProperty, value); }
        }
        public static readonly DependencyProperty SelectOnRightClickProperty =
            DependencyProperty.Register("SelectOnRightClick", typeof(bool), typeof(ModifiedTreeViewItem), new UIPropertyMetadata(false));

        public ModifiedTreeViewItem()
        {
            ContextMenuOpening += new ContextMenuEventHandler(ModifiedTreeViewItem_ContextMenuOpening);
        }

        void ModifiedTreeViewItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            TreeView tv = this.GetTreeView();

            if (tv is MultiSelectTreeView && ((MultiSelectTreeView)tv).DisableContextMenu)
            {
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectOnRightClick)
            {
                IsSelected = true;
            }

            base.OnPreviewMouseRightButtonDown(e);
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldStartingIndex > 0 && Items.Count != 0)
                {
                    (new Thread(new ParameterizedThreadStart(delegate
                        {
                            Thread.Sleep(75);
                            App.Current.Dispatch(app =>
                            {
                                (Items[e.OldStartingIndex - 1] as TreeViewItem).IsSelected = true;
                            });
                        }))).Start();
                }
            }
        }
    }
}
