using SharedLibrary;
using SharedLibrary.Entities;
using SharedLibrary.Exceptions;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace Metadata
{
    public class Metadata : MarshalByRefObject, IMetadataToPM, IMetadataToClient, IMetadataToDataServer, IMetadataToMetadata
    {
        private static string id;

        // logs
        private static Log log = new Log();

        // id / interface
        private static ConcurrentDictionary<string, IMetadataToMetadata> metadatas
            = new ConcurrentDictionary<string, IMetadataToMetadata>();
        private static string master = string.Empty;

        // file metadata table
        private static FileMetadataTable fileTable = new FileMetadataTable();

        // data servers
        private static DataServerRegister dataServers = new DataServerRegister();

        private static bool fail = true;
        // clock / action
        private static ConcurrentDictionary<int, Action> outOfOrder = new ConcurrentDictionary<int, Action>();
        // sequence clock
        private static int clock = 0;

        // infinite lease
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong Arguments");

            id = args[0];
            int port = Convert.ToInt32(args[1]);

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT * 5);
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

        /**
         * IMetadataToPM
         */

        public void MetadataLocation(string metadataId, string location)
        {
            Console.WriteLine("METADATA LOCATION " + metadataId);

            IMetadataToMetadata metadata = (IMetadataToMetadata)Activator.GetObject(
                typeof(IMetadataToMetadata),
                location);
            metadatas[metadataId] = metadata;
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("AVG = " + dataServers.AvgWeight);
            Console.WriteLine("Master = " + master + ":" + clock);
            Console.WriteLine("Files = " + fileTable);
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

            // metadata bootup
            // since we force a metadata to start with recover
            if (master == string.Empty)
            {
                MetadataBootup();
            }

            foreach (var entry in metadatas)
            {
                try
                {
                    IMetadataToMetadata metadata = entry.Value;

                    // stops on the first not failed metadata
                    master = metadata.Master();
                    // sets fail to false so it starts receiving requests
                    int sequence = clock;
                    fail = false;
                    // updates its state with the master
                    if (!ImMaster)
                    {
                        MetadataDiff diff = metadatas[master].UpdateMetadata(id);

                        Console.WriteLine("--RECEIVING STATE UPDATE--");
                        log.MergeDiff(this, diff);
                        Console.WriteLine("--STATE UPDATE FINISHED--");
                    }
                    return;
                }
                catch (ProcessFailedException) { }
            }

            // sets fail to false so it starts receiving requests
            master = id;
            fail = false;
        }

        private bool ImMaster
        {
            get { return (id == master); }
        }

        private bool CheckMaster()
        {
            bool ret = ImMaster;
            if (!ret)
            {
                try
                {
                    master = metadatas[master].Master();
                }
                catch (ProcessFailedException)
                {
                    master = Master();
                }
                finally
                {
                    ret = ImMaster;
                }
            }
            return !ret;
        }

        private void MetadataBootup()
        {
            // out of order requests thread
            Thread outOfOrder = new Thread(() =>
            {
                while (true)
                {
                    OrderFix();
                }
            });
            outOfOrder.Start();

            // load balancing thread
            Thread loadBalancing = new Thread(() =>
            {
                while (true)
                {
                    LoadBalancing();
                    Thread.Sleep(Helper.LOAD_BALANCING_INTERVAL);
                }
            });
            loadBalancing.Start();
        }

        private void OrderFix()
        {
            if (outOfOrder.Count == 0) return;

            if (outOfOrder.ContainsKey(clock))
            {
                Console.WriteLine("OUT OF ORDER FIX : " + clock);

                Action action;
                if (outOfOrder.TryRemove(clock, out action))
                {
                    action();
                }
            }
        }

        /**
         * LOAD BALANCING
         */
        private void LoadBalancing()
        {
            if (fail) return;
            if (!ImMaster) return;

            HashSet<string> alreadyMigrated = new HashSet<string>();
            foreach (DataServerFile file in dataServers.FileWeights)
            {
                string localFilename = file.LocalFilename;
                string filename = fileTable.FilenameByLocalFilename(localFilename);
                Weight fileWeight = file.Weight;
                string oldDataServerId = file.DataServerId;
                Weight oldDataServerWeight = dataServers.Weight(oldDataServerId);
                Weight avgWeight = dataServers.AvgWeight;

                // if data server is already balanced
                if (Weight.InsideThreshold(oldDataServerWeight, avgWeight, Helper.LOAD_BALANCING_THRESHOLD)) continue;

                foreach (var entry in dataServers.Weights)
                {
                    string newDataServerId = entry.Key;
                    Weight newDataServerWeight = entry.Value;

                    // BLACK LIST SKIP CASES
                    // skips if its the old dataServer
                    if (newDataServerId == oldDataServerId) continue;
                    // migrates just one file per data server
                    if (alreadyMigrated.Contains(newDataServerId)) continue;
                    // if file already is in dataserver
                    if (fileTable.FileInDataServer(filename, newDataServerId)) continue;
                    // if data server is known to be down
                    if (dataServers.Failed(newDataServerId)) continue;
                    // if the data server is already balanced
                    if (Weight.InsideThreshold(newDataServerWeight, avgWeight, Helper.LOAD_BALANCING_THRESHOLD)) continue;
                    // if its outside threshold
                    if (Weight.AboveThreshold(fileWeight + newDataServerWeight, avgWeight, Helper.LOAD_BALANCING_THRESHOLD)) continue;
                    // if it isnt free
                    if (!fileTable.Free(filename)) continue;

                    // LOCK
                    fileTable.Lock(filename);
                    
                    // reads from old, writes to new
                    string newLocalFilename = LocalFilename(filename, newDataServerId);
                    string oldLocalFilename = fileTable.LocalFilenameByFilename(filename, oldDataServerId);

                    // if swap was false migration failed
                    bool swaped = SwapDataServers(oldDataServerId, newDataServerId, oldLocalFilename, newLocalFilename, fileWeight);
                    if (swaped)
                    {
                        Console.WriteLine("MIGRATE " + filename + " TO " + newDataServerId);

                        // log operation
                        int sequence = clock++;
                        log.LogOperation(sequence, "MigrateFileOnMetadata", filename, oldDataServerId, newDataServerId, oldLocalFilename, newLocalFilename, sequence);

                        // swap files on data server register
                        dataServers.AddFile(newDataServerId, newLocalFilename);
                        dataServers.RemoveFile(oldDataServerId, oldLocalFilename);

                        // swaps data servers on file table
                        fileTable.RemoveDataServer(filename, oldDataServerId);
                        fileTable.AddDataServer(filename, newDataServerId, dataServers.Location(newDataServerId), newLocalFilename);

                        // migrate to metadatas
                        MigrateFileOnMetadatas(filename, oldDataServerId, newDataServerId, oldLocalFilename, newLocalFilename, sequence);

                        // adds file weight to data server weight and removes from previous
                        dataServers.AddWeight(newDataServerId, fileWeight);
                        dataServers.RemoveWeight(oldDataServerId, fileWeight);

                        // adds to data servers already migrated
                        alreadyMigrated.Add(oldDataServerId);
                    }

                    // UNLOCK
                    fileTable.Unlock(filename);

                    // if migration occured breaks this data server loop file
                    if (swaped) break;
                }
            }
        }

        private bool SwapDataServers(string oldDataServerId, string newDataServerId, string oldLocalFilename, string newLocalFilename, Weight fileWeight)
        {
            IDataServerToMetadata oldDataServer = (IDataServerToMetadata)Activator.GetObject(
                typeof(IDataServerToMetadata),
                dataServers.Location(oldDataServerId));

            IDataServerToMetadata newDataServer = (IDataServerToMetadata)Activator.GetObject(
                typeof(IDataServerToMetadata),
                dataServers.Location(newDataServerId));

            FileData file = null;

            try
            {
                file = oldDataServer.MigrationRead(oldLocalFilename);
            }
            catch (ProcessFreezedException) 
            {
                return false;
            }
            catch (ProcessFailedException)
            {
                return false;
            }

            try
            {
                newDataServer.MigrationWrite(newLocalFilename, file, fileWeight);
            }
            catch (ProcessFreezedException) 
            { 
                // accepting freezed as valid because write will happen no matter what
            }
            catch (ProcessFailedException)
            {
                return false;
            }

            // no need to delete the file since we have garbage collection :)

            return true;
        }

        private void MigrateFileOnMetadatas(string filename, string oldDataServerId, string newDataServerId, string oldLocalFilename, string newLocalFilename, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.MigrateFileOnMetadata(filename, oldDataServerId, newDataServerId, oldLocalFilename, newLocalFilename, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
                    }
                });
                request.Start();
            }
        }

        /**
         * IMetadataToClient
         */

        private void AddLogMark(string mark, int markSequence)
        {
            if (log.AddMark(mark, markSequence))
            {
                int sequence = clock++;
                log.LogOperation(sequence, "AddMarkOnMetadata", mark, markSequence, sequence);
                AddMarkOnMetadatas(mark, markSequence, sequence);
            }
        }

        public FileMetadata Open(string clientId, string filename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (CheckMaster()) throw new NotTheMasterException(id, master);

            Console.WriteLine("OPEN METADATA FILE " + filename);

            if (!fileTable.Contains(filename))
            {
                throw new FileDoesNotExistException(filename);
            }

            if (fileTable.Locked(filename))
            {
                throw new FileUnavailableException(filename);
            }

            if (fileTable.Open(clientId, filename))
            {
                int sequence = clock++;
                log.LogOperation(sequence, "OpenOnMetadata", clientId, filename, sequence);
                OpenOnMetadatas(clientId, filename, sequence);
            }

            return fileTable.FileMetadata(filename);
        }

        private void OpenOnMetadatas(string clientId, string filename, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.OpenOnMetadata(clientId, filename, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
                    }
                });
                request.Start();
            }
        }

        public void Close(string clientId, string filename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (CheckMaster()) throw new NotTheMasterException(id, master);

            Console.WriteLine("CLOSE METADATA FILE " + filename);

            if (!fileTable.Contains(filename))
            {
                throw new FileDoesNotExistException(filename);
            }

            if (fileTable.Close(clientId, filename))
            {
                int sequence = clock++;
                log.LogOperation(sequence, "CloseOnMetadata", clientId, filename, sequence);
                CloseOnMetadatas(clientId, filename, sequence);
            }
        }

        private void CloseOnMetadatas(string clientId, string filename, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.CloseOnMetadata(clientId, filename, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
                    }
                });
                request.Start();
            }
        }

        public FileMetadata Create(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessFailedException(id);
            if (CheckMaster()) throw new NotTheMasterException(id, master);

            if (fileTable.Contains(filename))
            {
                throw new FileAlreadyExistsException(filename);
            }

            Console.WriteLine("CREATE METADATA FILENAME: " + filename + " NBDATASERVERS: " + nbDataServers
                + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            int sequence = clock++;
            log.LogOperation(sequence, "CreateOnMetadata", clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);

            //create file
            fileTable.Create(clientId, filename, nbDataServers, readQuorum, writeQuorum);
            EnqueuePending(filename, nbDataServers);
            CreateOnMetadatas(clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);

            // select possible data servers
            int selected = 0;
            string dataServerId = null;
            while (dataServers.TryMoveNext(dataServerId, out dataServerId))
            {
                if (selected++ >= nbDataServers) break;

                // dequeues a request and then selects
                fileTable.DequeueSelect(filename);
                SelectDataServer(dataServerId, filename);
            }

            return fileTable.FileMetadata(filename);
        }

        private void EnqueuePending(string filename, int nbDataServers)
        {
            for (int i = 0; i < nbDataServers; i++)
            {
                fileTable.EnqueueSelect(filename, (futureId) => SelectDataServer(futureId, filename));
            }
        }

        private void SelectDataServer(string dataServerId, string filename)
        {
            Console.WriteLine("SELECT DATASERVER FOR " + filename);

            string localFilename = LocalFilename(filename, dataServerId);

            int sequence = clock++;
            log.LogOperation(sequence, "SelectOnMetadata", filename, dataServerId, localFilename, sequence);

            fileTable.AddDataServer(filename, dataServerId, dataServers.Location(dataServerId), localFilename);
            dataServers.AddFile(dataServerId, localFilename);
            SelectOnMetadatas(filename, dataServerId, localFilename, sequence);
        }

        private void SelectOnMetadatas(string filename, string dataServerId, string localFilename, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.SelectOnMetadata(filename, dataServerId, localFilename, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
                    }
                });
                request.Start();
            }
        }

        private static string LocalFilename(string filename, string dataServerId)
        {
            // GUID better probably?
            return filename + '$' + dataServerId + '$' + filename.GetHashCode();
        }

        private void CreateOnMetadatas(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.CreateOnMetadata(clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
                    }
                });
                request.Start();
            }
        }

        public void Delete(string clientId, string filename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (CheckMaster()) throw new NotTheMasterException(id, master);

            Console.WriteLine("DELETE METADATA FILE " + filename);

            if (!fileTable.Contains(filename))
            {
                throw new FileDoesNotExistException(filename);
            }

            if (fileTable.Locked(filename))
            {
                throw new FileUnavailableException(filename);
            }

            int sequence = clock++;
            log.LogOperation(sequence, "DeleteOnMetadata", filename, sequence);

            DeleteOnMetadatas(filename, sequence);
            FileMetadata removed = fileTable.Remove(filename);

            // remove localFilenames from data servers
            foreach (var entry in removed.LocalFilenames)
            {
                string dataServerId = entry.Key;
                string localFilename = entry.Value;

                dataServers.RemoveFile(dataServerId, localFilename);
            }
        }

        private void DeleteOnMetadatas(string filename, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.DeleteOnMetadata(filename, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
                    }
                });
                request.Start();
            }
        }

        private void AddMarkOnMetadatas(string mark, int markSequence, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToMetadata metadata = entry.Value;

                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.AddMarkOnMetadata(mark, markSequence, sequence);
                    }
                    // already failed before no need to log again
                    catch (ProcessFailedException) { }
                });
                request.Start();
            }
        }

        /**
         * IMetadataToDataServer
         */

        public void DataServer(string dataServerId, string location)
        {
            if (fail) throw new ProcessFailedException(id);
            if (CheckMaster()) throw new NotTheMasterException(id, master);

            Console.WriteLine("REGISTER DATA SERVER " + dataServerId);

            if (!dataServers.Contains(dataServerId))
            {
                int sequence = clock++;
                log.LogOperation(sequence, "DataServerOnMetadata", dataServerId, location, sequence);

                DataServerOnMetadatas(dataServerId, location, sequence);
                dataServers.Add(dataServerId, location);
                CheckPending(dataServerId);
            }
        }

        private void CheckPending(string dataServerId)
        {
            foreach (var entry in fileTable.Pending)
            {
                string filename = entry;

                Thread pendingRequest = new Thread(() =>
                {
                    fileTable.DequeueSelect(filename)(dataServerId);
                });
                pendingRequest.Start();
            }
        }

        private void DataServerOnMetadatas(string dataServerId, string location, int sequence)
        {
            foreach (var entry in metadatas)
            {
                string metadataId = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                Thread request = new Thread(() =>
                {
                    try
                    {
                        metadata.DataServerOnMetadata(dataServerId, location, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
                    }
                });
                request.Start();
            }
        }

        public GarbageCollected Heartbeat(string dataServerId, Heartbeat heartbeat)
        {
            if (fail) throw new ProcessFailedException(id);
            if (CheckMaster()) throw new NotTheMasterException(id, master);

            //Console.WriteLine("HEARTBEAT FROM " + dataServerId);

            if (dataServers.Contains(dataServerId))
            {
                GarbageCollected toDelete = new GarbageCollected();

                // update data server weight
                dataServers.Touch(dataServerId, heartbeat.DataServerWeight);
                foreach (var entry in heartbeat.FileWeights)
                {
                    string localFilename = entry.Key;
                    Weight fileWeight = entry.Value;

                    // garbage collection
                    if (!dataServers.ContainsFile(dataServerId, localFilename))
                    {
                        toDelete.Add(localFilename);
                    }
                    else
                    {
                        // update file weights
                        dataServers.UpdateFileWeight(dataServerId, localFilename, fileWeight);
                    }
                }

                // pending files if this data server returned
                if (dataServers.Failed(dataServerId))
                {
                    CheckPending(dataServerId);
                }

                return toDelete;
            }

            return null;
        }

        /**
         * IMetadataToMetadata
         */

        public void CreateOnMetadata(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // repeated operation, ignores
            if (sequence < clock) return;

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                outOfOrder[sequence] = () => CreateOnMetadata(clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);
                return;
            }

            Console.WriteLine("CREATE FROM MASTER");

            log.LogOperation(clock, "CreateOnMetadata", clientId, filename, nbDataServers, readQuorum, writeQuorum, clock);
            fileTable.Create(clientId, filename, nbDataServers, readQuorum, writeQuorum);
            EnqueuePending(filename, nbDataServers);
            clock++;
        }

        public void SelectOnMetadata(string filename, string dataServerId, string localFilename, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // repeated operation, ignores
            if (sequence < clock) return;

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                outOfOrder[sequence] = () => SelectOnMetadata(filename, dataServerId, localFilename, sequence);
                return;
            }

            Console.WriteLine("SELECT FROM MASTER");

            log.LogOperation(clock, "SelectOnMetadata", filename, dataServerId, localFilename, clock);
            fileTable.DequeueSelect(filename);
            fileTable.AddDataServer(filename, dataServerId, dataServers.Location(dataServerId), localFilename);
            dataServers.AddFile(dataServerId, localFilename);
            clock++;
        }

        public void DeleteOnMetadata(string filename, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // repeated operation, ignores
            if (sequence < clock) return;

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                outOfOrder[sequence] = () => DeleteOnMetadata(filename, sequence);
                return;
            }

            Console.WriteLine("DELETE FROM MASTER");

            log.LogOperation(clock, "DeleteOnMetadata", filename, clock);

            // remove localFilenames from data servers
            FileMetadata removed = fileTable.Remove(filename);
            foreach (var entry in removed.LocalFilenames)
            {
                string dataServerId = entry.Key;
                string localFilename = entry.Value;

                dataServers.RemoveFile(dataServerId, localFilename);
            }
            clock++;
        }

        public void OpenOnMetadata(string clientId, string filename, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // repeated operation, ignores
            if (sequence < clock) return;

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                outOfOrder[sequence] = () => OpenOnMetadata(clientId, filename, sequence);
                return;
            }

            Console.WriteLine("OPEN FROM MASTER");

            fileTable.Open(clientId, filename);
            log.LogOperation(clock, "OpenOnMetadata", clientId, filename, clock);
            clock++;
        }

        public void CloseOnMetadata(string clientId, string filename, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // repeated operation, ignores
            if (sequence < clock) return;

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                outOfOrder[sequence] = () => CloseOnMetadata(clientId, filename, sequence);
                return;
            }

            Console.WriteLine("CLOSE FROM MASTER");

            fileTable.Close(clientId, filename);
            log.LogOperation(clock, "CloseOnMetadata", clientId, filename, clock);
            clock++;
        }

        public void DataServerOnMetadata(string dataServerId, string location, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // repeated operation, ignores
            if (sequence < clock) return;

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                outOfOrder[sequence] = () => DataServerOnMetadata(dataServerId, location, sequence);
                return;
            }

            Console.WriteLine("DATA SERVER FROM MASTER");

            log.LogOperation(clock, "DataServerOnMetadata", dataServerId, location, clock);
            dataServers.Add(dataServerId, location);
            clock++;
        }

        public void AddMarkOnMetadata(string mark, int markSequence, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // repeated operation, ignores
            if (sequence < clock) return;

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                outOfOrder[sequence] = () => AddMarkOnMetadata(mark, markSequence, sequence);
                return;
            }

            Console.WriteLine("ADD LOG MARK FROM MASTER");

            // also update its own mark ... for consistency purposes
            log.ForceAddMark(mark, markSequence);
            log.LogOperation(clock, "AddMarkOnMetadata", mark, markSequence, clock);
            clock++;
        }

        public void MigrateFileOnMetadata(string filename, string oldDataServerId, string newDataServerId, string oldLocalFilename, string newLocalFilename, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // repeated operation, ignores
            if (sequence < clock) return;

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                outOfOrder[sequence] = () => MigrateFileOnMetadata(filename, oldDataServerId, newDataServerId, oldLocalFilename, newLocalFilename, sequence);
                return;
            }

            Console.WriteLine("FILE MIGRATION FROM MASTER");

            log.LogOperation(clock, "MigrateFileOnMetadata", filename, oldDataServerId, newDataServerId, oldLocalFilename, newLocalFilename, clock);
            
            // swap files on data server register
            dataServers.AddFile(newDataServerId, newLocalFilename);
            dataServers.RemoveFile(oldDataServerId, oldLocalFilename);

            // swaps data servers on file table
            fileTable.RemoveDataServer(filename, oldDataServerId);
            fileTable.AddDataServer(filename, newDataServerId, dataServers.Location(newDataServerId), newLocalFilename);

            clock++;
        }

        public MetadataDiff UpdateMetadata(string metadataId)
        {
            return log.BuildDiff(metadataId);
        }

        // Tries to elect himself as the new master sending a vote to
        // each alive metadata with its id:clock
        // Other metadatas compare the votes, the one with the highest clock / lowest id
        // wins, and returns that vote
        public string Master()
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("MASTER ELECTION");

            // if they are masters
            if (ImMaster) return id;
            
            // tries to elect itself
            MasterVote masterVote = new MasterVote(id, clock);
            foreach (var entry in metadatas)
            {
                IMetadataToMetadata metadata = entry.Value;
                try
                {
                    // chooses better vote between previous master and the one from the metadata
                    masterVote = MasterVote.Choose(metadata.Uprising(masterVote), masterVote);
                }
                catch (ProcessFailedException) { }
            }
            // second round to confirm votes
            foreach (var entry in metadatas)
            {
                IMetadataToMetadata metadata = entry.Value;
                try
                {
                    // chooses better vote between previous master and the one from the metadata
                    metadata.Uprising(masterVote, true);
                }
                catch (ProcessFailedException) { }
            }

            master = masterVote.Id;

            return master;
        }

        public MasterVote Uprising(MasterVote vote, bool force = false)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("UPRISING");

            MasterVote newMaster;

            // if force writes the vote id as master
            if (force)
            {
                master = vote.Id;
                newMaster = vote;
            }
            else
            {
                MasterVote myVote = new MasterVote(id, clock);
                newMaster = MasterVote.Choose(myVote, vote);
            }

            return newMaster;
        }
    }
}
