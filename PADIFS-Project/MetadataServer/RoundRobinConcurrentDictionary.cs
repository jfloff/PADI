using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class RoundRobinConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        private IEnumerator<KeyValuePair<TKey, TValue>> enumerator;


        public RoundRobinConcurrentDictionary()
            : base()
        {
            this.enumerator = base.GetEnumerator();
        }

        new public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            while (true)
            {
                if (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                else
                {
                    this.enumerator = base.GetEnumerator();
                }
            }
        }

        public override string ToString()
        {
            string ret = "[\n";
            IEnumerator<KeyValuePair<TKey, TValue>> ie = base.GetEnumerator();
            while (ie.MoveNext())
            {
                ret += "  <" + ie.Current.Key + ";" + ie.Current.Value + "> \n";
            }
            return ret + "]";
        }
    }
}