using System;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class Weight : IEquatable<Weight>, IComparable<Weight>
    {
        private int reads;
        private int writes;

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

        public Weight()
        {
            this.reads = 0;
            this.writes = 0;
        }

        public Weight(int reads, int writes)
        {
            this.reads = reads;
            this.writes = writes;
        }

        // Declare which operator to overload (+)
        public static Weight operator +(Weight w1, Weight w2)
        {
            return new Weight(w1.reads + w2.reads, w1.writes + w2.writes);
        }

        // Declare which operator to overload (-)
        public static Weight operator -(Weight w1, Weight w2)
        {
            int newReads = ((w1.reads - w2.reads) <= 0) ? 0 : w1.reads - w2.reads;
            int newWrites = ((w1.writes - w2.writes) <= 0) ? 0 : w1.writes - w2.writes;

            return new Weight(newReads, newWrites);
        }

        // Declare which operator to overload (+)
        public static Weight Avg(Weight sum, int n)
        {
            return new Weight(sum.reads/n, sum.writes/n);
        }

        public static bool InsideThreshold(Weight check, Weight around, double threshold)
        {
            int readThreshold = (int) Math.Ceiling(around.reads * threshold);
            int writeThreshold = (int) Math.Ceiling(around.writes * threshold);

            // if its either inside the threshold in reads or writes says true
            if (check.reads < (around.reads + readThreshold)) return true;
            if (check.writes < (around.writes + writeThreshold)) return true;

            return false;
        }

        public static int Compare(Weight w1, Weight w2)
        {
            //prioritizes reads
            if (w1.reads > w2.reads) return 1;
            if (w1.reads < w2.reads) return -1;

            //if reads are equal check writes
            if (w1.writes > w2.writes) return 1;
            if (w1.writes < w2.writes) return -1;

            return 0;
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
            return Compare(this, other);
        }
    }
}
