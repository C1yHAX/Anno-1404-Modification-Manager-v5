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
using AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers;
using ICSharpCode.AvalonEdit.Highlighting;
using AnnoModificationManager4.ModificationTypes.Userdefined;
using DevelopmentTools.Editors.XmlModule.Controls;
using AnnoModificationManager4.UserInterface.Misc;
using AnnoModificationManager4.ModificationTypes.XmlModule;
using System.Xml;
using AnnoModificationManager4.Misc;
using usd = AnnoModificationManager4.ModificationTypes.Userdefined;
using System.Xml.Linq;
using DevelopmentTools.Tools.Global;

namespace DevelopmentTools.Editors.XmlModule.ModuleEditors
{
    /// <summary>
    /// Interaction logic for XmlEditCreator.xaml
    /// </summary>
    public partial class XmlRemoveEditor : UserControl
    {
        public new XmlModuleEditor_Main Parent;
        public RemoveModifier Modifier;
        public bool IsEdit = false;

        public XmlRemoveEditor(RemoveModifier mod)
        {
            Modifier = mod;
            InitializeComponent();
            Field_InnerXml.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");

            LoadModification();
        }

        private void LoadModification()
        {
            #region Standard
            Field_Selector.Text = Modifier.Selector;
            Field_Deselector.Text = Modifier.DeSelector;
            Field_DestinationFile.Text = Modifier.File;

            UserdefinedValues_List.Items.Clear();
            foreach (XMLUserdefinedValue val in Modifier.UserdefinedValues)
            {
                UserdefinedValues_List.Items.Add(val);
            }
            #endregion

            Field_TagName.Text = Modifier.TagName;
            //Field_InnerXml.Text = Modifier.InnerXml.Replace("><", ">\r\n<");
            Field_InnerXml.Text = XExtension.IndentString(Modifier.InnerXml);
            Field_AddBefore.Value = Modifier.InsertBeforeIndex;

            CodeExtension.TC(() =>
            {
                List<XmlNode> w = Modifier.XMLFile.Select(Modifier.DeSelector);
                if (w.Count != 1)
                    throw new Exception();

                Field_AddBefore.Maximum = w[0].ChildNodes.Count;
            },
                (ex) => Field_AddBefore.Maximum = 35840);
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            #region Standard
            Modifier.Selector = Field_Selector.Text;
            Modifier.DeSelector = Field_Deselector.Text;

            Modifier.UserdefinedValues.Clear();
            foreach (XMLUserdefinedValue val in UserdefinedValues_List.Items)
            {
                Modifier.UserdefinedValues.Add(val);
            }
            #endregion


            Modifier.InnerXml = XmlExtension.RemoveEmptys(Field_InnerXml.Text);
            Modifier.TagName = Field_TagName.Text.Replace("<", "").Replace(">", "").Replace(" ", "");
            Modifier.InsertBeforeIndex = (int)Field_AddBefore.Value;
            Modifier.File = Field_DestinationFile.Text;

            if (IsEdit)
            {
                Parent.CurrentModuleList.Edit(Modifier);
                Parent.ModuleList_RefreshSelectedItem();
            }
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            LoadModification();
        }

        private void UserdefinedValues_List_Add_Click(object sender, RoutedEventArgs e)
        {
            XMLUserdefinedValue val = new XMLUserdefinedValue();
            val.Parent = Modifier;
            val.Key = "{" + UserdefinedValues_List.Items.Count + "}";

            UserdefinedValues_List.Items.Add(val);
        }

        private void UserdefinedValues_List_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (UserdefinedValues_List.SelectedItem != null)
            {
                UserdefinedValues_List.Items.Remove(UserdefinedValues_List.SelectedItem);
            }
        }

        private void Field_Selector_Inspect_Click(object sender, RoutedEventArgs e)
        {
            XmlFileTreeViewInspector ins = new XmlFileTreeViewInspector(Modifier.Parent.Parent);
            ins.File = Modifier.XMLFile;
            ins.Selector = Field_Selector.Text;

            ins.ShowDialog();
        }

        private void Field_TagName_Inspect_Click(object sender, RoutedEventArgs e)
        {
            XmlFileTreeViewInspector ins = new XmlFileTreeViewInspector(Modifier.Parent.Parent);
            ins.File = Modifier.XMLFile;
            ins.Selector = Field_Deselector.Text + "/" + Field_TagName.Text;

            ins.ShowDialog();
        }

