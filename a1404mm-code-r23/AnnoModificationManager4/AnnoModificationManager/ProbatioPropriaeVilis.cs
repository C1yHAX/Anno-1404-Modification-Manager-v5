namespace AnnoModificationManager
{
    using System;

    [Serializable]
    public class ProbatioPropriaeVilis
    {
        public int Maximum;
        public int Minimum;
        public Typus PTypus;

        public ProbatioPropriaeVilis()
        {
            this.Maximum = 100;
        }

        public ProbatioPropriaeVilis(int min, int max)
        {
            this.Maximum = 100;
            this.Minimum = min;
            this.Maximum = max;
        }

        public ProbatioPropriaeVilis Duplicate()
        {
            ProbatioPropriaeVilis vilis = new ProbatioPropriaeVilis();
            vilis.Minimum = this.Minimum;
            vilis.Maximum = this.Maximum;
            vilis.PTypus = this.PTypus;
            return vilis;
        }

        public enum Typus
        {
            String,
            Numeric
        }
    }
}

