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

            System.Console.ReadLine();
        }

        public void create()
        {
            // HARD CODED TEST
            System.Console.WriteLine("CREATE CLIENT FILE");
            IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
            metadata.create();
        }

        public void open()
        {
            // HARD CODED TEST
            System.Console.WriteLine("OPEN CLIENT FILE");
            IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
            metadata.open();
        }

        public void close()
        {
            // HARD CODED TEST
            System.Console.WriteLine("CLOSE CLIENT FILE");
            IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
            metadata.close();
        }

        public void delete()
        {
            // HARD CODED TEST
            System.Console.WriteLine("DELETE CLIENT FILE");
            IMetadataServer metadata = (IMetadataServer)Activator.GetObject(typeof(IMetadataServer), "tcp://localhost:1/m-1");
            metadata.delete();
        }

        public void read()
        {
            // HARD CODED TEST
            System.Console.WriteLine("READ CLIENT FILE");
            IDataServer data = (IDataServer)Activator.GetObject(typeof(IDataServer), "tcp://localhost:9/d-1");
            data.read();
        }

        public void write()
        {
            // HARD CODED TEST
            System.Console.WriteLine("WRITE CLIENT FILE");
            IDataServer data = (IDataServer)Activator.GetObject(typeof(IDataServer), "tcp://localhost:9/d-1");
            data.write();
        }

    }
}
