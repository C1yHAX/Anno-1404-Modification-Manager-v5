using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnoModificationManager5.ModificationTypes.Userdefined
{
    
    public class UserdefinedValue_FileItem
    {
        public string File { get; set; }

        public UserdefinedValue_FileItem(string file)
        {
            File = file;
        }
    }
}
