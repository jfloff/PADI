using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class MetadataSnapshot : IEnumerable<KeyValuePair<string, FileMetadata>>
    {
        private ConcurrentDictionary<string, FileMetadata> state;

        public MetadataSnapshot(ConcurrentDictionary<string, FileMetadata> state)
        {
            this.state = state;
        }

        public MetadataSnapshot(KeyValuePair<string, FileMetadata> state)
        {
            this.state[state.Key] = state.Value;
        }

        public IEnumerator<KeyValuePair<string, FileMetadata>> GetEnumerator()
        {
            foreach(var entry in state)
                yield return entry;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

