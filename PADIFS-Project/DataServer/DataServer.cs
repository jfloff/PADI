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


namespace DataServer
{
    public class DataServer : MarshalByRefObject, IDataServerToPM, IDataServerToClient
    {
        private class FileStatistics
        {
            public int reads = 1;
            public int writes = 1;
        };

        // localFilename / FileData
        private static ConcurrentDictionary<string, FileData> files = new ConcurrentDictionary<string, FileData>();
        private static ConcurrentDictionary<string, FileStatistics> statistics = new ConcurrentDictionary<string, FileStatistics>();

        // id / interface
        private static ConcurrentDictionary<string, IMetadataToDataServer> metadatas
            = new ConcurrentDictionary<string, IMetadataToDataServer>();
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

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);
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
                    Heartbeat heartbeat = new Heartbeat(Weight());
                    DataServerFiles metadataFiles = metadatas[master].Heartbeat(id, heartbeat);

                    // remove files
                    foreach (var entry in files)
                    {
                        string filename = entry.Key;

                        if (!metadataFiles.Contains(filename))
                        {
                            FileData ignoredFileData; files.TryRemove(filename, out ignoredFileData);
                            FileStatistics ignoredStats; statistics.TryRemove(filename, out ignoredStats);
                        }
                    }
                    return;
                }
                catch (ProcessFailedException)
                {
                    FindMaster();
                }
            }
        }

        private double Weight()
        {
            double weight = 0;
            if (statistics.Count != 0)
            {
                // SOM Fi(reads/writes) / nFiles
                foreach (var entry in statistics)
                {
                    FileStatistics fileStatistics = entry.Value;
                    weight += fileStatistics.reads / fileStatistics.writes;
                }
                weight /= statistics.Count;
            }
            return weight;
        }

        public void FindMaster()
        {
            // keeps looping in the metadatas
            while (true)
            {
                foreach (var entry in metadatas)
                {
                    string tryMaster = entry.Key;

                    if (tryMaster != master)
                    {
                        try
                        {
                            master = metadatas[tryMaster].Master();
                            return;
                        }
                        catch (ProcessFailedException) { }
                    }
                }
            }
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

            // in case data server is booting up
            if (master == string.Empty)
            {
                // since we are sure one of the metadatas is up
                // we can just wait for the next metada register to ask for master
                try
                {
                    // needs to ask for master since its doing a register
                    master = metadata.Master();
                    metadatas[master].DataServer(DataServer.id, Helper.GetUrl(DataServer.id, DataServer.port));
                    // start heartbeating
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

                return;
            }
        }

        // skips failed and freeze
        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("Opened File Datas");
            foreach (var entry in files)
            {
                string filename = entry.Key;
                FileData fileData = entry.Value;
                Console.WriteLine(filename + ":" + fileData);
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
                statistics[localFilename] = new FileStatistics();
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
                statistics[localFilename] = new FileStatistics();
                statistics[localFilename].reads++;
            }

            statistics[localFilename].reads++;
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

            Console.WriteLine("WRITE " + localFilename + " : CONTENTS " + Helper.BytesToString(newFile.Contents));

            if (!files.ContainsKey(localFilename))
            {
                Console.WriteLine("CREATE FILE " + localFilename);
                files[localFilename] = newFile;
                statistics[localFilename] = new FileStatistics();
                statistics[localFilename].writes++;
                return;
            }

            FileData currentFile = files[localFilename];
            files[localFilename] = FileData.Latest(currentFile, newFile);
            statistics[localFilename].writes++;
        }
    }
}
