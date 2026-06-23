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
using System.Threading;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.ModificationTypes.XmlModule;
using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers;
using AnnoModificationManager4.ModificationTypes.ListModule;
using AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers;
using AnnoModificationManager4.UserInterface.Misc;
using DevelopmentTools.Tools.Global.TestTool_Classes;
using AnnoModificationManager4.ModificationTypes.Userdefined;
using System.Xml;

using System.IO;
using System.Diagnostics;

namespace DevelopmentTools.Tools.Global
{
    /// <summary>
    /// Interaction logic for TestTool.xaml
    /// </summary>
    public partial class TestTool : Window
    {
        public XmlDocument LastResult = new XmlDocument();

        public TestTool()
        {
            InitializeComponent();
        }

        private void button_StartTest_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
            Thread nthread = new Thread(new ThreadStart(Test));
            nthread.Start();
        }

        private void Test()
        {
            Test_Notify("Starting test ...");
            #region Prepare
            //Folders
            if (Directory.Exists(Modification.Development_CurrentModification.Folder + "_Test"))
            {
                Directory.Delete(Modification.Development_CurrentModification.Folder + "_Test", true);
            }
            Directory.CreateDirectory(Modification.Development_CurrentModification.Folder + "_Test");
            Directory.CreateDirectory(Modification.Development_CurrentModification.Folder + "_Test\\Original");
            Directory.CreateDirectory(Modification.Development_CurrentModification.Folder + "_Test\\Activated");
            Directory.CreateDirectory(Modification.Development_CurrentModification.Folder + "_Test\\Deactivated");


            Test_Notify("Preparing XmlModules ...");

            Dictionary<string, XMLFile> xmlFiles = new Dictionary<string, XMLFile>();
            foreach (XmlModuleList list in Modification.Development_CurrentModification.XmlModules)
            {
                foreach (IXMLModifier mod in list.Get())
                {
                    if (!xmlFiles.ContainsKey(mod.File))
                    {
                        Test_Notify("-> Creating temp. XMLFile \"" + mod.File + "\" ...");
                        XMLFile x = XMLFileCollector.Request(mod.File, true);
                        xmlFiles.Add(mod.File, x);
                    }

                    mod.TemporaryXMLFile = xmlFiles[mod.File];
                }
            }

            Test_Notify("Preparing ListModules ...");

            Dictionary<string, ListFile> listFiles = new Dictionary<string, ListFile>();
            foreach (ListModuleList list in Modification.Development_CurrentModification.ListModules)
            {
                foreach (IListModifier mod in list.Get())
                {
                    if (!listFiles.ContainsKey(mod.File))
                    {
                        Test_Notify("-> Creating temp. ListFile \"" + mod.File + "\" ...");
                        ListFile x = ListFileCollector.Request(mod.ListFile.FileName, true);
                        listFiles.Add(mod.File, x);
                    }

                    mod.TemporaryListFile = listFiles[mod.File];
                }
            }

            //Result
            XmlNode globalFirstNode = ResultList_Notify("Result", "information.png", LastResult);
            LastResult.AppendChild(globalFirstNode);
            #endregion

            #region XML
            XmlNode globalXml = ResultList_Notify("XmlModules", "tag.png", LastResult);

            #region Save Files Original
            foreach (KeyValuePair<string, XMLFile> file in xmlFiles)
            {
                file.Value.WriteContent(Modification.Development_CurrentModification.Folder + "_Test\\Original\\" + file.Key.FormatProjectPath().Replace(";", "\\"));
            }
            #endregion
            #region Test xml 1/4

            Test_Notify("Testing XmlModules (1/4 - Activating) ...");
            Dictionary<IXMLModifier, Exception> Xml_Test1_Exceptions = new Dictionary<IXMLModifier, Exception>();

            foreach (XmlModuleList list in Modification.Development_CurrentModification.XmlModules.OrderBy(l => l.Index))
            {
                Test_Notify("> " + list.Name);

                foreach (IXMLModifier mod in list.Get())
                {
                    try
                    {
                        if (!mod.IsActive)
                        {
                            continue;
                        }
                        if (mod.Validitate())
                            throw new Exception("Already activated!");
                        mod.Activate();
                    }
                    catch (Exception ex)
                    {
                        Xml_Test1_Exceptions.Add(mod, ex);
                    }

                    //Save File to "Activated" Folder                    
                }
            }
            #endregion
            #region Test xml 2/4
            Test_Notify("Testing XmlModules (2/4 - Testing Status) ...");

            XmlNode Xml_Test1 = ResultList_Notify("Test 1/2: Activating", "resultset_next.png", LastResult);

            foreach (XmlModuleList list in Modification.Development_CurrentModification.XmlModules.OrderBy(l => l.Index))
            {
                XmlNode Xml_CurrentModuleList = ResultList_Notify("XmlModuleList \"" + list.Name + "\"", "page_white_code.png", LastResult);
                XmlNode Xml_CurrentGroup = ResultList_Notify("General", "key.png", LastResult);

                foreach (IXMLModifier mod in list.Get())
                {
                    //Group
                    if (Xml_CurrentGroup.Attributes["Message"].Value != mod.Group)
                    {
                        if (Xml_CurrentGroup.ChildNodes.Count != 0)
                            Xml_CurrentModuleList.AppendChild(Xml_CurrentGroup);
                        Xml_CurrentGroup = ResultList_Notify(mod.Group, "key.png", LastResult);
                    }

                    XmlNode Xml_CurrentMod = ResultList_Notify("[" + mod.Index + "] " + mod.GetType().Name + " @ " + mod.File, "brick.png", LastResult);


                    foreach (XMLUserdefinedValue val in mod.UserdefinedValues)
                    {
                        if (!val.Check())
                        {
                            Xml_CurrentMod.AppendChild(ResultList_Notify("XmlUserdefinedValue \"" + val.UserdefinedValueName + "\" not found!", "error.png", LastResult));
                        }
                        if (!val.Math.Contains("{value}"))
                        {
                            Xml_CurrentMod.AppendChild(ResultList_Notify("Warning: Cannot assign setted UserdefinedValue, beacause '{value}' ist not existing!", "error.png", LastResult));
                        }
                    }

                    if (!mod.ValiditateUserdefinedValueAppend())
                    {
                        Xml_CurrentMod.AppendChild(ResultList_Notify("Userdefined value not assigned? ({ or } found)", "information.png", LastResult));
                    }

                    //Exception
                    if (Xml_Test1_Exceptions.ContainsKey(mod))
                    {
                        Xml_CurrentMod.AppendChild(ResultList_Notify("Error: " + Xml_Test1_Exceptions[mod].Message, "exclamation.png", LastResult));
                    }


                    if (mod.IsActive)
                    {

                        if (!mod.Validitate())
                        {
                            Xml_CurrentMod.AppendChild(ResultList_Notify("Modifier is deactivated!!!", "exclamation.png", LastResult));
                        }
                        else
                        {
                            Xml_CurrentMod.AppendChild(ResultList_Notify("Modifier is activated.", "tick.png", LastResult));
                        }
                    }
                    else
                    {
                        Xml_CurrentMod.AppendChild(ResultList_Notify("Modifier is not active.", "information.png", LastResult));
                    }

                    Xml_CurrentGroup.AppendChild(Xml_CurrentMod);
                }

                Xml_CurrentModuleList.AppendChild(Xml_CurrentGroup);
                Xml_Test1.AppendChild(Xml_CurrentModuleList);
            }

            globalXml.AppendChild(Xml_Test1);
            #endregion
            #region Save Files Activated
            foreach (KeyValuePair<string, XMLFile> file in xmlFiles)
            {
                file.Value.WriteContent(Modification.Development_CurrentModification.Folder + "_Test\\Activated\\" + file.Key.FormatProjectPath().Replace(";", "\\"));
            }
            #endregion
            #region Test xml 3/4
            Test_Notify("Testing XmlModules (3/4 - Deactivating) ...");
            Dictionary<IXMLModifier, Exception> Xml_Test2_Exceptions = new Dictionary<IXMLModifier, Exception>();

            foreach (XmlModuleList list in Modification.Development_CurrentModification.XmlModules.OrderByDescending(l => l.Index))
            {
                foreach (IXMLModifier mod in list.Get().OrderByDescending(md => md.Index))
                {
                    try
                    {
                        if (!mod.IsActive)
                            continue;

                        if (!mod.Validitate())
                            throw new Exception("Already deactivated!");
                        mod.Deactivate();
                    }
                    catch (Exception ex)
                    {
                        Xml_Test2_Exceptions.Add(mod, ex);
                    }
                }
            }
            #endregion
            #region Test xml 4/4
            Test_Notify("Testing XmlModules (4/4 - Testing Status) ...");

            XmlNode Xml_Test2 = ResultList_Notify("Test 2/2: Deactivating", "resultset_next.png", LastResult);

            foreach (XmlModuleList list in Modification.Development_CurrentModification.XmlModules.OrderBy(l => l.Index))
            {
                XmlNode Xml_CurrentModuleList = ResultList_Notify("XmlModuleList \"" + list.Name + "\"", "page_white_code.png", LastResult);
                XmlNode Xml_CurrentGroup = ResultList_Notify("General", "key.png", LastResult);

                foreach (IXMLModifier mod in list.Get())
                {
                    //Group
                    if (Xml_CurrentGroup.Attributes["Message"].Value != mod.Group)
                    {
                        if (Xml_CurrentGroup.ChildNodes.Count != 0)
                            Xml_CurrentModuleList.AppendChild(Xml_CurrentGroup);
                        Xml_CurrentGroup = ResultList_Notify(mod.Group, "key.png", LastResult);
                    }

                    XmlNode Xml_CurrentMod = ResultList_Notify("[" + mod.Index + "] " + mod.GetType().Name + " @ " + mod.File, "brick.png", LastResult);


                    foreach (XMLUserdefinedValue val in mod.UserdefinedValues)
                    {
                        if (!val.Check())
                        {
                            Xml_CurrentMod.AppendChild(ResultList_Notify("XmlUserdefinedValue \"" + val.UserdefinedValueName + "\" not found!", "error.png", LastResult));
                        }
                    }

                    //Exception
                    if (Xml_Test1_Exceptions.ContainsKey(mod))
                    {
                        Xml_CurrentMod.AppendChild(ResultList_Notify("Error: " + Xml_Test1_Exceptions[mod].Message, "exclamation.png", LastResult));
                    }

                    if (mod.IsActive)
                    {
                        if (mod.Validitate())
                        {
                            Xml_CurrentMod.AppendChild(ResultList_Notify("Modifier is activated!!!", "exclamation.png", LastResult));
                        }
                        else
                        {
                            Xml_CurrentMod.AppendChild(ResultList_Notify("Modifier is deactivated.", "tick.png", LastResult));
                        }
                    }
                    else
                    {
                        Xml_CurrentMod.AppendChild(ResultList_Notify("Modifier is not active.", "information.png", LastResult));
                    }

                    Xml_CurrentGroup.AppendChild(Xml_CurrentMod);
                }

                Xml_CurrentModuleList.AppendChild(Xml_CurrentGroup);
                Xml_Test2.AppendChild(Xml_CurrentModuleList);
            }

            globalXml.AppendChild(Xml_Test2);
            #endregion
            #region Save Files Deactivated
            foreach (KeyValuePair<string, XMLFile> file in xmlFiles)
            {
                file.Value.WriteContent(Modification.Development_CurrentModification.Folder + "_Test\\Deactivated\\" + file.Key.FormatProjectPath().Replace(";", "\\"));
            }
            #endregion

            globalFirstNode.AppendChild(globalXml);
            #endregion
            #region List
            XmlNode globalList = ResultList_Notify("ListModules", "page_white_text.png", LastResult);

            #region Save Files Original
            foreach (KeyValuePair<string, ListFile> file in listFiles)
            {
                file.Value.WriteContent(Modification.Development_CurrentModification.Folder + "_Test\\Original\\" + file.Key.FormatProjectPath().Replace(";", "\\"));
            }
            #endregion
            #region Test List 1/4

            Test_Notify("Testing ListModules (1/4 - Activating) ...");

            Dictionary<IListModifier, Exception> List_Test1_Exceptions = new Dictionary<IListModifier, Exception>();

            foreach (ListModuleList list in Modification.Development_CurrentModification.ListModules.OrderBy(l => l.Index))
            {
                Test_Notify("> " + list.Name);

                foreach (IListModifier mod in list.Get())
                {
                    try
                    {
                        if (!mod.IsActive)
                            continue;

                        if (mod.Validitate())
                            throw new Exception("Already activated!");
                        mod.Activate();
                    }
                    catch (Exception ex)
                    {
                        List_Test1_Exceptions.Add(mod, ex);
                    }
                }
            }
            #endregion
            #region Test List 2/4
            Test_Notify("Testing ListModules (2/4 - Testing Status) ...");

            XmlNode List_Test1 = ResultList_Notify("Test 1/2: Activating", "resultset_next.png", LastResult);

            foreach (ListModuleList list in Modification.Development_CurrentModification.ListModules.OrderBy(l => l.Index))
            {
                XmlNode List_CurrentModuleList = ResultList_Notify("ListModuleList \"" + list.Name + "\"", "page_white_code.png", LastResult);
                XmlNode List_CurrentGroup = ResultList_Notify("General", "key.png", LastResult);

                foreach (IListModifier mod in list.Get())
                {
                    //Group
                    if (List_CurrentGroup.Attributes["Message"].Value != mod.Group)
                    {
                        if (List_CurrentGroup.ChildNodes.Count != 0)
                            List_CurrentModuleList.AppendChild(List_CurrentGroup);
                        List_CurrentGroup = ResultList_Notify(mod.Group, "key.png", LastResult);
                    }

                    XmlNode List_CurrentMod = ResultList_Notify("[" + mod.Index + "] " + mod.GetType().Name + " @ " + mod.File, "brick.png", LastResult);


                    foreach (ListUserdefinedValue val in mod.UserdefinedValues)
                    {
                        if (!val.Check())
                        {
                            List_CurrentMod.AppendChild(ResultList_Notify("ListUserdefinedValue \"" + val.UserdefinedValueName + "\" not found!", "error.png", LastResult));
                        }
                        if (!val.Math.Contains("{value}"))
                        {
                            List_CurrentMod.AppendChild(ResultList_Notify("Warning: Cannot assign setted UserdefinedValue, beacause '{value}' ist not existing!", "error.png", LastResult));
                        }
                    }

                    if (!mod.ValiditateUserdefinedValueAppend())
                    {
                        List_CurrentMod.AppendChild(ResultList_Notify("Userdefined value not assigned? ({ or } found)", "information.png", LastResult));
                    }

                    //Exception
                    if (List_Test1_Exceptions.ContainsKey(mod))
                    {
                        List_CurrentMod.AppendChild(ResultList_Notify("Error: " + List_Test1_Exceptions[mod].Message, "exclamation.png", LastResult));
                    }

                    if (mod.IsActive)
                    {
                        if (!mod.Validitate())
                        {
                            List_CurrentMod.AppendChild(ResultList_Notify("Modifier is deactivated!!!", "exclamation.png", LastResult));
                        }
                        else
                        {
                            List_CurrentMod.AppendChild(ResultList_Notify("Modifier is activated.", "tick.png", LastResult));
                        }
                    }
                    else
                    {
                        List_CurrentMod.AppendChild(ResultList_Notify("Modifier is not active.", "information.png", LastResult));
                    }

                    List_CurrentGroup.AppendChild(List_CurrentMod);
                }

                List_CurrentModuleList.AppendChild(List_CurrentGroup);
                List_Test1.AppendChild(List_CurrentModuleList);
            }

            globalList.AppendChild(List_Test1);
            #endregion
            #region Save Files Activated
            foreach (KeyValuePair<string, ListFile> file in listFiles)
            {
                file.Value.WriteContent(Modification.Development_CurrentModification.Folder + "_Test\\Activated\\" + file.Key.FormatProjectPath().Replace(";", "\\"));
            }
            #endregion
            #region Test List 3/4
            Test_Notify("Testing ListModules (3/4 - Deactivating) ...");

            Dictionary<IListModifier, Exception> List_Test2_Exceptions = new Dictionary<IListModifier, Exception>();

            foreach (ListModuleList list in Modification.Development_CurrentModification.ListModules.OrderByDescending(l => l.Index))
            {
                foreach (IListModifier mod in list.Get().OrderByDescending(md => md.Index))
                {
                    try
                    {
                        if (!mod.IsActive)
                            continue;

                        if (!mod.Validitate())
                            throw new Exception("Already deactivated!");
                        mod.Deactivate();
                    }
                    catch (Exception ex)
                    {
                        List_Test2_Exceptions.Add(mod, ex);
                    }
                }
            }
            #endregion
            #region Test List 4/4
            Test_Notify("Testing ListModules (4/4 - Testing Status) ...");

            XmlNode List_Test2 = ResultList_Notify("Test 2/2: Deactivating", "resultset_next.png", LastResult);

            foreach (ListModuleList list in Modification.Development_CurrentModification.ListModules.OrderBy(l => l.Index))
            {
                XmlNode List_CurrentModuleList = ResultList_Notify("ListModuleList \"" + list.Name + "\"", "page_white_code.png", LastResult);
                XmlNode List_CurrentGroup = ResultList_Notify("General", "key.png", LastResult);

                foreach (IListModifier mod in list.Get())
                {
                    //Group
                    if (List_CurrentGroup.Attributes["Message"].Value != mod.Group)
                    {
                        if (List_CurrentGroup.ChildNodes.Count != 0)
                            List_CurrentModuleList.AppendChild(List_CurrentGroup);
                        List_CurrentGroup = ResultList_Notify(mod.Group, "key.png", LastResult);
                    }

                    XmlNode List_CurrentMod = ResultList_Notify("[" + mod.Index + "] " + mod.GetType().Name + " @ " + mod.File, "brick.png", LastResult);


                    foreach (ListUserdefinedValue val in mod.UserdefinedValues)
                    {
                        if (!val.Check())
                        {
                            List_CurrentMod.AppendChild(ResultList_Notify("ListUserdefinedValue \"" + val.UserdefinedValueName + "\" not found!", "error.png", LastResult));
                        }
                    }

                    //Exception
                    if (List_Test2_Exceptions.ContainsKey(mod))
                    {
                        List_CurrentMod.AppendChild(ResultList_Notify("Error: " + List_Test2_Exceptions[mod].Message, "exclamation.png", LastResult));
                    }

                    if (mod.IsActive)
                    {
                        if (mod.Validitate())
                        {
                            List_CurrentMod.AppendChild(ResultList_Notify("Modifier is activated!!!", "exclamation.png", LastResult));
                        }
                        else
                        {
                            List_CurrentMod.AppendChild(ResultList_Notify("Modifier is deactivated.", "tick.png", LastResult));
                        }
                    }
                    else
                    {
                        List_CurrentMod.AppendChild(ResultList_Notify("Modifier is not active.", "information.png", LastResult));
                    }

                    List_CurrentGroup.AppendChild(List_CurrentMod);
                }

                List_CurrentModuleList.AppendChild(List_CurrentGroup);
                List_Test2.AppendChild(List_CurrentModuleList);
            }

            globalList.AppendChild(List_Test2);
            #endregion
            #region Save Files Deactivated
            foreach (KeyValuePair<string, ListFile> file in listFiles)
            {
                file.Value.WriteContent(Modification.Development_CurrentModification.Folder + "_Test\\Deactivated\\" + file.Key.FormatProjectPath().Replace(";", "\\"));
            }
            #endregion

            globalFirstNode.AppendChild(globalList);
            #endregion

            #region Finish
            Test_Notify("Cleaning XmlModules ...");

            foreach (XmlModuleList list in Modification.Development_CurrentModification.XmlModules)
            {
                foreach (IXMLModifier mod in list.Get())
                {
                    mod.TemporaryXMLFile = null;
                }
            }

            Test_Notify("Cleaning ListModules ...");

            foreach (ListModuleList list in Modification.Development_CurrentModification.ListModules)
            {
                foreach (IListModifier mod in list.Get())
                {
                    mod.TemporaryListFile = null;
                }
            }
            #endregion

            tabControl.Dispatch(tab =>
                {
                    Result_Show(LastResult);
                    tab.SelectedIndex = 2;
                });
        }

