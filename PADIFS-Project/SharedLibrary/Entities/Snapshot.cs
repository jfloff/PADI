using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    public struct Snapshot
    {
        public Dictionary<string, FileMetadata> table;
        public Dictionary<string, string> dataServers;
        public int sequence;

        public Snapshot(Dictionary<string, FileMetadata> table, Dictionary<string, string> dataServers, int sequence)
        {
            this.table = table;
            this.dataServers = dataServers;
            this.sequence = sequence;
        }
    }
}
