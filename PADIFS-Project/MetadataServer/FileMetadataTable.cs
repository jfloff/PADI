using SharedLibrary.Entities;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class FileMetadataTable
    {
        private struct Snapshot
        {
            public int clock;
            public Dictionary<string, FileMetadata> state;

            public Snapshot(int clock, ConcurrentDictionary<string, FileMetadata> state)
            {
                this.clock = clock;
                this.state = new Dictionary<string, FileMetadata>(state);
            }
        }

        // filename / FileMetadata
        private ConcurrentDictionary<string, FileMetadata> table;
        // marked medata id / snapshots
        private ConcurrentDictionary<string, Snapshot> marks = new ConcurrentDictionary<string, Snapshot>();
        // logical clock for each add/remove operation
        private int clock = 0;

        public FileMetadataTable(ConcurrentDictionary<string, FileMetadata> table)
        {
            this.table = table;
        }

        public FileMetadataTable()
        {
            this.table = new ConcurrentDictionary<string, FileMetadata>();
        }

        public bool Contains(string filename)
        {
            return table.ContainsKey(filename);
        }

        public FileMetadata this[string filename]
        {
            get { return table[filename]; }
            set { Add(filename, value); }
        }

        public void Remove(string filename)
        {
            FileMetadata ignored; table.TryRemove(filename, out ignored);
            clock++;
        }

        public void Add(string filename, FileMetadata fileMetadata)
        {
            table[filename] = fileMetadata;
            clock++;
        }

        public override string ToString()
        {
            string ret = "[\n";
            foreach (var entry in table)
            {
                FileMetadata fileMetadata = entry.Value;

                ret += "  <" + fileMetadata + "> \n";
            }
            return ret + " ]";
        }

        /**
         * Log Management
         */

        // adds mark to start keeping states
        public void AddMark(string mark)
        {
            marks[mark] = new Snapshot(clock, table);
        }

        // removes mark
        public void RemoveMark(string mark)
        {
            Snapshot ignored; marks.TryRemove(mark, out ignored);
        }

        public MetadataState State(string mark)
        {
            if (!marks.ContainsKey(mark))
            {
                return new MetadataState(new Dictionary<string, FileMetadata>(table));
            }
            return new MetadataState(marks[mark].state);
        }

        public void MergeState(MetadataState newState)
        {
            foreach (var entry in newState)
            {
                if (table.ContainsKey(entry.Key))
                {
                    Remove(entry.Key);
                }
                else
                {
                    table[entry.Key] = entry.Value;
                }
            }
        }
    }
}
