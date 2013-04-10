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
using System.Threading;

//@TODO MISSING CONCURRENCY

namespace Metadata
{
    class Metadata : MarshalByRefObject, IMetadataToPM, IMetadataToMetadata, IMetadataToClient, IMetadataToDataServer
    {
        // Return of SelectDataServers Function
        private struct SelectedServers
        {
            public Dictionary<string, string> LocalFilenames;
            public Dictionary<string, string> Locations;

            public SelectedServers(Dictionary<string, string> localFilenames, Dictionary<string, string> locations) 
            {
                this.LocalFilenames = localFilenames;
                this.Locations = locations;
            }
        }

        // Store on each entry of the data servers dictionary
        private struct DataServerInfo
        {
            public string Location;
            public IDataServerToMetadata DataServer;

            public DataServerInfo(string location, IDataServerToMetadata dataServer) 
            {
                this.Location = location;
                this.DataServer = dataServer;
            }
        }

        // filename / FileMetadata
        private static Dictionary<string, FileMetadata> fileMetadataTable
            = new Dictionary<string, FileMetadata>();
        // filename
        private static List<string> openedFiles = new List<string>();
        // id / Interface
        private static Dictionary<string, DataServerInfo> dataServers
            = new Dictionary<string, DataServerInfo>();
        // id / interface
        private static Dictionary<string, IMetadataToMetadata> metadatas
            = new Dictionary<string, IMetadataToMetadata>();

        private static string primary;
        private static string id;
        private static bool fail = false;


        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong Arguments");

            // Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);

