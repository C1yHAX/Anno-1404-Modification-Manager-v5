using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.Components;
using System.Windows.Controls;
using AnnoModificationManager5.Language.DictionarySystem;

namespace AnnoModificationManager5.ModificationTypes
{

    public class ModificationUICollector
    {
        public Modification CurrentModification;

        public ImageSource Icon
        {
            get
            {
                if (!ModificationHandler.Instance.IsCompatible(CurrentModification))
                {
                    return App.Current.Dispatch(app =>
                    {
                        return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/error.png"));
                    });
                }
                if (ModificationHandler.ActivationResponses.ContainsKey(CurrentModification))
                {
                    Enums.Modification_ActivationStatus resp = ModificationHandler.ActivationResponses[CurrentModification].Result();

                    if (resp == Enums.Modification_ActivationStatus.Activated)
                    {
                        return App.Current.Dispatch(app =>
                        {
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/tick.png"));
                        });
                    }
                    if (resp == Enums.Modification_ActivationStatus.Deactivated)
                    {
                        return App.Current.Dispatch(app =>
                        {
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/cross.png"));
                        });
                    }

                    #region Define partially
                    //Only Files
                    if (ModificationHandler.ActivationResponses[CurrentModification].FileModuleActive != 0
                        && (ModificationHandler.ActivationResponses[CurrentModification].ListModuleActive +
                        ModificationHandler.ActivationResponses[CurrentModification].XmlModuleActive) == 0)
                    {
                        return App.Current.Dispatch(app =>
                        {
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white.png"));
                        });
                    }
                    //Only ListModules
                    if (ModificationHandler.ActivationResponses[CurrentModification].ListModuleActive != 0
                        && (ModificationHandler.ActivationResponses[CurrentModification].FileModuleActive +
                        ModificationHandler.ActivationResponses[CurrentModification].XmlModuleActive) == 0)
                    {
                        return App.Current.Dispatch(app =>
                        {
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_text.png"));
                        });
                    }
                    //Only XmlModules
                    if (ModificationHandler.ActivationResponses[CurrentModification].XmlModuleActive != 0
                        && (ModificationHandler.ActivationResponses[CurrentModification].ListModuleActive +
                        ModificationHandler.ActivationResponses[CurrentModification].FileModuleActive) == 0)
                    {
                        return App.Current.Dispatch(app =>
                        {
                            return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/page_white_code.png"));
                        });
                    }
                    #endregion

                    return App.Current.Dispatch(app =>
                    {
                        return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/information.png"));
                    });
                }
                else
                {
                    return App.Current.Dispatch(app =>
                     {
                         return BitmapImageExtension.Load(("pack://application:,,,/Images/Icons/hourglass.png"));
                     });
                }
            }
        }

        public string AnnoExecutable
        {
            get
            {
                int mode = AnnoExecutableInteger;

                if (mode == 0)
                    return "Anno 1404";
                else if (mode == 1)
                    return "Anno 1404 " + LanguageDictionary.Get("UserInterface", "Venice");
                else if (mode == 3)
                    return "Anno I.A.A.M.";
                else
                    return LanguageDictionary.Get("UserInterface", "Several");
            }
        }

        /// <summary>
        /// 0=Ret
        /// 1=Add
        /// 2=several, all
        /// 3=iaam
        /// </summary>
        public int AnnoExecutableInteger
        {
            get
            {
                //0 = Ret
                //1 = Add
                //2 = r + a
                int mode = 0;

                if (CurrentModification.Info.AnnoVersions.Contains("All"))
                    mode = 2;
                else if (CurrentModification.Info.AnnoVersions.Contains("IAAM"))
                {
                    mode = 3;

                    if (CurrentModification.Info.AnnoVersions.Count != 0)
                        mode = 2;
                }
                else if (CurrentModification.Info.AnnoVersions.Find(str => str.Contains("Addon")) != null)
                {
                    mode = 1;
                    if (CurrentModification.Info.AnnoVersions.Count != CurrentModification.Info.AnnoVersions.Count(str => str.Contains("Addon")))
                        mode = 2;
                }
                else
                    mode = 0;

                return mode;
            }
        }

        public string Category
        {
            get
            {
                return CurrentModification.Info.Category.Get;
            }
        }
        public string Name
        {
            get
            {
                return CurrentModification.Info.Name.Get;
            }
        }
        public string Author
        {
            get
            {
                return CurrentModification.Info.Author;
            }
        }

        public string VersionString
        {
            get
            {
                return "Version " + CurrentModification.Info.Version;
            }
        }

        public ModificationUICollector(Modification mod)
        {
            CurrentModification = mod;
        }
    }
}
