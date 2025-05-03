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
using AnnoModificationManager4.ModificationTypes.TaskSystem;
using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.Misc;
using DevelopmentTools.Misc;
using AnnoModificationManager4.UserInterface.Misc;
using System.Threading;

namespace DevelopmentTools.Tools.TaskSystem
{
    /// <summary>
    /// Interaction logic for TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        public static TaskWindow CurrentTaskWindow;

        TaskList GetCurrentTaskList
        {
            get
            {
                if (TaskTreeView.SelectedItem != null)
                {
                    if (TaskTreeView.Items.Contains(TaskTreeView.SelectedItem))
                    {
                        return (TaskTreeView.SelectedItem as ContentTreeViewItem).Content as TaskList;
                    }
                    else
                    {
                        return ((TaskTreeView.SelectedItem as TreeViewItem).Parent as ContentTreeViewItem).Content as TaskList;
                    }
                }
                return null;
            }
        }

        public TaskWindow()
        {
            CurrentTaskWindow = this;
            InitializeComponent();
        }

        public static void Create()
        {
            new TaskWindow();
            CurrentTaskWindow.Show();
        }

        public void Refresh()
        {
            TaskTreeView.Items.Clear();
            CurrentTaskViewer.Content = null;

            foreach (TaskList list in Modification.Development_CurrentModification.Tasks)
            {
                ContentTreeViewItem titem = new ContentTreeViewItem();
                titem.Content = this.Dispatch(()=>list);
                titem.Header = list.ToHeader;

                foreach (Task task in list.Tasks)
                {
                    ContentTreeViewItem item = new ContentTreeViewItem();
                     item.Content = task;
                    item.Header = task.ToHeader;

                    titem.Items.Add(item);
                }

                TaskTreeView.Items.Add(titem);
                titem.IsExpanded = true;
            }

            if (TaskTreeView.Items.Count != 0)
                (TaskTreeView.Items[0] as TreeViewItem).IsSelected = true;
        }

        public void SelectTask(Task t)
        {
            if (t != null)
            {
                foreach (ContentTreeViewItem titem in TaskTreeView.Items)
                {
                    foreach (ContentTreeViewItem item in titem.Items)
                    {
                        if (item.Content == t)
                        {
                            item.IsSelected = true;
                            TaskTreeView.ExpandTo(item);

                            return;
                        }
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void TaskTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {            
            if (TaskTreeView.SelectedItem != null)
            {
                if (!TaskTreeView.Items.Contains(TaskTreeView.SelectedItem))
                {
                    Task task = (TaskTreeView.SelectedItem as ContentTreeViewItem).Content as Task;

                    if (task.Type == Task.TaskType.Text)
                    {
                        TaskViewer_Text vw;
                        if (CurrentTaskViewer.Content != null
                            && CurrentTaskViewer.Content is TaskViewer_Text)
                        {
                            vw = (TaskViewer_Text)CurrentTaskViewer.Content;
                        }
                        else
                        {
                            vw = new TaskViewer_Text();
                        }
                        vw.SetTask(task);

                        CurrentTaskViewer.Content = vw;
                    }
                    else if (task.Type == Task.TaskType.TextModule)
                    {
                        TaskViewer_TextModule vw;
                       
                        if (CurrentTaskViewer.Content != null
                            && CurrentTaskViewer.Content is TaskViewer_Text)
                        {
                            vw = (TaskViewer_TextModule)CurrentTaskViewer.Content;
                        }
                        else
                        {
                            vw = new TaskViewer_TextModule();
                        }
                        vw.SetTask(task);

                        CurrentTaskViewer.Content = vw;
                    }
                }
            }
        }

        private void AddTaskList_Click(object sender, RoutedEventArgs e)
        {
            TaskList n = new TaskList();
            n.Name = "New TaskList";
            while (Modification.Development_CurrentModification.Tasks.Find(t => t.Name == n.Name) != null)
            {
                n.Name += RandomProvider.Random.Next(0, 9);
            }

            string newname = MessageWindow.GetText("Tasklist Name", n.Name);

            if (newname != null)
            {
                n.Name = newname;
                Modification.Development_CurrentModification.Tasks.Add(n);

                Refresh();
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            TaskList currenttl = GetCurrentTaskList;

            if (currenttl != null)
            {
                Task t = new Task();
                currenttl.Tasks.Add(t);

                Refresh();
                SelectTask(t);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (TaskTreeView.SelectedItem != null)
            {
                CurrentTaskViewer.Content = null;
                if (TaskTreeView.Items.Contains(TaskTreeView.SelectedItem))
                {
                    Modification.Development_CurrentModification.Tasks.Remove(GetCurrentTaskList);
                }
                else
                {
                    GetCurrentTaskList.Tasks.Remove((TaskTreeView.SelectedItem as ContentTreeViewItem).Content as Task);
                }

                Refresh();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CurrentTaskWindow = null;
        }

        private void TaskTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TaskTreeView.SelectedItem != null && TaskTreeView.Items.Contains(TaskTreeView.SelectedItem))
            {
                TaskList curr = GetCurrentTaskList;
                string newname = MessageWindow.GetText("TaskList Name:", curr.Name);

                if (newname != null)
                {
                    curr.Name = newname;
                    Refresh();
                }
            }
        }
    }
}
