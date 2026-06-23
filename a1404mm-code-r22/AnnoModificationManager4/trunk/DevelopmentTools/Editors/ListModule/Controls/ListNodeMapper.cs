using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers;
using Borgstrup.DocBase.Client.Controls;
using AnnoModificationManager4.Misc;

namespace DevelopmentTools.Editors.ListModule.Controls
{
    public class ListNodeMapper
    {
        //public XmlNode Node { get; set; }
        public ListFileTreeView Parent { get; set; }
        public IListModifier Modifier;

        public bool IsParent = false;
        public string RelativePath;
        
        public ImageSource NodeImage
        {           
            get;
            set;
        }

        public string NodeName
        {
            get;
            set;
        }     

        public static void Generate(ListFileTreeView treeview)
        {
            treeview.treeView.Items.Clear();
            foreach (KeyValuePair<string, List<StringBuilder>> ed in treeview.ListFile.ListEntries_ReadOnly)
            {
                List<IListModifier> mod_add = new List<IListModifier>();
                List<IListModifier> mod_edit = new List<IListModifier>();
                List<IListModifier> mod_remove = new List<IListModifier>();

                if (Project.Development_CurrentProject.Modification.ModificationUtils.List_AddModifiers.ContainsKey(treeview.ListFile))
                    mod_add = Project.Development_CurrentProject.Modification.ModificationUtils.List_AddModifiers[treeview.ListFile]
                        .FindAll(md => md.ElementGroup == ed.Key).ToList();
                if (Project.Development_CurrentProject.Modification.ModificationUtils.List_EditModifiers.ContainsKey(treeview.ListFile))
                    mod_edit = Project.Development_CurrentProject.Modification.ModificationUtils.List_EditModifiers[treeview.ListFile]
                        .FindAll(md => md.ElementGroup == ed.Key).ToList();
                if (Project.Development_CurrentProject.Modification.ModificationUtils.List_RemoveModifiers.ContainsKey(treeview.ListFile))
                    mod_remove = Project.Development_CurrentProject.Modification.ModificationUtils.List_RemoveModifiers[treeview.ListFile]
                        .FindAll(md => md.ElementGroup == ed.Key).ToList();

                TreeViewItem trv = new TreeViewItem();
                EditableTextBlock header = new EditableTextBlock();
                header.Icon = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/key.png"));
                header.Text = ed.Key;
                trv.Header = header;

                bool imagesdisabled = ed.Value.Count > 100;

                foreach (StringBuilder bd in ed.Value)
                {
                    TreeViewItem ni = new TreeViewItem();
                    ni.HeaderTemplate = (DataTemplate)treeview.Resources["TreeViewItemHeader_Xml"];

                    ListNodeMapper nd = new ListNodeMapper();
                    nd.NodeName = bd.ToString();

                    if (mod_edit.Find(md => md.ElementValue == bd.ToString()) != null)
                    {
                        nd.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/pencil.png"));
                        nd.Modifier = mod_edit.Find(md => md.ElementValue == bd.ToString());
                        nd.NodeName = bd.ToString() + " -> " + (nd.Modifier as EditModifier).NewValue;
                    }
                    else if (mod_remove.Find(md => md.ElementValue == bd.ToString()) != null)
                    {
                        nd.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/delete.png"));
                        nd.Modifier = mod_remove.Find(md => md.ElementValue == bd.ToString());
                    }
                    else if (!imagesdisabled)
                        nd.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/bullet_white.png"));

                    ni.Header = nd;
                    trv.Items.Add(ni);
                }

                foreach (AddModifier mod in mod_add)
                {
                    TreeViewItem ni = new TreeViewItem();
                    ni.HeaderTemplate = (DataTemplate)treeview.Resources["TreeViewItemHeader_Xml"];

                    ListNodeMapper nd = new ListNodeMapper();
                    nd.NodeName = mod.ElementValue;
                    nd.NodeImage = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/add.png"));
                    nd.Modifier = mod;

                    ni.Header = nd;
                    trv.Items.Add(ni);
                }

                treeview.treeView.Items.Add(trv);
            }
        }    
    }
}
