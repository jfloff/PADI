using SharedLibrary.Entities;
using System.Collections.Generic;

namespace Client
{
    class ReadQuorum
    {
        private Dictionary<FileData, int> quorum = new Dictionary<FileData, int>();
        private int quorumSize;
        private int voteCount = 0;

        public ReadQuorum(int quorumSize) 
        {
            this.quorumSize = quorumSize;
        }

        public void AddVote(FileData vote)
        {
            if (quorum.ContainsKey(vote))
            {
                quorum[vote]++;
            }
            else
            {
                quorum[vote] = 1;
            }
            voteCount++;
        }

        public bool CheckQuorum(FileData vote)
        {
            return (quorum[vote] >= this.quorumSize);
        }

        public int Count
        {
            get { return this.voteCount;  }
        }
    }

    class WriteQuorum
    {
        private int written = 0;
        private int failed = 0;
        protected int quorumSize;

        public WriteQuorum(int quorumSize) 
        {
            this.quorumSize = quorumSize;
        }

        public void AddVote(bool vote)
        {
            if (vote) written++; else failed++;
        }

        public int Count
        {
            get { return this.written + this.failed; }
        }

        public bool CheckQuorum()
        {
            return (this.written >= this.quorumSize);
        }
    };
}
