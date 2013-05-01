using SharedLibrary;
using SharedLibrary.Entities;
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

        class WeightCompararer : IComparer<KeyValuePair<string, int>>
        {
            public int Compare(KeyValuePair<string, int> x, KeyValuePair<string, int> y)
            {
                if (x.Value > y.Value) return -1;
                if (x.Value < y.Value) return 1;
                return string.Compare(x.Key, y.Key);
            }
        };

        // CREATE DICTIONARY FOR FAILED METADATAS
        // id / location
        private ConcurrentDictionary<string, DataServerInfo> infos = new ConcurrentDictionary<string, DataServerInfo>();
        // score / id
        private SortedSet<KeyValuePair<string, int>> weights = new SortedSet<KeyValuePair<string, int>>(new WeightCompararer());


        // dummy object for lock
        private readonly object padlock = new object();
        // original enumerator
        private IEnumerator<KeyValuePair<string, int>> original;

        public DataServerRegister()
        {
            this.original = this.weights.GetEnumerator();
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

        public void Touch(string id, Heartbeat heartbeat)
        {
            this.infos[id].lastHeartbeat = DateTime.Now;
            this.UpdateWeight(id, heartbeat.Weight);
        }

        public string Location(string id)
        {
            return infos[id].location;
        }

        public void UpdateWeight(string id, int weight)
        {
            lock (padlock) 
            {
                this.weights.Add(new KeyValuePair<string, int>(id, weight));
                this.original = this.weights.GetEnumerator();
            }

            this.infos[id].weight = weight;
        }

        public void Add(string id, string location)
        {
            if (!this.infos.ContainsKey(id))
            {
                this.infos[id] = new DataServerInfo(location);
                UpdateWeight(id, 0);
            }
        }

        public bool TryMoveNext(string last, out string value)
        {
            lock (padlock)
            {
                bool hadNext;
                while (true)
                {
                    hadNext = this.original.MoveNext();
                    if (hadNext)
                    {
                        if (string.Compare(last, this.original.Current.Key) == 0) continue;
                        if (Failed(this.original.Current.Key)) continue;

                        value = this.original.Current.Key;
                    }
                    else
                    {
                        value = default(string);
                        this.original = this.weights.GetEnumerator();
                    }
                    break;
                }
                return hadNext;
            }
        }
    }
}
