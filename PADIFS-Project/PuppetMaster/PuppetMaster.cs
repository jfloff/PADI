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
        private static List<int> portsUsed = new List<int>();

        public static bool idUsed(string id)
        {
            return processes.ContainsKey(id);
        }
        public static bool portUsed(int port)
        {
            return portsUsed.Contains(port);
        }

        public static void StartMetadata(string id, int port)
        {
            Process.Start("MetadataServer.exe", id + " " + port);
            string urlLocation = Helper.GetUrlTemplate(id, port);
            IServerToPM metadata = (IServerToPM)Activator.GetObject(typeof(IServerToPM), urlLocation);
            processes.Add(id, metadata);
            metadataServersLocation.Add(urlLocation);
            metadata.SetPrimaryMetadata((metadataServersLocation.Count == 0));
        }

        public static void StartDataServer(string id, int port)
        {
            Process.Start("DataServer.exe", id + " " + port);
            IDataServerToPM dataServer = (IDataServerToPM) Activator.GetObject(
                typeof(IDataServerToPM),
                Helper.GetUrlTemplate(id, port)
            );
            processes.Add(id, dataServer);
            dataServer.ReceiveMetadataServersLocations(metadataServersLocation);
        }

        public static void StartClient(string id, int port)
        {
            Process.Start("Client.exe", id + " " + port);
            IClientToPM client = (IClientToPM)Activator.GetObject(
                typeof(IClientToPM),
                Helper.GetUrlTemplate(id, port)
            );
            processes.Add(id, client);
            client.ReceiveMetadataServersLocations(metadataServersLocation);
        }

        public static void CloseFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM)processes[id];
            client.Close(filename);
        }

        public static void DeleteFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM)  processes[id];
            client.Delete(filename);
        }

        public static void OpenFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM) processes[id];
            client.Open(filename);
        }

        public static void CreateFile(string id, string filename, int nbData, int readq, int writeq)
        {
            IClientToPM client = (IClientToPM) processes[id];
            client.Create(filename, nbData, readq, writeq);
        }

        public static void FailProcess(string id)
        {
            IServerToPM server = (IServerToPM) processes[id];
            server.Fail();
        }

        public static void RecoverProcess(string id)
        {
            IServerToPM server = (IServerToPM)processes[id];
            server.Fail();
        }

        public static void UnfreezeProcess(string id)
        {
             IDataServerToPM server = (IDataServerToPM) processes[id];
             server.Unfreeze();
        }

        public static void FreezeProcess(string id)
        {
            IDataServerToPM server = (IDataServerToPM) processes[id];
            server.Freeze();
        }
    }
}
