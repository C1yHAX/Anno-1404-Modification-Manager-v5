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
using AnnoModificationManager5.ModificationTypes.XmlModule;
using AnnoModificationManager5.Misc;
using System.Xml;
using System.Threading;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.ModificationTypes.XmlModule.XMLModifiers;
using DevelopmentTools.Editors.XmlModule.ModuleEditors;
using AnnoModificationManager5.ModificationTypes;
using Borgstrup.DocBase.Client.Controls;

namespace DevelopmentTools.Editors.XmlModule.Controls
{
    /// <summary>
    /// Interaction logic for XmlFileTreeview.xaml
    /// </summary>
    public partial class XmlFileTreeview : UserControl
    {
        public new XmlModuleEditor_ModuleCreator Parent;

        public XMLFile XmlFile;
        public Exception XmlFileException;

        public string Selector;

        public XmlFileTreeview()
        {
            InitializeComponent();
        }

        public void SetXmlFile(string FileName)
        {
            try
            {
                FileName = FileName.FormatDevelopmentFolders();

                //Prefer already existing files
                if (XmlFile == null || XmlFile.FileName != FileName)
                {
                    try
                    {
                        //XmlFile = new XMLFile(FileName);
                        XmlFile = XMLFileCollector.Request(FileName);
                        XmlFileException = null;
                    }
                    catch (Exception ex)
                    {
                        XmlFile = null;
                        XmlFileException = ex;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public TreeViewItem SearchModifier(IXMLModifier mod)
        {
            foreach (TreeViewItem itm in treeView.Items)
            {
                TreeViewItem fitm = SearchModifier(mod, itm);
                if (fitm != null)
                    return fitm;
            }
            return null;
        }

        public TreeViewItem SearchModifier(IXMLModifier mod, TreeViewItem item)
        {
            if ((item.Header as XmlNodeMapper).Modifier == mod)
                return item;
            foreach (TreeViewItem itm in item.Items)
            {
                TreeViewItem fitm = SearchModifier(mod, itm);
                if (fitm != null)
                    return fitm;
            }
            return null;
        }

        public void Select(string selector)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(delegate
                {
                    try
                    {
                        #region Prepare
                        Selector = selector;

                        ProgressMessage.Dispatch(prg =>
                            {
                                prg.Visibility = System.Windows.Visibility.Visible;
                            });

                        List<XmlNode> nodes = null;
                        Exception XmlSelectorError = new Exception();

                        if (XmlFile != null)
                        {
                            try
                            {
                                nodes = XmlFile.Select(selector);
                            }
                            catch (Exception ex)
                            {
                                XmlSelectorError = ex;
                            }
                        }
                        else
                        {
                            XmlSelectorError = XmlFileException;
                        }

                        //Clear TreeView
                        treeView.Dispatch(trv =>
                        {
                            trv.Items.Clear();
                            trv.IsHitTestVisible = false;
                        });
                        #endregion
                        //Update Label
                        #region Update Label
                        if (Parent != null)
                        {
                            Parent.Dispatch(mc =>
                                        {
                                            Parent.Xml_lastMessage_Inspect_ToolBar.Visibility = System.Windows.Visibility.Collapsed;
                                            if (nodes != null)
                                            {
                                                if (nodes.Count == 1)
                                                    Parent.Xml_LastCount.Text = "1 Element";
                                                else
                                                    Parent.Xml_LastCount.Text = nodes.Count + " Elements";
                                            }
                                            else
                                            {
                                                if (XmlSelectorError != null)
                                                    Parent.Xml_LastCount.Text = XmlSelectorError.Message.Replace("\r\n", " ");
                                            }
                                        });
                        }
                        #endregion
                        //Don't load
                        #region Load
                        if (nodes != null)
                        {
                            progressBar.Dispatch(prg =>
                                            {
                                                prg.Visibility = System.Windows.Visibility.Visible;
                                                prg.Maximum = nodes.Count;
                                                prg.Value = 1;
                                            });

                            foreach (XmlNode node in nodes)
                            {
                                Application.Current.Dispatch(app =>
                                    {
                                        TreeViewItem nd = XmlNodeMapper.Generate(node, this, "", nodes.Count < 100);
                                        (nd.Header as XmlNodeMapper).IsParent = true;

                                        treeView.Items.Add(nd);
                                        progressBar.Value++;
                                    });
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #region Update UI
                    progressBar.Dispatch(prg =>
                    {
                        prg.Visibility = System.Windows.Visibility.Collapsed;
                    });
                    treeView.Dispatch(trv =>
                    {
                        trv.IsHitTestVisible = true;
                    });
                    ProgressMessage.Dispatch(prg =>
                    {
                        prg.Visibility = System.Windows.Visibility.Hidden;
                    });
                    #endregion
                }));
            thread.Start();
        }
        public void SelectSync(string selector)
        {
            try
            {
                #region Prepare
                Selector = selector;

                ProgressMessage.Dispatch(prg =>
                {
                    prg.Visibility = System.Windows.Visibility.Visible;
                });

                List<XmlNode> nodes = null;
                Exception XmlSelectorError = new Exception();

                if (XmlFile != null)
                {
                    try
                    {
                        nodes = XmlFile.Select(selector);
                    }
                    catch (Exception ex)
                    {
                        XmlSelectorError = ex;
                    }
                }
                else
                {
                    XmlSelectorError = XmlFileException;
                }

                //Clear TreeView
                treeView.Dispatch(trv =>
                {
                    trv.Items.Clear();
                    trv.IsHitTestVisible = false;
                });
                #endregion
                //Update Label
                #region Update Label
                if (Parent != null)
                {
                    Parent.Dispatch(mc =>
                    {
                        Parent.Xml_lastMessage_Inspect_ToolBar.Visibility = System.Windows.Visibility.Collapsed;
                        if (nodes != null)
                        {
                            if (nodes.Count == 1)
                                Parent.Xml_LastCount.Text = "1 Element";
                            else
                                Parent.Xml_LastCount.Text = nodes.Count + " Elements";
                        }
                        else
                        {
                            Parent.Xml_LastCount.Text = XmlSelectorError.Message.Replace("\r\n", " ");
                        }
                    });
                }
                #endregion
                //Don't load
                #region Load
                if (nodes != null)
                {
                    progressBar.Dispatch(prg =>
                    {
                        prg.Visibility = System.Windows.Visibility.Visible;
                        prg.Maximum = nodes.Count;
                        prg.Value = 1;
                    });

                    foreach (XmlNode node in nodes)
                    {
                        Application.Current.Dispatch(app =>
                        {
                            TreeViewItem nd = XmlNodeMapper.Generate(node, this, "", nodes.Count < 100);
                            (nd.Header as XmlNodeMapper).IsParent = true;

                            treeView.Items.Add(nd);
                            progressBar.Value++;
                        });
                    }
                }
                #endregion
            }
            catch (Exception)
            {
            }
            #region Update UI
            progressBar.Dispatch(prg =>
            {
                prg.Visibility = System.Windows.Visibility.Collapsed;
            });
            treeView.Dispatch(trv =>
            {
                trv.IsHitTestVisible = true;
            });
            ProgressMessage.Dispatch(prg =>
            {
                prg.Visibility = System.Windows.Visibility.Hidden;
            });
            #endregion
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Parent != null)
            {
                if (treeView.SelectedItem != null &&
                    (Properties.Settings.Default.XmlModuleCreator_TreeView_AllowEditParent ?
                    true :
                    !treeView.Items.Contains(treeView.SelectedItem)))
                {
                    XmlNodeMapper map = ((treeView.SelectedItem as TreeViewItem).Header as XmlNodeMapper);

                    int matching = 1;
                    try
                    {
                        matching = XmlFile.Select(Selector + "/" + map.RelativePath).Count;
                    }
                    catch (Exception) { }
                    Parent.Xml_LastCount.Text = matching + " matching node" + (matching != 1 ? "s" : "") + " in selection";

                    Parent.Xml_lastMessage_Inspect_ToolBar.Visibility = System.Windows.Visibility.Visible;

                    //Update Selector
                    Parent.Selectors_Selector.Text = (Selector + "/" + map.RelativePath).TrimEnd('/');

                    //ToolBar
                    if (map.Modifier != null)
                    {
                        Parent.XmlNode_Add.Visibility = Visibility.Collapsed;
                        Parent.XmlNode_Edit.Visibility = Visibility.Collapsed;
                        Parent.XmlNode_Remove.Visibility = Visibility.Collapsed;
                        Parent.XmlNode_AddBeforeSelected.Visibility = Visibility.Collapsed;
                        Parent.XmlNode_Select.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Parent.XmlNode_Add.Visibility = Visibility.Visible;
                        Parent.XmlNode_AddBeforeSelected.Visibility = Visibility.Visible;
                        Parent.XmlNode_Edit.Visibility = Visibility.Visible;
                        Parent.XmlNode_Remove.Visibility = Visibility.Visible;
                        Parent.XmlNode_Select.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (treeView.Items.Count == 1)
                        Parent.Xml_LastCount.Text = "1 Element";
                    else
                        Parent.Xml_LastCount.Text = treeView.Items.Count + " Elements";

                    //ToolBar
                    Parent.XmlNode_Add.Visibility = Visibility.Collapsed;
                    Parent.XmlNode_Edit.Visibility = Visibility.Collapsed;
                    Parent.XmlNode_Remove.Visibility = Visibility.Collapsed;
                    Parent.XmlNode_Select.Visibility = Visibility.Collapsed;
                }
            }
        }

        #region AddEditRemove Modifiers
        public void Select()
        {
            TreeViewItem sel = treeView.SelectedItem as TreeViewItem;
            if (sel != null)
            {
                XmlNodeMapper xml = sel.Header as XmlNodeMapper;
                if (xml.Modifier != null)
                {
                    Parent.Parent.ModuleList_SelectModification(xml.Modifier);
                }
            }
        }

        /// <summary>
        /// Call after Adding a Mod
        /// </summary>
        public void RefreshModifier(IXMLModifier edit)
        {
            //Copy Source File to %mod%/OriginalFiles -> Not used anymore
            //Parent.Parent.Parent.Modification.CopySourceFileToFolder(Parent.Files_SourceFile.Text, edit.File);

            //Set src
            //Parent.Files_SourceFile.Text = edit.XMLFile.FileName.Replace(Modification.Development_CurrentModification.Folder, "%Project%");
            //SetXmlFile(Parent.Files_SourceFile.Text);
            Parent.Files_File.Text = edit.File;

            //Refresh treeview (Sync)
            SelectSync(Selector);

            //Get ModifierItem
            TreeViewItem toexpand = SearchModifier(edit);

            //Expand to
            if (toexpand != null)
            {
                treeView.ExpandTo(toexpand);
                toexpand.IsSelected = true;
            }
        }

        public bool Edit()
        {
            TreeViewItem sel = treeView.SelectedItem as TreeViewItem;
            if (sel != null)
            {
                XmlNodeMapper xml = sel.Header as XmlNodeMapper;
                //if (!xml.IsParent | Properties.Settings.Default.XmlModuleCreator_TreeView_AllowEditParent)
                {
                    //If there's already a modifier -> edit
                    if (xml.Modifier != null)
                    {
                        Parent.Parent.ModuleList_SelectModification(xml.Modifier);
                        return false;
                    }

                    #region Check
                    try
                    {
                        if (string.IsNullOrEmpty(Parent.Selectors_Selector.Text)
                                        || string.IsNullOrEmpty(Parent.Files_File.Text))
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    #endregion
                    #region Generate Modifier
                    //Generate Modifier before 
                    EditModifier edit = new EditModifier();
                    edit.File = Parent.Files_File.Text;
                    edit.Group = ((Parent.Parent.ModuleList.SelectedItem as TreeViewItem).Header as EditableTextBlock).Text;
                    edit.Selector = Selector + (!string.IsNullOrEmpty(xml.RelativePath) ? "/" + xml.RelativePath : "");
                    edit.DeSelector = !string.IsNullOrEmpty(Parent.Selectors_Deselector.Text)
                        ? Parent.Selectors_Deselector.Text + (!string.IsNullOrEmpty(xml.RelativePath) ? "/" + xml.RelativePath : "") : "";
                    edit.Parent = Parent.Parent.CurrentModuleList;
                    //Edit only
                    edit.OldValue = xml.Node.InnerXml;
                    edit.NewValue = edit.OldValue.Replace("><", ">\r\n<");

                    // xml.Modifier = edit;
                    //Modification.Development_CurrentModification.CopySourceFileToFolder(Parent.Files_SourceFile.Text, edit.File);
                    #endregion
                    #region Dialog + TreeView
                    XmlModuleCreator creator = new XmlModuleCreator();
                    XmlEditEditor dialog = new XmlEditEditor(edit);
                    creator.Set(dialog);

                    if (creator.ShowDialog() == true)
                    {
                        //Add Modifier to Modification and TreeView
                        Parent.Parent.CurrentModule.Add(edit);
                        Parent.Parent.ModuleList_XmlModuleCreator_Add(edit);

                        RefreshModifier(edit);
                        return true;
                    }
                    #endregion
                }
                /*else
                {
                    MessageWindow.Show("Cannot edit first node!");
                }*/
            }
            return false;
        }

        public bool Add()
        {
            TreeViewItem sel = treeView.SelectedItem as TreeViewItem;
            if (sel != null)
            {
                XmlNodeMapper xml = sel.Header as XmlNodeMapper;

                //If there's already a modifier -> edit
                if (xml.Modifier != null)
                {
                    Parent.Parent.ModuleList_SelectModification(xml.Modifier);
                    return false;
                }

                #region Check
                try
                {
                    if (string.IsNullOrEmpty(Parent.Selectors_Selector.Text)
                                         || string.IsNullOrEmpty(Parent.Files_File.Text))
                        throw new Exception();
                }
                catch (Exception)
                {
                    return false;
                }
                #endregion
                #region Generate Modifier
                //Generate Modifier before 
                AddModifier edit = new AddModifier();
                edit.File = Parent.Files_File.Text;
                edit.Group = ((Parent.Parent.ModuleList.SelectedItem as TreeViewItem).Header as EditableTextBlock).Text;
                edit.Selector = Selector + (!string.IsNullOrEmpty(xml.RelativePath) ? "/" + xml.RelativePath : "");
                edit.DeSelector = !string.IsNullOrEmpty(Parent.Selectors_Deselector.Text)
                    ? Parent.Selectors_Deselector.Text + (!string.IsNullOrEmpty(xml.RelativePath) ? "/" + xml.RelativePath : "") : "";
                edit.Parent = Parent.Parent.CurrentModuleList;
                //Add only
                edit.TagName = "NewTag";
                edit.Value = "";
                edit.InsertBeforeIndex = -1;

                //Modification.Development_CurrentModification.CopySourceFileToFolder(Parent.Files_SourceFile.Text, edit.File);
                // xml.Modifier = edit;
                #endregion
                #region Dialog + TreeView
                XmlModuleCreator creator = new XmlModuleCreator();
                XmlAddEditor dialog = new XmlAddEditor(edit);
                creator.Set(dialog);

                if (creator.ShowDialog() == true)
                {
                    //Add Modifier to Modification and TreeView
                    Parent.Parent.CurrentModule.Add(edit);
                    Parent.Parent.ModuleList_XmlModuleCreator_Add(edit);

                    RefreshModifier(edit);
                    return true;
                }
                #endregion
            }
            return false;
        }

        public bool AddBeforeSelected()
        {
            TreeViewItem sel = treeView.SelectedItem as TreeViewItem;
            if (sel != null && !treeView.Items.Contains(sel))
            {
                XmlNodeMapper xml = (sel.Parent as TreeViewItem).Header as XmlNodeMapper;

                #region Check
                try
                {
                    if (string.IsNullOrEmpty(Parent.Selectors_Selector.Text)
                                         || string.IsNullOrEmpty(Parent.Files_File.Text))
                        throw new Exception();
                }
                catch (Exception)
                {
                    return false;
                }
                #endregion
                #region Generate Modifier
                //Generate Modifier before 
                AddModifier edit = new AddModifier();
                edit.File = Parent.Files_File.Text;
                edit.Group = ((Parent.Parent.ModuleList.SelectedItem as TreeViewItem).Header as EditableTextBlock).Text;
                edit.Selector = Selector + (!string.IsNullOrEmpty(xml.RelativePath) ? "/" + xml.RelativePath : "");
                edit.DeSelector = !string.IsNullOrEmpty(Parent.Selectors_Deselector.Text)
                    ? Parent.Selectors_Deselector.Text + (!string.IsNullOrEmpty(xml.RelativePath) ? "/" + xml.RelativePath : "") : "";
                edit.Parent = Parent.Parent.CurrentModuleList;
                //Add only
                edit.TagName = "NewTag";
                edit.Value = "";
                edit.InsertBeforeIndex = (sel.Parent as TreeViewItem).Items.IndexOf(sel);

                //Modification.Development_CurrentModification.CopySourceFileToFolder(Parent.Files_SourceFile.Text, edit.File);
                // xml.Modifier = edit;
                #endregion
                #region Dialog + TreeView
                XmlModuleCreator creator = new XmlModuleCreator();
                XmlAddEditor dialog = new XmlAddEditor(edit);
                creator.Set(dialog);

                if (creator.ShowDialog() == true)
                {
                    //Add Modifier to Modification and TreeView
                    Parent.Parent.CurrentModule.Add(edit);
                    Parent.Parent.ModuleList_XmlModuleCreator_Add(edit);

                    RefreshModifier(edit);
                    return true;
                }
                #endregion
            }
            return false;
        }

        public bool Remove()
        {
            TreeViewItem sel = treeView.SelectedItem as TreeViewItem;
            //Cannot remove root!!!
            if (sel != null && !treeView.Items.Contains(sel))
            {
                XmlNodeMapper xml = sel.Header as XmlNodeMapper;

                //If there's already a modifier -> edit
                if (xml.Modifier != null)
                {
                    Parent.Parent.ModuleList_SelectModification(xml.Modifier);
                    return false;
                }

                #region Check
                try
                {
                    if (string.IsNullOrEmpty(Parent.Selectors_Selector.Text)
                                         || string.IsNullOrEmpty(Parent.Files_File.Text))
                        throw new Exception();
                }
                catch (Exception)
                {
                    return false;
                }
                #endregion
                #region Generate Modifier
                //Generate Modifier before 
                RemoveModifier edit = new RemoveModifier();
                edit.File = Parent.Files_File.Text;
                edit.Group = ((Parent.Parent.ModuleList.SelectedItem as TreeViewItem).Header as EditableTextBlock).Text;
                edit.DeSelector = (Selector + "/" + ((sel.Parent as TreeViewItem).Header as XmlNodeMapper).RelativePath).TrimEnd('/');
                edit.Selector = (Selector + "/" + ((sel as TreeViewItem).Header as XmlNodeMapper).RelativePath).TrimEnd('/');
                edit.Parent = Parent.Parent.CurrentModuleList;
                //Remove only
                edit.TagName = xml.Node.Name;
                edit.InnerXml = xml.Node.InnerXml;
                edit.InsertBeforeIndex = (sel.Parent as TreeViewItem).Items.IndexOf(sel);

                //Modification.Development_CurrentModification.CopySourceFileToFolder(Parent.Files_SourceFile.Text, edit.File);
                // xml.Modifier = edit;
                #endregion
                #region Dialog + TreeView
                XmlModuleCreator creator = new XmlModuleCreator();
                XmlRemoveEditor dialog = new XmlRemoveEditor(edit);
                creator.Set(dialog);

                if (creator.ShowDialog() == true)
                {
                    //Add Modifier to Modification and TreeView
                    Parent.Parent.CurrentModule.Add(edit);
                    Parent.Parent.ModuleList_XmlModuleCreator_Add(edit);

                    RefreshModifier(edit);
                    return true;
                }
                #endregion
            }
            return false;
        }
        #endregion

        #region 4020 Events
        private void XmlNode_Add_Click(object sender, RoutedEventArgs e)
        {
            Parent.XmlNode_Add_Click(sender, e);
        }

        private void XmlNode_Edit_Click(object sender, RoutedEventArgs e)
        {
            Parent.XmlNode_Edit_Click(sender, e);
        }

        private void XmlNode_Remove_Click(object sender, RoutedEventArgs e)
        {
            Parent.XmlNode_Remove_Click(sender, e);
        }

        private void treeView_ContextMenu_ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                treeView.ExpandAll();
            }
            catch (Exception) { }
        }
        #endregion
    }
}
