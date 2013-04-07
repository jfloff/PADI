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

        private static string primary = id;
        private static string id;


        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong Arguments");

            Console.SetWindowSize(Helper.WINDOW_WIDTH, Helper.WINDOW_HEIGHT);

            id = args[0];
            int port = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Metadata),
                id,
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Metadata Server " + id + " has started.");
            Console.ReadLine();
        }

        private bool ImPrimary()
        {
            return (primary == id);
        }

        private void FindPrimary()
        {
            foreach (var entry in metadatas)
            {
                string id = entry.Key;
                IMetadataToMetadata metadata = entry.Value;

                metadata.Ping();
                if (string.Compare(id, primary) < 0)
                {
                    primary = id;
                }
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
        }

        public void Fail()
        {
            Console.WriteLine("FAIL");
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER");
        }

        /**
         * IMetadataToMetadata Methods
         */

        public void Ping()
        {
            Console.WriteLine("PING");
        }

        /**
         * IMetadataToClient Methods
         */

        public FileMetadata Open(string fileName)
        {
            Console.WriteLine("OPEN METADATA FILE " + fileName);

            if (!HasFile(fileName))
                throw new FileDoesNotExistException(fileName);

            return fileMetadataTable[fileName];
        }

        // recheck
        public void Close(string fileName)
        {
            Console.WriteLine("CLOSE METADATA FILE " + fileName);

            if (!HasFile(fileName))
                throw new FileDoesNotExistException(fileName);
        }

        public FileMetadata Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("CREATE METADATA FILE");
            Console.WriteLine("FILENAME: " + fileName + " NBDATASERVERS: " + nbDataServers + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

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
            Console.WriteLine("HEARTBEAT");
            return null;
        }

        public void RegisterDataServer(string name, string location)
        {
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
