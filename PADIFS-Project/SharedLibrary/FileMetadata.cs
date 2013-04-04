using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    [Serializable]
    public class FileMetadata
    {
        private string fileName;
        private int nbDataServers;
        private int readQuorum;
        private int writeQuorum;
        private Dictionary<string,string> dataServersLocalFiles;

        public FileMetadata(string fileName, int nbDataServers, int readQuorum, int writeQuorum, Dictionary<string, string> dataServersLocalFiles)
        {
            this.fileName = fileName;
            this.nbDataServers = nbDataServers;
            this.readQuorum = readQuorum;
            this.writeQuorum = writeQuorum;
            this.dataServersLocalFiles = dataServersLocalFiles;
        }

        public string FileName
        {
            get { return this.fileName;  }
            set { this.fileName = value; }
        }

        public int NbDataServers
        {
            get { return this.nbDataServers; }
            set { this.nbDataServers = value; }
        }

        public int ReadQuorum
        {
            get { return this.readQuorum; }
            set { this.readQuorum = value; }
        }

        public int WriteQuorum
        {
            get { return this.writeQuorum; }
            set { this.writeQuorum = value; }
        }

        public Dictionary<string, string> DataServers
        {
            get { return this.dataServersLocalFiles; }
            set { this.dataServersLocalFiles = value; }
        }

        public void addDataServer(string dataServerName,string localFileName) {
            this.dataServersLocalFiles.Add(dataServerName,localFileName);
        }

        public void removeDataServer(string dataServerName)
        {
            this.dataServersLocalFiles.Remove(dataServerName);
        }

        public bool hasDataServer(string dataServerName)
        {
            return this.dataServersLocalFiles.ContainsKey(dataServerName);
        }

    }
}
