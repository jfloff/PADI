using System;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class Heartbeat
    {
        private int weight;

        public Heartbeat(int weight)
        {
            this.weight = weight;
        }

        public int Weight { 
            get { return this.weight; } 
        }
    }
}