        private void button_test_Click(object sender, RoutedEventArgs e)
        {
            #region
            RemoveModifier Modifier = new RemoveModifier();
            Modifier.File = this.Modifier.File;
            Modifier.Parent = this.Modifier.Parent;

            #region Standard
            Modifier.Selector = Field_Selector.Text;
            Modifier.DeSelector = Field_Deselector.Text;

            Modifier.UserdefinedValues.Clear();
            foreach (XMLUserdefinedValue val in UserdefinedValues_List.Items)
            {
                Modifier.UserdefinedValues.Add(val);
            }
            #endregion

            Modifier.TagName = Field_TagName.Text;
            #endregion

            string testlog = "";

            testlog += "Checking XMLUserdefinedValues [Mod <= XMLU <= U] ...\n";
            foreach (XMLUserdefinedValue val in Modifier.UserdefinedValues)
            {
                testlog += val.UserdefinedValueName + " -> " + (val.Check() ? "OK." : "Not found!") + "\n";
            }

            testlog += "\nTesting modifier ...\n";
            try
            {
                testlog += "Creating temporary XmlFile ...\n";
                XMLFile tempfile = XMLFileCollector.Request(Modifier.File, true);
                Modifier.TemporaryXMLFile = tempfile;

                testlog += "(1/2) Test: Activating modifier ...\n";
                Modifier.Activate();
                testlog += "=> " + (Modifier.Validitate() ? "Activated.\n" : "Not activated!!!\n");

                testlog += "(2/2) Test: Deactivating modifier ...\n";
                Modifier.Deactivate();
                testlog += "=> " + (Modifier.Validitate() ? "Activated!!!\n" : "Not activated.\n");
            }
            catch (Exception ex)
            {
                testlog += "Error: " + ex.Message + "\n";
            }

            if (!testlog.Contains("!!!") && !testlog.Contains("Error"))
                testlog += "\nTest successful.";
            else
                testlog += "\nTest failed!";

            Modifier.TemporaryXMLFile = null;
            MessageWindow.Show(testlog);
        }

        private void Field_TagName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Check for Incompatible Text
            if (Field_TagName.Text.Contains("<")
                || Field_TagName.Text.Contains(">")
                || Field_TagName.Text.Contains(" "))
            {
                int idx = Field_TagName.CaretIndex;
                Field_TagName.Text = Field_TagName.Text.Replace("<", "")
                    .Replace(">", "")
                    .Replace(" ", "");

                try
                {
                    if (idx <= Field_TagName.Text.Length)
                        Field_TagName.CaretIndex = idx;
                    else
                        Field_TagName.CaretIndex = Field_TagName.Text.Length;
                }
                catch (Exception)
                {
                }
            }
        }

        private void Field_DestinationFile_Inspect_Click(object sender, RoutedEventArgs e)
        {
            FileBrowser.Process_OpenFile(Modifier.File);
        }

        private void UserdefinedValue_ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox Sender = sender as ComboBox;
            Sender.Items.Clear();

            foreach (usd.UserdefinedValue val in Project.Development_CurrentProject.Modification.UserdefinedValues)
            {
                Sender.Items.Add(new ComboBoxItem() { Content = val.Name });
            }
        }

        private void button_select_Click(object sender, RoutedEventArgs e)
        {
            Parent.ModuleCreator.Selectors_Selector.Text = Field_Selector.Text;
            Parent.ModuleCreator.Selectors_Deselector.Text = Field_Deselector.Text;
            Parent.ModuleCreator.Files_File.Text = Modifier.File;
            (Parent.ModuleList.Items[0] as TreeViewItem).IsSelected = true;
            Parent.ModuleCreator.Selector_Selector_Refresh_Click(null, null);
        }

        private void Field_DestinationFile_DropDownOpened(object sender, EventArgs e)
        {
            Field_DestinationFile.ItemsSource = Project.Development_CurrentProject.Modification.CollectedFiles_Xml_List;
            Field_DestinationFile.Items.Refresh();
        }

        private void b_DestinationFile_Select_Click(object sender, RoutedEventArgs e)
        {
            FileBrowser browser = new FileBrowser();
            if (browser.ShowDialog() == true)
            {
                Field_DestinationFile.Text = browser.ChoosenFile;
            }
        }
    }
}
