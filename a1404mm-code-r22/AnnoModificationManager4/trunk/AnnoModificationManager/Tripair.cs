namespace AnnoModificationManager
{
    // using AnnoModificationManager.Properties;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class Tripair
    {      
        public Tripair Duplicate()
        {
            Tripair tripair = new Tripair();
            tripair.Name = this.Name;
            tripair.EName = this.EName;
            tripair.Value = this.Value;
            return tripair;
        }

        public static int IndexOf(string Value, List<Tripair> i)
        {
            foreach (Tripair tripair in i)
            {
                if (tripair.Value == Value)
                {
                    return i.IndexOf(tripair);
                }
            }
            return -1;
        }

        public string EName
        {
            get;
            set;
        }             

        public string Name
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }
    }
}

