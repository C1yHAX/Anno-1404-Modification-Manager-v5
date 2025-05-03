using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoModificationManager5.ModificationTypes.XmlModule;
using System.Windows.Controls;
using System.Windows;
using AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers;
using AnnoModificationManager5.Misc;

namespace DevelopmentTools.Editors.XmlModule.Controls
{
    public class XmlNodeMapper
    {
        public XmlNode Node { get; set; }
        public XmlFileTreeview Parent { get; set; }
        public IXMLModifier Modifier;

        public bool IsParent = false;
        public string RelativePath;
        
        public ImageSource NodeImage
        {
            /*get
            {
                if (AlternativeNodeImage != null)
                    return AlternativeNodeImage;

                if (!IsParent)
                {
                   string uristring = "/Images/Icons/tag.png";                   
                        
                        if (Project.CurrentProject.Modification.XmlModule_NodeModifiers_Edit.ContainsKey(Node))
                        {
                            uristring = "/Images/Icons/pencil.png";
                            Modifier = Project.CurrentProject.Modification.XmlModule_NodeModifiers_Edit[Node][0];
                        }
                        if (Project.CurrentProject.Modification.XmlModule_NodeModifiers_Remove.ContainsKey(Node))
                        {
                            uristring = "/Images/Icons/delete.png";
                            Modifier = Project.CurrentProject.Modification.XmlModule_NodeModifiers_Remove[Node][0];
                        }                    

                    return BitmapImageExtension.Load(("pack://application:,,," + uristring));
                }
                else
                    return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_code.png"));
            }*/
            get;
            set;
        }

        public string NodeName
        {
            get;
            set;
        }

        public void Refresh()
        {
            /*if (Modifier == null)
            {*/
                NodeName = Node.Name;

                if (Node.FirstChild != null && Node.FirstChild.NodeType == XmlNodeType.Text
                    && !string.IsNullOrEmpty(Node.FirstChild.Value))
                {
                    NodeName += " = " + Node.FirstChild.Value;
                }
            /*}
            else
            {
                if (Modifier.GetType() == typeof(EditModifier))
                {
                    EditModifier edit = (EditModifier)Modifier;
                    NodeName = Node.Name + " = " + (edit.NewValue.Contains("<") ? "<Xml Data>" : edit.NewValue);
                }
            }*/
        }

        public static TreeViewItem Generate(XmlNode bas, XmlFileTreeview treeview, string path, bool icon)
        {
            TreeViewItem trv = new TreeViewItem();
            trv.HeaderTemplate = (DataTemplate)treeview.Resources["TreeViewItemHeader_Xml"];

            path += "/" + bas.Name;

            XmlNodeMapper map = new XmlNodeMapper();
            map.Node = bas;
            map.Parent = treeview;

            //Generate RelativePath
            map.RelativePath = "";

            try
            {
                string[] pathsegments = path.Split('/');
                for (int i = 2; i < pathsegments.Length; i++)
                {
                    map.RelativePath += "/" + pathsegments[i];
                }                
            }
            catch (Exception)
            {
            }
            map.RelativePath = map.RelativePath.Trim('/');        
            map.Refresh();

            if (Project.Development_CurrentProject.Modification.ModificationUtils.Xml_EditModifiers.ContainsKey(bas))
            {
                List<IXMLModifier> XmlNodeList_Edit = Project.Development_CurrentProject.Modification.ModificationUtils.Xml_EditModifiers[bas];
                EditModifier mod = (EditModifier)XmlNodeList_Edit[0];

                map.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/pencil.png"));
                map.NodeName = bas.Name;
                map.Modifier = mod;

                if (!string.IsNullOrEmpty(mod.NewValue))
                {
                    if (mod.NewValue.Length > 7)
                        map.NodeName += " = " + mod.NewValue.Remove(6).Replace("\n", "").Replace("\r", "") + "...";
                    else
                        map.NodeName += " = " + mod.NewValue.Replace("\n", "").Replace("\r", "");
                }
            }
            else if (Project.Development_CurrentProject.Modification.ModificationUtils.Xml_RemoveModifiers.ContainsKey(bas))
            {
                map.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/delete.png"));
                map.Modifier = Project.Development_CurrentProject.Modification.ModificationUtils.Xml_RemoveModifiers[bas][0] as RemoveModifier;
            }
            else
            {
                if (icon)
                {
                    map.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/tag.png"));
                }

                //Generate Children
                if (bas.FirstChild != null && bas.FirstChild.NodeType != XmlNodeType.Text)
                {
                    foreach (XmlNode child in bas.ChildNodes)
                    {
                        trv.Items.Add(Generate(child, treeview, path, bas.ChildNodes.Count < 100));
                    }
                }

                //Add Add Modifier (If possible)
                if (Project.Development_CurrentProject.Modification.ModificationUtils.Xml_AddModifiers.ContainsKey(bas))
                {
                    List<IXMLModifier> XmlNodeList_Add = Project.Development_CurrentProject.Modification.ModificationUtils.Xml_AddModifiers[bas];

                    foreach (AddModifier add in XmlNodeList_Add)
                    {
                        TreeViewItem additem = new TreeViewItem();
                        additem.HeaderTemplate = (DataTemplate)treeview.Resources["TreeViewItemHeader_Xml"];

                        XmlNodeMapper addpanel = new XmlNodeMapper();
                        addpanel.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/add.png"));
                        addpanel.Modifier = add;
                        addpanel.NodeName = (add as AddModifier).TagName;
                        addpanel.RelativePath = (map.RelativePath + "/" + addpanel.NodeName).Trim('/');

                        if (!string.IsNullOrEmpty(add.Value))
                        {
                            if (add.Value.Length > 7)
                                addpanel.NodeName += " = " + add.Value.Remove(6).Replace("\n", "").Replace("\r", "") + "...";
                            else
                                addpanel.NodeName += " = " + add.Value.Replace("\n", "").Replace("\r", "");
                        }

                        additem.ToolTip = treeview.Selector + "/" + addpanel.RelativePath;
                        additem.Header = addpanel;

                        if (add.InsertBeforeIndex < 0)
                        {
                            trv.Items.Add(additem);
                        }
                        else
                        {
                            CodeExtension.TC(() => trv.Items.Insert(add.InsertBeforeIndex, additem), (ex) => trv.Items.Add(additem));
                        }
                    }
                }                
            }           

            trv.Header = map;
            trv.ToolTip = treeview.Selector + "/" + map.RelativePath;

            return trv;
        }
    }
}
