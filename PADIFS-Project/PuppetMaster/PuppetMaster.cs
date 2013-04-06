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
        private static int port = 1;

        private static Dictionary<string, IProcessToPM> processes = new Dictionary<string, IProcessToPM>();
        private static List<string> metadataServersLocation = new List<string>();
        private static List<int> portsUsed = new List<int>();

        public static void StartMetadata(string id, int port)
        {
            Process.Start("MetadataServer.exe", id + " " + port);
            string urlLocation = Helper.GetUrlTemplate(id, port);
            IMetadataServerToPM metadata = (IMetadataServerToPM)Activator.GetObject(typeof(IMetadataServerToPM), urlLocation);
            processes.Add(id, metadata);
            metadataServersLocation.Add(urlLocation);
            metadata.SetPrimaryMetadata((metadataServersLocation.Count == 0));
        }

        public static void StartDataServer(string id, int port)
        {
            Process.Start("DataServer.exe", id + " " + port);
            IDataServerToPM dataServer = (IDataServerToPM)Activator.GetObject(
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

        private static IProcessToPM GetProcess(string id)
        {
            if (!processes.ContainsKey(id))
            {
                switch (id.First())
                {
                    default: break;
                    case 'm': StartMetadata(id, port++); break;
                    case 'd': StartDataServer(id, port++); break;
                    case 'c': StartClient(id, port++); break;
                }
            }

            return processes[id];
        }

        public static void CloseFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            client.Close(filename);
        }

        public static void DeleteFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            client.Delete(filename);
        }

        public static void OpenFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            client.Open(filename);
        }

        public static void CreateFile(string id, string filename, int nbData, int readq, int writeq)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            client.Create(filename, nbData, readq, writeq);
        }

        public static void FailProcess(string id)
        {
            IServerToPM server = (IServerToPM)GetProcess(id);
            server.Fail();
        }

        public static void RecoverProcess(string id)
        {
            IServerToPM server = (IServerToPM)GetProcess(id);
            server.Fail();
        }

        public static void UnfreezeProcess(string id)
        {
            IDataServerToPM server = (IDataServerToPM)GetProcess(id);
            server.Unfreeze();
        }

        public static void FreezeProcess(string id)
        {
            IDataServerToPM server = (IDataServerToPM)GetProcess(id);
            server.Freeze();
        }

        public static void ReadFile(string id, int fileRegister, string semantic, int stringRegister)
        {
            int semanticId = getSemanticId(semantic);
        }

        public static void WriteFile(string id, int fileRegister, int byteArrayRegister)
        {
            if (!processes.ContainsKey(id)) StartClient(id, port++);
        }

        public static void WriteFile(string id, int fileRegister, string contents)
        {
            if (!processes.ContainsKey(id)) StartClient(id, port++);
        }

        public static void CopyFile(string id, int fileRegister1, string semantic, int fileRegister2, string salt)
        {
            int semanticId = getSemanticId(semantic);
        }

        public static void DumpProcess(string id)
        {
            IProcessToPM process = (IProcessToPM)GetProcess(id);
            process.Dump();
        }

        private static int getSemanticId(string semantic)
        {
            return (semantic.Equals("monotonic")) ? Helper.MONOTONIC : Helper.DEFAULT;
        }
    }
}
