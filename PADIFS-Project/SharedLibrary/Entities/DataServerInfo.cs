using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class DataServerInfo
    {
        private string location;
        private double weight;
        private DateTime lastHeartbeat;

        public DataServerInfo(string location)
        {
            this.location = location;
            this.weight = 0;
            this.lastHeartbeat = DateTime.Now;
        }

        public string Location
        {
            get { return this.location; }
        }

        public double Weight
        {
            get { return this.weight; }
            set { this.weight = value; }
        }

        public DateTime LastHeartbeat
        {
            get { return this.lastHeartbeat; }
            set { this.lastHeartbeat = value;  }
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
            DataServerInfo dataServer = obj as DataServerInfo;
            if ((System.Object)dataServer == null)
            {
                return false;
            }

            return (this.location == dataServer.location) && (this.weight == dataServer.weight);
        }

        public bool Equals(DataServerInfo dataServer)
        {
            // If parameter is null return false:
            if ((object)dataServer == null)
            {
                return false;
            }

            return (this.location == dataServer.location) && (this.weight == dataServer.weight);
        }

        public override int GetHashCode()
        {
            return this.location.GetHashCode() ^ this.weight.GetHashCode();
        }
    }
}
