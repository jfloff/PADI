using SharedLibrary;
using SharedLibrary.Entities;
using System.Collections.Generic;

namespace Client
{
    class ReadQuorum
    {
        private int quorumSize;
        private Helper.Semantics semantics;
        private Dictionary<FileVersion, int> quorum = new Dictionary<FileVersion, int>();
        private int totalVotes = 0;

        public ReadQuorum(int quorumSize, Helper.Semantics semantics)
        {
            this.quorumSize = quorumSize;
            this.semantics = semantics;
        }

        public void AddVote(FileVersion vote)
        {
            if (vote != null)
            {
                if (quorum.ContainsKey(vote)) quorum[vote]++;
                else quorum[vote] = 1;
            }
            totalVotes++;
        }

        public bool CheckQuorum(FileVersion vote, FileVersion original)
        {
            if ((vote != null) && (quorum[vote] >= this.quorumSize))
            {
                if ((semantics == Helper.Semantics.MONOTONIC) && (FileVersion.MostRecent(vote, original) < 0))
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
        private int totalVotes = 0;
        protected int quorumSize;

        public WriteQuorum(int quorumSize)
        {
            this.quorumSize = quorumSize;
        }

        public void AddVote(bool vote)
        {
            if (vote) written++; 
            totalVotes++;
        }

        public int Count
        {
            get { return this.totalVotes; }
        }

        public bool CheckQuorum()
        {
            return (this.written >= this.quorumSize);
        }
    };
}
