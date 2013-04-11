using SharedLibrary;
using SharedLibrary.Entities;
using System.Collections.Generic;

namespace Client
{
    class ReadQuorum
    {
        private Dictionary<FileData, int> quorum = new Dictionary<FileData, int>();
        private int quorumSize;
        private int totalVotes = 0;
        private Helper.Semantics semantics;

        public ReadQuorum(int quorumSize, Helper.Semantics semantics)
        {
            this.quorumSize = quorumSize;
            this.semantics = semantics;
        }

        public void AddVote(FileData vote)
        {
            if (vote != null)
            {
                if (quorum.ContainsKey(vote))
                {
                    quorum[vote]++;
                }
                else
                {
                    quorum[vote] = 1;
                }
            }
            totalVotes++;
        }

        public bool CheckQuorum(FileData vote, FileData original)
        {
            if ((vote != null) && (quorum[vote] >= this.quorumSize))
            {
                if ((semantics == Helper.Semantics.MONOTONIC) && (FileData.MostRecent(vote, original) < 0))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public int Count
        {
            get { return this.totalVotes; }
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
