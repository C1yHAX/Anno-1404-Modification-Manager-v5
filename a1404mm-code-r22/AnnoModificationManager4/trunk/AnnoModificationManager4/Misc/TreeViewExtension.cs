using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace AnnoModificationManager4.Misc
{
    public static class TreeViewExtension
    {
        public static void ExpandTo(this TreeView treeView, TreeViewItem item)
        {
            if (item != null)
            {
                item = item.Parent as TreeViewItem;
                while (item != null)
                {
                    item.IsExpanded = true;
                    item = item.Parent as TreeViewItem;
                }
            }
        }

        public static void ExpandAll(this TreeView treeView)
        {
            foreach (TreeViewItem item in treeView.Items)
            {
                item.ExpandAll();
            }
        }

        public static void ExpandAll(this TreeViewItem item)
        {
            item.IsExpanded = true;
            foreach (TreeViewItem i in item.Items)
            {
                i.ExpandAll();
            }
        }
    }
}
