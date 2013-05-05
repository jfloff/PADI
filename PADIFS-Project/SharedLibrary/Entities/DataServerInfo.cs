using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class DataServerInfo
    {
        private string location;
        private Weight weight;
        private DateTime lastHeartbeat;
        private ConcurrentDictionary<string, int> files = new ConcurrentDictionary<string, int>();

        public DataServerInfo(string location)
        {
            this.location = location;
            this.weight = new Weight();
            this.lastHeartbeat = DateTime.Now;
        }

        public string Location
        {
            get { return this.location; }
        }

        public Weight Weight
        {
            get { return this.weight; }
            set { this.weight = value; }
        }

        public DateTime LastHeartbeat
        {
            get { return this.lastHeartbeat; }
            set { this.lastHeartbeat = value;  }
        }

        public ICollection<string> Files
        {
            get { return this.files.Keys; }
        }

        public void AddFile(string localFilename)
        {
            this.files[localFilename] = 1;
        }

        public void RemoveFile(string localFilename)
        {
            int ignored; this.files.TryRemove(localFilename, out ignored);
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
