using SharedLibrary;
using SharedLibrary.Entities;
using SharedLibrary.Exceptions;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Client
{
    class Primary
    {
        string id;
        IMetadataToClient metadata;

        public Primary(string id, IMetadataToClient metadata)
        {
            this.id = id;
            this.metadata = metadata;
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public IMetadataToClient Metadata
        {
            get { return this.metadata; }
            set { this.metadata = value; }
        }
    }

    public class Client : MarshalByRefObject, IClientToPM
    {
        private static Dictionary<string, FileMetadata> openedFilesMetadata
             = new Dictionary<string, FileMetadata>(Helper.MAX_FILE_REGISTERS);

        private static Dictionary<string, IMetadataToClient> metadatas 
            = new Dictionary<string, IMetadataToClient>();

        private static Primary primary = null;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong arguments");

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);

            string id = args[0];
            int port = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Client),
                id,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Client " + id + " has started.");

            Console.ReadLine();
        }

        public void UpdatePrimary()
        {
            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToClient metadata = entry.Value;
            }
        }

        /**
         * IClientToPM Methods
         */

        public void MetadataLocation(string id, string location)
        {
            IMetadataToClient metadata = (IMetadataToClient)Activator.GetObject(
                typeof(IMetadataToClient), 
                location);
            metadatas.Add(id, metadata);

            if (primary == null)
            {
                primary = new Primary(id, metadata);
            }
        }

        public void Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            System.Console.WriteLine("CREATE CLIENT FILE " + filename);
            try
            {
                if (openedFilesMetadata.ContainsKey(filename)) throw new FileAlreadyExistsException(filename);

                // missing testing if metadata is down
                FileMetadata file = primary.Metadata.Create(filename, nbDataServers, readQuorum, writeQuorum);
                openedFilesMetadata.Add(filename, file);
            }
            catch (FileAlreadyExistsException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (NotEnoughDataServersException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Open(string filename)
        {
            System.Console.WriteLine("OPEN CLIENT FILE " + filename);
            try
            {
                if (!openedFilesMetadata.ContainsKey(filename))
                {
                    FileMetadata file = primary.Metadata.Open(filename);
                    openedFilesMetadata.Add(filename, file);
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Close(string filename)
        {
            System.Console.WriteLine("CLOSE CLIENT FILE " + filename);
            try
            {
                if (openedFilesMetadata.ContainsKey(filename))
                {
                    primary.Metadata.Close(filename);
                    openedFilesMetadata.Remove(filename);
                }
                else
                {
                    //Recheck: Exception??
                    Console.WriteLine("File " + filename + " is not opened.");
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Delete(string filename)
        {
            System.Console.WriteLine("DELETE CLIENT FILE " + filename);
            try
            {
                if (!openedFilesMetadata.ContainsKey(filename))
                {
                    primary.Metadata.Delete(filename);
                }
                else
                {
                    //Recheck: Exception??
                    Console.WriteLine("File " + filename + " is opened. Please close the file first.");
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Read()
        {
            // HARD CODED TEST
            System.Console.WriteLine("READ CLIENT FILE");
            IDataServerToClient data = (IDataServerToClient)Activator.GetObject(typeof(IDataServerToClient), "tcp://localhost:9/d-1");
            data.Read();
        }

        public void Write()
        {
            // HARD CODED TEST
            System.Console.WriteLine("WRITE CLIENT FILE");
            IDataServerToClient data = (IDataServerToClient)Activator.GetObject(typeof(IDataServerToClient), "tcp://localhost:9/d-1");
            data.Write();
        }

        public void Dump()
        {
            System.Console.WriteLine("DUMP PROCESS");
        }
    }
}
