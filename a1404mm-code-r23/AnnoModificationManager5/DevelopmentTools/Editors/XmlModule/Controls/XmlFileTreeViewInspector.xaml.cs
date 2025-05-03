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
using AnnoModificationManager5.ModificationTypes.XmlModule;
using AnnoModificationManager5.ModificationTypes;

namespace DevelopmentTools.Editors.XmlModule.Controls
{
    /// <summary>
    /// Interaction logic for XmlFileTreeViewInspector.xaml
    /// </summary>
    public partial class XmlFileTreeViewInspector : Window
    {
        public string Selector;
        public XMLFile File;      

        public XmlFileTreeViewInspector(Modification Modification)
        {
            InitializeComponent();          

            Loaded += new RoutedEventHandler(XmlFileTreeViewInspector_Loaded);
        }

        void XmlFileTreeViewInspector_Loaded(object sender, RoutedEventArgs e)
        {
            xmlFileTreeview.XmlFile = File;
            xmlFileTreeview.Select(Selector);
        }
    }
}
