using SharedLibrary.Entities;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    class FileMetadataTable : IEnumerable<KeyValuePair<string, FileMetadata>>
    {
        // filename / FileMetadata
        private ConcurrentDictionary<string, FileMetadata> table = new ConcurrentDictionary<string, FileMetadata>();
        // metadata id / dictionary
        private ConcurrentDictionary<string, ConcurrentDictionary<string, FileMetadata>> marks
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, FileMetadata>>();

        public IEnumerator<KeyValuePair<string, FileMetadata>> GetEnumerator()
        {
            foreach (var entry in table)
                yield return entry;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string filename)
        {
            return table.ContainsKey(filename);
        }

        public FileMetadata this[string filename]
        {
            get { return table[filename]; }
            set { 
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

        public ConcurrentDictionary<string, FileMetadata> Snapshot(string mark)
        {
            if (!marks.ContainsKey(mark))
            {
                return table;
            }
            return marks[mark];
        }
    }
}
