using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary;
using SharedLibrary.Exceptions;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;
using SharedLibrary.Entities;
using SharedLibrary.Interfaces;
using System.Threading;

namespace Metadata
{
    class Metadata : MarshalByRefObject, IMetadataToPM, IMetadataToMetadata, IMetadataToClient, IMetadataToDataServer
    {
        // filename / FileMetadata
        private static Dictionary<string, FileMetadata> fileMetadataTable
            = new Dictionary<string, FileMetadata>();

        private static List<string> openedFiles = new List<string>();

        // id / Interface
        private static Dictionary<string, IDataServerToMetadata> dataServers
            = new Dictionary<string, IDataServerToMetadata>();
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
            Console.WriteLine("PRIMARY = " + primary);
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
        public Dictionary<string, string> SelectDataServers(int nbDataServers, string filename)
        {
            if (nbDataServers > dataServers.Count)
            {
                // o que fazer neste caso ??
                // para que server entao o nbDataServers
                // Se o create falhar será provavelmente aqui
            }

            Dictionary<string, string> selectedDataServers = new Dictionary<string, string>();
            for (int i = 0; i < nbDataServers; i++)
            {
                selectedDataServers.Add(dataServers.ElementAt(i).Key, localFilename(filename));
            }

            return selectedDataServers;
        }

        // Places files in the given dataServers. Doesn't care if they were created or not.
        public void CreateFileOnDataServers(FileMetadata fileMetadata)
        {
            foreach (var entry in fileMetadata.DataServers)
            {
                string dataServerId = entry.Key;
                string localFilename = entry.Value;

                Thread request = new Thread(() =>
                {
                    IDataServerToMetadata dataServer = (IDataServerToMetadata)dataServers[dataServerId];
                    dataServer.Create(localFilename);
                });
                request.Start();
            }
        }

        //Tratar o caso em que possivelmente algum dos servidores não conseguiu apagar o ficheiro
        public void DeleteFileOnDataServers(FileMetadata fileMetadata)
        {
            foreach (var entry in fileMetadata.DataServers)
            {
                string dataServerId = entry.Key;
                string localFilename = entry.Value;

                Thread request = new Thread(() =>
                {
                    IDataServerToMetadata dataServer = (IDataServerToMetadata)dataServers[dataServerId];
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

                Console.WriteLine("  " + id);
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

            Console.WriteLine("PING");
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
            Dictionary<string, string> selectedDataServersList = SelectDataServers(nbDataServers, filename);
            FileMetadata fileMetadata
                = new FileMetadata(filename, nbDataServers, readQuorum, writeQuorum, selectedDataServersList);
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
                dataServers.Add(id, dataServer);
            }
        }
    }
}
