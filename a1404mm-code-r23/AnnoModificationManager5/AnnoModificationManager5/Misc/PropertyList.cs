using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnoModificationManager5.Misc
{
    public class PropertyList
    {
        public List<string> Items = new List<string>();

        public void Load(string property)
        {
            if (!string.IsNullOrEmpty(property))
                Items = property.Split(';').ToList();
            Items.RemoveAll(it => string.IsNullOrEmpty(it));
        }

        public string Save()
        {
            string property = "";
            foreach (string i in Items)
            {
                property += ";" + i;
            }

            return property.Trim(';');
        }
    }
}
