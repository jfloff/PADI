using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class MetadataState : IEnumerable<KeyValuePair<string, FileMetadata>>
    {
        private Dictionary<string, FileMetadata> state;

        public MetadataState(Dictionary<string, FileMetadata> state)
        {
            this.state = state;
        }

        public IEnumerator<KeyValuePair<string, FileMetadata>> GetEnumerator()
        {
            foreach (var entry in state)
                yield return entry;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

