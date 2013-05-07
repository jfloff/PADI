using System;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable()]
    public class Heartbeat
    {
        private Weight dataServerWeight;
        private Dictionary<string, Weight> fileWeights;

        public Heartbeat(Weight dataServerWeight, Dictionary<string, Weight> fileWeights)
        {
            this.dataServerWeight = dataServerWeight;
            this.fileWeights = fileWeights;
        }

        public Weight DataServerWeight
        {
            get { return this.dataServerWeight; } 
        }

        public Dictionary<string, Weight> FileWeights
        {
            get { return this.fileWeights; }
        }
    }
}
