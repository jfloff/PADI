using SharedLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    // implements IEnumerable so we can foreach this class
    public class MetadataDiff
    {
        // filename / metadata
        private DictionaryDiff<string, FileMetadata> tableDiff;
        // id / location
        private DictionaryDiff<string, string> dataServersDiff;
        // mark / snapshot
        private Dictionary<string, MetadataSnapshot> marks;
        // sequence for number
        private int sequence;

        public MetadataDiff(DictionaryDiff<string, FileMetadata> tableDiff,  
            DictionaryDiff<string, string> dataServersDiff,
            Dictionary<string, MetadataSnapshot> marks, 
            int sequence)
        {
            this.tableDiff = tableDiff;
            this.dataServersDiff = dataServersDiff;
            this.marks = marks;
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

        public Dictionary<string, MetadataSnapshot> Marks
        {
            get { return this.marks; }
        }

        public int Sequence 
        {
            get { return this.sequence; }
        }
         
    }
}

