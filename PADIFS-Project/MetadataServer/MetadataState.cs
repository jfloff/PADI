using SharedLibrary.Entities;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Metadata
{
    // Store on each entry of the data servers dictionary
    public class DataServerInfo
    {
        private string location;
        private IDataServerToMetadata dataServer;

        public DataServerInfo(string location)
        {
            this.location = location;
            this.dataServer = (IDataServerToMetadata)Activator.GetObject(
                        typeof(IDataServerToMetadata),
                        location);
        }

        public string Location
        {
            get { return this.location; }
            private set { this.location = value; }
        }

        public IDataServerToMetadata DataServer
        {
            get { return this.dataServer; }
            private set { this.dataServer = value; }
        }
    }

    public class MetadataState
    {
        private struct Snapshot
        {
            public Dictionary<string, FileMetadata> table;
            public Dictionary<string, string> dataServers;

            public Snapshot(ConcurrentDictionary<string, FileMetadata> table, ConcurrentDictionary<string, DataServerInfo> dataServers)
            {
                this.table = new Dictionary<string, FileMetadata>(table);
                this.dataServers = new Dictionary<string, string>();
                foreach (var entry in dataServers)
                {
                    string id = entry.Key;
                    string location = entry.Value.Location;

                    this.dataServers[id] = location;
                }
            }
        }

        // filename / FileMetadata
        private ConcurrentDictionary<string, FileMetadata> table
            = new ConcurrentDictionary<string, FileMetadata>();
        // id / Interface
        private ConcurrentDictionary<string, DataServerInfo> dataServers
            = new ConcurrentDictionary<string, DataServerInfo>();
        // CREATE DICTIONARY FOR FAILED METADATAS

        // marked medata id / snapshots
        private ConcurrentDictionary<string, Snapshot> marks = new ConcurrentDictionary<string, Snapshot>();

        public ConcurrentDictionary<string, FileMetadata> FileMetadataTable
        {
            get { return this.table; }
        }

        public ConcurrentDictionary<string, DataServerInfo> DataServers
        {
            get { return this.dataServers; }
        }

        public override string ToString()
        {

            string ret = "Files = [\n";
            foreach (var entry in table)
            {
                FileMetadata fileMetadata = entry.Value;

                ret += "  <" + fileMetadata + "> \n";
            }
            ret += "]\n";
            ret += "Data Server = [\n";
            foreach (var entry in dataServers)
            {
                string id = entry.Key;

                ret += "  <" + id + "> \n";
            }
            return ret + "]";
        }

        /**
         * Log Management
         */

        public bool HasMark(string mark)
        {
            return marks.ContainsKey(mark);
        }

        // adds mark to start keeping states
        public void AddMark(string mark)
        {
            // keeps older marks
            if (!marks.ContainsKey(mark))
            {
                marks[mark] = new Snapshot(table, dataServers);
            }
        }

        // removes mark
        public void RemoveMark(string mark)
        {
            Snapshot ignored; marks.TryRemove(mark, out ignored);
        }

        public MetadataDiff GetDiff(string mark)
        {
            Snapshot current = new Snapshot(table, dataServers);
            DictionaryDiff<string, FileMetadata> tableDiff;
            DictionaryDiff<string, string> dataServersDiff;

            if (!marks.ContainsKey(mark))
            {
                tableDiff = new DictionaryDiff<string, FileMetadata>(current.table);
                dataServersDiff = new DictionaryDiff<string, string>(current.dataServers);
            }
            else
            {
                Snapshot past = marks[mark];
                tableDiff = new DictionaryDiff<string, FileMetadata>(past.table, current.table);
                dataServersDiff = new DictionaryDiff<string, string>(past.dataServers, current.dataServers);
            }

            return new MetadataDiff(tableDiff, dataServersDiff);
        }

        public void MergeDiff(MetadataDiff diff)
        {
            // files
            foreach (var entry in diff.TableDiff.Plus)
            {
                table[entry.Key] = entry.Value;
            }
            foreach (var entry in diff.TableDiff.Minus)
            {
                FileMetadata ingored; table.TryRemove(entry.Key, out ingored);
            }

            // data servers
            foreach (var entry in diff.DataServersDiff.Plus)
            {
                dataServers[entry.Key] = new DataServerInfo(entry.Value);
            }
        }
    }
}
