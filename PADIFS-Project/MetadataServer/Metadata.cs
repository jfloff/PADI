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
        // clock / action
        private static ConcurrentDictionary<int, Action> pendingSequence = new ConcurrentDictionary<int, Action>();

        private static string master;
        private static int clock = 0;
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

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT * 6);
            Console.Title = id;

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Metadata),
                id,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Metadata Server " + id + " has started.");

            Thread pending = new Thread(() =>
            {
                while (true)
                {
                    if (pendingSequence.Count == 0) continue;

                    if (pendingSequence.ContainsKey(clock))
                    {
                        Console.WriteLine("OUT OF ORDER FIX : " + clock);
                        Action action;
                        pendingSequence.TryRemove(clock, out action);
                        action();
                    }
                }
            });
            pending.Start();

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
         * IMetadataToPM Methods
         */

        public void MetadataLocation(string id, string location)
        {
            Console.WriteLine("RECEIVED METADATA LOCATION " + location);

            IMetadataToMetadata metadata = (IMetadataToMetadata)Activator.GetObject(
                typeof(IMetadataToMetadata),
                location);
            metadatas[id] = metadata;

            // since we are sure one of the metadatas is up
            // we can just wait for the next metada register to ask for master
            try
            {
                // needs to ask for master since its doing a register
                master = metadata.Master();
            }
            catch (ProcessFailedException) { }
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("Master = " + master + ":" + clock);
            Console.WriteLine("Files = " + fileMetadataTable);
            Console.WriteLine("Data Servers = " + dataServers);
        }

        public void Fail()
        {
            Console.WriteLine("FAIL");
            fail = true;
        }

        private void UpdateState(MetadataDiff diff)
        {
            Console.WriteLine("RECEIVING STATE UPDATE");

            if (diff.Sequence != clock)
            {
                Console.WriteLine(diff.Sequence + ":" + clock);
                pendingSequence[clock] = () => UpdateState(diff);
                return;
            }
            clock += log.MergeDiff(diff, FutureSelectDataServer);
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER");

            foreach (var entry in metadatas)
            {
                IMetadataToMetadata metadata = entry.Value;

                try
                {
                    master = metadata.Master();
                    if (!ImMaster)
                    {
                        UpdateState(metadatas[master].UpdateMetadata(id));
                    }
                    fail = false;
                    return;
                }
                catch (ProcessFailedException) { }
            }

            master = id;
            fail = false;
        }

        /**
         * IMetadataToMetadata Methods
         */

        public void HeartbeatOnMetadata(string id, Heartbeat heartbeat)
        {
            if (fail) throw new ProcessFailedException(id);

            if(dataServers.Contains(id))
            {
                dataServers.Touch(id, heartbeat);
            }
        }

        public void LogMarkOnMetadata(string mark, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            if (mark != id)
            {
                log.AddMark(mark, sequence);
            }
        }

        public MetadataDiff UpdateMetadata(string id)
        {
            return log.BuildDiff(id);
        }

        public MasterVote MasterVoting(MasterVote vote)
        {
            if (fail) throw new ProcessFailedException(id);

            MasterVote newMaster = MasterVote.Choose(new MasterVote(id, clock), vote);

            master = newMaster.Id;
            return newMaster;
        }

        public Action<string> FutureSelectDataServer(FileMetadata fileMetadata)
        {
            return (futureId) => SelectDataServer(futureId, fileMetadata);
        }

        public void CreateOrUpdateOnMetadata(FileMetadata fileMetadata, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                pendingSequence[sequence] = () => CreateOrUpdateOnMetadata(fileMetadata, sequence);
                return;
            }

            int clockSkip = 0;

            // if it doesn't contain its a create we need to move clock
            if (!fileMetadataTable.Contains(fileMetadata.Filename)) clockSkip++;
            // if it didn't carry any data servers, doesnt increment
            if (fileMetadata.CurrentNbDataServers != 0) clockSkip++;

            fileMetadataTable.SetFileMetadata(fileMetadata.Filename, fileMetadata, FutureSelectDataServer(fileMetadata));
            foreach (var entry in fileMetadata.LocalFilenames)
            {
                string dataServerId = entry.Key;
                string localFilename = entry.Value;

                dataServers.AddFile(dataServerId, localFilename);
            }
            clock += clockSkip;
        }

        public void DeleteOnMetadata(FileMetadata fileMetadata, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            if (sequence != clock)
            {
                pendingSequence[sequence] = () => DeleteOnMetadata(fileMetadata, sequence);
                return;
            }

            if (fileMetadataTable.Contains(fileMetadata.Filename))
            {
                fileMetadataTable.Remove(fileMetadata.Filename);
                foreach (var entry in fileMetadata.LocalFilenames)
                {
                    string dataServerId = entry.Key;
                    string localFilename = entry.Value;

                    dataServers.RemoveFile(dataServerId, localFilename);
                }
                clock++;
            }
        }

        /**
         * IMetadataToClient Methods
         */

        private void LogMarkOnMetadatas(string mark, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToMetadata metadata = entry.Value;

                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.LogMarkOnMetadata(mark, sequence);
                    }
                    catch (ProcessFailedException) { }
                });
                request.Start();
            }
        }

        private void CreateOrUpdateOnMetadatas(FileMetadata fileMetadata, int sequence)
        {
            int requests = metadatas.Count;

            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToMetadata metadata = entry.Value;

                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.CreateOrUpdateOnMetadata(fileMetadata, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        log.AddMark(id, sequence);
                        LogMarkOnMetadatas(id, sequence);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref requests);
                    }
                });
                request.Start();
            }

            while (requests > 0) ;
        }

        // select single data server. refactored for future selects
        private void SelectDataServer(string id, FileMetadata fileMetadata)
        {
            string location = dataServers.Location(id);
            string localFilename = LocalFilename(fileMetadata.Filename);

            // it it has already a version in the table, works with that one
            if (fileMetadataTable.Contains(fileMetadata.Filename))
            {
                fileMetadata = fileMetadataTable.FileMetadata(fileMetadata.Filename);
            }
            
            fileMetadata.AddDataServer(id, location, localFilename);
            dataServers.AddFile(id, localFilename);
            
            int sequence = clock;
            CreateOrUpdateOnMetadatas(fileMetadata, sequence);
            clock++;
        }

        public FileMetadata Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

            if (fileMetadataTable.Contains(filename))
                throw new FileAlreadyExistsException(filename);

            Console.WriteLine("CREATE METADATA FILENAME: " + filename + " NBDATASERVERS: " + nbDataServers
                + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            //Select Data Servers and creates files within them
            FileMetadata fileMetadata = new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum);

            // create file on metadatas
            CreateOrUpdateOnMetadatas(fileMetadata, clock);
            clock++;
            
            // select possible data servers
            int selected = 0;
            string dataServerId = null;
            while(dataServers.TryMoveNext(dataServerId, out dataServerId))
            {
                if (selected++ >= fileMetadata.NbDataServers) break;

                SelectDataServer(dataServerId, fileMetadata);
            }

            fileMetadataTable.SetFileMetadata(filename, fileMetadata, FutureSelectDataServer(fileMetadata));
            return fileMetadata;
        }

        public FileMetadata Open(string filename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

            Console.WriteLine("OPEN METADATA FILE " + filename);

            if (!fileMetadataTable.Contains(filename))
                throw new FileDoesNotExistException(filename);

            return fileMetadataTable.FileMetadata(filename);
        }

        public void Close(string filename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

            Console.WriteLine("CLOSE METADATA FILE " + filename);

            if (!fileMetadataTable.Contains(filename))
                throw new FileDoesNotExistException(filename);
        }

        private void DeleteOnMetadatas(FileMetadata fileMetadata, int sequence)
        {
            int requests = metadatas.Count;

            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.DeleteOnMetadata(fileMetadata, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        log.AddMark(id, sequence);
                        LogMarkOnMetadatas(id, sequence);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref requests);
                    }
                });
                request.Start();
            }

            while (requests > 0) ;
        }

        public void Delete(string filename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

            Console.WriteLine("DELETE METADATA FILE " + filename);

            if (!fileMetadataTable.Contains(filename))
                throw new FileDoesNotExistException(filename);

            FileMetadata fileMetadata = fileMetadataTable.FileMetadata(filename);

            DeleteOnMetadatas(fileMetadata, clock);
            fileMetadataTable.Remove(fileMetadata.Filename);
            foreach (var entry in fileMetadata.LocalFilenames)
            {
                string dataServerId = entry.Key;
                string localFilename = entry.Value;

                dataServers.RemoveFile(dataServerId, localFilename);
            }
            clock++;
        }

        /**
         * IMetadataToDataServer Methods
         */

        private void HeartbeatOnMetadatas(string id, Heartbeat heartbeat)
        {
            int requests = metadatas.Count;

            foreach (var entry in metadatas)
            {
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.HeartbeatOnMetadata(id, heartbeat);
                    }
                    catch (ProcessFailedException) { }
                    finally
                    {
                        Interlocked.Decrement(ref requests);
                    }
                });
                request.Start();
            }

            while (requests > 0) ;
        }

        public DataServerFiles Heartbeat(string id, Heartbeat heartbeat)
        {
            if (fail) throw new ProcessFailedException(id);

            if (dataServers.Contains(id))
            {
                if (dataServers.Failed(id))
                {
                    CheckPending(id);
                }

                HeartbeatOnMetadatas(id, heartbeat);
                dataServers.Touch(id, heartbeat);

                return new DataServerFiles(dataServers.DataServerFiles(id));
            }

            return null;
        }

        private void DataServerOnMetadatas(string id, string location, int sequence)
        {
            int requests = metadatas.Count;

            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.DataServerOnMetadata(id, location, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        log.AddMark(metadataId, sequence);
                        LogMarkOnMetadatas(id, sequence);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref requests);
                    }
                });
                request.Start();
            }

            while (requests > 0) ;
        }

        public void DataServer(string id, string location)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

            Console.WriteLine("REGISTER DATA SERVER " + id);

            if (!dataServers.Contains(id))
            {
                DataServerOnMetadatas(id, location, clock);
                clock++;
                dataServers.Add(id, location);

                CheckPending(id);
            }
        }

        private static void CheckPending(string id)
        {
            foreach (var entry in fileMetadataTable.Pending)
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

        /**
         * IMetadataToProcess Methods
         */

        public void DataServerOnMetadata(string id, string location, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            if (sequence != clock)
            {
                pendingSequence[sequence] = () => DataServerOnMetadata(id, location, sequence);
                return;
            }

            Console.WriteLine("RECEIVE DATA SERVER " + id + " FROM METADATA");

            if (!dataServers.Contains(id))
            {
                dataServers.Add(id, location);
                clock++;
            }
        }

        public string Master()
        {
            if (fail) throw new ProcessFailedException(id);

            MasterVote masterVote = new MasterVote(id, clock);
            int requests = metadatas.Count;

            foreach (var entry in metadatas)
            {
                Thread vote = new Thread(() =>
                {
                    IMetadataToMetadata metadata = entry.Value;
                    try
                    {
                        masterVote = MasterVote.Choose(metadata.MasterVoting(masterVote), masterVote);
                    }
                    catch (ProcessFailedException) { }
                    finally
                    {
                        Interlocked.Decrement(ref requests);
                    }
                });
                vote.Start();
            }
            while (requests > 0) ;

            return master = masterVote.Id;
        }
    }
}
