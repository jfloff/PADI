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
        private static MetadataState state = new MetadataState();
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
                        if (state.HasMark(id))
                        {
                            metadata.UpdateState(state.GetDiff(id));
                            state.RemoveMark(id);
                        }
                    }
                    catch (ProcessFailedException)
                    {
                        state.AddMark(id);
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
                        if (state.HasMark(id))
                        {
                            metadata.UpdateState(state.GetDiff(id));
                            state.RemoveMark(id);
                        }
                    }
                    catch (ProcessFailedException)
                    {
                        state.AddMark(id);
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
                metadata.UpdateState(state.GetDiff(id));
                state.RemoveMark(id);
            }
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine(state);
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

        public void UpdateState(MetadataDiff snapshot)
        {
            state.MergeDiff(snapshot);
        }

        public void CreateOrUpdateOnMetadata(FileMetadata fileMetadata)
        {
            if (fail) throw new ProcessFailedException(id);

            if (!state.ContainsFile(fileMetadata.Filename))
            {
                pendingRequests[fileMetadata.Filename] = new ConcurrentQueue<Action<string>>();
            }
            state.AddOrUpdateFile(fileMetadata.Filename, fileMetadata);
        }

        public void DeleteOnMetadata(FileMetadata fileMetadata)
        {
            if (fail) throw new ProcessFailedException(id);

            if (state.ContainsFile(fileMetadata.Filename))
            {
                state.RemoveFile(fileMetadata.Filename);
                ConcurrentQueue<Action<string>> queueIgnored; pendingRequests.TryRemove(fileMetadata.Filename, out queueIgnored);
            }
        }

        /**
         * IMetadataToClient Methods
         */

        // select single data server. refactored for future selects
        private void SelectDataServer(string id, FileMetadata fileMetadata)
        {
            string location = state.DataServerLocation(id);
            string localFilename = LocalFilename(fileMetadata.Filename);

            fileMetadata.AddDataServer(id, location, localFilename);
            CreateOrUpdateOnMetadatas(fileMetadata);
        }

        public FileMetadata Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessFailedException(id);

            if (state.ContainsFile(filename))
                throw new FileAlreadyExistsException(filename);

            Console.WriteLine("CREATE METADATA FILENAME: " + filename + " NBDATASERVERS: " + nbDataServers
                + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            //Select Data Servers and creates files within them
            FileMetadata fileMetadata = new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum);
            pendingRequests[filename] = new ConcurrentQueue<Action<string>>();

            // select possible data servers
            int selected = 0;
            foreach(var entry in state.UniqueDataServers)
            {
                if (selected++ >= fileMetadata.NbDataServers) break;

                string dataServerId = entry.Key;
                SelectDataServer(dataServerId, fileMetadata);
            }

            // create requests for future selectes
            for (int i = selected; i < fileMetadata.NbDataServers; i++)
            {
                pendingRequests[fileMetadata.Filename].Enqueue(
                    (futureId) => SelectDataServer(futureId, fileMetadata));
            }

            state.AddOrUpdateFile(filename, fileMetadata);
            return fileMetadata;
        }

        public FileMetadata Open(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("OPEN METADATA FILE " + filename);

            if (!state.ContainsFile(filename))
                throw new FileDoesNotExistException(filename);

            return state.FileMetadata(filename);
        }

        public void Close(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("CLOSE METADATA FILE " + filename);

            if (!state.ContainsFile(filename))
                throw new FileDoesNotExistException(filename);
        }

        public void Delete(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("DELETE METADATA FILE " + filename);

            if (!state.ContainsFile(filename))
                throw new FileDoesNotExistException(filename);

            FileMetadata fileMetadata = state.FileMetadata(filename);
            state.RemoveFile(fileMetadata.Filename);
            ConcurrentQueue<Action<string>> queueRemove; pendingRequests.TryRemove(filename, out queueRemove);

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
                        if (state.HasMark(metadataId))
                        {
                            metadata.UpdateState(state.GetDiff(metadataId));
                            state.RemoveMark(metadataId);
                        }
                    }
                    catch (ProcessFailedException)
                    {
                        state.AddMark(metadataId);
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

            if (!state.ContainsDataServer(id))
            {
                DataServerOnMetadatas(id, location);

                state.AddOrUpdateDataServer(id, location);

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

        public void DataServerOnMetadata(string id, string location)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("RECEIVE DATA SERVER " + id + " FROM METADATA");

            if (!state.ContainsDataServer(id))
            {
                state.AddOrUpdateDataServer(id, location);
            }
        }
    }
}
