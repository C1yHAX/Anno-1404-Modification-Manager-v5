using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AnnoModificationManager4.Misc;

namespace AnnoModificationManager4.ModificationTypes.TaskSystem
{
    
    public class TaskList
    {
        public List<Task> Tasks = new List<Task>();
        public string Name = "TaskList";

        public object ToHeader
        {
            get
            {
                StackPanel pnl = new StackPanel() { Orientation = Orientation.Horizontal };
                pnl.Children.Add(new Image()
                {
                    Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/key.png")),
                });
                pnl.Children.Add(new TextBlock() { Text = Name, Margin = new System.Windows.Thickness(5, 0, 0, 0) });

                return pnl;
            }
        }

        public void SaveToXml(string FileName)
        {
            if (File.Exists(FileName))
                FileName = FileExtension.MakeFileUnique(FileName);

            XmlDocument doc = new XmlDocument();           
            doc.AppendChild(doc.CreateNode(XmlNodeType.Element, "TaskList", null));

            foreach (Task ti in Tasks)
            {
                doc.FirstChild.AppendChild(ti.ToXml(doc));
            }

            if (File.Exists(FileName))
                File.Delete(FileName);

            doc.Save(FileName);
        }

        public static TaskList LoadFromXml(string FileName)
        {
            TaskList tasklist = new TaskList();
            tasklist.Name = Path.GetFileNameWithoutExtension(FileName);

            XmlDocument doc = new XmlDocument();
            doc.Load(FileName);

            foreach (XmlNode c in doc.FirstChild.ChildNodes)
            {
                tasklist.Tasks.Add(Task.FromXml(c));
            }

            return tasklist;
        }
    }
}
