using SharedLibrary;
using SharedLibrary.Entities;
using SharedLibrary.Exceptions;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Concurrent;
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
        private static string master;

        // file metadata table
        private static FileMetadataTable table = new FileMetadataTable();

        // data servers
        private static DataServerRegister dataServers = new DataServerRegister();

        private static bool fail = false;
        // clock / action
        private static ConcurrentDictionary<int, Action> pending = new ConcurrentDictionary<int, Action>();
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

            id = master = args[0];
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

            Thread pending = new Thread(() =>
            {
                while (true) {
                    OrderFix();
                }
            });
            pending.Start();

            Console.ReadLine();
        }

        private static void OrderFix()
        {
            if (pending.Count == 0) return;

            if (pending.ContainsKey(clock))
            {
                Console.WriteLine("OUT OF ORDER FIX : " + clock);
                Action action;
                pending.TryRemove(clock, out action);
                action();
            }
        }

        private bool ImMaster
        {
            get { return (id == master); }
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
            Console.WriteLine("Files = " + table);
            Console.WriteLine("Data Servers = " + dataServers);
        }

        public void Fail()
        {
            Console.WriteLine("FAIL");
            fail = true;
        }

        private void UpdateMyself(MetadataDiff diff)
        {
            Console.WriteLine("RECEIVING STATE UPDATE ...");

            log.MergeDiff(this, diff);
            
            Console.WriteLine("... STATE UPDATE FINISHED");
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER");

            foreach (var entry in metadatas)
            {
                try
                {
                    IMetadataToMetadata metadata = entry.Value;

                    // stops on the first not failed metadata
                    master = metadata.Master();
                    // sets fail to false so it starts receiving requests
                    fail = false;
                    
                    if (!ImMaster)
                    {
                        MetadataDiff diff = metadatas[master].UpdateMetadata(id);
                        UpdateMyself(diff);
                    }
                    return;
                }
                catch (ProcessFailedException) { }
            }

            // sets fail to false so it starts receiving requests
            master = id;
            fail = false;
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
            if (!ImMaster) throw new NotTheMasterException(id);

            Console.WriteLine("OPEN METADATA FILE " + filename);

            if (!table.Contains(filename))
            {
                throw new FileDoesNotExistException(filename);
            }

            if (table.Open(clientId, filename))
            {
                int sequence = clock++;
                log.LogOperation(sequence, "OpenOnMetadata", clientId, filename, sequence);
                OpenOnMetadatas(clientId, filename, sequence);
            }

            return table.FileMetadata(filename);
        }

        private void OpenOnMetadatas(string clientId, string filename, int sequence)
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
                        metadata.OpenOnMetadata(clientId, filename, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
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

        public void Close(string clientId, string filename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

            Console.WriteLine("CLOSE METADATA FILE " + filename);

            if (!table.Contains(filename))
            {
                throw new FileDoesNotExistException(filename);
            }

            if (table.Close(clientId, filename))
            {
                int sequence = clock++;
                log.LogOperation(sequence, "CloseOnMetadata", clientId, filename, sequence);
                CloseOnMetadatas(clientId, filename, sequence);
            }
        }

        private void CloseOnMetadatas(string clientId, string filename, int sequence)
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
                        metadata.CloseOnMetadata(clientId, filename, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
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

        public FileMetadata Create(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

            if (table.Contains(filename))
            {
                throw new FileAlreadyExistsException(filename);
            }

            Console.WriteLine("CREATE METADATA FILENAME: " + filename + " NBDATASERVERS: " + nbDataServers
                + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            int sequence = clock++;
            log.LogOperation(sequence, "CreateOnMetadata", clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);

            //create file
            table.Create(clientId, filename, nbDataServers, readQuorum, writeQuorum);
            EnqueuePending(filename, nbDataServers);
            CreateOnMetadatas(clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);

            // select possible data servers
            int selected = 0;
            string dataServerId = null;
            while (dataServers.TryMoveNext(dataServerId, out dataServerId))
            {
                if (selected++ >= nbDataServers) break;

                // dequeues a request and then selects
                table.DequeueSelect(filename);
                SelectDataServer(dataServerId, filename);
            }

            return table.FileMetadata(filename);
        }

        private void EnqueuePending(string filename, int nbDataServers)
        {
            for (int i = 0; i < nbDataServers; i++)
            {
                table.EnqueueSelect(filename, (futureId) => SelectDataServer(futureId, filename));
            }
        }

        private void SelectDataServer(string dataServerId, string filename)
        {
            string localFilename = LocalFilename(filename);

            int sequence = clock++;
            log.LogOperation(sequence, "SelectOnMetadata", filename, dataServerId, localFilename, sequence);

            table.AddDataServer(filename, dataServerId, dataServers.Location(dataServerId), localFilename);
            dataServers.AddFile(dataServerId, localFilename);
            SelectOnMetadatas(filename, dataServerId, localFilename, sequence);
        }

        private void SelectOnMetadatas(string filename, string dataServerId, string localFilename, int sequence)
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
                        metadata.SelectOnMetadata(filename, dataServerId, localFilename, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
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

        private string LocalFilename(string filename)
        {
            // GUID better probably?
            return filename + "$" + filename.GetHashCode();
        }

        private void CreateOnMetadatas(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum, int sequence)
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
                        metadata.CreateOnMetadata(clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
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

        public void Delete(string clientId, string filename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

            Console.WriteLine("DELETE METADATA FILE " + filename);

            if (!table.Contains(filename))
            {
                throw new FileDoesNotExistException(filename);
            }

            int sequence = clock++;
            log.LogOperation(sequence, "DeleteOnMetadata", filename, sequence);

            DeleteOnMetadatas(filename, sequence);
            FileMetadata removed = table.Remove(filename);

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
            int requests = metadatas.Count;
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
                    finally
                    {
                        Interlocked.Decrement(ref requests);
                    }
                });
                request.Start();
            }

            while (requests > 0) ;
        }

        private void AddMarkOnMetadatas(string mark, int markSequence, int sequence)
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
                        metadata.AddMarkOnMetadata(mark, markSequence, sequence);
                    }
                    // already failed before no need to log again
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

        /**
         * IMetadataToDataServer
         */

        public void DataServer(string dataServerId, string location)
        {
            if (fail) throw new ProcessFailedException(id);
            if (!ImMaster) throw new NotTheMasterException(id);

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
            foreach (var entry in table.Pending)
            {
                string filename = entry;

                Thread pendingRequest = new Thread(() =>
                {
                    table.DequeueSelect(filename)(dataServerId);
                });
                pendingRequest.Start();
            }
        }

        private void DataServerOnMetadatas(string dataServerId, string location, int sequence)
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
                        metadata.DataServerOnMetadata(dataServerId, location, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
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

        public GarbageCollected Heartbeat(string dataServerId, Heartbeat heartbeat)
        {
            if (fail) throw new ProcessFailedException(id);

            if (dataServers.Contains(dataServerId))
            {
                /**
                 * IGNORING HEARTBEAT REPLICATION
                 */
                //int sequence = clock++;
                //log.LogOperation(sequence, "HeartbeatOnMetadata", dataServerId, heartbeat, sequence);
                //HeartbeatOnMetadatas(dataServerId, heartbeat, sequence);

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

                    // lacking load balancing
                }


                if (dataServers.Failed(dataServerId))
                {
                    CheckPending(dataServerId);
                }

                return toDelete;
            }

            return null;
        }

        private void HeartbeatOnMetadatas(string dataServerId, Heartbeat heartbeat, int sequence)
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
                        metadata.HeartbeatOnMetadata(dataServerId, heartbeat, sequence);
                    }
                    catch (ProcessFailedException)
                    {
                        AddLogMark(metadataId, sequence);
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

        /**
         * IMetadataToMetadata
         */

        public void CreateOnMetadata(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                pending[sequence] = () => CreateOnMetadata(clientId, filename, nbDataServers, readQuorum, writeQuorum, sequence);
                return;
            }

            Console.WriteLine("CREATE FROM MASTER");

            log.LogOperation(clock, "CreateOnMetadata", clientId, filename, nbDataServers, readQuorum, writeQuorum, clock);
            table.Create(clientId, filename, nbDataServers, readQuorum, writeQuorum);
            EnqueuePending(filename, nbDataServers);
            clock++;
        }

        public void SelectOnMetadata(string filename, string dataServerId, string localFilename, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                pending[sequence] = () => SelectOnMetadata(filename, dataServerId, localFilename, sequence);
                return;
            }

            Console.WriteLine("SELECT FROM MASTER");

            log.LogOperation(clock, "SelectOnMetadata", filename, dataServerId, localFilename, clock);
            table.DequeueSelect(filename);
            table.AddDataServer(filename, dataServerId, dataServers.Location(dataServerId), localFilename);
            dataServers.AddFile(dataServerId, localFilename);
            clock++;
        }

        public void DeleteOnMetadata(string filename, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                pending[sequence] = () => DeleteOnMetadata(filename, sequence);
                return;
            }

            Console.WriteLine("DELETE FROM MASTER");

            log.LogOperation(clock, "DeleteOnMetadata", filename, clock);

            // remove localFilenames from data servers
            FileMetadata removed = table.Remove(filename);
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

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                pending[sequence] = () => OpenOnMetadata(clientId, filename, sequence);
                return;
            }

            Console.WriteLine("OPEN FROM MASTER");

            if (table.Open(clientId, filename))
            {
                log.LogOperation(clock, "OpenOnMetadata", clientId, filename, clock);
                clock++;
            }
        }

        public void CloseOnMetadata(string clientId, string filename, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                pending[sequence] = () => CloseOnMetadata(clientId, filename, sequence);
                return;
            }

            Console.WriteLine("CLOSE FROM MASTER");

            if (table.Close(clientId, filename))
            {
                log.LogOperation(clock, "CloseOnMetadata", clientId, filename, clock);
                clock++;
            }
        }

        public void DataServerOnMetadata(string dataServerId, string location, int sequence)
        {
            if (fail) throw new ProcessFailedException(id);

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                pending[sequence] = () => DataServerOnMetadata(dataServerId, location, sequence);
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

            // message out of sequence, adds to queue
            if (sequence != clock)
            {
                pending[sequence] = () => AddMarkOnMetadata(mark, markSequence, sequence);
                return;
            }

            Console.WriteLine("ADD LOG MARK FROM MASTER");

            // also update its own mark ... for consistency purposes
            log.ForceAddMark(mark, markSequence);
            log.LogOperation(clock, "AddMarkOnMetadata", mark, markSequence, clock);
            clock++;
        }

        public void HeartbeatOnMetadata(string dataServerId, Heartbeat heartbeat, int sequence)
        {
            throw new NotImplementedException();
        }

        public MasterVote MasterVoting(MasterVote vote)
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("TAKE MY MASTER VOTE");

            MasterVote newMaster = MasterVote.Choose(new MasterVote(id, clock), vote);
            master = newMaster.Id;
            return newMaster;
        }

        public MetadataDiff UpdateMetadata(string metadataId)
        {
            return log.BuildDiff(metadataId, clock);
        }

        /**
         * IMetadataToProcess
         */

        // Tries to elect himself as the new master sending a vote to
        // each alive metadata with its id:clock
        // Other metadatas compare the votes, the one with the highest clock / lowest id
        // wins, and returns that vote
        public string Master()
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("MASTER VOTING");

            MasterVote masterVote = new MasterVote(id, clock);
            foreach (var entry in metadatas)
            {
                IMetadataToMetadata metadata = entry.Value;
                try
                {
                    // chooses better vote between previous master and the one from the metadata
                    masterVote = MasterVote.Choose(metadata.MasterVoting(masterVote), masterVote);
                }
                catch (ProcessFailedException) { }
            }

            return master = masterVote.Id;
        }
    }
}
