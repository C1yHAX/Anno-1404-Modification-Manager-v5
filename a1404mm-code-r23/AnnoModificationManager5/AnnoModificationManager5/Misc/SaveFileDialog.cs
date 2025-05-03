using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnoModificationManager5.Misc
{
    public class SaveFileDialog
    {
        public System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();

        public string InitialDirectory
        {
            get
            {
                return dialog.InitialDirectory;
            }
            set
            {
                dialog.InitialDirectory = value;
            }
        }      

        public string Filter
        {
            get
            {
                return dialog.Filter;
            }
            set
            {
                dialog.Filter = value;
            }
        }

        public string FileName
        {
            get
            {
                return dialog.FileName;
            }
        }

        public List<string> FileNames
        {
            get
            {
                return dialog.FileNames.ToList();
            }
        }

        public bool? ShowDialog()
        {
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return true;
            return false;
        }
    }
}
