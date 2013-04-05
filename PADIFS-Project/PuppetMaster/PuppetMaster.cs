using SharedLibrary;
using SharedLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetMaster
    {
        private static Hashtable processes = new Hashtable();
        private static List<string> metadataServersLocation = new List<string>();
        private static List<string> portsUsed = new List<string>();

        public static bool idUsed(string id)
        {
            return processes.ContainsKey(id);
        }
        public static bool portUsed(string port)
        {
            return portsUsed.Contains(port);
        }

        public static void StartMetadata(string id, string port)
        {
            Process.Start("MetadataServer.exe", id + " " + port);
            string urlLocation = Helper.getUrlTemplate(id, port);
            IServerToPM metadata = (IServerToPM)Activator.GetObject(typeof(IServerToPM), urlLocation);
            processes.Add(id, metadata);
            metadataServersLocation.Add(urlLocation);
        }

        public static void StartDataServer(string id, string port)
        {
            Process.Start("DataServer.exe", id + " " + port);
            IDataServerToPM dataServer = (IDataServerToPM) Activator.GetObject(
                typeof(IDataServerToPM), 
                Helper.getUrlTemplate(port, id)
            );
            processes.Add(id, dataServer);
            dataServer.ReceiveMetadataServersLocations(metadataServersLocation);
        }

        public static void StartClient(string id, string port)
        {
            Process.Start("Client.exe", id + " " + port);
            IClientToPM client = (IClientToPM)Activator.GetObject(
                typeof(IClientToPM),
                Helper.getUrlTemplate(port, id)
            );
            processes.Add(id, client);
            client.ReceiveMetadataServersLocations(metadataServersLocation);
        }
    }
}
