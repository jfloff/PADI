using SharedLibrary;
using SharedLibrary.Entities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public struct DataServerFile 
    {
        public string DataServerId;
        public string LocalFilename;
        public Weight Weight;

        public DataServerFile(string dataServerId, string localFilename, Weight weight)
        {
            this.DataServerId = dataServerId;
            this.LocalFilename = localFilename;
            this.Weight = weight;
        }
    }

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
        private ConcurrentDictionary<string, DataServerInfo> dataServers = new ConcurrentDictionary<string, DataServerInfo>();
        // score / set of ids
        private SortedDictionary<Weight, SortedSet<string>> weights = new SortedDictionary<Weight, SortedSet<string>>();
        // dummy object for lock
        private readonly object padlock = new object();


        public Weight AvgWeight {
            get
            {
                Weight sum = new Weight();

                // no data servers
                if (dataServers.Count == 0) return sum;

                foreach (var entry in dataServers)
                {
                    sum += entry.Value.weight;
                }
                return SharedLibrary.Entities.Weight.Avg(sum, dataServers.Count);
            }
        }

        public override string ToString()
        {
            string ret = "[\n";
            foreach (var entry in dataServers)
            {
                ret += "  <" + entry.Key + ":" + entry.Value.location + ":" + entry.Value.weight + ":" + entry.Value.lastHeartbeat + "> \n";
            }
            return ret + "]";
        }

        public bool Contains(string id)
        {
            return this.dataServers.ContainsKey(id);
        }

        public string Location(string id)
        {
            return dataServers[id].location;
        }

        public void Add(string id, string location)
        {
            if (!this.dataServers.ContainsKey(id))
            {
                this.dataServers[id] = new DataServerInfo(location);
                UpdateWeight(id, new Weight());
            }
        }

        public void AddFile(string id, string localFilename)
        {
            this.dataServers[id].files[localFilename] = new Weight();
        }

        public void RemoveFile(string id, string localFilename)
        {
            Weight ignored; this.dataServers[id].files.TryRemove(localFilename, out ignored);
        }

        public bool Failed(string id)
        {
            double elapsed = Math.Abs(this.dataServers[id].lastHeartbeat.Subtract(DateTime.Now).TotalMilliseconds);
            return !(elapsed < (Helper.DATASERVER_HEARTBEAT_INTERVAL * Helper.HEARTBEAT_EXPIRE));
        }

        public void Touch(string id, Weight weight)
        {
            this.dataServers[id].lastHeartbeat = DateTime.Now;
            this.UpdateWeight(id, weight);
        }

        private void UpdateWeight(string id, Weight weight)
        {
            lock (padlock)
            {
                // if new weight
                if (!this.weights.ContainsKey(weight))
                {
                    this.weights[weight] = new SortedSet<string>();
                }

                // remove the previous weight
                this.weights[this.dataServers[id].weight].Remove(id);

                this.weights[weight].Add(id);
                this.dataServers[id].weight = weight;
            }
        }

        public bool ContainsFile(string id, string localFilename)
        {
            return this.dataServers[id].files.ContainsKey(localFilename);
        }

        public IEnumerable<DataServerFile> FileWeights
        {
            get
            {
                foreach (var entry in dataServers)
                {
                    foreach (var fileEntry in entry.Value.files)
                    {
                        yield return new DataServerFile(entry.Key, fileEntry.Key, fileEntry.Value);
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<string, Weight>> Weights
        {
            get
            {
                foreach (var entry in dataServers)
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

        public void AddWeight(string dataServerId, Weight fileWeight)
        {
            UpdateWeight(dataServerId, this.dataServers[dataServerId].weight + fileWeight);
        }

        public void RemoveWeight(string dataServerId, Weight fileWeight)
        {
            UpdateWeight(dataServerId, this.dataServers[dataServerId].weight - fileWeight);
        }

        public Weight Weight(string dataServerId)
        {
            return this.dataServers[dataServerId].weight;
        }

        public void UpdateFileWeight(string dataServerId, string localFilename, Weight fileWeight)
        {
            this.dataServers[dataServerId].files[localFilename] = fileWeight;
        }
    }
}
