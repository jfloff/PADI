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
            public Weight weight;
            public DateTime lastHeartbeat;
            public ConcurrentDictionary<string, Weight> files = new ConcurrentDictionary<string, Weight>();

            public DataServerInfo(string location)
            {
                this.location = location;
                this.weight = new Weight();
                this.lastHeartbeat = DateTime.Now;
            }
        };

        // CREATE DICTIONARY FOR FAILED METADATAS
        // id / location
        private ConcurrentDictionary<string, DataServerInfo> infos = new ConcurrentDictionary<string, DataServerInfo>();
        // score / set of ids
        private SortedDictionary<Weight, SortedSet<string>> weights = new SortedDictionary<Weight, SortedSet<string>>();
        // dummy object for lock
        private readonly object padlock = new object();


        public Weight MedianWeight {
            get
            {
                Weight sum = new Weight();
                foreach (var entry in infos)
                {
                    sum += entry.Value.weight;
                }
                return Weight.Median(sum, infos.Count);
            }
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

        public bool Contains(string id)
        {
            return this.infos.ContainsKey(id);
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
                UpdateWeight(id, new Weight());
            }
        }

        public void AddFile(string id, string localFilename)
        {
            this.infos[id].files[localFilename] = new Weight();
        }

        public void RemoveFile(string id, string localFilename)
        {
            Weight ignored; this.infos[id].files.TryRemove(localFilename, out ignored);
        }

        public bool Failed(string id)
        {
            double elapsed = Math.Abs(this.infos[id].lastHeartbeat.Subtract(DateTime.Now).TotalMilliseconds);
            return !(elapsed < (Helper.DATASERVER_HEARTBEAT_INTERVAL * Helper.HEARTBEAT_EXPIRE));
        }

        public void Touch(string id, Weight weight)
        {
            this.infos[id].lastHeartbeat = DateTime.Now;
            this.UpdateWeight(id, weight);
        }

        public void UpdateWeight(string id, Weight weight)
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

        public bool ContainsFile(string id, string localFilename)
        {
            return this.infos[id].files.ContainsKey(localFilename);
        }

        public IEnumerable<KeyValuePair<string, Weight>> Weights
        {
            get
            {
                foreach (var entry in infos)
                {
                    yield return new KeyValuePair<string, Weight>(entry.Key, entry.Value.weight);
                }
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
