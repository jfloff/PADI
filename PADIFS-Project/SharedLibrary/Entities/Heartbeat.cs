using System;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class Heartbeat
    {
        private int score;

        public Heartbeat(int score)
        {
            this.score = score;
        }

        public int Score { 
            get { return this.score; } 
        }
    }
}
