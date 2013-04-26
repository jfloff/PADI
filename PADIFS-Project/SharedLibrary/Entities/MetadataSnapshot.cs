using System;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public struct MetadataSnapshot
    {
        public Dictionary<string, FileMetadata> table;
        public Dictionary<string, string> dataServers;
        public int sequence;

        public MetadataSnapshot(Dictionary<string, FileMetadata> table, Dictionary<string, string> dataServers, int sequence)
        {
            this.table = table;
            this.dataServers = dataServers;
            this.sequence = sequence;
        }
    }
}
