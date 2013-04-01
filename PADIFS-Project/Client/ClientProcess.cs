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
    public class ClientProcess : MarshalByRefObject, IClient
    {
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

            // Notify Metadata Server

            System.Console.ReadLine();
        }

        public void Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum)
        {
            // HARD CODED TEST
            System.Console.WriteLine("CREATE CLIENT FILE");
            IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
            metadata.Create(fileName, nbDataServers, readQuorum, writeQuorum);
        }

        public void Open(string fileName)
        {
            // HARD CODED TEST
            System.Console.WriteLine("OPEN CLIENT FILE");
            IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
            metadata.Open(fileName);
        }

        public void Close(string fileName)
        {
            // HARD CODED TEST
            System.Console.WriteLine("CLOSE CLIENT FILE");
            IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
            metadata.Close(fileName);
        }

        public void Delete(string fileName)
        {
            // HARD CODED TEST
            System.Console.WriteLine("DELETE CLIENT FILE");
            IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
            metadata.Delete(fileName);
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
