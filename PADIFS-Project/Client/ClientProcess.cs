using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Client
{
    public class ClientProcess : MarshalByRefObject, IClientPM
    {
        private static Dictionary<string, FileMetadata> openedFilesMetadata;
        
        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception();

            TcpChannel channel = new TcpChannel(Convert.ToInt32(args[1]));
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ClientProcess),
                args[0],
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Client " + args[0] + " Started");

            openedFilesMetadata = new Dictionary<string, FileMetadata>(Config.MAX_FILE_REGISTERS);

            // Missing: Notify Metadata Server

            System.Console.ReadLine();
        }

        public void Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum)
        {
            // HARD CODED TEST
            System.Console.WriteLine("CREATE CLIENT FILE");
            try
            {
                if (!openedFilesMetadata.ContainsKey(fileName))
                {
                    IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
                    FileMetadata file = metadata.Create(fileName, nbDataServers, readQuorum, writeQuorum);
                    openedFilesMetadata.Add(fileName, file);
                }
                else
                {
                    //Recheck: Exception??
                    Console.WriteLine("File " + fileName + " was already created.");
                }
            }
            catch (FileAlreadyExistsException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Open(string fileName)
        {
            // HARD CODED TEST
            System.Console.WriteLine("OPEN CLIENT FILE");
            try
            {
                if (!openedFilesMetadata.ContainsKey(fileName))
                {
                    IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
                    FileMetadata file = metadata.Open(fileName);
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
            // HARD CODED TEST
            System.Console.WriteLine("CLOSE CLIENT FILE");
            try
            {
                if (openedFilesMetadata.ContainsKey(fileName))
                {
                    IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
                    metadata.Close(fileName);
                    openedFilesMetadata.Remove(fileName);
                }
                else
                {
                    //Recheck: Exception??
                    Console.WriteLine("File " + fileName + " is not opened.");
                }
            }
            catch (FileDoesNotExistException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Delete(string fileName)
        {
            // HARD CODED TEST
            System.Console.WriteLine("DELETE CLIENT FILE");
            try
            {
                if (!openedFilesMetadata.ContainsKey(fileName))
                {
                    IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
                    metadata.Delete(fileName);
                    openedFilesMetadata.Remove(fileName);
                }
                else
                {
                    //Recheck: Exception??
                    Console.WriteLine("File " + fileName + " is opened. Please close the file first.");
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
            IDataServer data = (IDataServer)Activator.GetObject(typeof(IDataServer), "tcp://localhost:9/d-1");
            data.Read();
        }

        public void Write()
        {
            // HARD CODED TEST
            System.Console.WriteLine("WRITE CLIENT FILE");
            IDataServer data = (IDataServer)Activator.GetObject(typeof(IDataServer), "tcp://localhost:9/d-1");
            data.Write();
        }
    }
}
