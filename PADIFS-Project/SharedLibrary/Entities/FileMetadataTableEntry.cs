using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class FileMetadataTableEntry
    {
        private FileMetadata fileMetadata;
        private ConcurrentQueue<Action<string>> pending;

        public FileMetadataTableEntry(FileMetadata fileMetadata)
        {
            this.fileMetadata = fileMetadata;
            this.pending = new ConcurrentQueue<Action<string>>();
        }

        public override string ToString()
        {
            string pendingString = "[ ";
            foreach (var entry in pending)
            {
                pendingString += "<" + entry.ToString() + "> ";
            }
            pendingString += "]";

            return this.fileMetadata.ToString() + ":" + pendingString;
        }

        public FileMetadata FileMetadata
        {
            get { return this.fileMetadata; }
            set { this.fileMetadata = value; }
        }

        public ConcurrentQueue<Action<string>> PendingRequests
        {
            get { return this.pending; }
        }
    }
}
