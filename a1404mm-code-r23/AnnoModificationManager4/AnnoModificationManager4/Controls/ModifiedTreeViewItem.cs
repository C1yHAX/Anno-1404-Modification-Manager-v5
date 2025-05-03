using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using AnnoModificationManager4.Misc;
using System.Threading;

namespace AnnoModificationManager4.Controls
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
