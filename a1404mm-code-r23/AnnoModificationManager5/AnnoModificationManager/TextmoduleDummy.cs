namespace AnnoModificationManager
{
    using System;
    using System.Runtime.CompilerServices;

    public class TextmoduleDummy
    {
        [CompilerGenerated]      
        public int Active;
        public string AdderString = "";
        public string Modified = "";
        public string Original = "";

        public TextmoduleDummy getPositive()
        {
            TextmoduleDummy dummy = new TextmoduleDummy();
            dummy.Header = this.Header.Replace("-", "+");
            dummy.Original = this.Original;
            dummy.Modified = this.Modified;
            dummy.Active = this.Active;
            dummy.AdderString = this.AdderString;
            return dummy;
        }

        public string Header
        {
            get;
            set;
        }
    }
}

