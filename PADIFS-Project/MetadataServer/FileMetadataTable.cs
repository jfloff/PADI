using SharedLibrary.Entities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class FileMetadataTable
    {
        // filename / FileMetadata+queue of pending requests for each file 
        private ConcurrentDictionary<string, FileMetadata> files = new ConcurrentDictionary<string, FileMetadata>();
        private ConcurrentDictionary<string, ConcurrentQueue<Action<string>>> pending
            = new ConcurrentDictionary<string, ConcurrentQueue<Action<string>>>();

        public override string ToString()
        {
            string ret = "[\n";
            foreach (var entry in files)
            {
                ret += "  <" + entry.Value + ":" + pending[entry.Key].Count + "> \n";
            }
            return ret + "]";
        }

        public Dictionary<string, FileMetadata> ToDictionary()
        {
            Dictionary<string, FileMetadata> clone = new Dictionary<string, FileMetadata>();
            foreach (var entry in files)
            {
                clone[entry.Key] = entry.Value.Clone();
            }
            return clone;
        }

        public bool Contains(string filename)
        {
            return this.files.ContainsKey(filename);
        }

        public void Remove(string filename)
        {
            FileMetadata ingoredFile; this.files.TryRemove(filename, out ingoredFile);
            ConcurrentQueue<Action<string>> ignoredPending; this.pending.TryRemove(filename, out ignoredPending);
        }

        public FileMetadata FileMetadata(string filename)
        {
            return files[filename];
        }

        public void EnqueueSelectDataServer(string filename, Action<string> enqueueAction, int number = 1)
        {
            if (number <= 0) return;

            for (; number > 0; number--)
            {
                pending[filename].Enqueue(enqueueAction);
            }
        }

        public void DequeueSelectDataServer(string filename, int number = -1)
        {
            if (number >= 0) return;

            for (; number < 0; number++)
            {
                Action<string> ignored; pending[filename].TryDequeue(out ignored);
            }
        }

        public void SetFileMetadata(string filename, FileMetadata fileMetadata, Action<string> enqueueAction)
        {
            if (this.Contains(filename))
            {
                int number = fileMetadata.NbDataServers - fileMetadata.CurrentNbDataServers - pending[filename].Count;
                EnqueueSelectDataServer(filename, enqueueAction, number);
                DequeueSelectDataServer(filename, number);
                this.files[filename] = fileMetadata;
            }
            else
            {
                this.files[filename] = fileMetadata;
                this.pending[filename] = new ConcurrentQueue<Action<string>>();
                int number = fileMetadata.NbDataServers - fileMetadata.CurrentNbDataServers;
                this.EnqueueSelectDataServer(filename, enqueueAction, number);
            }
        }

        public IEnumerable<KeyValuePair<string, FileMetadata>> Files
        {
            get
            {
                foreach (var entry in files)
                {
                    yield return entry;
                }
            }
        }

        public IEnumerable<ConcurrentQueue<Action<string>>> Pending
        {
            get
            {
                foreach (var entry in pending)
                {
                    if (entry.Value.Count != 0)
                    {
                        yield return entry.Value;
                    }
                }
            }
        }
    }
}
