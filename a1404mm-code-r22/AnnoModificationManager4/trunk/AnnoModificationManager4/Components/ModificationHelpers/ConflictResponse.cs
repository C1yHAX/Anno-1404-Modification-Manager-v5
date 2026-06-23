using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager4.ModificationTypes;
using xmltype = AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers;
using listtype = AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers;
using System.IO;
using System.Xml;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.ModificationTypes.ListModule;
using AnnoModificationManager4.Language.DictionarySystem;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AnnoModificationManager4.Components.ModificationHelpers
{
    public class ConflictResponse
    {
        public Modification CurrentModification;
        public Modification ToCheck;

        /// <summary>
        /// Root is %Anno% [string]
        /// </summary>
        public List<string> ConflictFiles_Anno = new List<string>();
        /// <summary>
        /// Root is %AppData% [string]
        /// </summary>
        public List<string> ConflictFiles_AppData = new List<string>();
        /// <summary>
        /// Of ToCheck
        /// </summary>
        public List<xmltype.IXMLModifier> ConflictXmlModules = new List<xmltype.IXMLModifier>();
        /// <summary>
        /// Of ToCheck
        /// </summary>
        public List<listtype.IListModifier> ConflictListModules = new List<listtype.IListModifier>();

        public int ConflictCount
        {
            get
            {
                return ConflictFiles_Anno.Count +
                ConflictFiles_AppData.Count +
                ConflictXmlModules.Count +
                ConflictListModules.Count;
            }
        }

        //UserInterface
        public string UI_ModificationName
        {
            get
            {
                return CurrentModification.Info.Name.Get;
            }
        }

        public string UI_ConflictCountString
        {
            get
            {
                return LanguageDictionary.Get("ActivationDialog", "Conflict_ConflictionCountString").
                    Replace("{0}", ConflictCount.ToString());
            }
        }

        public string UI_ModifierCountString
        {
            get
            {
                return LanguageDictionary.Get("DeactivationDialog", "ModifierCountString").
                    Replace("{0}", ConflictCount.ToString());
            }
        }

        public object UI_ConflictTypesIconsStack
        {
            get
            {
                StackPanel stack = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                #region Files
                if (ConflictFiles_Anno.Count + ConflictFiles_AppData.Count > 0)
                {
                    StackPanel s = new StackPanel()
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new System.Windows.Thickness(5, 0, 0, 0)
                    };

                    s.Children.Add(new TextBlock()
                    {
                        Text = (ConflictFiles_Anno.Count + ConflictFiles_AppData.Count).ToString(),                       
                    });
                    s.Children.Add(new Image()
                    {
                        Width = 16,
                        Height = 16,
                        Margin = new System.Windows.Thickness(3, 0, 5, 0),
                        Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white.png"))
                    });

                    stack.Children.Add(s);
                } 
                #endregion
                #region Xml
                if (ConflictXmlModules.Count > 0)
                {
                    StackPanel s = new StackPanel()
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new System.Windows.Thickness(5, 0, 0, 0)
                    };

                    s.Children.Add(new TextBlock()
                    {
                        Text = ConflictXmlModules.Count.ToString()
                    });
                    s.Children.Add(new Image()
                    {
                        Width = 16,
                        Height = 16,
                        Margin = new System.Windows.Thickness(3, 0, 0, 0),
                        Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_code.png"))
                    });

                    stack.Children.Add(s);
                } 
                #endregion
                #region List
                if (ConflictListModules.Count > 0)
                {
                    StackPanel s = new StackPanel()
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new System.Windows.Thickness(5, 0, 0, 0)
                    };

                    s.Children.Add(new TextBlock()
                    {
                        Text = ConflictListModules.Count.ToString()
                    });
                    s.Children.Add(new Image()
                    {
                        Width = 16,
                        Height = 16,
                        Margin = new System.Windows.Thickness(3, 0, 0, 0),
                        Source = BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_text.png"))
                    });

                    stack.Children.Add(s);
                } 
                #endregion

                return stack;
            }
        }

        public string UI_Deactivation_Assign
        {
            get
            {
                return AssignDeactivationToModification().ToLocatedString();
            }
        }


        /// <summary>
        /// Generates a Conflict Response, Recognizes Conflicts, if Modifier ist enabled, only!!!
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public static ConflictResponse Generate(Modification curr, Modification check)
        {
            ConflictResponse resp = new ConflictResponse();
            resp.CurrentModification = check;
            resp.ToCheck = check;

            List<string> LockedFiles_Curr = curr.ModificationUtils.Files_GetLockedFiles_InProject;
            List<string> LockedFiles_Check = check.ModificationUtils.Files_GetLockedFiles_InProject;

            List<string> AnnoFiles_Curr = curr.ModificationUtils.Files_Anno;
            List<string> AnnoFiles_Check = check.ModificationUtils.Files_Anno;

            List<string> AppDataFiles_Curr = curr.ModificationUtils.Files_AppData;
            List<string> AppDataFiles_Check = check.ModificationUtils.Files_AppData;

            #region Anno 1404 Directory
            foreach (string file in AnnoFiles_Curr)
            {
                //Is Locked => continue
                if (LockedFiles_Curr.Contains(file))
                    continue;

                string cleanfile = file.Replace(curr.Folder + "\\Files\\Anno1404", "");

                if (AnnoFiles_Check.Find(
                    f =>
                    {
                        //Is Locked => continue
                        if (LockedFiles_Check.Contains(file))
                            return false;

                        return cleanfile == f.Replace(check.Folder + "\\Files\\Anno1404", "");
                    }) != null)
                {
                    resp.ConflictFiles_Anno.Add("%Anno%" + cleanfile);
                }
            } 
            #endregion
            #region AppData Directory
            foreach (string file in AppDataFiles_Curr)
            {
                //Is Locked => continue
                if (LockedFiles_Curr.Contains(file))
                    continue;

                string cleanfile = file.Replace(curr.Folder + "\\Files\\AppData", "");

                if (AppDataFiles_Check.Find(
                    f =>
                    {
                        //Is Locked => continue
                        if (LockedFiles_Check.Contains(file))
                            return false;

                        return cleanfile == f.Replace(check.Folder + "\\Files\\AppData", "");
                    }) != null)
                {
                    resp.ConflictFiles_AppData.Add("%AppData%" + cleanfile);
                }
            }
            #endregion
            #region XmlModules
            //All Xml-Modules / File <->
            foreach (xmltype.IXMLModifier mod in curr.ModificationUtils.Xml_AllModififers)
            {
                string modfile = mod.File.Replace("%Anno%", check.Folder + "\\Files\\Anno1404")
                    .Replace("%AppData%", check.Folder + "\\Files\\AppData");

                if (AnnoFiles_Check.Contains(modfile))
                {
                    resp.ConflictFiles_Anno.Add(modfile.Replace(check.Folder + "\\Files\\Anno1404", "%Anno%")
                        .Replace(check.Folder + "\\Files\\AppData", "%AppData%"));
                }
            }
            foreach (xmltype.IXMLModifier mod in check.ModificationUtils.Xml_AllModififers)
            {
                string modfile = mod.File.Replace("%Anno%", curr.Folder + "\\Files\\Anno1404")
                    .Replace("%AppData%", curr.Folder + "\\Files\\AppData");

                if (AnnoFiles_Curr.Contains(modfile))
                {
                    resp.ConflictXmlModules.Add(mod);
                }
            }
            //Edit
            foreach (KeyValuePair<XmlNode, List<xmltype.IXMLModifier>> node in curr.ModificationUtils.Xml_EditModifiers)
            {
                foreach (KeyValuePair<XmlNode, List<xmltype.IXMLModifier>> chnode in check.ModificationUtils.Xml_EditModifiers)
                {
                    if (XmlExtension.IsOrContainsParent(node.Key, chnode.Key))
                    {                      
                        resp.ConflictXmlModules.AddRange(chnode.Value);
                    }
                }
            }
            //Add
            foreach (KeyValuePair<XmlNode, List<xmltype.IXMLModifier>> node in curr.ModificationUtils.Xml_AddModifiers)
            {
                if (check.ModificationUtils.Xml_AddModifiers.ContainsKey(node.Key))
                {
                    //Find modifiers, which TagName and InnerXml equals
                    foreach (xmltype.AddModifier add in node.Value)
                    {
                        xmltype.IXMLModifier ctest = check.ModificationUtils.Xml_AddModifiers[node.Key].Find(addmodifier =>
                            {                              
                                return ((addmodifier as xmltype.AddModifier).TagName == add.TagName
                                    && (addmodifier as xmltype.AddModifier).Value == add.Value);
                            });
                        if (ctest != null)
                        {
                            resp.ConflictXmlModules.Add(ctest);
                        }
                    }
                }
            }
            //Remove
            foreach (KeyValuePair<XmlNode, List<xmltype.IXMLModifier>> node in curr.ModificationUtils.Xml_RemoveModifiers)
            {
                //Find same RemoveModifiers                   
                if (check.ModificationUtils.Xml_RemoveModifiers.ContainsKey(node.Key))
                {                  
                    resp.ConflictXmlModules.AddRange(check.ModificationUtils.Xml_RemoveModifiers[node.Key]);
                }

                //Check other types
                foreach (XmlNode addnode in check.ModificationUtils.Xml_AddModifiers.Keys)
                {
                    if (XmlExtension.IsOrContainsParent(node.Key, addnode))
                    {                       
                        resp.ConflictXmlModules.AddRange(check.ModificationUtils.Xml_AddModifiers[addnode]);
                    }
                }
                foreach (XmlNode addnode in check.ModificationUtils.Xml_EditModifiers.Keys)
                {
                    if (XmlExtension.IsOrContainsParent(node.Key, addnode))
                    {                       
                        resp.ConflictXmlModules.AddRange(check.ModificationUtils.Xml_EditModifiers[addnode]);
                    }
                }
            }
            #endregion
            #region ListModules
            //All Xml-Modules / File <->
            foreach (listtype.IListModifier mod in curr.ModificationUtils.List_AllModififers)
            {
                string modfile = mod.File.Replace("%Anno%", check.Folder + "\\Files\\Anno1404")
                    .Replace("%AppData%", check.Folder + "\\Files\\AppData");

                if (AnnoFiles_Check.Contains(modfile))
                {
                    resp.ConflictFiles_Anno.Add(modfile.Replace(check.Folder + "\\Files\\Anno1404", "%Anno%")
                        .Replace(check.Folder + "\\Files\\AppData", "%AppData%"));
                }
            }
            foreach (listtype.IListModifier mod in check.ModificationUtils.List_AllModififers)
            {
                string modfile = mod.File.Replace("%Anno%", curr.Folder + "\\Files\\Anno1404")
                    .Replace("%AppData%", curr.Folder + "\\Files\\AppData");

                if (AnnoFiles_Curr.Contains(modfile))
                {
                    resp.ConflictListModules.Add(mod);
                }
            }
            //Add
            foreach (KeyValuePair<ListFile, List<listtype.IListModifier>> listnode in curr.ModificationUtils.List_AddModifiers)
            {
                if (check.ModificationUtils.List_AddModifiers.ContainsKey(listnode.Key))
                {
                    foreach (listtype.AddModifier add in listnode.Value)
                    {
                        try
                        {
                            listtype.IListModifier tosearch =
                                               check.ModificationUtils.List_AddModifiers[listnode.Key].Find(a => a.ElementValue == add.ElementValue);

                            if (tosearch != null)
                            {
                                resp.ConflictListModules.Add(tosearch);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            //Edit
            foreach (KeyValuePair<ListFile, List<listtype.IListModifier>> listnode in curr.ModificationUtils.List_EditModifiers)
            {
                if (check.ModificationUtils.List_AddModifiers.ContainsKey(listnode.Key))
                {
                    foreach (listtype.EditModifier add in listnode.Value)
                    {
                        try
                        {
                            listtype.IListModifier tosearch =
                                                check.ModificationUtils.List_EditModifiers[listnode.Key].Find(a => a.ElementValue == add.ElementValue);

                            if (tosearch != null)
                            {
                                resp.ConflictListModules.Add(tosearch);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            //Remove
            //Add
            foreach (KeyValuePair<ListFile, List<listtype.IListModifier>> listnode in curr.ModificationUtils.List_RemoveModifiers)
            {
                if (check.ModificationUtils.List_RemoveModifiers.ContainsKey(listnode.Key))
                {
                    //Find same
                    foreach (listtype.RemoveModifier add in listnode.Value)
                    {
                        try
                        {
                            listtype.IListModifier tosearch =
                                                check.ModificationUtils.List_AddModifiers[listnode.Key].Find(a => a.ElementValue == add.ElementValue);

                            if (tosearch != null)
                            {
                                resp.ConflictListModules.Add(tosearch);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    //Other types
                    if (check.ModificationUtils.List_AddModifiers.ContainsKey(listnode.Key)) //Add
                    {
                        foreach (listtype.RemoveModifier add in listnode.Value)
                        {
                            try
                            {
                                listtype.IListModifier tosearch =
                                                       check.ModificationUtils.List_AddModifiers[listnode.Key].Find(a => a.ElementValue == add.ElementValue);

                                if (tosearch != null)
                                {
                                    resp.ConflictListModules.Add(tosearch);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    if (check.ModificationUtils.List_EditModifiers.ContainsKey(listnode.Key)) //Edit
                    {
                        foreach (listtype.RemoveModifier add in listnode.Value)
                        {
                            try
                            {
                                listtype.IListModifier tosearch =
                                                        check.ModificationUtils.List_EditModifiers[listnode.Key].Find(a => a.ElementValue == add.ElementValue);

                                if (tosearch != null)
                                {
                                    resp.ConflictListModules.Add(tosearch);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            #endregion

            resp.ConflictListModules = resp.ConflictListModules.Distinct().ToList();
            resp.ConflictXmlModules = resp.ConflictXmlModules.Distinct().ToList();
            resp.RemoveDisabledModifiers();
            return resp;
        }

        public bool IsConflict()
        {
            return (ConflictCount != 0);              
        }

        public void RemoveDisabledModifiers()
        {
            ConflictFiles_Anno.RemoveAll(str =>
                {
                    return !File.Exists(str.Replace("%Anno%", AnnoDirectoryHandler.GetCurrent()));
                });
            ConflictFiles_AppData.RemoveAll(str =>
            {
                return !File.Exists(str.Replace("%AppData%", DirectoryExtension.GetAppDataFolder()));
            });
            ConflictXmlModules.RemoveAll(xml => !xml.Validitate());
            ConflictListModules.RemoveAll(list => !list.Validitate());
        }

        public ModificationActivationResponse AssignDeactivationToModification()
        {
            ModificationActivationResponse mod = new ModificationActivationResponse();
            mod.FileModuleActive = ModificationHandler.ActivationResponses[CurrentModification].FileModuleActive
                - (ConflictFiles_Anno.Count + ConflictFiles_AppData.Count);
            mod.ListModuleActive = ModificationHandler.ActivationResponses[CurrentModification].ListModuleActive -
                ConflictListModules.Count;
            mod.XmlModuleActive = ModificationHandler.ActivationResponses[CurrentModification].XmlModuleActive -
                ConflictXmlModules.Count;

            return mod;
        }

        public static ConflictResponse Merge(List<ConflictResponse> resps)
        {
            ConflictResponse output = new ConflictResponse();
            foreach (ConflictResponse r in resps)
            {
                output.ConflictFiles_Anno.AddRange(r.ConflictFiles_Anno);
                output.ConflictFiles_AppData.AddRange(r.ConflictFiles_AppData);
                output.ConflictListModules.AddRange(r.ConflictListModules);
                output.ConflictXmlModules.AddRange(r.ConflictXmlModules);
            }

            return output;
        }
    }
}
