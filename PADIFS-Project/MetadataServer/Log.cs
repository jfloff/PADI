using SharedLibrary.Entities;
using SharedLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Metadata
{
    public class Log
    {
        private struct Snapshot {
            public Dictionary<string, FileMetadata> table; 
            public Dictionary<string, string> dataServers;
            public int sequence;

            public Snapshot(FileMetadataTable table, DataServerRegister dataServers, int sequence)
            {
                this.table = table.ToDictionary();
                this.dataServers = dataServers.ToDictionary();
                this.sequence = sequence;
            }
        }

        private DataServerRegister dataServers;
        private FileMetadataTable table;

        // marked medata id / snapshots
        private ConcurrentDictionary<string, Snapshot> marks = new ConcurrentDictionary<string, Snapshot>();

        public Log(FileMetadataTable table, DataServerRegister dataServers)
        {
            this.table = table;
            this.dataServers = dataServers;
        }

        public bool HasMark(string mark)
        {
            return marks.ContainsKey(mark);
        }

        // adds mark to start keeping states
        public void AddMark(string mark, int sequence)
        {
            // keeps older marks
            if (!marks.ContainsKey(mark))
            {
                marks[mark] = new Snapshot(table, dataServers, sequence);
            }
            else if (marks[mark].sequence > sequence)
            {
                marks[mark] = new Snapshot(table, dataServers, sequence);
            }
        }

        public MetadataLogDiff BuildDiff(string mark, int sequence)
        {
            Snapshot current = new Snapshot(table, dataServers, sequence);
            DictionaryDiff<string, FileMetadata> tableDiff;
            DictionaryDiff<string, string> dataServersDiff;
            int sequenceToDiff = sequence;

            if (!marks.ContainsKey(mark))
            {
                tableDiff = new DictionaryDiff<string, FileMetadata>(current.table);
                dataServersDiff = new DictionaryDiff<string, string>(current.dataServers);
            }
            else
            {
                Snapshot past;
                marks.TryRemove(mark, out past);
                tableDiff = new DictionaryDiff<string, FileMetadata>(past.table, current.table);
                dataServersDiff = new DictionaryDiff<string, string>(past.dataServers, current.dataServers);
                sequenceToDiff = past.sequence;
            }

            return new MetadataLogDiff(tableDiff, dataServersDiff, sequenceToDiff);
        }

        public int MergeDiff(MetadataLogDiff diff, Func<FileMetadata,Action<string>> funcFactory)
        {
            int clockDiff = 0;

            // files
            foreach (var entry in diff.TableDiff.Plus)
            {
                string filename = entry.Key;
                FileMetadata fileMetadata = entry.Value;

                // create file found
                if (!this.table.Contains(filename))
                {
                    // clocks data servers and create file
                    clockDiff += fileMetadata.CurrentNbDataServers + 1;
                }
                else
                {
                    // clocks new data servers
                    clockDiff += fileMetadata.CurrentNbDataServers - this.table.FileMetadata(filename).CurrentNbDataServers;
                }

                this.table.SetFileMetadata(filename, fileMetadata, funcFactory(fileMetadata));
            }
            foreach (var entry in diff.TableDiff.Minus)
            {
                clockDiff++;
                this.table.Remove(entry.Key);
            }

            // data servers
            foreach (var entry in diff.DataServersDiff.Plus)
            {
                clockDiff++;
                this.dataServers[entry.Key] = entry.Value;
            }

            return clockDiff;
        }
    }
}
