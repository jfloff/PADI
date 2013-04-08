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

// @TODO Missing re-connect to other metadata

namespace Client
{
    public class Client : MarshalByRefObject, IClientToPM
    {
        struct Primary
        {
            public string Id;
            public IMetadataToClient Metadata;
        };

        // filename / file contents
        private static Dictionary<string, FileData> openedFilesData
             = new Dictionary<string, FileData>();
        // filename / file metadata
        private static Dictionary<string, FileMetadata> openedFilesMetadata
             = new Dictionary<string, FileMetadata>();
        // metadataId / Proxy to metadata
        private static Dictionary<string, IMetadataToClient> metadatas 
            = new Dictionary<string, IMetadataToClient>();

        // filename 
        List<string> fileRegisters = new List<string>();
        List<byte[]> byteRegisters = new List<byte[]>();

        private static Primary primary = new Primary() { Id = null, Metadata = null };

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

        /**
         * IClientToPM Methods
         */

        public void MetadataLocation(string id, string location)
        {
            Console.WriteLine("RECEIVED METADATA LOCATION " + location);

            IMetadataToClient metadata = (IMetadataToClient)Activator.GetObject(
                typeof(IMetadataToClient), 
                location);
            metadatas.Add(id, metadata);

            // missing recheck of metadata
            if (primary.Id == null)
            {
                primary.Id = id;
                primary.Metadata = metadata;
            }
        }

        public void Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("CREATE CLIENT FILE " + filename);

            try
            {
                if (openedFilesMetadata.ContainsKey(filename)) 
                    throw new FileAlreadyExistsException(filename);

                // missing testing if metadata is down
                FileMetadata file = primary.Metadata.Create(filename, nbDataServers, readQuorum, writeQuorum);
                openedFilesMetadata.Add(filename, file);
            }
            catch (FileAlreadyExistsException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Open(string filename)
        {
            Console.WriteLine("OPEN CLIENT FILE " + filename);
            try
            {
                if (!openedFilesMetadata.ContainsKey(filename))
                {
                    FileMetadata fileMetadata = primary.Metadata.Open(filename);
                    openedFilesMetadata.Add(filename, fileMetadata);
                    fileRegisters.Add(filename);
                    Console.WriteLine("FILE METADATA => " + fileMetadata.ToString());
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Close(string filename)
        {
            Console.WriteLine("CLOSED FILE " + filename);
            try
            {
                if (openedFilesMetadata.ContainsKey(filename))
                {
                    primary.Metadata.Close(filename);
                    openedFilesMetadata.Remove(filename);
                    fileRegisters.Remove(filename);
                }
                else
                {
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
            Console.WriteLine("DELETE CLIENT FILE " + filename);
            try
            {
                if (!openedFilesMetadata.ContainsKey(filename))
                {
                    primary.Metadata.Delete(filename);
                }
                else
                {
                    Console.WriteLine("File " + filename + " is opened. Please close the file first.");
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Read(int fileRegister, Helper.Semantics semantics, int stringRegister)
        {
            Console.WriteLine("READ FILE = " + fileRegister);

            if (fileRegister > (fileRegisters.Count - 1))
            {
                Console.WriteLine("File register " + fileRegister + " does not exist");
                return;
            }

            string filename = fileRegisters[fileRegister];

            FileMetadata fileMetadata = openedFilesMetadata[filename];

            // broadcast reads to all dataServers that have that file
            ConcurrentDictionary<string, FileData> reads = new ConcurrentDictionary<string,FileData>();
            List<Thread> requests = new List<Thread>();
            foreach(var entry in fileMetadata.Locations)
            {
                string id = entry.Key;
                string location = entry.Value;
                string localFilename = fileMetadata.LocalFilenames[id];

                Thread request = new Thread(() => 
                {
                    // missing checking if its down
                    IDataServerToClient dataServer = (IDataServerToClient)Activator.GetObject(
                        typeof(IDataServerToClient), 
                        localFilename);

                    reads.TryAdd(id, dataServer.Read(localFilename));
                });
                requests.Add(request);
                request.Start();
            }

            // wait on all the requests to end
            foreach (Thread request in requests)
            {
                request.Join();
            }

            //quoruns
            
        }

        public void Write(int fileRegister, int stringRegister)
        {

        }

        public void Write(int fileRegister, string contents)
        {
            //IDataServerToClient data = (IDataServerToClient)Activator.GetObject(typeof(IDataServerToClient), "tcp://localhost:9/d-1");
        }
        
        public void Copy(int fileRegister1, Helper.Semantics semantics, int fileRegister2, string salt)
        {
            //
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("Opened File Metadatas");
            foreach (var entry in openedFilesMetadata)
            {
                string filename = entry.Key;
                FileMetadata fileMetadata = entry.Value;

                Console.WriteLine("  " + filename);
                Console.WriteLine("    " + fileMetadata.ToString());
            }
        }
    }
}
