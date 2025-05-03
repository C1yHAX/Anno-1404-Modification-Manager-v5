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
using System.IO;
using System.Windows.Threading;
using AnnoModificationManager5.ModificationTypes.TaskSystem;
using AnnoModificationManager5.ModificationTypes;
using AnnoModificationManager5.Misc;
using System.ComponentModel;
using System.Threading;

namespace DevelopmentTools.Tools.TaskSystem
{
    /// <summary>
    /// Interaction logic for TaskButton.xaml
    /// </summary>
    public partial class TaskButton : Button
    {
        DispatcherTimer timer = new DispatcherTimer();

        public TaskButton()
        {
            InitializeComponent();
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            timer_Tick(null, null);

            timer.Interval = new TimeSpan(0, 0, 7);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void Button_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            List<Task> toDo = new List<Task>();

            if (Modification.Development_CurrentModification != null)
            {
                foreach (TaskList l in Modification.Development_CurrentModification.Tasks)
                {
                    foreach (Task t in l.Tasks)
                    {
                        if (!t.Done)
                            toDo.Add(t);
                    }
                }
            }

            if (toDo.Count == 0)
            {
                Content = "Tasks";
                FontWeight = FontWeights.Normal;
            }
            else
            {
                Content = toDo.Count + " tasks to do!";
                FontWeight = FontWeights.Bold;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TaskWindow.CurrentTaskWindow == null)
            {
                TaskWindow.Create();
            }
            else
            {
                TaskWindow.CurrentTaskWindow.Dispatch(tw =>
                    {
                        tw.Show();
                        tw.Activate();
                    });
            }
        }
    }
}
