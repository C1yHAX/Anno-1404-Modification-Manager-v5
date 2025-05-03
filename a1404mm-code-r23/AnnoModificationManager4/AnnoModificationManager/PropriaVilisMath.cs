namespace AnnoModificationManager
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class PropriaVilisMath
    {
        /*[CompilerGenerated]
        private string <Math>k__BackingField;
        [CompilerGenerated]
        private string <Name>k__BackingField;*/

        public PropriaVilisMath Duplicate()
        {
            PropriaVilisMath math = new PropriaVilisMath();
            math.Name = this.Name;
            math.Math = this.Math;
            return math;
        }

        public string getMath
        {
            get
            {
                if (string.IsNullOrEmpty(this.Math))
                {
                    this.Math = "{value}";
                }
                return this.Math;
            }
            set
            {
                this.Math = value;
            }
        }

        public string Math
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
    }
}

