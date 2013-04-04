using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using SharedLibrary.Exceptions;
using SharedLibrary.Interfaces;

namespace DataServer
{
    public class DataServerProcess : MarshalByRefObject, IDataServerToClient, IDataServerToPM, IDataServerToMetadataServer
    {
        private static string dataServerStartedTemplate = "Data Server {0} has started.";
        private static List<Tuple<IMetadataServerToDataServer, string>> metadataServers = new List<Tuple<IMetadataServerToDataServer, string>>();
        private static string dataServerName;
        private static int dataServerPort;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception("Wrong Arguments");

            dataServerName = args[0];
            dataServerPort = Convert.ToInt32(args[1]);

            TcpChannel channel = new TcpChannel(dataServerPort);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(DataServer.DataServerProcess),
                dataServerName,
                WellKnownObjectMode.Singleton);

            Console.WriteLine(string.Format(dataServerStartedTemplate, dataServerName));

            System.Console.ReadLine();
        }

        public void Freeze()
        {
            Console.WriteLine("FREEZE DATA SERVER " + dataServerName);
        }

        public void Unfreeze()
        {
            Console.WriteLine("UNFREEZE DATA SERVER " + dataServerName);
        }

        public void Fail()
        {
            Console.WriteLine("FAIL DATA SERVER " + dataServerName);
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER DATA SERVER " + dataServerName);
        }

        public void Read()
        {
            Console.WriteLine("READ DATA SERVER FILE " + dataServerName);
        }

        public void Write()
        {
            Console.WriteLine("WRITE DATA SERVER FILE  " + dataServerName);
        }

        public void CreateFile(string fileName)
        {
            Console.WriteLine("CREATE FILE " + fileName);
        }

        public void DeleteFile(string fileName)
        {
            Console.WriteLine("DELETE FILE " + fileName);
        }


        public void ReceiveMetadataServersLocations(List<string> metadataServerList)
        {
            for (int i = 0; i < metadataServerList.Count; i++)
            {   
                string urlLocation = metadataServerList.ElementAt(i);
                IMetadataServerToDataServer metadata = (IMetadataServerToDataServer)Activator.GetObject(typeof(IMetadataServerToDataServer), urlLocation);
                metadataServers.Add(Tuple.Create(metadata, urlLocation));
            }

            //Notify Primary Metadata Server
            Tuple<IMetadataServerToDataServer,string> metadataServerTuple = metadataServers.First();
            if (!metadataServerTuple.Item1.RegisterDataServer(dataServerName, string.Format(Config.URL,dataServerPort ,dataServerName)))
                throw new CouldNotRegistOnMetadataServer(dataServerName);
        }
    }
}
