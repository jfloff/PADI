using SharedLibrary;
using SharedLibrary.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Metadata
{
    public class Log
    {
        private static readonly char separator = '\n';

        // marked medata id / snapshots
        private ConcurrentDictionary<string, int> marks = new ConcurrentDictionary<string, int>();
        // sequence / operation
        private ConcurrentDictionary<int, string> operations = new ConcurrentDictionary<int, string>();

        // overwrites mark even if there is a new one -- to avoid creating a RemoveMarkOnMetadata
        public void ForceAddMark(string mark, int sequence)
        {
            marks[mark] = sequence;
        }

        // adds mark to start keeping states
        public bool AddMark(string mark, int sequence)
        {
            // keeps older marks
            if (!marks.ContainsKey(mark) || (marks[mark] > sequence))
            {
                marks[mark] = sequence;
                return true;
            }
            return false;
        }

        public void LogOperation(int sequence, string operation, params object[] args)
        {
            string serialize = operation + separator;
            foreach (object arg in args)
            {
                serialize += Helper.SerializeObject<object>(arg) + separator;
            }
            operations[sequence] = serialize;
        }

        public MetadataDiff BuildDiff(string mark, int clock)
        {
            MetadataDiff diff = new MetadataDiff();

            // if no operations logged
            if (operations.Count == 0) return diff;

            // starts at clock to discount for last increment
            int startAt = clock - 1;
            // either has a mark, or we need the whole state
            int stopAt = GetMarkSequence(mark);

            // adds operations missing to the diff
            for (int i = startAt; i >= stopAt; i--)
            {
                diff.AddOperation(operations[i]);
            }

            return diff;
        }

        private int GetMarkSequence(string mark)
        {
            int sequence = 0;
            if (marks.ContainsKey(mark))
            {
                marks.TryRemove(mark, out sequence);
            }
            return sequence;
        }

        public void MergeDiff(Metadata metadata, MetadataDiff diff)
        {
            foreach (string operation in diff.Operations)
            {
                string[] words = operation.Split(separator);
                string methodName = words[0];

                switch (methodName)
                {
                    case ("AddMarkOnMetadata"):
                        {
                            string mark = Helper.DeserializeObject<string>(words[1]);
                            int markSequence = Helper.DeserializeObject<int>(words[2]);
                            int sequence = Helper.DeserializeObject<int>(words[3]);

                            metadata.AddMarkOnMetadata(mark, markSequence, sequence);
                            break;
                        }
                    case ("OpenOnMetadata"):
                        {
                            string clientId = Helper.DeserializeObject<string>(words[1]);
                            string filename = Helper.DeserializeObject<string>(words[2]);
                            int sequence = Helper.DeserializeObject<int>(words[3]);

                            metadata.OpenOnMetadata(clientId, filename, sequence);
                            break;
                        }
                    case ("CloseOnMetadata"):
                        {
                            string clientId = Helper.DeserializeObject<string>(words[1]);
                            string filename = Helper.DeserializeObject<string>(words[2]);
                            int sequence = Helper.DeserializeObject<int>(words[3]);

                            metadata.CloseOnMetadata(clientId, filename, sequence);
                            break;
                        }
                    case ("CreateOnMetadata"):
                        {
                            string clientId = Helper.DeserializeObject<string>(words[1]);
                            string filename = Helper.DeserializeObject<string>(words[2]);
                            int nbDataServers = Helper.DeserializeObject<int>(words[3]);
                            int readQuorum = Helper.DeserializeObject<int>(words[4]);
                            int writeQuorum = Helper.DeserializeObject<int>(words[5]);
                            int sequence = Helper.DeserializeObject<int>(words[6]);

                            metadata.CreateOnMetadata(clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);
                            break;
                        }
                    case ("SelectOnMetadata"):
                        {
                            string filename = Helper.DeserializeObject<string>(words[1]);
                            string dataServerId = Helper.DeserializeObject<string>(words[2]);
                            string localFilename = Helper.DeserializeObject<string>(words[3]);
                            int sequence = Helper.DeserializeObject<int>(words[4]);

                            metadata.SelectOnMetadata(filename, dataServerId, localFilename, sequence);
                            break;
                        }
                    case ("DeleteOnMetadata"):
                        {
                            string filename = Helper.DeserializeObject<string>(words[1]);
                            int sequence = Helper.DeserializeObject<int>(words[2]);

                            metadata.DeleteOnMetadata(filename, sequence);
                            break;
                        }
                    case ("DataServerOnMetadata"):
                        {
                            string dataServerId = Helper.DeserializeObject<string>(words[1]);
                            string location = Helper.DeserializeObject<string>(words[2]);
                            int sequence = Helper.DeserializeObject<int>(words[3]);

                            metadata.DataServerOnMetadata(dataServerId, location, sequence);
                            break;
                        }
                    case ("MigrateFileOnMetadata"):
                        {
                            string filename = Helper.DeserializeObject<string>(words[1]);
                            string oldDataServerId = Helper.DeserializeObject<string>(words[2]);
                            string newDataServerId = Helper.DeserializeObject<string>(words[3]);
                            string oldLocalFilename = Helper.DeserializeObject<string>(words[4]);
                            string newLocalFilename = Helper.DeserializeObject<string>(words[5]);
                            int sequence = Helper.DeserializeObject<int>(words[6]);

                            metadata.MigrateFileOnMetadata(filename, oldDataServerId, newDataServerId, oldLocalFilename, newLocalFilename, sequence);
                            break;
                        }
                    default:
                        {
                            throw new Exception("METHOD CALL NOT SERIALIZABLE: " + methodName);
                        }
                }
            }
        }
    }
}