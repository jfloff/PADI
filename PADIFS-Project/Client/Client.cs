﻿using SharedLibrary;
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
        class LatestVersion
        {
            public FileVersion Version;
            public string DataServerId;

            public LatestVersion(FileVersion version, string dataServerId)
            {
                this.Version = version;
                this.DataServerId = dataServerId;
            }
        };

        // metadataId / Proxy to metadata
        private static ConcurrentDictionary<string, IMetadataToClient> metadatas
            = new ConcurrentDictionary<string, IMetadataToClient>();

        // index / fileregister struct
        FileRegister fileRegister = new FileRegister();
        // index / byte contents
        private static Dictionary<int, byte[]> byteRegister = new Dictionary<int, byte[]>();

        private static string master = string.Empty;
        private static string id;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong arguments");

            id = args[0];
            int port = Convert.ToInt32(args[1]);

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);
            Console.Title = id;

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Client),
                id,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Client " + id + " has started.");
            Console.ReadLine();
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
         * IClientToPM Methods
         */

        public void MetadataLocation(string id, string location)
        {
            Console.WriteLine("RECEIVED METADATA LOCATION " + location);

            IMetadataToClient metadata = (IMetadataToClient)Activator.GetObject(
                typeof(IMetadataToClient),
                location);
            metadatas[id] = metadata;

            // is master is empty client is bootstraping
            if ((master == string.Empty) || (string.Compare(master, id) >= 0))
            {
                master = id;
            }
        }

        public void Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("CREATE CLIENT FILE " + filename);

            // keeps looping untill a master answers
            while (true)
            {
                try
                {
                    FileMetadata fileMetadata = metadatas[master].Create(filename, nbDataServers, readQuorum, writeQuorum);
                    fileRegister.AddOrUpdate(filename, fileMetadata);
                    return;
                }
                catch (ProcessFailedException)
                {
                    FindMaster();
                }
                catch (FileAlreadyExistsException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        public void Open(string filename)
        {
            Console.WriteLine("OPEN CLIENT FILE " + filename);
            
            // keeps looping untill a master answers
            while (true)
            {
                try
                {
                    FileMetadata fileMetadata = OpenFileMetadata(filename);
                    Console.WriteLine("FILE METADATA => " + fileMetadata.ToString());
                    return;
                }
                catch (ProcessFailedException)
                {
                    FindMaster();
                }
                catch (FileDoesNotExistException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        private FileMetadata OpenFileMetadata(string filename)
        {
            // keeps looping untill a master answers
            while (true)
            {
                try
                {
                    FileMetadata fileMetadata = metadatas[master].Open(filename);
                    fileRegister.AddOrUpdate(filename, fileMetadata);
                    return fileMetadata;
                }
                catch (ProcessFailedException)
                {
                    FindMaster();
                }
            }
        }

        public void Close(string filename)
        {
            Console.WriteLine("CLOSED FILE " + filename);

            // keeps looping untill a master answers
            while (true)
            {
                try
                {
                    fileRegister.Remove(filename);
                    metadatas[master].Close(filename);
                    return;
                }
                catch (ProcessFailedException)
                {
                    FindMaster();
                }
                catch (FileDoesNotExistException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        public void Delete(string filename)
        {
            Console.WriteLine("DELETE CLIENT FILE " + filename);

            // keeps looping untill a master answers
            while (true)
            {
                try
                {
                    if (!fileRegister.Contains(filename))
                    {
                        metadatas[master].Delete(filename);
                    }
                    else
                    {
                        Console.WriteLine("File " + filename + " is opened. Please close the file first.");
                    }
                    return;
                }
                catch (ProcessFailedException)
                {
                    FindMaster();
                }
                catch (FileDoesNotExistException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        private FileData ReadFileData(int fileRegisterIndex, Helper.Semantics semantics)
        {
            // gets latest version
            LatestVersion latestVersion = ReadVersion(fileRegisterIndex, semantics);

            // requests file data to dataServer with latest version
            string id = latestVersion.DataServerId;
            string location = fileRegister.FileMetadataAt(fileRegisterIndex).Locations[id];
            string localFilename = fileRegister.FileMetadataAt(fileRegisterIndex).LocalFilenames[id];
            FileData fileData = null;

            Thread request = new Thread(() =>
            {
                IDataServerToClient dataServer = (IDataServerToClient)Activator.GetObject(
                    typeof(IDataServerToClient),
                    location);
                // working under the assumptions there is no FAILs between the process
                fileData = dataServer.Read(localFilename);
            });
            request.Start();

            // waits for answer
            while (fileData == null) ;

            // update file registers
            fileRegister.SetFileDataAt(fileRegisterIndex, fileData);
            return fileData;
        }

        public void Read(int fileRegisterIndex, Helper.Semantics semantics, int byteRegisterIndex)
        {
            Console.WriteLine("READ FILE = " + fileRegisterIndex);

            FileData fileData = ReadFileData(fileRegisterIndex, semantics);
            byteRegister[byteRegisterIndex] = fileData.Contents;
        }

        private LatestVersion ReadVersion(int fileRegisterIndex, Helper.Semantics semantics)
        {
            string filename = fileRegister.FilenameAt(fileRegisterIndex);
            FileVersion original = fileRegister.FileDataAt(fileRegisterIndex).Version;
            FileMetadata fileMetadata = fileRegister.FileMetadataAt(fileRegisterIndex);
            // data server id / file data
            ConcurrentDictionary<string, FileVersion> reads = new ConcurrentDictionary<string, FileVersion>();
            int requests = 0;
            LatestVersion quorumVersion = null;


            //QUORUM
            while (true)
            {
                // voting
                ReadQuorum quorum = new ReadQuorum(fileMetadata.ReadQuorum, semantics);
                foreach (var entry in reads)
                {
                    FileVersion vote = entry.Value;
                    string dataServerId = entry.Key;

                    quorum.AddVote(vote);
                    if (quorum.CheckQuorum(vote, original))
                    {
                        quorumVersion = new LatestVersion(vote, dataServerId);
                        break;
                    }
                }

                // found the quorum file
                if (quorumVersion != null) break;

                // if all the votes arrived at the quorum
                // stops when all requests are counted (requests = 0)
                if (quorum.Count == (requests + quorum.Count))
                {
                    // get possible new fileMetadata locations
                    // possible optimization
                    // check if there are no dataa servers
                    fileMetadata = OpenFileMetadata(filename);

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
                            FileVersion fileVersion = null;
                            try
                            {
                                fileVersion = dataServer.Version(localFilename);

                            }
                            catch (ProcessFailedException) { }
                            catch (ProcessFreezedException) { }
                            catch (FileDoesNotExistException) { }
                            finally
                            {
                                reads[id] = fileVersion;
                                Interlocked.Decrement(ref requests);
                            }
                        });
                        request.Start();
                    }
                }
            }

            return quorumVersion;
        }

        public void Write(int fileRegisterIndex, byte[] contents)
        {
            Console.WriteLine("WRITE FILE = " + fileRegisterIndex);

            // forces to get always the most recent file
            FileVersion latest = ReadVersion(fileRegisterIndex, Helper.Semantics.MONOTONIC).Version;
            latest.Increment(Client.id);
            FileData fileData = new FileData(latest, contents);

            string filename = fileRegister.FilenameAt(fileRegisterIndex);
            FileMetadata fileMetadata = fileRegister.FileMetadataAt(fileRegisterIndex);
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
                    // get possible new fileMetadata locations
                    // possible optimization
                    // check if there are no dataa servers
                    fileMetadata = OpenFileMetadata(filename);

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
                            catch (ProcessFailedException) { }
                            catch (ProcessFreezedException) { }
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

            // update file registers
            fileRegister.SetFileDataAt(fileRegisterIndex, fileData);
        }

        public void Write(int fileRegisterIndex, int byteRegisterIndex)
        {
            if ((byteRegisterIndex > (byteRegister.Count - 1)) || (byteRegister[byteRegisterIndex] == null))
            {
                Console.WriteLine("Byte register " + byteRegisterIndex + " does not exist");
                return;
            }

            Write(fileRegisterIndex, byteRegister[byteRegisterIndex]);
        }

        public void Copy(int fileRegisterIndex1, Helper.Semantics semantics, int fileRegisterIndex2, byte[] salt)
        {
            Console.WriteLine("COPY FILE " + fileRegisterIndex1 + " TO + " + fileRegisterIndex2);

            FileData fileData = ReadFileData(fileRegisterIndex1, semantics);
            byte[] saltedContents = Helper.AppendBytes(fileData.Contents, salt);
            Write(fileRegisterIndex2, saltedContents);
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("File Registers");
            Console.WriteLine(fileRegister);
            Console.WriteLine("Byte Registers");
            for (int i = 0; i < byteRegister.Count; i++)
            {
                Console.WriteLine(i + ":" + Helper.BytesToString(byteRegister[i]));
            }
        }
    }
}
