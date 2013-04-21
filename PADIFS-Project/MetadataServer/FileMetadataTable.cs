using SharedLibrary.Entities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class FileMetadataTable : IEnumerable<KeyValuePair<string, FileMetadataTableEntry>>
    {
        // filename / FileMetadata+queue of pending requests for each file 
        private ConcurrentDictionary<string, FileMetadataTableEntry> table = new ConcurrentDictionary<string, FileMetadataTableEntry>();

        public override string ToString()
        {
            string ret = "[\n";
            foreach (var entry in table)
            {
                ret += "  <" + entry.Value + "> \n";
            }
            return ret + "]";
        }

        public IEnumerator<KeyValuePair<string, FileMetadataTableEntry>> GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        public bool Contains(string filename)
        {
            return this.table.ContainsKey(filename);
        }

        public void Remove(string filename)
        {
            FileMetadataTableEntry ignored; this.table.TryRemove(filename, out ignored);
        }

        public FileMetadata FileMetadata(string filename)
        {
            return table[filename].FileMetadata;
        }

        public void EnqueuePendingRequest(string filename, Action<string> action)
        {
            table[filename].PendingRequests.Enqueue(action);
        }

        public Action<string> DequeuePendingRequest(string filename)
        {
            Action<string> ret;
            table[filename].PendingRequests.TryDequeue(out ret);
            return ret;
        }

        public void SetFileMetadata(string filename, FileMetadata fileMetadata)
        {
            if (this.Contains(filename))
            {
                this.table[filename].FileMetadata = fileMetadata;
                // missing requests
            }
            else
            {
                this.table[filename] = new FileMetadataTableEntry(fileMetadata);
            }
        }

        public IEnumerable<ConcurrentQueue<Action<string>>> PendingRequests { 
            get {
                foreach (var entry in table)
                {
                    if (entry.Value.PendingRequests.Count != 0)
                    {
                        yield return entry.Value.PendingRequests;
                    }
                }
            } 
        }
    }
}
