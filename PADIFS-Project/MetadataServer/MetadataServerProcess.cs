using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace MetadataServer
{
    class MetadataServerProcess : MarshalByRefObject, IMetadataServer, IServerPM
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception();

            TcpChannel channel = new TcpChannel(Convert.ToInt32(args[1]));
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MetadataServerProcess),
                args[0],
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Metadata Server " + args[0] + " Started");

            System.Console.ReadLine();
        }


        public void open()
        {
            Console.WriteLine("OPEN METADATA FILE");
        }

        public void close()
        {
            Console.WriteLine("CLOSE METADATA FILE");
        }

        public void create()
        {
            Console.WriteLine("CREATE METADATA FILE");
        }

        public void delete()
        {
            Console.WriteLine("DELETE METADATA FILE");
        }

        public void fail()
        {
            Console.WriteLine("FAIL METADATA");
        }

        public void recover()
        {
            Console.WriteLine("RECOVER METADATA");
        }
    }
}
