using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AnnoModificationManager5.Misc;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AnnoModificationManager5.ModificationTypes.TaskSystem
{
    
    public class Task
    {
        public enum TaskType
        {
            Text,
            TextModule
        };

        public string Message { get; set; }

        public string Original { get; set; }
        public string Modified { get; set; }

        public string Name { get; set; }
        public bool Done { get; set; }
        public TaskType Type { get; set; }

        public object ToHeader
        {
            get
            {
                StackPanel pnl = new StackPanel() { Orientation = Orientation.Horizontal };
                pnl.Children.Add(new Image()
                {
                    Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/" + 
                        (Done ? "tick.png" : "hourglass.png")))
                });
                pnl.Children.Add(new TextBlock() { Text = Name, Margin = new System.Windows.Thickness(5, 0, 0, 0) });

                return pnl;
            }
        }

        public Task()
        {
            Message = "Insert Message";
            Original = "";
            Modified = "";
            Name = "Task";
            Done = false;
            Type = TaskType.Text;
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "Task", null);
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Name", Name));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Message", Message));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Original", Original));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Modified", Modified));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Done", Done.ToString()));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Type", Type.ToString()));

            return nd;
        }

        public static Task FromXml(XmlNode nd)
        {
            Task i = new Task();
            i.Name = nd.Attributes["Name"].Value;
            i.Message = nd.Attributes["Message"].Value;
            i.Original = nd.Attributes["Original"].Value;
            i.Modified = nd.Attributes["Modified"].Value;
            i.Done = bool.Parse(nd.Attributes["Done"].Value);
            i.Type = (TaskType)Enum.Parse(typeof(TaskType), nd.Attributes["Type"].Value);

            return i;
        }
    }
}
