using System;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class MasterVote
    {
        private string id;
        private int clock;

        public MasterVote(string id, int clock)
        {
            this.id = id;
            this.clock = clock;
        }

        public int Clock
        {
            get { return this.clock; }
        }

        public string Id
        {
            get { return this.id; }
        }

        public static MasterVote Choose(MasterVote v1, MasterVote v2)
        {
            if (v1 == null) return v2;
            if (v2 == null) return v1;

            if (v1.clock == v2.clock)
            {
                if (string.Compare(v1.Id, v2.Id) >= 0)
                {
                    return v2;
                }
            }
            else if (v1.clock > v2.clock)
            {
                return v1;
            }

            return v2;
        }
    }
}
