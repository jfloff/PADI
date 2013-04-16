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
            string location = state.DataServers[id].Location;
            string localFilename = LocalFilename(fileMetadata.Filename);

            fileMetadata.AddDataServer(id, location, localFilename);

            Thread request = new Thread(() =>
            {
                try
                {
                    state.DataServers[id].DataServer.Create(localFilename);
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
            foreach (var entry in state.DataServers)
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
                        state.DataServers[id].DataServer.Delete(localFilename);
                    }
                    catch (ProcessFailedException) { }
                    catch (ProcessFreezedException) { }
                });
                request.Start();
            }
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
                        if(state.HasMark(id))
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
            Console.WriteLine("File Metadatas");
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

            if (!state.FileMetadataTable.ContainsKey(fileMetadata.Filename))
            {
                pendingRequests[fileMetadata.Filename] = new ConcurrentQueue<Action<string>>();
            }
            state.FileMetadataTable[fileMetadata.Filename] = fileMetadata;
        }

        public void DeleteOnMetadata(FileMetadata fileMetadata)
        {
            if (fail) throw new ProcessFailedException(id);

            if (state.FileMetadataTable.ContainsKey(fileMetadata.Filename))
            {
                FileMetadata fileMetadataIgnored; state.FileMetadataTable.TryRemove(fileMetadata.Filename, out fileMetadataIgnored);
                ConcurrentQueue<Action<string>> queueIgnored; pendingRequests.TryRemove(fileMetadata.Filename, out queueIgnored);
            }
        }

        /**
         * IMetadataToClient Methods
         */
        public FileMetadata Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessFailedException(id);

            if (state.FileMetadataTable.ContainsKey(filename))
                throw new FileAlreadyExistsException(filename);

            Console.WriteLine("CREATE METADATA FILENAME: " + filename + " NBDATASERVERS: " + nbDataServers
                + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            //Select Data Servers and creates files within them
            FileMetadata fileMetadata = new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum);
            pendingRequests[filename] = new ConcurrentQueue<Action<string>>();

            CreateOnDataServers(fileMetadata);

            state.FileMetadataTable[filename] = fileMetadata;

            return fileMetadata;
        }

        public FileMetadata Open(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("OPEN METADATA FILE " + filename);

            if (!state.FileMetadataTable.ContainsKey(filename))
                throw new FileDoesNotExistException(filename);

            return state.FileMetadataTable[filename];
        }

        public void Close(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("CLOSE METADATA FILE " + filename);

            if (!state.FileMetadataTable.ContainsKey(filename))
                throw new FileDoesNotExistException(filename);
        }

        public void Delete(string filename)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("DELETE METADATA FILE " + filename);

            if (!state.FileMetadataTable.ContainsKey(filename))
                throw new FileDoesNotExistException(filename);

            FileMetadata fileMetadata = state.FileMetadataTable[filename];
            FileMetadata fileMetadataIgnored; state.FileMetadataTable.TryRemove(fileMetadata.Filename, out fileMetadataIgnored);
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
            foreach(Thread request in requests)
            {
                request.Join();
            }
        }

        public void DataServer(string id, string location)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("REGISTER DATA SERVER " + id);

            if (!state.DataServers.ContainsKey(id))
            {
                DataServerOnMetadatas(id, location);

                state.DataServers[id] = new DataServerInfo(location);

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

            if (!state.DataServers.ContainsKey(id))
            {
                state.DataServers[id] = new DataServerInfo(location);
            }
        }
    }
}
