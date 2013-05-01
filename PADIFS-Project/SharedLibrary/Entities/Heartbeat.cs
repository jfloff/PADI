using System;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class Heartbeat
    {
        private double weight;

        public Heartbeat(double weight)
        {
            this.weight = weight;
        }

        public double Weight
        { 
            get { return this.weight; } 
        }
    }
}
