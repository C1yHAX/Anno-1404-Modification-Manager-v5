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
using AnnoModificationManager4.ModificationTypes.TaskSystem;

namespace DevelopmentTools.Tools.TaskSystem
{
    /// <summary>
    /// Interaction logic for TaskViewer_TextModule.xaml
    /// </summary>
    public partial class TaskViewer_TextModule : UserControl
    {
        Task CurrentTask;
        private bool IsTaskSetted = false;

        public TaskViewer_TextModule()
        {
            InitializeComponent();

            Field_Original.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.HighlightingDefinitions[13];
            Field_Modified.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.HighlightingDefinitions[13];
        }

        public void SetTask(Task task)
        {
            CurrentTask = task;

            Field_Original.Text = task.Original;
            Field_Modified.Text = task.Modified;
            Field_Name.Text = task.Name;
            Checkbox_Done.IsChecked = task.Done;

            IsTaskSetted = true;
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (IsTaskSetted)
            {
                CurrentTask.Original = Field_Original.Text;
                CurrentTask.Modified = Field_Modified.Text;
                CurrentTask.Name = Field_Name.Text;
                CurrentTask.Done = Checkbox_Done.IsChecked.Value;

                TaskWindow.CurrentTaskWindow.Refresh();
                TaskWindow.CurrentTaskWindow.SelectTask(CurrentTask);
            }
        }
    }
}
