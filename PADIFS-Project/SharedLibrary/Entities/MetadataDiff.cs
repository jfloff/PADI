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
        // id location
        private DictionaryDiff<string, string> dataServersDiff;

        public MetadataDiff(DictionaryDiff<string, FileMetadata> tableDiff, DictionaryDiff<string, string> dataServersDiff)
        {
            this.tableDiff = tableDiff;
            this.dataServersDiff = dataServersDiff;
        }

        public DictionaryDiff<string, FileMetadata> TableDiff
        {
            get { return this.tableDiff; }
        }

        public DictionaryDiff<string, string> DataServersDiff
        {
            get { return this.dataServersDiff; }
        }
    }
}

