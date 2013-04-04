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

namespace SharedLibrary
{
    class MetadataServerProcess : MarshalByRefObject, IMetadataServerToClient, IServerToPM, IMetadataServerToDataServer
    {
        private static string metadataStartedTemplate = "Metadata Server {0} has started.";
        private static Dictionary<string, FileMetadata> fileMetadataTable = new Dictionary<string, FileMetadata>();
        private static Dictionary<string, IDataServerToMetadataServer> dataServers = new Dictionary<string, IDataServerToMetadataServer>();
        private static List<string> clients;
        private static string metadataServerName;
        private static int metadataServerPort;

        public bool hasFile(string fileName)
        {
            return fileMetadataTable.ContainsKey(fileName);
        }

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

        public FileMetadata Open(string fileName)
        {
            Console.WriteLine("OPEN METADATA FILE " + fileName);

            if (!hasFile(fileName))
                throw new FileDoesNotExistException(fileName);

            return fileMetadataTable[fileName];
        }

        // recheck
        public void Close(string fileName)
        {
            Console.WriteLine("CLOSE METADATA FILE " + fileName);

            if (!hasFile(fileName))
                throw new FileDoesNotExistException(fileName);
        }

        public Dictionary<string, string> selectDataServersForFilePlacement(int nbDataServers, string localFileName)
        {
            int actualNrDataServers = dataServers.Count;

            if (nbDataServers > actualNrDataServers)
                throw new NotEnoughDataServersException(nbDataServers, actualNrDataServers);

            Dictionary<string, string> selectedDataServers = new Dictionary<string, string>();
            for (int i = 0; i < nbDataServers; i++)
            {
                Random random = new Random();
                int ran = random.Next(actualNrDataServers);
                selectedDataServers.Add(dataServers.ElementAt(ran).Key, localFileName);
            }

            return selectedDataServers;
        }

        public bool filePlacementOnSelectedDataServers(Dictionary<string, string> dataServersAndLocalFileList)
        {
            //Async request?
            for (int i = 0; i < dataServersAndLocalFileList.Count; i++)
            {
                string dataServerName = dataServersAndLocalFileList.ElementAt(i).Key;
                string localFileName = dataServersAndLocalFileList.ElementAt(i).Value;

                dataServers[dataServerName].Create(localFileName);
            }

            //exception Handler
            return true;
        }


        public FileMetadata Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("CREATE METADATA FILE");
            Console.WriteLine("FILENAME: " + fileName + " NBDATASERVERS: " + nbDataServers + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            if (hasFile(fileName))
                throw new FileAlreadyExistsException(fileName);

            //Select Data Servers For File Placement

            //Missing: create localFileName
            String localFileName = fileName;

            Dictionary<string, string> selectedDataServersList = selectDataServersForFilePlacement(nbDataServers, localFileName);
            FileMetadata fileMetadata = new FileMetadata(fileName, nbDataServers, readQuorum, writeQuorum, selectedDataServersList);
            fileMetadataTable.Add(fileName, fileMetadata);

            filePlacementOnSelectedDataServers(selectedDataServersList);

            return fileMetadata;
        }

        public void Delete(string fileName)
        {
            Console.WriteLine("DELETE METADATA FILE " + fileName);

            if (!hasFile(fileName))
                throw new FileDoesNotExistException(fileName);

            //data servers updates (delete files)
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

            IDataServerToMetadataServer metadata = (IDataServerToMetadataServer)Activator.GetObject(typeof(IDataServerToMetadataServer), urlLocation);
            dataServers.Add(dataServerName, metadata);

            return true;
        }
    }
}
