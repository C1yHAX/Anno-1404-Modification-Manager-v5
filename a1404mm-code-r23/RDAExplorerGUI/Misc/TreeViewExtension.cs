using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ammctrls = AnnoModificationManager5.Controls;
using ammmisc = AnnoModificationManager5.Misc;
using RDAExplorer.Misc;
using RDAExplorerGUI.Controls;

namespace RDAExplorerGUI.Misc
{
    public static class TreeViewExtension
    {
        public static TreeView GetTreeView(this TreeViewItem item)
        {
            TreeViewItem curr = item;
            while (curr.Parent as TreeView == null)
            {
                curr = curr.Parent as TreeViewItem;
            }

            return curr.Parent as TreeView;
        }

        public static string GetNavigator(this ModifiedTreeViewItem item)
        {
            string nav = item.SemanticValue;

            if (item.Parent != null && item.Parent as ModifiedTreeViewItem != null)
            {
                nav = (item.Parent as ModifiedTreeViewItem).GetNavigator() + "/" + nav;
            }

            return nav.Trim('/');
        }

        public static ModifiedTreeViewItem NavigateTo(this TreeView view, string path, bool autocreate)
        {
            path = path.Replace("\\", "/");
            List<string> pathSegments = path.Split('/').ToList();

            string currentSegment = pathSegments[0];
            foreach (ModifiedTreeViewItem item in view.Items)
            {
                if (item.SemanticValue == currentSegment)
                {
                    if (pathSegments.Count == 1)
                        return item;
                    else
                    {
                        pathSegments.RemoveAt(0);
                        return NavigateTo(item, StringExtension.PutTogether(pathSegments, '/'), autocreate);
                    }
                }
            }

            if (autocreate)
            {
                ModifiedTreeViewItem newitem = new ModifiedTreeViewItem();
                newitem.Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/folder.png", currentSegment);
                newitem.SemanticValue = currentSegment;

                view.Items.Add(newitem);

                if (pathSegments.Count == 1)
                    return newitem;
                else
                {
                    pathSegments.RemoveAt(0);
                    return NavigateTo(newitem, StringExtension.PutTogether(pathSegments, '/'), autocreate);
                }
            }

            return null;
        }

        private static ModifiedTreeViewItem NavigateTo(ModifiedTreeViewItem view, string path, bool autocreate)
        {
            path = path.Replace("\\", "/");
            List<string> pathSegments = path.Split('/').ToList();

            string currentSegment = pathSegments[0];
            foreach (ModifiedTreeViewItem item in view.Items)
            {
                if (item.SemanticValue == currentSegment)
                {
                    if (pathSegments.Count == 1)
                        return item;
                    else
                    {
                        pathSegments.RemoveAt(0);
                        return NavigateTo(item, StringExtension.PutTogether(pathSegments, '/'), autocreate);
                    }
                }
            }

            if (autocreate)
            {
                ModifiedTreeViewItem newitem = new ModifiedTreeViewItem();
                newitem.Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/folder.png", currentSegment);
                newitem.SemanticValue = currentSegment;

                view.Items.Add(newitem);

                if (pathSegments.Count == 1)
                    return newitem;
                else
                {
                    pathSegments.RemoveAt(0);
                    return NavigateTo(newitem, StringExtension.PutTogether(pathSegments, '/'), autocreate);
                }
            }

            return null;
        }
    }
}
