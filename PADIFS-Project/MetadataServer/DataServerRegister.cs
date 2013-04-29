using SharedLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class DataServerRegister
    {
        private class DataServerInfo
        {
            public string location;
            public int weight;
            public DateTime lastHeartbeat;

            public DataServerInfo(string location)
            {
                this.location = location;
                this.weight = 1;
                this.lastHeartbeat = DateTime.Now;
            }
        };

        // CREATE DICTIONARY FOR FAILED METADATAS
        // id / location
        private ConcurrentDictionary<string, DataServerInfo> infos = new ConcurrentDictionary<string, DataServerInfo>();
        private IEnumerator<KeyValuePair<string, DataServerInfo>> enumerator;

        public DataServerRegister()
        {
            this.enumerator = infos.GetEnumerator();
        }

        public override string ToString()
        {
            string ret = "[\n";
            foreach (var entry in infos)
            {
                ret += "  <" + entry.Key + ":" + entry.Value.location + ":" + entry.Value.weight + ":" + entry.Value.lastHeartbeat + "> \n";
            }
            return ret + "]";
        }

        // LACKS PASSING THE REST OF THE INFORMATION
        public Dictionary<string, string> ToDictionary() 
        {
            Dictionary<string, string> copy = new Dictionary<string, string>();
            foreach (var entry in infos)
            {
                copy[entry.Key] = entry.Value.location;
            }
            return copy;
        }

        public bool Contains(string id)
        {
            return this.infos.ContainsKey(id);
        }

        public bool Failed(string id)
        {
            double elapsed = Math.Abs(this.infos[id].lastHeartbeat.Subtract(DateTime.Now).TotalMilliseconds);
            return !(elapsed < (Helper.DATASERVER_HEARTBEAT_INTERVAL * Helper.HEARTBEAT_EXPIRE));
        }

        public void Touch(string id)
        {
            this.infos[id].lastHeartbeat = DateTime.Now;
        }

        public string Location(string id)
        {
            return infos[id].location;
        }

        public void Add(string id, string location)
        {
            if (!this.infos.ContainsKey(id))
            {
                this.infos[id] = new DataServerInfo(location);
                this.enumerator.MoveNext();
            }
        }

        public IEnumerable<KeyValuePair<string, string>> UniqueDataServers
        {
            get
            {
                string firstId = this.enumerator.Current.Key;
                while (true)
                {
                    if (firstId == null) break;

                    if (!Failed(this.enumerator.Current.Key))
                    {
                        yield return new KeyValuePair<string, string>(this.enumerator.Current.Key, this.enumerator.Current.Value.location);
                    }
                    if (!this.enumerator.MoveNext())
                    {
                        this.enumerator = this.infos.GetEnumerator();
                        this.enumerator.MoveNext();
                    }

                    if (this.enumerator.Current.Key == firstId) break;
                }
            }
        }
    }
}
