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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.UserInterface.MainUI;

namespace AnnoModificationManager4.MainControls.Buttons
{
    /// <summary>
    /// Interaction logic for RDAChangesButton.xaml
    /// </summary>
    public partial class RDAChangesButton : Button
    {
        public RDAChangesButton()
        {
            InitializeComponent();
        }

        private void SetAlertMode(bool alert)
        {
            this.Dispatch(() =>
              {
                  if (alert)
                  {
                      Content = this.Resources["state_Alert"];
                  }
                  else
                  {
                      Content = this.Resources["state_Normal"];
                  }
              });
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            SetAlertMode(false);
            Modification.AMMRDA.OnClear += new Action<object>(AMMRDA_OnClear);
            Modification.AMMRDA.OnCommitted += new Action<object, RDAExplorer.RDAReader, RDA.AMMRDAManager.RDARequestType>(AMMRDA_OnCommitted);
        }

        void AMMRDA_OnCommitted(object arg1, RDAExplorer.RDAReader arg2, RDA.AMMRDAManager.RDARequestType arg3)
        {
            SetAlertMode(true);
        }

        void AMMRDA_OnClear(object obj)
        {
            SetAlertMode(false);
        }

        public void ApplyChanges(bool refreshmodsafter)
        {
            if (Modification.AMMRDA.Pending)
            {
                ApplyChangesWindow w = new ApplyChangesWindow();
                if (w.ShowDialog() == true && refreshmodsafter)
                {
                    MainWindow.CurrentMainWindow.ReloadModifications(false);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges(true);
        }
    }
}
