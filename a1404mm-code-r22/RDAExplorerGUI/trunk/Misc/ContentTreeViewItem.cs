using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using AnnoModificationManager4.Controls;

namespace RDAExplorerGUI.Misc
{
    public class ContentTreeViewItem : ModifiedTreeViewItem
    {
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(ContentTreeViewItem), new UIPropertyMetadata(""));

    }
}
