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


namespace DataServer
{
    public class DataServer : MarshalByRefObject, IDataServerToPM, IDataServerToClient, IDataServerToMetadata
    {
        // localFilename / FileData
        private static ConcurrentDictionary<string, FileData> files = new ConcurrentDictionary<string, FileData>();
        private static ConcurrentDictionary<string, Weight> weights = new ConcurrentDictionary<string, Weight>();

        // id / interface
        private static ConcurrentDictionary<string, IMetadataToDataServer> metadatas
            = new ConcurrentDictionary<string, IMetadataToDataServer>();
        private static IEnumerator<KeyValuePair<string, IMetadataToDataServer>> metadatasEnumerator;
        private static string master = string.Empty;

        private static string id;
        private static int port;

        private static bool fail = false;
        private static bool freeze = false;
        private static ConcurrentQueue<Action> freezed = new ConcurrentQueue<Action>();


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
            port = Convert.ToInt32(args[1]);

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT*2);
            Console.Title = id;

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(DataServer),
                id,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Data Server " + id + " has started.");

            Console.ReadLine();
        }

        public void SendHeartbeat()
        {
            if (fail) return;
            if (freeze)
            {
                freezed.Enqueue(() => SendHeartbeat());
                return;
            }

            while (true)
            {
                try
                {
                    Heartbeat heartbeat = new Heartbeat(MyWeight(), new Dictionary<string, Weight>(weights));
                    //Console.WriteLine("HEARTBEAT TO " + master);
                    GarbageCollected toDelete = metadatas[master].Heartbeat(id, heartbeat);

                    foreach (string localFilename in toDelete)
                    {
                        Console.WriteLine("GARBAGE COLLECTOR = " + localFilename);
                        FileData ignoredFileData; files.TryRemove(localFilename, out ignoredFileData);
                        Weight ignoredStats; weights.TryRemove(localFilename, out ignoredStats);
                    }

                    return;
                }
                catch (ProcessFailedException)
                {
                    RandomMaster();
                }
                catch (NotTheMasterException e)
                {
                    master = e.NewMaster;
                }
            }
        }

        // can't be in loop since we know for sure 1 metadata is up
        private void RandomMaster()
        {
            while (true)
            {
                if (!metadatasEnumerator.MoveNext())
                {
                    metadatasEnumerator = metadatas.GetEnumerator();
                    continue;
                }

                if (master != metadatasEnumerator.Current.Key)
                {
                    master = metadatasEnumerator.Current.Key;
                    return;
                }
            }
        }

        private Weight MyWeight()
        {
            Weight weight = new Weight();
            if (weights.Count != 0)
            {
                // SOM Fi(reads/writes) / nFiles
                foreach (var entry in weights)
                {
                    Weight fileWeight = entry.Value;
                    weight.Reads += fileWeight.Reads;
                    weight.Writes += fileWeight.Writes;
                }
            }
            return weight;
        }

        /**
         * IDataServerToPM Methods
         */

        // skips failed and freeze
        public void MetadataLocation(string id, string location)
        {
            Console.WriteLine("RECEIVED METADATA LOCATION " + location);

            IMetadataToDataServer metadata = (IMetadataToDataServer)Activator.GetObject(
                typeof(IMetadataToDataServer),
                location);
            metadatas[id] = metadata;
            metadatasEnumerator = metadatas.GetEnumerator();

            // in case data server is booting up
            if (master == string.Empty)
            {
                // since we are sure one of the metadatas is up
                // we can just wait for the correct data server
                // to appear so we can register with it
                try
                {
                    metadata.DataServer(DataServer.id, Helper.GetUrl(DataServer.id, DataServer.port));
                    
                    // if it was neither down, or it isnt the master ... it is the master
                    master = id;
                    // starts heartbeating
                    Thread heartbeat = new Thread(() =>
                    {
                        while (true)
                        {
                            SendHeartbeat();
                            Thread.Sleep(Helper.DATASERVER_HEARTBEAT_INTERVAL);
                        }
                    });
                    heartbeat.Start();
                }
                catch (ProcessFailedException) { }
                catch (NotTheMasterException) { }
            }
        }

        // skips failed and freeze
        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("WEIGHT = " + MyWeight());
            Console.WriteLine("FILE DATAS");
            foreach (var entry in files)
            {
                string localFilename = entry.Key;
                FileData fileData = entry.Value;

                Console.WriteLine(localFilename + ":" + fileData + ":" + weights[localFilename]);
            }
        }

        public void Fail()
        {
            if (freeze)
            {
                freezed.Enqueue(() => Fail());
                throw new ProcessFreezedException(id);
            }
            Console.WriteLine("FAIL");
            fail = true;
        }

        public void Recover()
        {
            if (freeze)
            {
                freezed.Enqueue(() => Recover());
                throw new ProcessFreezedException(id);
            }

            Console.WriteLine("RECOVER");
            fail = false;
            SendHeartbeat();
        }

        public void Freeze()
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("FREEZE");
            freeze = true;
        }

        public void Unfreeze()
        {
            if (fail) throw new ProcessFailedException(id);

            Console.WriteLine("UNFREEZE");

            freeze = false;
            while (freezed.Count != 0)
            {
                Thread pending = new Thread(() =>
                {
                    Action action;
                    if (freezed.TryDequeue(out action)) action();
                });
                pending.Start();
            }
        }

        /**
         * IDataServerToClient Methods
         */

        public FileVersion Version(string localFilename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (freeze)
            {
                freezed.Enqueue(() => Version(localFilename));
                throw new ProcessFreezedException(id);
            }

            Console.WriteLine("VERSION " + localFilename);

            if (!files.ContainsKey(localFilename))
            {
                Console.WriteLine("CREATE FILE " + localFilename);
                files[localFilename] = new FileData();
                weights[localFilename] = new Weight();
            }

            return files[localFilename].Version;
        }

        public FileData Read(string localFilename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (freeze)
            {
                freezed.Enqueue(() => Read(localFilename));
                throw new ProcessFreezedException(id);
            }

            Console.WriteLine("READ " + localFilename);
            if (!files.ContainsKey(localFilename))
            {
                Console.WriteLine("CREATE FILE " + localFilename);
                files[localFilename] = new FileData();
                weights[localFilename] = new Weight();
            }

            weights[localFilename].Reads++;
            return files[localFilename];
        }

        public void Write(string localFilename, FileData newFile)
        {
            if (fail) throw new ProcessFailedException(id);
            if (freeze)
            {
                freezed.Enqueue(() => Write(localFilename, newFile));
                throw new ProcessFreezedException(id);
            }

            Console.WriteLine("WRITE " + localFilename 
                + " : CONTENTS " + Helper.BytesToString(newFile.Contents) 
                + " : VERSION " + newFile.Version);

            if (!files.ContainsKey(localFilename))
            {
                Console.WriteLine("CREATE FILE " + localFilename);
                files[localFilename] = newFile;
                weights[localFilename] = new Weight();
            }
            else
            {
                files[localFilename] = FileData.Latest(files[localFilename], newFile);
            }

            weights[localFilename].Writes++;
            Console.WriteLine("STORED = " + files[localFilename].Version);
        }

        /**
         * IDataServerToMetadata
         */

        public FileData MigrationRead(string localFilename)
        {
            if (fail) throw new ProcessFailedException(id);
            if (freeze)
            {
                freezed.Enqueue(() => MigrationRead(localFilename));
                throw new ProcessFreezedException(id);
            }

            Console.WriteLine("MIGRATION READ " + localFilename);

            return (files.ContainsKey(localFilename)) ? files[localFilename] : new FileData();
        }

        public void MigrationWrite(string localFilename, FileData newFile, Weight weight)
        {
            if (fail) throw new ProcessFailedException(id);
            if (freeze)
            {
                freezed.Enqueue(() => MigrationWrite(localFilename, newFile, weight));
                throw new ProcessFreezedException(id);
            }

            Console.WriteLine("MIGRATION WRITE " + localFilename);

            files[localFilename] = newFile;
            weights[localFilename] = weight;
        }
    }
}