            id = primary = args[0];
            int port = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Metadata),
                id,
                WellKnownObjectMode.Singleton);

            // Start pinging the metatadas in a set interval
            Thread t = new Thread(() => 
            { 
                while (true)
                {
                    // if in fail doesn't ping anyone either
                    while (fail) Thread.Sleep(Helper.PING_INTERVAL);
                    FindPrimary();
                    Thread.Sleep(Helper.PING_INTERVAL);
                }
            });
            t.Start();

            Console.WriteLine("Metadata Server " + id + " has started.");
            Console.ReadLine();
        }

        /**
         * PING / Fault Detection functions
         */

        // Function to run perodically to detect metadatas faults.
        // Pings all the known metadatas and chooses the lowest id.
        private static void FindPrimary()
        {
            List<string> pings = new List<string>();
            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToMetadata metadata = entry.Value;
                try
                {
                    metadata.Ping();
                    pings.Add(id);
                }
                catch (ProcessDownException) { }
            }
            pings.Add(Metadata.id);
            primary = pings.Min();
            //Console.WriteLine("PRIMARY = " + primary);
        }

        /**
         * Open File related methods
         */

        // returns localFilename for dataServer
        private static string localFilename(string filename)
        {
            // GUID better probably?
            return filename + "$" + filename.GetHashCode();
        }

        // Returns a dictionray dataServerId / localFilename
        private SelectedServers SelectDataServers(int nbDataServers, string filename)
        {
            Dictionary<string, string> localFilenames = new Dictionary<string, string>();
            Dictionary<string, string> locations = new Dictionary<string, string>();

            int nbDataServersSelected = 0;
            foreach (var entry in dataServers)
            {
                if (nbDataServersSelected++ > nbDataServers) break;

                string dataServerId = entry.Key;
                string dataServerLocation = entry.Value.Location;
                localFilenames.Add(dataServerId, localFilename(filename));
                locations.Add(dataServerId, dataServerLocation);
            }
            return new SelectedServers(localFilenames, locations);
        }

        // Places files in the given dataServers. Doesn't care if they were created or not.
        public void CreateFileOnDataServers(FileMetadata fileMetadata)
        {
            foreach (var entry in fileMetadata.LocalFilenames)
            {
                string id = entry.Key;
                string localFilename = entry.Value;

                Thread request = new Thread(() =>
                {
                    IDataServerToMetadata dataServer = dataServers[id].DataServer;
                    dataServer.Create(localFilename);
                });
                request.Start();
            }
        }

        //Tratar o caso em que possivelmente algum dos servidores não conseguiu apagar o ficheiro
        public void DeleteFileOnDataServers(FileMetadata fileMetadata)
        {
            foreach (var entry in fileMetadata.LocalFilenames)
            {
                string id = entry.Key;
                string localFilename = entry.Value;

                Thread request = new Thread(() =>
                {
                    IDataServerToMetadata dataServer = dataServers[id].DataServer;
                    dataServer.Delete(localFilename);
                });
                request.Start();
            }
        }

        /**
         * IMetadataToPM Methods
         */

        public void MetadataLocation(string id, string location)
        {
            Console.WriteLine("RECEIVED METADATA LOCATION " + location);

            IMetadataToMetadata metadata = (IMetadataToMetadata)Activator.GetObject(
                typeof(IMetadataToMetadata),
                location);
            metadatas.Add(id, metadata);

            //Force invocation of primary decision
            FindPrimary();
        }

        public void Dump()
        {
            Console.WriteLine("DUMP");
            Console.WriteLine("File Metadatas");
            foreach (var entry in fileMetadataTable)
            {
                string filename = entry.Key;
                FileMetadata fileMetadata = entry.Value;

                Console.WriteLine("  " + filename);
                Console.WriteLine("    " + fileMetadata.ToString());
            }
        }

        public void Fail()
        {
            Console.WriteLine("FAIL");
            fail = true;
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER");
            fail = false;
        }

        /**
         * IMetadataToMetadata Methods
         */

        public void Ping()
        {
            if (fail) throw new ProcessDownException(id);

            // Console.WriteLine("PING");
        }

        /**
         * IMetadataToClient Methods
         */

        public FileMetadata Create(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("CREATE METADATA FILENAME: " + filename + " NBDATASERVERS: " + nbDataServers
                + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            if (fileMetadataTable.ContainsKey(filename))
                throw new FileAlreadyExistsException(filename);

            //Select Data Servers and creates files within them
            SelectedServers selectedServers = SelectDataServers(nbDataServers, filename);
            FileMetadata fileMetadata = new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum, 
                selectedServers.LocalFilenames, selectedServers.Locations);
            CreateFileOnDataServers(fileMetadata);
            fileMetadataTable.Add(filename, fileMetadata);
            openedFiles.Add(filename);
            return fileMetadata;
        }

        public FileMetadata Open(string filename)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("OPEN METADATA FILE " + filename);

            if (!fileMetadataTable.ContainsKey(filename))
                throw new FileDoesNotExistException(filename);

            // does nothing if file is already open
            if (!openedFiles.Contains(filename))
            {
                openedFiles.Add(filename);
            }
            
            return fileMetadataTable[filename];
        }

        public void Close(string filename)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("CLOSE METADATA FILE " + filename);

            if (!fileMetadataTable.ContainsKey(filename)) 
                throw new FileDoesNotExistException(filename);

            // does nothing if file was never open
            if (openedFiles.Contains(filename))
            {
                openedFiles.Remove(filename);
            }
        }

        public void Delete(string filename)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("DELETE METADATA FILE " + filename);

            if (!fileMetadataTable.ContainsKey(filename)) 
                throw new FileDoesNotExistException(filename);

            if (openedFiles.Contains(filename))
                throw new FileCurrentlyOpenException(filename);

            openedFiles.Remove(filename);

            // missing check if they are all deleted?
            DeleteFileOnDataServers(fileMetadataTable[filename]);
            fileMetadataTable.Remove(filename);
        }

        /**
         * IMetadataToDataServer Methods
         */

        public Heartbeat Heartbeat(string id)
        {
            if (fail) throw new ProcessDownException(id);

            // what should the system do if a data-server fails?
            Console.WriteLine("HEARTBEAT");
            return null;
        }

        public void RegisterDataServer(string id, string location)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("REGISTER DATA SERVER " + id);

            if (!dataServers.ContainsKey(id))
            {
                IDataServerToMetadata dataServer = (IDataServerToMetadata)Activator.GetObject(
                    typeof(IDataServerToMetadata),
                    location);
                dataServers.Add(id, new DataServerInfo(location, dataServer));
            }
        }
    }
}
