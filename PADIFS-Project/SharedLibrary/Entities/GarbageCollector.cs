using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class GarbageCollector : IEnumerable<string>
    {
        private HashSet<string> toDelete = new HashSet<string>();

        public void Add(string filename)
        {
            toDelete.Add(filename);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this.toDelete.GetEnumerator();
        }

        // Explicit interface implementation for nongeneric interface
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
