using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using SharedLibrary.Exceptions;
using SharedLibrary.Entities;
using SharedLibrary.Interfaces;

namespace Client
{
    public class ClientProcess : MarshalByRefObject, IClientToPM
    {
        private static string clientStartedTemplate = "Client {0} has started.";
        private static string fileAlreadyCreatedTemplate = "File {0} was already created.";
        private static string fileNotOpenedTemplate = "File {0} is not opened.";
        private static string fileIsOpenedTemplate = "File {0} is opened. Please close the file first.";
        private static Dictionary<string, FileMetadata> openedFilesMetadata;
        private static List<IMetadataServerToClient> metadataServers;
        private static string clientName;
        private static int clientPort;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong arguments");

            clientName = args[0];
            clientPort = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(clientPort);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ClientProcess),
                clientName,
                WellKnownObjectMode.Singleton);

            Console.WriteLine(string.Format(clientStartedTemplate,clientName));

            openedFilesMetadata = new Dictionary<string, FileMetadata>(Config.MAX_FILE_REGISTERS);
            metadataServers = new List<IMetadataServerToClient>();

            System.Console.ReadLine();
        }

        public void ReceiveMetadataServersLocations(List<string> metadataServerList)
        {
            for (int i = 0; i < metadataServerList.Count; i++)
            {
                IMetadataServerToClient metadata = (IMetadataServerToClient)Activator.GetObject(typeof(IMetadataServerToClient), metadataServerList.ElementAt(i));
                metadataServers.Add(metadata);
            }

            //Notify Primary Metadata Server
            if (!metadataServers.First().RegisterClient(clientName))
                throw new CouldNotRegistOnMetadataServer(clientName);
        }

        public void Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum)
        {
            System.Console.WriteLine("CREATE CLIENT FILE " + fileName);
            try
            {
                if (!openedFilesMetadata.ContainsKey(fileName))
                {
                    FileMetadata file = metadataServers.First().Create(fileName, nbDataServers, readQuorum, writeQuorum);
                    openedFilesMetadata.Add(fileName, file);
                }
                else
                {
                    //Recheck: Exception??
                    Console.WriteLine(string.Format(fileAlreadyCreatedTemplate, fileName));
                }
            }
            catch (FileAlreadyExistsException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (NotEnoughDataServersException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Open(string fileName)
        {
            System.Console.WriteLine("OPEN CLIENT FILE " + fileName);
            try
            {
                if (!openedFilesMetadata.ContainsKey(fileName))
                {
                    FileMetadata file = metadataServers.First().Open(fileName);
                    openedFilesMetadata.Add(fileName, file);
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Close(string fileName)
        {
            System.Console.WriteLine("CLOSE CLIENT FILE " + fileName);
            try
            {
                if (openedFilesMetadata.ContainsKey(fileName))
                {
                    metadataServers.First().Close(fileName);
                    openedFilesMetadata.Remove(fileName);
                }
                else
                {
                    //Recheck: Exception??
                    Console.WriteLine(string.Format(fileNotOpenedTemplate, fileName));
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Delete(string fileName)
        {
            System.Console.WriteLine("DELETE CLIENT FILE " + fileName);
            try
            {
                if (!openedFilesMetadata.ContainsKey(fileName))
                {
                    metadataServers.First().Delete(fileName);
                    openedFilesMetadata.Remove(fileName);
                }
                else
                {
                    //Recheck: Exception??
                    Console.WriteLine(string.Format(fileIsOpenedTemplate, fileName));
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Read()
        {
            // HARD CODED TEST
            System.Console.WriteLine("READ CLIENT FILE");
            IDataServerToClient data = (IDataServerToClient)Activator.GetObject(typeof(IDataServerToClient), "tcp://localhost:9/d-1");
            data.Read();
        }

        public void Write()
        {
            // HARD CODED TEST
            System.Console.WriteLine("WRITE CLIENT FILE");
            IDataServerToClient data = (IDataServerToClient)Activator.GetObject(typeof(IDataServerToClient), "tcp://localhost:9/d-1");
            data.Write();
        }
    }
}