        private void Result_Show(XmlDocument doc)
        {
            foreach (XmlNode nd in doc.FirstChild.ChildNodes)
            {
                resultList.Items.Add(Result_Show_Generate(nd));
            }
            resultList.ExpandAll();
        }

        private TreeViewItem Result_Show_Generate(XmlNode nd)
        {
            TreeViewItem trv = new TreeViewItem();

            StackPanel pnl = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
            pnl.Children.Add(new Image()
            {
                Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/" + nd.Attributes["Icon"].Value))
            });
            pnl.Children.Add(new TextBlock()
            {
                Text = nd.Attributes["Message"].Value,
                Margin = new Thickness(3, 0, 0, 0)
            });
            trv.Header = pnl;

            foreach (XmlNode ch in nd.ChildNodes)
            {
                trv.Items.Add(Result_Show_Generate(ch));
            }

            return trv;
        }

        private XmlNode ResultList_Notify(string msg, string icon, XmlDocument doc)
        {
            XmlNode nd = doc.CreateNode(XmlNodeType.Element, "Element", null);
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Message", msg));
            nd.Attributes.Append(XmlExtension.CreateAttribute(doc, "Icon", icon));

            return nd;
        }

        private void Test_Notify(string msg)
        {
            testList.Dispatch(list =>
                {
                    list.Items.Add(new ListViewItem() { Content = msg });
                });
        }

        private void button_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void button_SaveResult_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Result|*.xml";

            try
            {
                if (save.ShowDialog() == true)
                {
                    if (File.Exists(save.FileName))
                        File.Delete(save.FileName);
                    LastResult.Save(save.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
            }
        }

        private void button_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Modification.Development_CurrentModification.Folder + "_Test");
        }
    }
}
