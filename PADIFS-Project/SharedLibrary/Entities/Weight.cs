using System;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class Weight : IEquatable<Weight>, IComparable<Weight>
    {
        private int reads = 0;
        private int writes = 0;

        public int Reads
        {
            get { return this.reads; }
            set { this.reads = value; }
        }

        public int Writes
        {
            get { return this.writes; }
            set { this.writes = value; }
        }

        public override string ToString()
        {
            return "(" + this.reads + ";" + this.writes + ")";
        }

        public override int GetHashCode()
        {
            return this.reads ^ this.writes;
        }

        // for comparasion on hash keys, etc
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Weight weight = obj as Weight;
            if ((System.Object)weight == null)
            {
                return false;
            }

            return (this.reads == weight.reads) && (this.writes == weight.reads);
        }

        public bool Equals(Weight weight)
        {
            // If parameter is null return false:
            if ((object)weight == null)
            {
                return false;
            }

            return (this.reads == weight.reads) && (this.writes == weight.reads);
        }

        // priority for reads
        public int CompareTo(Weight other)
        {
            if (this.reads > other.reads) return 1;
            if (this.reads < other.reads) return -1;

            //if reads are equal check writes
            if (this.writes > other.writes) return 1;
            if (this.writes < other.writes) return -1;

            return 0;
        }
    }
}
