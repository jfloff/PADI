using SharedLibrary.Entities;
using SharedLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Metadata
{
    public class MetadataState
    {
        private struct Snapshot
        {
            public Dictionary<string, FileMetadata> table;
            public Dictionary<string, string> dataServers;

            public Snapshot(ConcurrentDictionary<string, FileMetadata> table, ConcurrentDictionary<string, string> dataServers)
            {
                this.table = new Dictionary<string, FileMetadata>(table);
                this.dataServers = new Dictionary<string, string>(dataServers);
            }
        }

        // filename / FileMetadata
        private ConcurrentDictionary<string, FileMetadata> table = new ConcurrentDictionary<string, FileMetadata>();
        // id / location
        private ConcurrentDictionary<string, string> dataServers = new ConcurrentDictionary<string, string>();
        // id / weight
        private ConcurrentDictionary<string, int> dataServersWeight = new ConcurrentDictionary<string, int>();
        // CREATE DICTIONARY FOR FAILED METADATAS

        // marked medata id / snapshots
        private ConcurrentDictionary<string, Snapshot> marks = new ConcurrentDictionary<string, Snapshot>();

        public MetadataState()
        {
            this.dataServersEnumerator = dataServers.GetEnumerator();
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
            ret += "Data Servers = ";
            ret += "[\n";
            foreach (var entry in dataServers)
            {
                ret += "  <" + entry.Key + ";" + entry.Value + "> \n";
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
                this.AddOrUpdateFile(entry.Key, entry.Value);
            }
            foreach (var entry in diff.TableDiff.Minus)
            {
                this.RemoveFile(entry.Key);
            }

            // data servers
            foreach (var entry in diff.DataServersDiff.Plus)
            {
                this.AddOrUpdateDataServer(entry.Key, entry.Value);
            }
        }

        /**
         * Data Servers Methods
         */

        public bool ContainsDataServer(string id)
        {
            return this.dataServers.ContainsKey(id);
        }

        public void AddOrUpdateDataServer(string id, string location)
        {
            if (!this.dataServers.ContainsKey(id))
            {
                dataServersWeight[id] = 1;
            }

            this.dataServers[id] = location;
            this.dataServersEnumerator.MoveNext();
        }

        public string DataServerLocation(string id)
        {
            return this.dataServers[id];
        }

        private IEnumerator<KeyValuePair<string, string>> dataServersEnumerator;

        public IEnumerable<KeyValuePair<string, string>> UniqueDataServers
        {
            get
            {
                string firstId = this.dataServersEnumerator.Current.Key;
                while (true)
                {
                    yield return this.dataServersEnumerator.Current;
                    if (!this.dataServersEnumerator.MoveNext())
                    {
                        this.dataServersEnumerator = this.dataServers.GetEnumerator();
                        this.dataServersEnumerator.MoveNext();
                    }

                    if (this.dataServersEnumerator.Current.Key == firstId) break;

                }
            }
        }


        /**
         * Table Methods
         */

        public bool ContainsFile(string filename)
        {
            return this.table.ContainsKey(filename);
        }

        public void AddOrUpdateFile(string filename, FileMetadata fileMetadata)
        {
            this.table[filename] = fileMetadata;
        }

        public void RemoveFile(string filename)
        {
            FileMetadata ignored; this.table.TryRemove(filename, out ignored);
        }

        public FileMetadata FileMetadata(string filename)
        {
            return this.table[filename];
        }
    }
}
