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

namespace DevelopmentTools.Editors.XmlModule.FilterSystem
{
    /// <summary>
    /// Interaction logic for FilterEditor.xaml
    /// </summary>
    public partial class FilterEditor : Window
    {
        public FilterEditor()
        {
            InitializeComponent();
        }

        public void Refresh()
        {
            Filters.ItemsSource = Filter.Filters;
            Filters.Items.Refresh();

            Filters.SelectedIndex = 0;
        }

        private void Filters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Filters.SelectedItem != null)
            {
                filter_name.SetBinding(TextBox.TextProperty,
                    new Binding("SelectedItem.Name") { ElementName = "Filters", Mode = BindingMode.TwoWay });
                filter_destination.SetBinding(TextBox.TextProperty,
                    new Binding("SelectedItem.DestinationFile") { ElementName = "Filters", Mode = BindingMode.TwoWay });

                filter_selector.SetBinding(TextBox.TextProperty,
                    new Binding("SelectedItem.Selector") { ElementName = "Filters", Mode = BindingMode.TwoWay });
                filter_deselector.SetBinding(TextBox.TextProperty,
                    new Binding("SelectedItem.Deselector") { ElementName = "Filters", Mode = BindingMode.TwoWay });

                filter_list_defaultreplace.ItemsSource = (Filters.SelectedItem as Filter).FilterValues.FindAll(trp => !trp.IsAttribute);
                filter_list_defaultreplace.Items.Refresh();

                filter_list_attributes.ItemsSource = (Filters.SelectedItem as Filter).FilterValues.FindAll(trp => trp.IsAttribute);
                filter_list_attributes.Items.Refresh();
            }
        }

        private void filter_list_defaultreplace_add_Click(object sender, RoutedEventArgs e)
        {
            Filter fil = (Filters.SelectedItem as Filter);
            fil.FilterValues.Add(new FilterTripel()
            {
                Key = "{" + (fil.FilterValues.Count + 1) + "}",
                Name = "Default Replace " + (fil.FilterValues.Count + 1)
            });

            Filters_SelectionChanged(null, null);
        }

        private void filter_list_defaultreplace_delete_Click(object sender, RoutedEventArgs e)
        {
            if (filter_list_defaultreplace.SelectedItem != null)
            {
                Filter fil = (Filters.SelectedItem as Filter);
                fil.FilterValues.Remove(filter_list_defaultreplace.SelectedItem as FilterTripel);
            }

            Filters_SelectionChanged(null, null);
        }

        private void filter_list_attributes_add_Click(object sender, RoutedEventArgs e)
        {
            Filter fil = (Filters.SelectedItem as Filter);
            fil.FilterValues.Add(new FilterTripel()
            {
                Key = "{a" + (fil.FilterValues.Count + 1) + "}",
                Name = "Attribute " + (fil.FilterValues.Count + 1),
                Attribute="",
                IsAttribute=true
            });

            Filters_SelectionChanged(null, null);
        }

        private void filter_list_attributes_delete_Click(object sender, RoutedEventArgs e)
        {
            if (filter_list_attributes.SelectedItem != null)
            {
                Filter fil = (Filters.SelectedItem as Filter);
                fil.FilterValues.Remove(filter_list_attributes.SelectedItem as FilterTripel);
            }

            Filters_SelectionChanged(null, null);
        }

        private void button_add_Click(object sender, RoutedEventArgs e)
        {
            Filter fil = new Filter() { Name = "Filter " + (Filters.Items.Count + 1) };           
            
            Filter.Filters.Add(fil);

            Filters.Items.Refresh();
            Filters.SelectedItem = fil;
        }

        private void button_delete_Click(object sender, RoutedEventArgs e)
        {
            if (Filters.SelectedItem != null)
            {
                Filter toremove = Filters.SelectedItem as Filter;               

                Filter.Filters.Remove(toremove);
            }

            Refresh();
        }

        private void File_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void File_Save_Click(object sender, RoutedEventArgs e)
        {
            Filter.ToXml();
        }

        private void button_Move_Up_Click(object sender, RoutedEventArgs e)
        {
            if (Filters.SelectedItem != null)
            {
                if (Filters.SelectedIndex != 0)
                {
                    int idx = Filters.SelectedIndex;
                    object curr = Filters.SelectedItem;

                    Filter.Filters.Remove(curr as Filter);
                    Filter.Filters.Insert(idx - 1, curr as Filter);

                    Filters.Items.Refresh();
                }
            }
        }

        private void button_Move_Down_Click(object sender, RoutedEventArgs e)
        {
            if (Filters.SelectedItem != null)
            {
                if (Filters.SelectedIndex != Filters.Items.Count - 1)
                {
                    int idx = Filters.SelectedIndex;
                    object curr = Filters.SelectedItem;

                    Filter.Filters.Remove(curr as Filter);
                    Filter.Filters.Insert(idx + 1, curr as Filter);

                    Filters.Items.Refresh();
                }
            }
        }

        private void filter_TextChanged_UpdateBinding(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        } 
    }
}
