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
        class Primary
        {
            public string Id;
            public IMetadataToClient Metadata;
        };

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
        private static string id;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong arguments");

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);

            id = args[0];
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
                FileMetadata fileMetadata = primary.Metadata.Open(filename);
                openedFilesMetadata[filename] = fileMetadata;
                fileRegisters.Add(filename);
                Console.WriteLine("FILE METADATA => " + fileMetadata.ToString());
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

            FileData fileData = ReadFileData(fileRegister, semantics);
            byteRegisters.Insert(byteRegister, fileData.Contents);
        }

        private FileData ReadFileData(int fileRegister, Helper.Semantics semantics)
        {
            if (fileRegister > (fileRegisters.Count - 1))
            {
                Console.WriteLine("File register " + fileRegister + " does not exist");
                return null;
            }

            string filename = fileRegisters[fileRegister];
            FileMetadata fileMetadata = openedFilesMetadata[filename];
            // data server id / file data
            ConcurrentDictionary<string, FileData> reads = new ConcurrentDictionary<string, FileData>();
            int requests = 0;
            FileData quorumFile = null;

            //QUORUM
            while (true)
            {
                // voting
                ReadQuorum quorum = new ReadQuorum(fileMetadata.ReadQuorum);

                foreach (var entry in reads)
                {
                    FileData vote = entry.Value;

                    quorum.AddVote(vote);
                    if (quorum.CheckQuorum(vote))
                    {
                        quorumFile = vote;
                        break;
                    }
                }

                // found the quorum file
                if (quorumFile != null) break;

                // if all the votes arrived at the quorum
                // stops when all requests are counted (requests = 0)
                if (quorum.Count == (requests + quorum.Count))
                {
                    // broadcast to all dataServers that have that file
                    foreach (var entry in fileMetadata.Locations)
                    {
                        string id = entry.Key;
                        string location = entry.Value;
                        string localFilename = fileMetadata.LocalFilenames[id];

                        // increment right away so it doesn't request untill its decremented
                        Interlocked.Increment(ref requests);
                        Thread request = new Thread(() =>
                        {
                            IDataServerToClient dataServer = (IDataServerToClient)Activator.GetObject(
                                typeof(IDataServerToClient),
                                location);
                            FileData fileData = null;
                            try
                            {
                                fileData = dataServer.Read(localFilename);

                            }
                            catch (ProcessDownException) { }
                            catch (FileDoesNotExistException) { }
                            finally
                            {
                                reads[id] = fileData;
                                Interlocked.Decrement(ref requests);
                            }
                        });
                        request.Start();
                    }
                }
            }

            return quorumFile;
        }

        public void Write(int fileRegister, byte[] contents)
        {
            Console.WriteLine("WRITE FILE = " + fileRegister);

            if (fileRegister > (fileRegisters.Count - 1))
            {
                Console.WriteLine("File register " + fileRegister + " does not exist");
                return;
            }

            // forces to get always the most recent file
            FileData fileData = ReadFileData(fileRegister, Helper.Semantics.MONOTONIC);
            fileData.Contents = contents;
            fileData.IncrementVersion(Client.id);

            string filename = fileRegisters[fileRegister];
            FileMetadata fileMetadata = openedFilesMetadata[filename];
            // data server id / bool write
            ConcurrentDictionary<string, bool> writes = new ConcurrentDictionary<string, bool>();
            int requests = 0;
            bool quorumReached = false;

            //QUORUM
            while (true)
            {
                // voting
                WriteQuorum quorum = new WriteQuorum(fileMetadata.WriteQuorum);
                foreach (var entry in writes)
                {
                    bool vote = entry.Value;

                    quorum.AddVote(vote);
                    if (quorum.CheckQuorum())
                    {
                        quorumReached = true;
                        break;
                    }
                }

                // found the quorum file
                if (quorumReached) break;

                // if all the votes arrived at the quorum
                // stops when all requests are counted (requests = 0)
                if (quorum.Count == (requests + quorum.Count))
                {
                    // broadcast to all dataServers that have that file
                    foreach (var entry in fileMetadata.Locations)
                    {
                        string id = entry.Key;
                        string location = entry.Value;
                        string localFilename = fileMetadata.LocalFilenames[id];

                        // increment right away so it doesn't request untill its decremented
                        Interlocked.Increment(ref requests);
                        Thread request = new Thread(() =>
                        {
                            IDataServerToClient dataServer = (IDataServerToClient)Activator.GetObject(
                                typeof(IDataServerToClient),
                                location);
                            bool vote = false;
                            try
                            {
                                dataServer.Write(localFilename, fileData);
                                vote = true;
                            }
                            catch (ProcessDownException) { }
                            catch (FileDoesNotExistException) { }
                            finally
                            {
                                writes[id] = vote;
                                Interlocked.Decrement(ref requests);
                            }
                        });
                        request.Start();
                    }
                }
            }
        }

        public void Write(int fileRegister, int byteRegister)
        {
            if ((byteRegister > (byteRegisters.Count - 1)) || (byteRegisters[byteRegister] == null))
            {
                Console.WriteLine("Byte register " + byteRegister + " does not exist");
                return;
            }

            Write(fileRegister, byteRegisters[byteRegister]);
        }

        public void Copy(int fileRegister1, Helper.Semantics semantics, int fileRegister2, byte[] salt)
        {
            Console.WriteLine("COPY FILE " + fileRegister1 + " TO + " + fileRegister2);

            if (fileRegister1 > (fileRegisters.Count - 1))
            {
                Console.WriteLine("File register " + fileRegister1 + " does not exist");
                return;
            }

            if (fileRegister2 > (fileRegisters.Count - 1))
            {
                Console.WriteLine("File register " + fileRegister2 + " does not exist");
                return;
            }

            FileData fileData = ReadFileData(fileRegister1, semantics);
            byte[] saltedContents = Helper.AppendBytes(fileData.Contents, salt);
            Write(fileRegister2, saltedContents);
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
            Console.WriteLine("File Registers");
            for (int i = 0; i < fileRegisters.Count; i++)
            {
                Console.WriteLine("  " + i + ":" + fileRegisters[i]);
            }
            Console.WriteLine("Byte Registers");
            for (int i = 0; i < byteRegisters.Count; i++)
            {
                Console.WriteLine("  " + i + ":" + Helper.BytesToString(byteRegisters[i]));
            }
        }
    }
}
