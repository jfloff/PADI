using SharedLibrary;
using SharedLibrary.Entities;
using SharedLibrary.Exceptions;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

// @TODO Fix primary decision

namespace DataServer
{
    public class DataServer : MarshalByRefObject, IDataServerToPM, IDataServerToMetadata, IDataServerToClient
    {
        struct Primary
        {
            public string Id;
            public IMetadataToDataServer Metadata;
        };

        // localFilename / FileData
        private static Dictionary<string, FileData> files = new Dictionary<string, FileData>();

        private static List<IMetadataToDataServer> metadatas = new List<IMetadataToDataServer>();
        private static Primary primary = new Primary() { Id = null, Metadata = null };

        private static string id;
        private static int port;

        private static bool fail = false;
        private static bool freeze = false;
        private static Queue<Action> freezed = new Queue<Action>();

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong Arguments");

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);

            id = args[0];
            port = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(DataServer),
                id,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Data Server " + id + " has started.");
            Console.ReadLine();
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
            metadatas.Add(metadata);

            if (primary.Id == null)
            {
                primary.Id = id;
                primary.Metadata = metadata;
                primary.Metadata.RegisterDataServer(DataServer.id, Helper.GetUrl(DataServer.id, DataServer.port));
            }
        }

        // skips failed and freeze
        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("Opened File Metadatas");
            foreach (var entry in files)
            {
                string localFilename = entry.Key;
                FileData fileData = entry.Value;

                Console.WriteLine("  " + localFilename);
                Console.WriteLine("    " + fileData.ToString());
            }
        }

        public void Fail()
        {
            if (freeze) freezed.Enqueue(() => Fail());

            Console.WriteLine("FAIL");
            fail = true;
        }

        public void Recover()
        {
            if (freeze) freezed.Enqueue(() => Recover());

            Console.WriteLine("RECOVER");
            fail = false;
        }

        public void Freeze()
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("FREEZE");
            freeze = true;
        }

        public void Unfreeze()
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("UNFREEZE");
            freeze = false;
            // do this after introducing concurrency in the DS
            //Thread unfreezeAll = new Thread(() =>
            //{
                while (freezed.Any())
                {
                    Action action = freezed.Dequeue();
                    action();
                }
            //});
        }

        /**
         * IDataServerToMetadata Methods
         */

        public void Create(string localFilename)
        {
            if (fail) throw new ProcessDownException(id);
            if (freeze) freezed.Enqueue(() => Create(localFilename));

            Console.WriteLine("CREATE FILE " + localFilename);

            if (!files.ContainsKey(localFilename))
            {
                files.Add(localFilename, new FileData());
            }
        }

        public void Delete(string localFilename)
        {
            if (fail) throw new ProcessDownException(id);
            if (freeze) freezed.Enqueue(() => Delete(localFilename));

            Console.WriteLine("DELETE FILE " + localFilename);

            if (files.ContainsKey(localFilename))
            {
                files.Remove(localFilename);
            }
        }

        /**
         * IDataServerToClient Methods
         */

        public FileData Read(string localFilename)
        {
            if (fail) throw new ProcessDownException(id);
            if (freeze) freezed.Enqueue(() => Read(localFilename));

            Console.WriteLine("READ " + localFilename);

            if (!files.ContainsKey(localFilename))
                throw new FileDoesNotExistException(localFilename);

            return files[localFilename];
        }

        public void Write(string localFilename, FileData newFile)
        {
            if (fail) throw new ProcessDownException(id);
            if (freeze) freezed.Enqueue(() => Write(localFilename, newFile));

            Console.WriteLine("WRITE " + localFilename);

            if (!files.ContainsKey(localFilename))
                throw new FileDoesNotExistException(localFilename);

            FileData currentFile = files[localFilename];
            files[localFilename] = FileData.LatestVersion(currentFile, newFile);
        }
    }
}
