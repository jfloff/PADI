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
        private static Dictionary<string, FileMetadata> fileMetadataTable
            = new Dictionary<string, FileMetadata>();

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

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);

            id = primary = args[0];
            int port = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Metadata),
                id,
                WellKnownObjectMode.Singleton);

            // Start pinging the metatadas in a set interval
            Thread t = new Thread(FindPrimary);
            t.Start();

            Console.WriteLine("Metadata Server " + id + " has started.");
            Console.ReadLine();
        }

        /**
         * PING / Fault Detection functions
         */

        private bool ImPrimary()
        {
            return (primary == id);
        }

        // Function to run perodically to detect metadatas faults.
        // Pings all the known metadatas and chooses the lowest id.
        private static void FindPrimary()
        {
            while (true)
            {
                // if in fail doesn't ping anyone either
                while (fail) Thread.Sleep(Helper.PING_INTERVAL);

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
                    catch (ProcessDownException e) { }
                }
                pings.Add(Metadata.id);
                primary = pings.Min();
                Console.WriteLine("PRIMARY = " + primary);
                Thread.Sleep(Helper.PING_INTERVAL);
            }
        }

        ////// TO RECHECK 

        public bool HasFile(string fileName)
        {
            return fileMetadataTable.ContainsKey(fileName);
        }

        public Dictionary<string, string> SelectDataServersForFilePlacement(int nbDataServers, string localFileName)
        {
            int actualNrDataServers = dataServers.Count;

            if (nbDataServers > actualNrDataServers)
                throw new NotEnoughDataServersException(nbDataServers, actualNrDataServers);

            Dictionary<string, string> selectedDataServers = new Dictionary<string, string>();
            for (int i = 0; i < nbDataServers; i++)
            {
                selectedDataServers.Add(dataServers.ElementAt(i).Key, localFileName);
            }

            return selectedDataServers;
        }

        public bool FilePlacementOnSelectedDataServers(Dictionary<string, string> dataServersAndLocalFileList)
        {
            //Async request?
            for (int i = 0; i < dataServersAndLocalFileList.Count; i++)
            {
                string dataServerName = dataServersAndLocalFileList.ElementAt(i).Key;
                string fileName = dataServersAndLocalFileList.ElementAt(i).Value;
                IDataServerToMetadata dataServer = (IDataServerToMetadata)dataServers[dataServerName];
                dataServer.Create(fileName);
            }
            return true;
        }

        public bool FileDeletionOnCorrespondingDataServers(Dictionary<string, string> dataServersAndLocalFileList)
        {

            //Async request?
            for (int i = 0; i < dataServersAndLocalFileList.Count; i++)
            {
                string dataServerName = dataServersAndLocalFileList.ElementAt(i).Key;
                string fileName = dataServersAndLocalFileList.ElementAt(i).Value;
                IDataServerToMetadata dataServer = (IDataServerToMetadata)dataServers[dataServerName];
                dataServer.Delete(fileName);
            }
            return true;
        }
        /////////

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

        public FileMetadata Open(string fileName)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("OPEN METADATA FILE " + fileName);

            if (!HasFile(fileName))
                throw new FileDoesNotExistException(fileName);

            return fileMetadataTable[fileName];
        }

        // recheck
        public void Close(string fileName)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("CLOSE METADATA FILE " + fileName);

            if (!HasFile(fileName))
                throw new FileDoesNotExistException(fileName);
        }

        public FileMetadata Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("CREATE METADATA FILENAME: " + fileName + " NBDATASERVERS: " + nbDataServers + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            if (HasFile(fileName))
                throw new FileAlreadyExistsException(fileName);

            //Select Data Servers For File Placement
            Dictionary<string, string> selectedDataServersList = SelectDataServersForFilePlacement(nbDataServers, fileName + fileName.GetHashCode());
            FileMetadata fileMetadata = new FileMetadata(fileName, nbDataServers, readQuorum, writeQuorum, selectedDataServersList);
            fileMetadataTable.Add(fileName, fileMetadata);
            FilePlacementOnSelectedDataServers(selectedDataServersList);
            return fileMetadata;
        }

        public void Delete(string filename)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("DELETE METADATA FILE " + filename);

            if (!HasFile(filename))
                throw new FileDoesNotExistException(filename);

            //Missing: verificar se o ficheiro está a ser utilizado por outro cliente? 
            //Tratar o caso em que possivelmente algum dos servidores não conseguiu apagar o ficheiro
            if (FileDeletionOnCorrespondingDataServers(fileMetadataTable[filename].DataServers))
                fileMetadataTable.Remove(filename);
        }

        /**
         * IMetadataToDataServer Methods
         */

        public Heartbeat Heartbeat()
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("HEARTBEAT");
            return null;
        }

        public void RegisterDataServer(string name, string location)
        {
            if (fail) throw new ProcessDownException(id);

            Console.WriteLine("REGISTER DATA SERVER " + name);

            if (!dataServers.ContainsKey(name))
            {
                IDataServerToMetadata data = (IDataServerToMetadata)Activator.GetObject(
                    typeof(IDataServerToMetadata),
                    location);
                dataServers.Add(name, data);
            }
        }
    }
}
