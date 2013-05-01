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
                ret += "  <" + entry.Key + ":" + entry.Value.Location + ":" + entry.Value.Weight + ":" + entry.Value.LastHeartbeat + "> \n";
            }
            return ret + "]";
        }

        public Dictionary<string, DataServerInfo> ToDictionary() 
        {
            return new Dictionary<string, DataServerInfo>(this.infos);
        }

        public bool Contains(string id)
        {
            return this.infos.ContainsKey(id);
        }

        public bool Failed(string id)
        {
            double elapsed = Math.Abs(this.infos[id].LastHeartbeat.Subtract(DateTime.Now).TotalMilliseconds);
            return !(elapsed < (Helper.DATASERVER_HEARTBEAT_INTERVAL * Helper.HEARTBEAT_EXPIRE));
        }

        public void Touch(string id, Heartbeat heartbeat)
        {
            this.infos[id].LastHeartbeat = DateTime.Now;
            this.UpdateWeight(id, heartbeat.Weight);
        }

        public string Location(string id)
        {
            return infos[id].Location;
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
                this.weights[this.infos[id].Weight].Remove(id);
                    
                this.weights[weight].Add(id);
                this.infos[id].Weight = weight;
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

        internal void Add(string id, DataServerInfo dataServerInfo)
        {
            this.infos[id] = dataServerInfo;
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
