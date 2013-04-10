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
        private static Dictionary<string, string> metadataLocations = new Dictionary<string, string>();
        private static List<int> portsUsed = new List<int>();

        private static List<Process> consoles = new List<Process>();
        public static void KillConsoles()
        {
            foreach (Process console in consoles)
            {
                console.Kill();
            }
        }

        private static void SendMetadataLocations(IProcessToPM process)
        {
            foreach (var entry in metadataLocations)
            {
                string id = entry.Key;
                string location = entry.Value;

                process.MetadataLocation(id, location);
            }
        }

        private static void NewMetadataLocationToProcesses(string id, string location)
        {
            foreach (var entry in processes)
            {
                IProcessToPM process = entry.Value;
                process.MetadataLocation(id, location);
            }
        }

        public static void StartMetadata(string id, int port)
        {
            consoles.Add(Process.Start("Metadata.exe", id + " " + port));
            string location = Helper.GetUrl(id, port);
            IMetadataToPM metadata = (IMetadataToPM)Activator.GetObject(
                typeof(IMetadataToPM),
                location);
            SendMetadataLocations(metadata);
            NewMetadataLocationToProcesses(id, location);
            // keep as last line to avoid pings to self
            metadataLocations.Add(id, location);
            processes.Add(id, metadata);
        }

        public static void StartDataServer(string id, int port)
        {
            consoles.Add(Process.Start("DataServer.exe", id + " " + port));
            IDataServerToPM dataServer = (IDataServerToPM)Activator.GetObject(
                typeof(IDataServerToPM),
                Helper.GetUrl(id, port)
            );
            processes.Add(id, dataServer);
            SendMetadataLocations(dataServer);
        }
        
        public static void StartClient(string id, int port)
        {
            consoles.Add(Process.Start("Client.exe", id + " " + port));
            IClientToPM client = (IClientToPM)Activator.GetObject(
                typeof(IClientToPM),
                Helper.GetUrl(id, port)
            );
            processes.Add(id, client);
            SendMetadataLocations(client);
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
            server.Recover();
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

        public static void ReadFile(string id, int fileRegister, string semantics, int byteRegister)
        {
            Helper.Semantics semanticsId = getSemanticsId(semantics);

            IClientToPM client = (IClientToPM)GetProcess(id);
            client.Read(fileRegister, semanticsId, byteRegister);
        }

        public static void WriteFile(string id, int fileRegister, int byteRegister)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
        }

        public static void WriteFile(string id, int fileRegister, string contents)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            client.Write(fileRegister, Helper.StringToBytes(contents));
        }

        public static void CopyFile(string id, int fileRegister1, string semantics, int fileRegister2, string salt)
        {
            Helper.Semantics semanticsId = getSemanticsId(semantics);
        }

        public static void DumpProcess(string id)
        {
            IProcessToPM process = (IProcessToPM)GetProcess(id);
            process.Dump();
        }

        private static Helper.Semantics getSemanticsId(string semantics)
        {
            return (semantics.Equals("monotonic")) ? Helper.Semantics.MONOTONIC : Helper.Semantics.DEFAULT;
        }
    }
}
