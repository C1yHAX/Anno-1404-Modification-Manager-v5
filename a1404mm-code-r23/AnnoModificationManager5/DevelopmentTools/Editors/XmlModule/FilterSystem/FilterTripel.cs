using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentTools.Editors.XmlModule.FilterSystem
{
    public class FilterTripel
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Attribute { get; set; }

        public bool IsAttribute { get; set; }
    }
}
