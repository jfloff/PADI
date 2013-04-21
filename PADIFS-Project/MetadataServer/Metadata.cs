using SharedLibrary;
using SharedLibrary.Entities;
using SharedLibrary.Exceptions;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace Metadata
{
    public class Metadata : MarshalByRefObject, IMetadataToPM, IMetadataToMetadata, IMetadataToClient, IMetadataToDataServer
    {
        // id / interface
        private static ConcurrentDictionary<string, IMetadataToMetadata> metadatas
            = new ConcurrentDictionary<string, IMetadataToMetadata>();
        // register with data servers
        private static DataServerRegister dataServers = new DataServerRegister();
        // file metadata table
        private static FileMetadataTable fileMetadataTable = new FileMetadataTable();
        // log
        private static Log log = new Log(fileMetadataTable, dataServers);

        // statics are all thread safe
        private static string master;
        private static string id;
        private static bool fail = false;


        // infinite lease
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong Arguments");

            id = master = args[0];
            int port = Convert.ToInt32(args[1]);

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);
            Console.Title = id;

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Metadata),
                id,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Metadata Server " + id + " has started.");
            Console.ReadLine();
        }

        private bool ImMaster
        {
            get { return (id == master); }
        }

        /**
         * File related methods
         */

        // returns localFilename for dataServer
        private static string LocalFilename(string filename)
        {
            // GUID better probably?
            return filename + "$" + filename.GetHashCode();
        }

        /**
         * Functions to deal with other metadatas
         */
        private void CreateOrUpdateOnMetadatas(FileMetadata fileMetadata)
        {
            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToMetadata metadata = entry.Value;

                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.CreateOrUpdateOnMetadata(fileMetadata);

                        // if it didnt launch exception until now
                        // we check if there is a mark so we can update
                        // we only do after create to avoid unecessary diff operations
                        if (log.HasMark(id))
                        {
                            metadata.UpdateState(log.BuildDiff(id));
                            log.RemoveMark(id);
                        }
                    }
                    catch (ProcessFailedException)
                    {
                        log.AddMark(id);
                    }
                });
                request.Start();
            }
        }

        private void DeleteOnMetadatas(FileMetadata fileMetadata)
        {
            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.DeleteOnMetadata(fileMetadata);

                        // if it didnt launch exception until now
                        // we check if there is a mark so we can update
                        // we only do after create to avoid unecessary diff operations
                        if (log.HasMark(id))
                        {
                            metadata.UpdateState(log.BuildDiff(id));
                            log.RemoveMark(id);
                        }
                    }
                    catch (ProcessFailedException)
                    {
                        log.AddMark(id);
                    }
                });
                request.Start();
            }
        }

        /**
         * IMetadataToPM Methods
         */

        public void MetadataLocation(string id, string location)
        {
            Console.WriteLine("RECEIVED METADATA LOCATION " + location);

            IMetadataToMetadata metadata = (IMetadataToMetadata)Activator.GetObject(
                typeof(IMetadataToMetadata),
                location);
            metadatas[id] = metadata;

            //Force invocation of primary decision
            master = (string.Compare(id, master) > 0) ? master : id;

            //sends the current metadata state
            if (ImMaster)
            {
                metadata.UpdateState(log.BuildDiff(id));
                log.RemoveMark(id);
            }
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("Files = " + fileMetadataTable);
            Console.WriteLine("Data Servers = " + dataServers);
        }

        public void Fail()
        {
            Console.WriteLine("FAIL");
            fail = true;
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER");
            fail = false;
        }

        /**
         * IMetadataToMetadata Methods
         */

        public void Ping()
        {
            if (fail) throw new ProcessFailedException(id);

            // Console.WriteLine("PONG");
        }

        public Action<string> FutureSelectDataServer(FileMetadata fileMetadata)
        {
            return (futureId) => SelectDataServer(futureId, fileMetadata);
        }

        public void UpdateState(MetadataLogDiff diff)
        {
            log.MergeDiff(diff, FutureSelectDataServer);
        }

        public void CreateOrUpdateOnMetadata(FileMetadata fileMetadata)
        {
            if (fail) throw new ProcessFailedException(id);

            fileMetadataTable.SetFileMetadata(fileMetadata.Filename, fileMetadata, FutureSelectDataServer(fileMetadata));
        }

        public void DeleteOnMetadata(FileMetadata fileMetadata)
        {
            if (fail) throw new ProcessFailedException(id);

            if (fileMetadataTable.Contains(fileMetadata.Filename))
            {
                fileMetadataTable.Remove(fileMetadata.Filename);
            }
        }

        /**
         * IMetadataToClient Methods
         */

        // select single data server. refactored for future selects
        private void SelectDataServer(string id, FileMetadata fileMetadata)
        {
            string location = dataServers[id];
            string localFilename = LocalFilename(fileMetadata.Filename);

            fileMetadata.AddDataServer(id, location, localFilename);
            CreateOrUpdateOnMetadatas(fileMetadata);
        }

        public FileMetadata Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessFailedException(id);

            if (fileMetadataTable.Contains(filename))
                throw new FileAlreadyExistsException(filename);

            Console.WriteLine("CREATE METADATA FILENAME: " + filename + " NBDATASERVERS: " + nbDataServers
                + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            //Select Data Servers and creates files within them
            FileMetadata fileMetadata = new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum);
            
            // select possible data servers
            int selected = 0;
            foreach(var entry in dataServers.UniqueDataServers)
            {
                if (selected++ >= fileMetadata.NbDataServers) break;

                string dataServerId = entry.Key;
                SelectDataServer(dataServerId, fileMetadata);
            }

            fileMetadataTable.SetFileMetadata(filename, fileMetadata, FutureSelectDataServer(fileMetadata));
            return fileMetadata;
        }

        public FileMetadata Open(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("OPEN METADATA FILE " + filename);

            if (!fileMetadataTable.Contains(filename))
                throw new FileDoesNotExistException(filename);

            return fileMetadataTable.FileMetadata(filename);
        }

        public void Close(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("CLOSE METADATA FILE " + filename);

            if (!fileMetadataTable.Contains(filename))
                throw new FileDoesNotExistException(filename);
        }

        public void Delete(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("DELETE METADATA FILE " + filename);

            if (!fileMetadataTable.Contains(filename))
                throw new FileDoesNotExistException(filename);

            FileMetadata fileMetadata = fileMetadataTable.FileMetadata(filename);
            fileMetadataTable.Remove(fileMetadata.Filename);

            DeleteOnMetadatas(fileMetadata);
        }

        /**
         * IMetadataToDataServer Methods
         */

        public void Heartbeat(string id, Heartbeat heartbeat)
        {
            if (fail) throw new ProcessFailedException(id);

            // what should the system do if a data-server fails?
            Console.WriteLine("HEARTBEAT");
        }

        private void DataServerOnMetadatas(string id, string location)
        {
            List<Thread> requests = new List<Thread>();

            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                // clock conflict!
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.DataServerOnMetadata(id, location);

                        // if it didnt launch exception until now
                        // we check if there is a mark so we can update
                        // we only do after create to avoid unecessary diff operations
                        if (log.HasMark(metadataId))
                        {
                            metadata.UpdateState(log.BuildDiff(metadataId));
                            log.RemoveMark(metadataId);
                        }
                    }
                    catch (ProcessFailedException)
                    {
                        log.AddMark(metadataId);
                    }
                });
                request.Start();
                requests.Add(request);
            }

            // joins due to state clock
            foreach (Thread request in requests)
            {
                request.Join();
            }
        }

        public void DataServer(string id, string location)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("REGISTER DATA SERVER " + id);

            if (!dataServers.Contains(id))
            {
                DataServerOnMetadatas(id, location);

                dataServers[id] = location;

                foreach (var entry in fileMetadataTable.PendingRequests)
                {
                    string futureId = id;
                    ConcurrentQueue<Action<string>> pending = entry;

                    Thread pendingRequest = new Thread(() =>
                    {
                        Action<string> action;
                        pending.TryDequeue(out action);
                        action(futureId);
                    });
                    pendingRequest.Start();
                }
            }
        }

        /**
         * IMetadataToProcess Methods
         */

        public string Master()
        {
            if (fail) throw new ProcessFailedException(id);

            string newMaster = master;

            // loops untill new master is found
            while (true)
            {
                if (!ImMaster)
                {
                    IMetadataToMetadata masterI = metadatas[newMaster];
                    try
                    {
                        masterI.Ping();
                        Console.WriteLine("MASTER IS " + newMaster);
                    }
                    catch (ProcessFailedException)
                    {
                        List<string> metadatasIds = new List<string>(metadatas.Keys);
                        metadatasIds.Add(id);
                        metadatasIds.Remove(master);
                        newMaster = metadatasIds.Min();
                    }
                    finally
                    {
                        master = newMaster;
                    }
                }
                else
                {
                    break;
                }
            }

            return newMaster;
        }

        public void DataServerOnMetadata(string id, string location)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("RECEIVE DATA SERVER " + id + " FROM METADATA");

            if (!dataServers.Contains(id))
            {
                dataServers[id] = location;
            }
        }
    }
}
