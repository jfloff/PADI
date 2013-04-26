using SharedLibrary.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metadata
{
    public class Log
    {
        private DataServerRegister dataServers;
        private FileMetadataTable table;

        // marked medata id / snapshots
        private ConcurrentDictionary<string, MetadataSnapshot> marks = new ConcurrentDictionary<string, MetadataSnapshot>();

        public Log(FileMetadataTable table, DataServerRegister dataServers)
        {
            this.table = table;
            this.dataServers = dataServers;
        }

        // adds mark to start keeping states
        public void AddMark(string mark, int sequence)
        {
            // keeps older marks
            if (!marks.ContainsKey(mark))
            {
                marks[mark] = new MetadataSnapshot(table.ToDictionary(), dataServers.ToDictionary(), sequence);
            }
            else if (marks[mark].sequence > sequence)
            {
                marks[mark] = new MetadataSnapshot(table.ToDictionary(), dataServers.ToDictionary(), sequence);
            }
        }

        public MetadataDiff BuildDiff(string mark)
        {
            // ignores sequence parameter
            MetadataSnapshot current = new MetadataSnapshot(table.ToDictionary(), dataServers.ToDictionary(), -1);
            DictionaryDiff<string, FileMetadata> tableDiff;
            DictionaryDiff<string, string> dataServersDiff;
            int sequenceToDiff;

            // if BuildDiff was called on a non-existant mark, give whole state
            if (!marks.ContainsKey(mark))
            {
                tableDiff = new DictionaryDiff<string, FileMetadata>(current.table);
                dataServersDiff = new DictionaryDiff<string, string>(current.dataServers);
                sequenceToDiff = 0;
            }
            // otherwise we give the state since last mark, and the sequence the snapshot was taken
            else
            {
                MetadataSnapshot past;
                marks.TryRemove(mark, out past);
                tableDiff = new DictionaryDiff<string, FileMetadata>(past.table, current.table);
                dataServersDiff = new DictionaryDiff<string, string>(past.dataServers, current.dataServers);
                sequenceToDiff = past.sequence;
            }

            Dictionary<string, MetadataSnapshot> marksCopy = new Dictionary<string,MetadataSnapshot>(marks);

            return new MetadataDiff(tableDiff, dataServersDiff, marksCopy, sequenceToDiff);
        }

        public int MergeDiff(MetadataDiff diff, Func<FileMetadata,Action<string>> funcFactory)
        {
            int clockDiff = 0;

            // marks
            foreach (var entry in diff.Marks)
            {
                marks[entry.Key] = entry.Value;
            }

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
