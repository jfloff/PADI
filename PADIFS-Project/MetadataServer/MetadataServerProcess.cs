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


        public void Open(string fileName)
        {
            Console.WriteLine("OPEN METADATA FILE");
            Console.WriteLine("FILENAME " + fileName);
        }

        public void Close(string fileName)
        {
            Console.WriteLine("CLOSE METADATA FILE");
            Console.WriteLine("FILENAME " + fileName);
        }

        public void Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("CREATE METADATA FILE");
            Console.WriteLine("FILENAME: " + fileName + " NBDATASERVERS: " + nbDataServers + " READQUORUM: " + readQuorum + " WRITEQUORUM: " + writeQuorum);
        }

        public void Delete(string fileName)
        {
            Console.WriteLine("DELETE METADATA FILE");
            Console.WriteLine("FILENAME " + fileName);
        }

        public void Fail()
        {
            Console.WriteLine("FAIL METADATA");
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER METADATA");
        }


        public void RegisterClient()
        {
            Console.WriteLine("REGISTER CLIENT");
        }

        public void RegisterDataServer()
        {
            Console.WriteLine("REGISTER DATA SERVER");
        }
    }   
}
