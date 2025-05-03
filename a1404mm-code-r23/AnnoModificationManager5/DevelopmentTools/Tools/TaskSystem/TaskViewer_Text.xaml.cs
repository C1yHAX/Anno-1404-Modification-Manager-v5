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
using AnnoModificationManager5.ModificationTypes.TaskSystem;

namespace DevelopmentTools.Tools.TaskSystem
{
    /// <summary>
    /// Interaction logic for TaskViewer_Text.xaml
    /// </summary>
    public partial class TaskViewer_Text : UserControl
    {
        Task CurrentTask;
        private bool IsTaskSetted = false;

        public TaskViewer_Text()
        {
            InitializeComponent();
        }

        public void SetTask(Task task)
        {
            CurrentTask = task;

            Field_Message.Text = task.Message;
            Field_Name.Text = task.Name;
            Checkbox_Done.IsChecked = task.Done;

            IsTaskSetted = true;
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (IsTaskSetted)
            {
                CurrentTask.Message = Field_Message.Text;
                CurrentTask.Name = Field_Name.Text;
                CurrentTask.Done = Checkbox_Done.IsChecked.Value;

                TaskWindow.CurrentTaskWindow.Refresh();
                TaskWindow.CurrentTaskWindow.SelectTask(CurrentTask);
            }
        }
    }
}
