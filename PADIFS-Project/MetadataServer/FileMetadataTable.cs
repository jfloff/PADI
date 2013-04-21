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

        public void EnqueueSelectDataServer(string filename, Action<string> enqueueAction, int number = 1)
        {
            if (number <= 0) return;

            for (; number > 0; number--)
            {
                table[filename].PendingRequests.Enqueue(enqueueAction);
            }
        }

        public void DequeueSelectDataServer(string filename, int number = -1)
        {
            if (number >= 0) return;

            for (; number < 0; number++)
            {
                Action<string> ignored; table[filename].PendingRequests.TryDequeue(out ignored);
            }
        }

        public void SetFileMetadata(string filename, FileMetadata fileMetadata, Action<string> enqueueAction)
        {
            if (this.Contains(filename))
            {
                int number = fileMetadata.NbDataServers - fileMetadata.CurrentNbDataServers - table[filename].PendingRequests.Count;
                EnqueueSelectDataServer(filename, enqueueAction, number);
                DequeueSelectDataServer(filename, number);
                this.table[filename].FileMetadata = fileMetadata;
            }
            else
            {
                this.table[filename] = new FileMetadataTableEntry(fileMetadata);
                int number = fileMetadata.NbDataServers - fileMetadata.CurrentNbDataServers;
                this.EnqueueSelectDataServer(filename, enqueueAction, number);
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
