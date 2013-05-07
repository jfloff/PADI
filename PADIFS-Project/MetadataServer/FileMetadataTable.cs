using SharedLibrary.Entities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class FileMetadataTable
    {
        private class TableEntry 
        {
            public FileMetadata metadata;
            public ConcurrentQueue<Action<string>> pending = new ConcurrentQueue<Action<string>>();
            public ConcurrentDictionary<string, bool> clients = new ConcurrentDictionary<string,bool>();

            public TableEntry(FileMetadata metadata)
            {
                this.metadata = metadata;
            }
        }

        // filename / FileMetadata+queue of pending requests for each file 
        private ConcurrentDictionary<string, TableEntry> files = new ConcurrentDictionary<string, TableEntry>();
        // localFilename / filename
        private ConcurrentDictionary<string, string> localFilenames = new ConcurrentDictionary<string, string>();
        
        public override string ToString()
        {
            string ret = "[\n";
            foreach (var entry in files)
            {
                ret += "  <" + entry.Value.metadata + ":" + entry.Value.pending.Count + ":" + entry.Value.clients.Count + "> \n";
            }
            return ret + "]";
        }

        public bool Contains(string filename)
        {
            return this.files.ContainsKey(filename);
        }

        public FileMetadata FileMetadata(string filename)
        {
            return files[filename].metadata;
        }
        
        // returns true if it signal was really made (not duplicate)
        public bool Open(string clientId, string filename)
        {
            if (!files[filename].clients.ContainsKey(clientId))
            {
                files[filename].clients[clientId] = true;
                return true;
            }
            // already open before
            return false;
        }

        // returns true if it signal was really made (not duplicate)
        public bool Close(string clientId, string filename)
        {
            if (files[filename].clients.ContainsKey(clientId))
            {
                bool ignored; files[filename].clients.TryRemove(clientId, out ignored);
                return true;
            }
            // never opened the file
            return false;
        }

        public FileMetadata Remove(string filename)
        {
            TableEntry tableEntry;
            this.files.TryRemove(filename, out tableEntry);

            // remove localfilenames
            foreach (var entry in tableEntry.metadata.LocalFilenames)
            {
                string localFilename = entry.Value;
                string ingoredFilename; this.localFilenames.TryRemove(localFilename, out ingoredFilename);
            }

            return tableEntry.metadata;
        }

        public void Create(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            this.files[filename] = new TableEntry(new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum));
            Open(clientId, filename);
        }

        public void AddDataServer(string filename, string dataServerId, string location, string localFilename)
        {
            this.files[filename].metadata.AddDataServer(dataServerId, location, localFilename);
            localFilenames[localFilename] = filename;
        }

        public void EnqueueSelect(string filename, Action<string> pending)
        {
            this.files[filename].pending.Enqueue(pending);
        }

        public Action<string> DequeueSelect(string filename)
        {
            Action<string> pending;
            this.files[filename].pending.TryDequeue(out pending);
            return pending;
        }

        // returns files that have pending requests
        public IEnumerable<string> Pending
        {
            get
            {
                foreach (var entry in files)
                {
                    if (entry.Value.pending.Count != 0)
                    {
                        yield return entry.Key;
                    }
                }
            }
        }
    }
}
