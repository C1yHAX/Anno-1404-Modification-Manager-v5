using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.Language.DictionarySystem;

namespace AnnoModificationManager4.ModificationTypes
{
    public class ModificationActivationResponse
    {
        public StringBuilder Log = new StringBuilder();

        public int XmlModuleCount;
        public int XmlModuleActive;

        public int ListModuleCount;
        public int ListModuleActive;

        public int FileModuleCount;
        public int FileModuleActive;

        //Not used
        public int FileModuleAnnoCount;
        public int FileModuleAnnoActive;

        public int FileModuleAppDataCount;
        public int FileModuleAppDataActive;

        public Enums.Modification_ActivationStatus Result()
        {
            if (XmlModuleActive + ListModuleActive + FileModuleActive <= 0)
            {
                return Enums.Modification_ActivationStatus.Deactivated;
            }
            if (XmlModuleActive + ListModuleActive + FileModuleActive
                == XmlModuleCount + ListModuleCount + FileModuleCount)
            {
                return Enums.Modification_ActivationStatus.Activated;
            }

            return Enums.Modification_ActivationStatus.Partially;
        }

        public string ToLocatedString()
        {
            Enums.Modification_ActivationStatus r = Result();

            switch (r)
            {
                case Enums.Modification_ActivationStatus.Activated:
                    return LanguageDictionary.Get("UserInterface", "Activated");
                case Enums.Modification_ActivationStatus.Deactivated:
                    return LanguageDictionary.Get("UserInterface", "Deactivated");
                default:
                    return LanguageDictionary.Get("UserInterface", "PartiallyActivated");
            }
        }

        public int GetPercent()
        {
            return (XmlModuleActive + ListModuleActive + FileModuleActive) /
                (XmlModuleCount + ListModuleCount + FileModuleCount) * 100;
        }
    }
}
