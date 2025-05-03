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
using DevelopmentTools.Editors.XmlModule.ModuleEditors;

namespace DevelopmentTools.Editors.XmlModule.Controls
{
    /// <summary>
    /// Interaction logic for XmlModuleCreator.xaml
    /// </summary>
    public partial class XmlModuleCreator : Window
    {
        public XmlModuleCreator()
        {
            InitializeComponent();
        }

        public void Set(XmlEditEditor edit)
        {
            Content = edit;

            edit.button_ok.Content = "OK";
            edit.button_cancel.Content = "Cancel";
            edit.button_test.Visibility = System.Windows.Visibility.Collapsed;
            edit.button_select.Visibility = System.Windows.Visibility.Collapsed;

            edit.button_ok.Click += new RoutedEventHandler(button_ok_Click);
            edit.button_cancel.Click += new RoutedEventHandler(button_cancel_Click);
        }

        public void Set(XmlAddEditor edit)
        {
            Content = edit;

            edit.button_ok.Content = "OK";
            edit.button_cancel.Content = "Cancel";
            edit.button_test.Visibility = System.Windows.Visibility.Collapsed;
            edit.button_select.Visibility = System.Windows.Visibility.Collapsed;

            edit.button_ok.Click += new RoutedEventHandler(button_ok_Click);
            edit.button_cancel.Click += new RoutedEventHandler(button_cancel_Click);
        }

        public void Set(XmlRemoveEditor edit)
        {
            Content = edit;

            edit.button_ok.Content = "OK";
            edit.button_cancel.Content = "Cancel";
            edit.button_test.Visibility = System.Windows.Visibility.Collapsed;
            edit.button_select.Visibility = System.Windows.Visibility.Collapsed;

            edit.button_ok.Click += new RoutedEventHandler(button_ok_Click);
            edit.button_cancel.Click += new RoutedEventHandler(button_cancel_Click);
        }

        void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        void button_ok_Click(object sender, RoutedEventArgs e)
        {
            if (Content is XmlAddEditor)
            {
                if (!(Content as XmlAddEditor).Check())
                    return;
            }

            DialogResult = true;
        }
    }
}
