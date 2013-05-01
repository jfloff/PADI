using SharedLibrary;
using SharedLibrary.Entities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class DataServerRegister
    {
        private class DataServerInfo
        {
            public string location;
            public double weight;
            public DateTime lastHeartbeat;

            public DataServerInfo(string location)
            {
                this.location = location;
                this.weight = 0;
                this.lastHeartbeat = DateTime.Now;
            }
        };

        class WeightCompararer : IComparer<double>
        {
            public int Compare(double x, double y)
            {
                if (x > y) return 1;
                if (x < y) return -1;
                return 0;
            }
        };

        // CREATE DICTIONARY FOR FAILED METADATAS
        // id / location
        private ConcurrentDictionary<string, DataServerInfo> infos = new ConcurrentDictionary<string, DataServerInfo>();
        // score / set of ids
        private SortedDictionary<double, SortedSet<string>> weights = new SortedDictionary<double, SortedSet<string>>(new WeightCompararer());
        // dummy object for lock
        private readonly object padlock = new object();

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

        public void UpdateWeight(string id, double weight)
        {
            lock (padlock)
            {
                // if new weight
                if (!this.weights.ContainsKey(weight))
                {
                    this.weights[weight] = new SortedSet<string>();
                }
                
                // remove the previous weight
                this.weights[this.infos[id].weight].Remove(id);
                    
                this.weights[weight].Add(id);
                this.infos[id].weight = weight;
            }
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
            bool ret = false;
            value = default(string);
            bool skipped = (last == null) ? true : false;

            lock (padlock)
            {
                foreach (var weight in weights)
                {
                    foreach (string id in weight.Value)
                    {
                        if (skipped)
                        {
                            value = id;
                            ret = true;
                            break;
                        }

                        if (id == last) skipped = true;
                    }

                    if (ret) break;
                }
            }

            return ret;
        }
    }
}
