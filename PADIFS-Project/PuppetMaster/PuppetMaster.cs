using SharedLibrary;
using SharedLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

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
                try
                {
                    console.Kill();
                }
                catch (Exception) { }
            }
        }

        public static void Reset()
        {
            KillConsoles();
            consoles.Clear();
            processes.Clear();
            metadataLocations.Clear();
            portsUsed.Clear();
            port = 1;
        }

        private static void SendMetadataLocations(IProcessToPM process)
        {
            List<Thread> requests = new List<Thread>();

            foreach (var entry in metadataLocations)
            {
                string id = entry.Key;
                string location = entry.Value;

                Thread metadataLocation = new Thread(() => process.MetadataLocation(id, location));
                requests.Add(metadataLocation);
                metadataLocation.Start();
            }

            foreach (Thread request in requests)
            {
                request.Join();
            }
        }

        private static void NewMetadataLocationToProcesses(string id, string location)
        {
            List<Thread> requests = new List<Thread>();

            foreach (var entry in processes)
            {
                IProcessToPM process = entry.Value;
                if (id.First() == 'm')
                {
                    process.MetadataLocation(id, location);
                }
                else
                {
                    Thread request = new Thread(() => process.MetadataLocation(id, location));
                    requests.Add(request);
                }
            }

            // sends to all non-metadata the new metadata in broadcast
            foreach (Thread request in requests)
            {
                request.Start();
            }
            foreach (Thread request in requests)
            {
                request.Join();
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
            metadataLocations[id] = location;
            processes[id] = metadata;
        }

        public static void StartDataServer(string id, int port)
        {
            consoles.Add(Process.Start("DataServer.exe", id + " " + port));
            IDataServerToPM dataServer = (IDataServerToPM)Activator.GetObject(
                typeof(IDataServerToPM),
                Helper.GetUrl(id, port)
            );
            processes[id] = dataServer;
            SendMetadataLocations(dataServer);
        }

        public static void StartClient(string id, int port)
        {
            consoles.Add(Process.Start("Client.exe", id + " " + port));
            IClientToPM client = (IClientToPM)Activator.GetObject(
                typeof(IClientToPM),
                Helper.GetUrl(id, port)
            );
            processes[id] = client;
            SendMetadataLocations(client);
        }

        private static IProcessToPM GetProcess(string id)
        {
            if (!processes.ContainsKey(id))
            {
                Thread start = null;
                switch (id.First())
                {
                    default: break;
                    case 'm': start = new Thread(() => StartMetadata(id, port++)); break;
                    case 'd': start = new Thread(() => StartDataServer(id, port++)); break;
                    case 'c': start = new Thread(() => StartClient(id, port++)); break;
                }
                start.Start();
                start.Join();
            }

            return processes[id];
        }

        public static void CloseFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            Thread close = new Thread(() => client.Close(filename));
            close.Start();
        }

        public static void DeleteFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            Thread delete = new Thread(() => client.Delete(filename));
            delete.Start();
        }

        public static void OpenFile(string id, string filename)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            Thread open = new Thread(() => client.Open(filename));
            open.Start();
        }

        public static void CreateFile(string id, string filename, int nbData, int readq, int writeq)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            Thread create = new Thread(() => client.Create(filename, nbData, readq, writeq));
            create.Start();
        }

        public static void FailProcess(string id)
        {
            IServerToPM server = (IServerToPM)GetProcess(id);
            Thread fail = new Thread(() => server.Fail());
            fail.Start();
        }

        public static void RecoverProcess(string id)
        {
            IServerToPM server = (IServerToPM)GetProcess(id);
            Thread recover = new Thread(() => server.Recover());
            recover.Start();
        }

        public static void UnfreezeProcess(string id)
        {
            IDataServerToPM server = (IDataServerToPM)GetProcess(id);
            Thread unfreeze = new Thread(() => server.Unfreeze());
            unfreeze.Start();
        }

        public static void FreezeProcess(string id)
        {
            IDataServerToPM server = (IDataServerToPM)GetProcess(id);
            Thread freeze = new Thread(() => server.Freeze());
            freeze.Start();
        }

        public static void ReadFile(string id, int fileRegister, string semantics, int byteRegister)
        {
            Helper.Semantics semanticsId = getSemanticsId(semantics);
            IClientToPM client = (IClientToPM)GetProcess(id);
            Thread read = new Thread(() => client.Read(fileRegister, semanticsId, byteRegister));
            read.Start();
        }

        public static void WriteFile(string id, int fileRegister, int byteRegister)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            Thread write = new Thread(() => client.Write(fileRegister, byteRegister));
            write.Start();
        }

        public static void WriteFile(string id, int fileRegister, string contents)
        {
            IClientToPM client = (IClientToPM)GetProcess(id);
            Thread write = new Thread(() => client.Write(fileRegister, Helper.StringToBytes(contents)));
            write.Start();
        }

        public static void CopyFile(string id, int fileRegister1, string semantics, int fileRegister2, string salt)
        {
            Helper.Semantics semanticsId = getSemanticsId(semantics);
            IClientToPM client = (IClientToPM)GetProcess(id);
            Thread copy = new Thread(() => client.Copy(fileRegister1, semanticsId, fileRegister2, Helper.StringToBytes(salt)));
            copy.Start();
        }

        public static void DumpProcess(string id)
        {
            IProcessToPM process = (IProcessToPM)GetProcess(id);
            Thread dump = new Thread(() => process.Dump());
            dump.Start();
        }

        private static Helper.Semantics getSemanticsId(string semantics)
        {
            return (semantics.Equals("monotonic")) ? Helper.Semantics.MONOTONIC : Helper.Semantics.DEFAULT;
        }
    }
}
