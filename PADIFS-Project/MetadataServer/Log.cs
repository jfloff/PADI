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
                serialize += arg.ToString() + separator;
            }
            operations[sequence] = serialize;
        }

        public MetadataDiff BuildDiff(string mark, int clock)
        {
            // starts at clock-1 becasue clock is incremented at the end of an operation
            int startAt = clock - 1;
            // either has a mark, or we need the whole state
            int stopAt = (marks.ContainsKey(mark)) ? marks[mark] : 0;

            // adds operations missing to the diff
            MetadataDiff diff = new MetadataDiff();
            // only if there is any operations
            if (operations.Count != 0)
            {
                for (int i = startAt; i >= stopAt; i--)
                {
                    diff.AddOperation(operations[i]);
                }
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
                            metadata.AddMarkOnMetadata(words[1], int.Parse(words[2]), int.Parse(words[3]));
                            break;
                        }
                    case ("OpenOnMetadata"):
                        {
                            metadata.OpenOnMetadata(words[1], words[2], int.Parse(words[3]));
                            break;
                        }
                    case ("CloseOnMetadata"):
                        {
                            metadata.CloseOnMetadata(words[1], words[2], int.Parse(words[3]));
                            break;
                        }
                    case ("CreateOnMetadata"):
                        {
                            metadata.CreateOnMetadata(words[1], words[2], int.Parse(words[3]), int.Parse(words[4]), int.Parse(words[5]), int.Parse(words[6]));
                            break;
                        }
                    case ("SelectOnMetadata"):
                        {
                            metadata.SelectOnMetadata(words[1], words[2], words[3], int.Parse(words[4]));
                            break;
                        }
                    case ("DeleteOnMetadata"):
                        {
                            metadata.DeleteOnMetadata(words[1], int.Parse(words[2]));
                            break;
                        }
                    case ("DataServerOnMetadata"):
                        {
                            metadata.DataServerOnMetadata(words[1], words[2], int.Parse(words[3]));
                            break;
                        }
                    case ("HeartbeatOnMetadata"):
                        {
                            throw new NotImplementedException();
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