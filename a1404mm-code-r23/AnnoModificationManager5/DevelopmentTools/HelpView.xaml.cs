using System.Windows;
using System.Windows.Controls;

namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : UserControl
    {
        public object PreviousContent { get; set; }

        public HelpView()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (PreviousContent != null)
                MainWindow.CurrentMainWindow.Content = PreviousContent;
            else
                MainWindow.CurrentMainWindow.MainWindow_Loaded(null, null);
        }
    }
}
