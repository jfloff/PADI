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
            int stopAt = (marks.ContainsKey(mark)) ? marks[mark] : 0;

            // adds operations missing to the diff
            for (int i = startAt; i >= stopAt; i--)
            {
                diff.AddOperation(operations[i]);
            }

            return diff;
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
                            metadata.AddMarkOnMetadata(Helper.DeserializeObject<string>(words[1]), 
                                Helper.DeserializeObject<int>(words[2]), 
                                Helper.DeserializeObject<int>(words[3]));
                            break;
                        }
                    case ("OpenOnMetadata"):
                        {
                            metadata.OpenOnMetadata(Helper.DeserializeObject<string>(words[1]), 
                                Helper.DeserializeObject<string>(words[2]), 
                                Helper.DeserializeObject<int>(words[3]));
                            break;
                        }
                    case ("CloseOnMetadata"):
                        {
                            metadata.CloseOnMetadata(Helper.DeserializeObject<string>(words[1]),
                                Helper.DeserializeObject<string>(words[2]),
                                Helper.DeserializeObject<int>(words[3]));
                            break;
                        }
                    case ("CreateOnMetadata"):
                        {
                            metadata.CreateOnMetadata(Helper.DeserializeObject<string>(words[1]), 
                                Helper.DeserializeObject<string>(words[2]), 
                                Helper.DeserializeObject<int>(words[3]),
                                Helper.DeserializeObject<int>(words[4]), 
                                Helper.DeserializeObject<int>(words[5]), 
                                Helper.DeserializeObject<int>(words[6]));
                            break;
                        }
                    case ("SelectOnMetadata"):
                        {
                            metadata.SelectOnMetadata(Helper.DeserializeObject<string>(words[1]), 
                                Helper.DeserializeObject<string>(words[2]),
                                Helper.DeserializeObject<string>(words[3]), 
                                Helper.DeserializeObject<int>(words[4]));
                            break;
                        }
                    case ("DeleteOnMetadata"):
                        {
                            metadata.DeleteOnMetadata(Helper.DeserializeObject<string>(words[1]), 
                                Helper.DeserializeObject<int>(words[2]));
                            break;
                        }
                    case ("DataServerOnMetadata"):
                        {
                            metadata.DataServerOnMetadata(Helper.DeserializeObject<string>(words[1]), 
                                Helper.DeserializeObject<string>(words[2]), 
                                Helper.DeserializeObject<int>(words[3]));
                            break;
                        }
                    case ("HeartbeatOnMetadata"):
                        {
                            metadata.HeartbeatOnMetadata(Helper.DeserializeObject<string>(words[1]),
                                Helper.DeserializeObject<Heartbeat>(words[2]),
                                Helper.DeserializeObject<int>(words[3]));
                            break;
                        }
                    default:
                        {
                            throw new Exception("NOT SUPPOSED TO REACH HERE" + methodName);
                        }
                }
            }
        }
    }
}