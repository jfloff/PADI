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

namespace MetadataServer
{
    class MetadataServerProcess : MarshalByRefObject, IMetadataServerToClient, IMetadataServerToPM, IMetadataServerToDataServer
    {
        private static string metadataStartedTemplate = "Metadata Server {0} has started.";
        private static Dictionary<string, FileMetadata> fileMetadataTable = new Dictionary<string, FileMetadata>();
        private static Dictionary<string, IDataServerToMetadataServer> dataServers = new Dictionary<string, IDataServerToMetadataServer>();
        private static Dictionary<string, IMetadataServerToMetadadataServer> metadataReplicas = new Dictionary<string, IMetadataServerToMetadadataServer>();
        private static List<string> clients = new List<string>();
        private static string metadataServerName;
        private static int metadataServerPort;
        private static bool iAmPrimary = false;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong Arguments");

            metadataServerName = args[0];
            metadataServerPort = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(metadataServerPort);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MetadataServerProcess),
                metadataServerName,
                WellKnownObjectMode.Singleton);

            Console.WriteLine(string.Format(metadataStartedTemplate, metadataServerName));

            System.Console.ReadLine();
        }

        public bool HasFile(string fileName)
        {
            return fileMetadataTable.ContainsKey(fileName);
        }

        public void SetPrimaryMetadata(bool primary)
        {
            Console.WriteLine("SET PRIMARY METADATA");
            iAmPrimary = primary;
        }

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
                IDataServerToMetadataServer dataServer = (IDataServerToMetadataServer)dataServers[dataServerName];
                dataServer.CreateFile(fileName);
            }
            return true;
        }

        public bool FileDeletionOnCorrespondingDataServers(Dictionary<string, string> dataServersAndLocalFileList) { 
             
            //Async request?
            for (int i = 0; i < dataServersAndLocalFileList.Count; i++)
            {
                string dataServerName = dataServersAndLocalFileList.ElementAt(i).Key;
                string fileName = dataServersAndLocalFileList.ElementAt(i).Value;
                IDataServerToMetadataServer dataServer = (IDataServerToMetadataServer)dataServers[dataServerName];
                dataServer.DeleteFile(fileName);
            }
            return true;
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

        public void Delete(string fileName)
        {
            Console.WriteLine("DELETE METADATA FILE " + fileName);

            if (!HasFile(fileName))
                throw new FileDoesNotExistException(fileName);

            //Missing: verificar se o ficheiro está a ser utilizado por outro cliente? 
            //Tratar o caso em que possivelmente algum dos servidores não conseguiu apagar o ficheiro
            if (FileDeletionOnCorrespondingDataServers(fileMetadataTable[fileName].DataServers))
                fileMetadataTable.Remove(fileName);
        }

        public void Fail()
        {
            Console.WriteLine("FAIL METADATA");
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER METADATA");
        }


        public bool RegisterClient(string clientName)
        {
            Console.WriteLine("REGISTER CLIENT " + clientName);

            if (clients.Contains(clientName))
                return false;

            clients.Add(clientName);

            return true;
        }

        public Heartbeat Heartbeat()
        {
            Console.WriteLine("HEARTBEAT");
            return null;
        }

        public bool RegisterDataServer(string dataServerName, string urlLocation)
        {
            Console.WriteLine("REGISTER DATA SERVER " + dataServerName);

            if (dataServers.ContainsKey(dataServerName))
                return false;

            IDataServerToMetadataServer data = (IDataServerToMetadataServer)Activator.GetObject(typeof(IDataServerToMetadataServer), urlLocation);
            dataServers.Add(dataServerName, data);
            return true;
        }

        public void Dump()
        {
            System.Console.WriteLine("DUMP PROCESS");
        }

        public void ReceiveMetadataServersLocations(List<string> metadataServerList)
        {
            System.Console.WriteLine("RECEIVE METADATA LOCATIONS");
        }
    }
}
