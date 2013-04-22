using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class DataServerRegister
    {
        // CREATE DICTIONARY FOR FAILED METADATAS
        // id / location
        private ConcurrentDictionary<string, string> locations = new ConcurrentDictionary<string, string>();
        private IEnumerator<KeyValuePair<string, string>> enumerator;
        // id / weight
        private ConcurrentDictionary<string, int> weights = new ConcurrentDictionary<string, int>();

        public DataServerRegister()
        {
            this.enumerator = locations.GetEnumerator();
        }

        public override string ToString()
        {
            string ret = "[\n";
            foreach (var entry in locations)
            {
                ret += "  <" + entry.Key + ";" + entry.Value + "> \n";
            }
            return ret + "]";
        }

        public Dictionary<string, string> ToDictionary() 
        {
            return new Dictionary<string, string>(locations);
        }

        public bool Contains(string id)
        {
            return this.locations.ContainsKey(id);
        }

        public string this[string id]
        {
            get { return locations[id]; }
            set
            {
                if (!this.locations.ContainsKey(id))
                {
                    weights[id] = 1;
                }

                this.locations[id] = value;
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

                    yield return this.enumerator.Current;
                    if (!this.enumerator.MoveNext())
                    {
                        this.enumerator = this.locations.GetEnumerator();
                        this.enumerator.MoveNext();
                    }

                    if (this.enumerator.Current.Key == firstId) break;

                }
            }
        }
    }
}
