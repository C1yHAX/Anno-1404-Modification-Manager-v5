using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnoModificationManager5.ModificationTypes.Userdefined
{
    
    public class UserdefinedValue_ComboBoxItem
    {
        public Language.Label Name { get; set; }
        public string Value { get; set; }

        public string GetText
        {
            get
            {
                if (!string.IsNullOrEmpty(Name.Get))
                    return Name.Get;
                return Value;
            }
        }

        public UserdefinedValue_ComboBoxItem(Language.Label label, string value)
        {
            Name = label;
            Value = value;
        }

        public UserdefinedValue_ComboBoxItem()
        {
            Name = new Language.Label() { Name = "Element" };
        }
    }
}
