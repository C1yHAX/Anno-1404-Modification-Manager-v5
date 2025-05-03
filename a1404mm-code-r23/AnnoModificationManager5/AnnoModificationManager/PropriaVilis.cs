namespace AnnoModificationManager
{
    // using AnnoModificationManager.Properties;   
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;  

    [Serializable]
    public class PropriaVilis
    {
        private List<Tripair> _AviableItems = new List<Tripair>();
        private bool? _isEditable = true;
        private bool? _showNumericMinMax = true;
       
        public string DefaultValue = "";
        public string EExplanation = "";
        public string Explanation = "";
        public string Math = "{value}";
        public ProbatioPropriaeVilis Validitation = new ProbatioPropriaeVilis();
        public List<PropriaVilisMath> Variations = new List<PropriaVilisMath>();

        public PropriaVilis Duplicate()
        {
            PropriaVilis vilis2 = new PropriaVilis();
            vilis2.Name = this.Name;
            vilis2.EName = this.EName;
            vilis2.Category = this.Category;
            vilis2.ECategory = this.ECategory;
            vilis2.Keyword = this.Keyword;
            vilis2.DefaultValue = this.DefaultValue;
            vilis2.CurrentValue = this.CurrentValue;
            vilis2.Math = this.Math;
            vilis2.Explanation = this.Explanation;
            vilis2.EExplanation = this.EExplanation;
            vilis2.Validitation = this.Validitation.Duplicate();
            vilis2.isEditable = this.isEditable;
            vilis2.ShowNumericMinMax = this.ShowNumericMinMax;
            PropriaVilis vilis = vilis2;
            foreach (Tripair tripair in this._AviableItems)
            {
                vilis._AviableItems.Add(tripair.Duplicate());
            }
            foreach (PropriaVilisMath math in this.Variations)
            {
                vilis.Variations.Add(math.Duplicate());
            }
            return vilis;
        }

        public bool isValid()
        {
            if (this.Validitation.PTypus == ProbatioPropriaeVilis.Typus.String)
            {
                return true;
            }
            int num = 0;
            try
            {
                num = int.Parse(this.CurrentValue);
            }
            catch (Exception)
            {
                return false;
            }
            return ((num >= this.Validitation.Minimum) & (num <= this.Validitation.Maximum));
        }

        public static bool isValid(IEnumerable n)
        {
            foreach (PropriaVilis vilis in n)
            {
                if (!vilis.isValid())
                {
                    return false;
                }
            }
            return true;
        }     
      

        public List<Tripair> AviableItems
        {
            get
            {
                return this._AviableItems;
            }
            set
            {
                this._AviableItems = value;
            }
        }

        public string AviableItemsString
        {
            get
            {
                try
                {
                    string str = "";
                    foreach (Tripair tripair in this._AviableItems)
                    {
                        string str3 = str;
                        str = str3 + tripair.Value + "~" + tripair.Name + "~" + tripair.EName + "|";
                    }
                    return str.TrimEnd(new char[] { '|' });
                }
                catch (Exception)
                {
                    return "";
                }
            }
            set
            {
                this.AviableItems = new List<Tripair>();
                try
                {
                    foreach (string str in value.Split(new char[] { '|' }))
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            Tripair item = new Tripair();
                            string[] strArray = str.Split(new char[] { '~' });
                            item.Value = strArray[0];
                            item.Name = strArray[1];
                            item.EName = strArray[2];
                            this.AviableItems.Add(item);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public string Category
        {
            get;
            set;
        }

        public string CurrentValue
        {
            get;
            set;
        }

        public string CurrentValueSetter
        {
            get
            {
                return this.CurrentValue;
            }
            set
            {
                if (this.isEditable)
                {
                    this.CurrentValue = value;
                }
            }
        }

        public string ECategory
        {
            get;
            set;
        }

        public string EName
        {
            get;
            set;
        }
      

        public string getDefaultValue
        {
            get
            {
                return this.DefaultValue;
            }
            set
            {
                this.DefaultValue = value;
            }
        }    

        public string getEExplanation
        {
            get
            {
                return this.EExplanation;
            }
            set
            {
                this.EExplanation = value;
            }
        }

        public string getExplanation
        {
            get
            {
                return this.Explanation;
            }
            set
            {
                this.Explanation = value;
            }
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

        public string getsetKeyword
        {
            get
            {
                return this.Keyword.Replace("{", "").Replace("}", "");
            }
            set
            {
                this.Keyword = "{" + value.Replace("{", "").Replace("}", "") + "}";
            }
        }

        public int getsetMax
        {
            get
            {
                return this.Validitation.Maximum;
            }
            set
            {
                this.Validitation.Maximum = value;
            }
        }

        public int getsetMin
        {
            get
            {
                return this.Validitation.Minimum;
            }
            set
            {
                this.Validitation.Minimum = value;
            }
        }

        public int getsetTypeIndex
        {
            get
            {
                if (this.Validitation.PTypus == ProbatioPropriaeVilis.Typus.Numeric)
                {
                    return 0;
                }
                return 1;
            }
            set
            {
                if (value == 0)
                {
                    this.Validitation.PTypus = ProbatioPropriaeVilis.Typus.Numeric;
                }
                else
                {
                    this.Validitation.PTypus = ProbatioPropriaeVilis.Typus.String;
                }
            }
        }

        public string getType
        {
            get
            {
                if (this.ShowNumericMinMax)
                {
                    return this.Validitation.PTypus.ToString();
                }
                return "";
            }
        }

        public int IndexOfCurrentValue
        {
            get
            {
                try
                {
                    if (!this.isEditable)
                    {
                        return Tripair.IndexOf(this.CurrentValue, this._AviableItems);
                    }
                    return -1;
                }
                catch (Exception)
                {
                    return -1;
                }
            }
            set
            {
                if (!this.isEditable)
                {
                    this.CurrentValue = this.AviableItems[value].Value;
                }
            }
        }

        public bool isEditable
        {
            get
            {
                return (!this._isEditable.HasValue || this._isEditable.Value);
            }
            set
            {
                this._isEditable = new bool?(value);
            }
        }

        public string Keyword
        {
            get;
            set;
        }
    
        public string Name
        {
            get;
            set;
        }

        public bool ShowNumericMinMax
        {
            get
            {
                return (!this._showNumericMinMax.HasValue || this._showNumericMinMax.Value);
            }
            set
            {
                this._showNumericMinMax = new bool?(value);
            }
        }
    }
}

