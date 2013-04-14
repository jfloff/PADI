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
    class Metadata : MarshalByRefObject, IMetadataToPM, IMetadataToMetadata, IMetadataToClient, IMetadataToDataServer
    {
        // Store on each entry of the data servers dictionary
        private struct DataServerInfo
        {
            public string Location;
            public IDataServerToMetadata DataServer;

            public DataServerInfo(string location, IDataServerToMetadata dataServer)
            {
                this.Location = location;
                this.DataServer = dataServer;
            }
        }

        private static FileMetadataTable fileMetadataTable = new FileMetadataTable();
        // id / Interface
        private static ConcurrentDictionary<string, DataServerInfo> dataServers
            = new ConcurrentDictionary<string, DataServerInfo>();
        // CREATE DICTIONARY FOR FAILED METADATAS
        // id / interface
        private static ConcurrentDictionary<string, IMetadataToMetadata> metadatas
            = new ConcurrentDictionary<string, IMetadataToMetadata>();
        // filename / queue for each file 
        private static ConcurrentDictionary<string, ConcurrentQueue<Action<string>>> pendingRequests
            = new ConcurrentDictionary<string, ConcurrentQueue<Action<string>>>();

        // statics are all thread safe
        private static string master;
        private static string id;
        private static bool fail = false;


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

        // create file on a single data server. refactored for future creates
        private void CreateOnDataServer(string id, FileMetadata fileMetadata)
        {
            string location = dataServers[id].Location;
            string localFilename = LocalFilename(fileMetadata.Filename);

            fileMetadata.AddDataServer(id, location, localFilename);
            Thread request = new Thread(() =>
            {
                try
                {
                    dataServers[id].DataServer.Create(localFilename);
                }
                catch (ProcessFailedException) { }
                catch (ProcessFreezedException) { }
            });
            request.Start();
            CreateOrUpdateOnMetadatas(fileMetadata);
        }

        // Places files in dataServers. Doesn't care if they were created or not.
        public void CreateOnDataServers(FileMetadata fileMetadata)
        {
            int nbDataServersSelected = 0;

            // create possible ones
            foreach (var entry in dataServers)
            {
                if (++nbDataServersSelected > fileMetadata.NbDataServers) break;

                string id = entry.Key;
                CreateOnDataServer(id, fileMetadata);
            }

            // create requests for future creates
            for (int i = nbDataServersSelected; i < fileMetadata.NbDataServers; i++)
            {
                pendingRequests[fileMetadata.Filename].Enqueue(
                    (futureId) => CreateOnDataServer(futureId, fileMetadata));
            }
        }

        // Not dealing with files that are already deleted
        public void DeleteOnDataServers(FileMetadata fileMetadata)
        {
            foreach (var entry in fileMetadata.LocalFilenames)
            {
                string id = entry.Key;
                string localFilename = entry.Value;

                Thread request = new Thread(() =>
                {
                    try
                    {
                        dataServers[id].DataServer.Delete(localFilename);
                    }
                    catch (ProcessFailedException) { }
                    catch (ProcessFreezedException) { }
                });
                request.Start();
            }
        }

        private void CreateOrUpdateOnMetadatas(FileMetadata fileMetadata)
        {
            foreach (var entry in metadatas)
            {
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.CreateOrUpdate(fileMetadata);
                    }
                    catch (ProcessFailedException)
                    {
                        fileMetadataTable.AddMark(id);
                    }
                });
                request.Start();
            }
        }

        private void DeleteOnMetadatas(FileMetadata fileMetadata)
        {
            foreach (var entry in metadatas)
            {
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {

                        metadata.Delete(fileMetadata);
                    }
                    catch (ProcessFailedException)
                    {
                        fileMetadataTable.AddMark(id);
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

            //sends the current metadata state
            if (ImMaster) metadata.UpdateState(fileMetadataTable.State(id));

            //Force invocation of primary decision
            master = (string.Compare(id,master) > 0) ? master : id;
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("File Metadatas");
            Console.WriteLine(fileMetadataTable);
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

        public void UpdateState(MetadataState snapshot)
        {
            fileMetadataTable.MergeState(snapshot);
        }

        public void CreateOrUpdate(FileMetadata fileMetadata)
        {
            if (!fileMetadataTable.Contains(fileMetadata.Filename))
            {
                pendingRequests[fileMetadata.Filename] = new ConcurrentQueue<Action<string>>();
            }
            fileMetadataTable[fileMetadata.Filename] = fileMetadata;
        }

        public void Delete(FileMetadata fileMetadata)
        {
            if (fileMetadataTable.Contains(fileMetadata.Filename))
            {
                fileMetadataTable.Remove(fileMetadata.Filename);
                ConcurrentQueue<Action<string>> queueRemove; pendingRequests.TryRemove(fileMetadata.Filename, out queueRemove);
            }
        }

        /**
         * IMetadataToClient Methods
         */
        public FileMetadata Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessFailedException(id);

            if (fileMetadataTable.Contains(filename))
                throw new FileAlreadyExistsException(filename);

            Console.WriteLine("CREATE METADATA FILENAME: " + filename + " NBDATASERVERS: " + nbDataServers
                + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            //Select Data Servers and creates files within them
            FileMetadata fileMetadata = new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum);
            pendingRequests[filename] = new ConcurrentQueue<Action<string>>();

            CreateOnDataServers(fileMetadata);
            CreateOrUpdateOnMetadatas(fileMetadata);

            fileMetadataTable[filename] = fileMetadata;
            return fileMetadata;
        }

        public FileMetadata Open(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("OPEN METADATA FILE " + filename);

            if (!fileMetadataTable.Contains(filename))
                throw new FileDoesNotExistException(filename);

            return fileMetadataTable[filename];
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

            FileMetadata fileMetadata = fileMetadataTable[filename];
            fileMetadataTable.Remove(filename);
            ConcurrentQueue<Action<string>> queueRemove; pendingRequests.TryRemove(filename, out queueRemove);

            DeleteOnDataServers(fileMetadata);
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

        public void RegisterDataServer(string id, string location)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("REGISTER DATA SERVER " + id);

            if (!dataServers.ContainsKey(id))
            {
                IDataServerToMetadata dataServer = (IDataServerToMetadata)Activator.GetObject(
                    typeof(IDataServerToMetadata),
                    location);
                dataServers[id] = new DataServerInfo(location, dataServer);

                foreach (var entry in pendingRequests)
                {
                    string futureId = id;
                    ConcurrentQueue<Action<string>> pending = entry.Value;

                    if (pending.Any())
                    {
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
    }
}
