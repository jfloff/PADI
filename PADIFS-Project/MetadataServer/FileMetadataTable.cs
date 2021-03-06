﻿using SharedLibrary.Entities;
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
            public bool locked = false;

            public TableEntry(FileMetadata metadata)
            {
                this.metadata = metadata;
            }
        }

        // filename / FileMetadata+queue of pending requests for each file 
        private ConcurrentDictionary<string, TableEntry> files = new ConcurrentDictionary<string, TableEntry>();
        // localFilename / filename
        private ConcurrentDictionary<string, string> filenamesByLocal = new ConcurrentDictionary<string, string>();
        
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
            return files[filename].clients.TryAdd(clientId, true);
        }

        // returns true if it signal was really made (not duplicate)
        public bool Close(string clientId, string filename)
        {
            bool ignored; return files[filename].clients.TryRemove(clientId, out ignored);
        }

        public FileMetadata Remove(string filename)
        {
            TableEntry tableEntry;
            if (this.files.TryRemove(filename, out tableEntry))
            {
                // remove localfilenames
                foreach (var entry in tableEntry.metadata.LocalFilenames)
                {
                    string localFilename = entry.Value;
                    string ingoredFilename; this.filenamesByLocal.TryRemove(localFilename, out ingoredFilename);
                }

                return tableEntry.metadata;
            }
            return null;
        }

        public void Create(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            this.files[filename] = new TableEntry(new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum));
            Open(clientId, filename);
        }

        public void AddDataServer(string filename, string dataServerId, string location, string localFilename)
        {
            this.files[filename].metadata.AddDataServer(dataServerId, location, localFilename);
            filenamesByLocal[localFilename] = filename;
        }

        public void RemoveDataServer(string filename, string oldDataServerId)
        {
            string localFilename = this.files[filename].metadata.RemoveDataServer(oldDataServerId);
            string ignored; filenamesByLocal.TryRemove(localFilename, out ignored);
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

        public string FilenameByLocalFilename(string localFilename)
        {
            return this.filenamesByLocal[localFilename];
        }

        public string LocalFilenameByFilename(string filename, string dataServerid)
        {
            return this.files[filename].metadata.LocalFilename(dataServerid);
        }

        // checks if file doesn't have any clients
        public bool Free(string filename)
        {
            return (this.files[filename].clients.Count == 0);
        }

        public bool Locked(string filename)
        {
            return this.files[filename].locked;
        }

        public void Lock(string filename)
        {
            this.files[filename].locked = true;
        }

        public void Unlock(string filename)
        {
            this.files[filename].locked = false;
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

        public bool FileInDataServer(string filename, string dataServerId)
        {
            return this.files[filename].metadata.InDataServer(dataServerId);
        }
    }
}
