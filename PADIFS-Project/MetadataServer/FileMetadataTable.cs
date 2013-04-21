﻿using SharedLibrary.Entities;
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
            for (int i = 0; i < number; i++)
            {
                table[filename].PendingRequests.Enqueue(enqueueAction);
            }
        }

        public void SetFileMetadata(string filename, FileMetadata fileMetadata, Action<string> enqueueAction)
        {
            if (this.Contains(filename))
            {
                this.table[filename].FileMetadata = fileMetadata;
                // missing requests
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