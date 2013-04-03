using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;

namespace SharedLibrary
{
    class MetadataServerProcess : MarshalByRefObject, IMetadataServerToClient, IServerToPM, IMetadataServerToDataServer
    {
        private static string metadataStartedTemplate = "Metadata Server {0} has started."; 
        private static Dictionary<string, FileMetadata> fileMetadataTable = new Dictionary<string, FileMetadata>();

        public bool hasFile(string fileName) {
            return fileMetadataTable.ContainsKey(fileName);
        }
        
        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong Arguments");

            TcpChannel channel = new TcpChannel(Convert.ToInt32(args[1]));
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MetadataServerProcess),
                args[0],
                WellKnownObjectMode.Singleton);

            Console.WriteLine(string.Format(metadataStartedTemplate, args[0]));

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

        public FileMetadata Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("CREATE METADATA FILE");
            Console.WriteLine("FILENAME: " + fileName + " NBDATASERVERS: " + nbDataServers + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);

            if (hasFile(fileName))
                throw new FileAlreadyExistsException(fileName);

            //Missing: selecionar data servers
            List<string> dataServersList = new List<string>();
            FileMetadata fileMetadata = new FileMetadata(fileName, nbDataServers, readQuorum, writeQuorum, dataServersList);
            fileMetadataTable.Add(fileName, fileMetadata);

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
            return true;
        }

        public Heartbeat Heartbeat()
        {
            Console.WriteLine("HEARTBEAT");
            return null;
        }

        public bool RegisterDataServer(string dataServerName)
        {
            Console.WriteLine("REGISTER DATA SERVER " + dataServerName);
            return true;
        }
    }   
}
