using SharedLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    // implements IEnumerable so we can foreach this class
    public class MetadataLogDiff
    {
        // filename / metadata
        private DictionaryDiff<string, FileMetadata> tableDiff;
        // id / location
        private DictionaryDiff<string, string> dataServersDiff;
        // mark / snapshot
        //
        private int sequence;

        public MetadataLogDiff(DictionaryDiff<string, FileMetadata> tableDiff,  DictionaryDiff<string, string> dataServersDiff, int sequence)
        {
            this.tableDiff = tableDiff;
            this.dataServersDiff = dataServersDiff;
            this.sequence = sequence;
        }

        public DictionaryDiff<string, FileMetadata> TableDiff
        {
            get { return this.tableDiff; }
        }

        public DictionaryDiff<string, string> DataServersDiff
        {
            get { return this.dataServersDiff; }
        }

        public int Sequence 
        {
            get { return this.sequence; }
        }
         
    }
}

