using SharedLibrary.Entities;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class FileMetadataTable
    {
        // filename / FileMetadata
        private ConcurrentDictionary<string, FileMetadata> table;
        // metadata id / dictionary
        private ConcurrentDictionary<string, ConcurrentDictionary<string, FileMetadata>> marks
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, FileMetadata>>();

        public FileMetadataTable(ConcurrentDictionary<string, FileMetadata> table)
        {
            this.table = table;
        }

        public FileMetadataTable()
        {
            this.table = new ConcurrentDictionary<string, FileMetadata>();
        }

        public bool ContainsKey(string filename)
        {
            return table.ContainsKey(filename);
        }

        public FileMetadata this[string filename]
        {
            get { return table[filename]; }
            set
            {
                table[filename] = value;
                foreach (var mark in marks)
                {
                    mark.Value[filename] = value;
                }
            }
        }

        public void Remove(string filename)
        {
            FileMetadata ignored; table.TryRemove(filename, out ignored);
        }

        public override string ToString()
        {
            string ret = "[\n";
            foreach (var entry in table)
            {
                string filename = entry.Key;
                FileMetadata fileMetadata = entry.Value;

                ret += "  <" + fileMetadata + "> \n";
            }
            return ret + " ]";
        }

        // adds mark to start keeping states
        public void AddMark(string mark)
        {
            marks[mark] = new ConcurrentDictionary<string, FileMetadata>();
        }

        // removes mark
        public void RemoveMark(string mark)
        {
            ConcurrentDictionary<string, FileMetadata> ignored; marks.TryRemove(mark, out ignored);
        }

        public MetadataSnapshot Snapshot(string mark)
        {
            if (!marks.ContainsKey(mark))
            {
                return new MetadataSnapshot(table);
            }
            return new MetadataSnapshot(marks[mark]);
        }

        public void MergeSnapshot(MetadataSnapshot snapshot)
        {
            foreach (var entry in snapshot)
            {
                table[entry.Key] = entry.Value;
            }
        }
    }
}
