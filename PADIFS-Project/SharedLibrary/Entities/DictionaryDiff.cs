using System;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class DictionaryDiff<K, V>
    {
        private Dictionary<K, V> plus = new Dictionary<K, V>();
        private Dictionary<K, V> minus = new Dictionary<K, V>();

        public DictionaryDiff(Dictionary<K, V> origD, Dictionary<K, V> newD)
        {
            foreach (var entry in newD)
            {
                // key is in both orig and newD
                if (origD.ContainsKey(entry.Key))
                {
                    // only plus if its a new value
                    if (!origD[entry.Key].Equals(entry.Value))
                    {
                        plus[entry.Key] = entry.Value;
                    }
                }
                // key is only in newD - CREATE
                else
                {
                    plus[entry.Key] = entry.Value;
                }
            }

            foreach (var entry in origD)
            {
                // key is not on newD - DELETE
                if (!newD.ContainsKey(entry.Key))
                {
                    minus[entry.Key] = entry.Value;
                }
            }
        }

        public DictionaryDiff(Dictionary<K, V> origD)
        {
            foreach (var entry in origD)
            {
                plus[entry.Key] = entry.Value;
            }
        }

        public Dictionary<K, V> Plus
        {
            get { return this.plus; }
        }

        public Dictionary<K, V> Minus
        {
            get { return this.minus; }
        }
    }
}
