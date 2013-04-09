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
using SharedLibrary.Entities;

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
        private static List<string> fileRegisters = new List<string>();
        private static List<byte[]> byteRegisters = new List<byte[]>();

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
            metadatas[id] = metadata;

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
                openedFilesMetadata[filename] = file;
                fileRegisters.Add(filename);
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
                    openedFilesMetadata[filename] = fileMetadata;
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

        public void Read(int fileRegister, Helper.Semantics semantics, int byteRegister)
        {
            Console.WriteLine("READ FILE = " + fileRegister);

            if (fileRegister > (fileRegisters.Count - 1))
            {
                Console.WriteLine("File register " + fileRegister + " does not exist");
                return;
            }

            string filename = fileRegisters[fileRegister];
            FileMetadata fileMetadata = openedFilesMetadata[filename];
            ConcurrentDictionary<string, FileData> reads = new ConcurrentDictionary<string, FileData>();
            // data server id / thread
            ConcurrentDictionary<string, Thread> requests = new ConcurrentDictionary<string, Thread>();
            FileData quorumFile = null;

            //QUORUM
            while (true)
            {
                // voting
                Dictionary<FileData, int> quorum = new Dictionary<FileData, int>();
                foreach (var entry in reads)
                {
                    FileData fileData = entry.Value;

                    if (quorum.ContainsKey(fileData))
                    {
                        quorum[fileData]++;
                    }
                    else
                    {
                        quorum[fileData] = 1;
                    }

                    if (quorum[fileData] >= fileMetadata.ReadQuorum)
                    {
                        quorumFile = fileData;
                        break;
                    }
                }

                // found the quorum file
                if (quorumFile != null) break;
                
                // if did not found check if it has requests pending
                if (requests.Count == 0)
                {
                    // broadcast to all dataServers that have that file
                    foreach (var entry in fileMetadata.Locations)
                    {
                        string id = entry.Key;
                        string location = entry.Value;
                        string localFilename = fileMetadata.LocalFilenames[id];

                        Thread request = new Thread(() =>
                        {
                            // missing checking if its down
                            IDataServerToClient dataServer = (IDataServerToClient)Activator.GetObject(
                                typeof(IDataServerToClient),
                                location);
                            reads[id] = dataServer.Read(localFilename);
                            Thread ignored;
                            requests.TryRemove(id, out ignored);
                        });
                        requests[id] = request;
                        request.Start();
                    }
                }
            }

            //byteRegistersbyteRegister = quorumFile.Contents;
        }

        public void Write(int fileRegister, string contents)
        {

        }

        public void Write(int fileRegister, int stringRegister)
        {

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
