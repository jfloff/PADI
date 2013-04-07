using SharedLibrary;
using SharedLibrary.Entities;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace DataServer
{
    class Primary
    {
        string id;
        IMetadataToDataServer metadata;

        public Primary(string id, IMetadataToDataServer metadata)
        {
            this.id = id;
            this.metadata = metadata;
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public IMetadataToDataServer Metadata
        {
            get { return this.metadata; }
            set { this.metadata = value; }
        }
    }

    public class DataServer : MarshalByRefObject, IDataServerToPM, IDataServerToMetadata, IDataServerToClient
    {
        private static List<IMetadataToDataServer> metadatas = new List<IMetadataToDataServer>();
        private static Dictionary<string, FileData> files = new Dictionary<string, FileData>();
        private static Primary primary = null;
        
        private static string id;
        private static int port;

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

        public void MetadataLocation(string id, string location)
        {
            Console.WriteLine("RECEIVED METADATA LOCATION " + location);

            IMetadataToDataServer metadata = (IMetadataToDataServer)Activator.GetObject(
                typeof(IMetadataToDataServer),
                location);
            metadatas.Add(metadata);

            if (primary == null)
            {
                primary = new Primary(id, metadata);
                primary.Metadata.RegisterDataServer(id, Helper.GetUrl(DataServer.id, port));
            }
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
        }

        public void Fail()
        {
            Console.WriteLine("FAIL");
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER");
        }

        public void Freeze()
        {
            Console.WriteLine("FREEZE");
        }

        public void Unfreeze()
        {
            Console.WriteLine("UNFREEZE");
        }

        /**
         * IDataServerToMetadata Methods
         */

        public void Create(string filename)
        {
            Console.WriteLine("CREATE FILE " + filename);

            FileData file = new FileData(0);
            files.Add(filename, file);
        }

        public void Delete(string filename)
        {
            Console.WriteLine("DELETE FILE " + filename);

            files.Remove(filename);
        }

        /**
         * IDataServerToClient Methods
         */

        public void Read()
        {
            Console.WriteLine("READ DATA");
        }

        public void Write()
        {
            Console.WriteLine("WRITE DATA");
        }
    }
}
