using SharedLibrary;
using SharedLibrary.Entities;
using System;
using System.Collections.Concurrent;

namespace Metadata
{
    public class Log
    {
        private static readonly char SEPARATOR = '\n';

        // sequence / operation
        private ConcurrentDictionary<int, string> operations = new ConcurrentDictionary<int, string>();

        public void LogOperation(int sequence, string operation, params object[] args)
        {
            string serialize = operation + SEPARATOR;
            foreach (object arg in args)
            {
                serialize += Helper.SerializeObject<object>(arg) + SEPARATOR;
            }
            operations[sequence] = serialize;
        }

        public MetadataDiff BuildDiff(string mark, int sequence)
        {
            MetadataDiff diff = new MetadataDiff();
            
            foreach (var entry in operations)
            {   
                // skips marks that don't need
                if (entry.Key >= sequence)
                {
                    diff.AddOperation(entry.Value);
                }
            }

            return diff;
        }

        public void MergeDiff(Metadata metadata, MetadataDiff diff)
        {
            foreach (string operation in diff.Operations)
            {
                string[] words = operation.Split(SEPARATOR);
                string methodName = words[0];

                switch (methodName)
                {
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