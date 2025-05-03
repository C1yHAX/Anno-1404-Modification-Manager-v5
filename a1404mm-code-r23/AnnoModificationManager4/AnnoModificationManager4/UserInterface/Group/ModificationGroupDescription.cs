using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using AnnoModificationManager4.ModificationTypes;
using AnnoModificationManager4.Components;
using AnnoModificationManager4.Language.DictionarySystem;
using AnnoModificationManager4.Misc;

namespace AnnoModificationManager4.UserInterface.Group
{   
    public class ModificationGroupDescription : GroupDescription
    {
        public string PropertyName = Properties.Settings.Default.modificationList_SortProperty;

        public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
        {
            Modification mod = item as Modification;

            switch (PropertyName)
            {               
                case "Author":
                    return mod.UICollector.Author;
                case "Status":
                    if (ModificationHandler.ActivationResponses.ContainsKey(mod))
                    {
                        return ModificationHandler.ActivationResponses[mod].ToLocatedString();
                    }
                    else
                        return LanguageDictionary.Get("UserInterface", "Unknown");    
                case "AnnoVersions":
                    return StringExtension.PutTogetherComma(mod.Info.AnnoVersions);
                /*default:
                    return mod.UICollector.AnnoExecutable + "|" + mod.UICollector.Category;*/
                default:
                return mod.UICollector.Category;
            }
        }
    }
}
