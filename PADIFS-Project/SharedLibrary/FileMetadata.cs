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
        private List<string> dataServers;

        public FileMetadata(string fileName, int nbDataServers, int readQuorum, int writeQuorum, List<string> dataServers) {
            this.fileName = fileName;
            this.nbDataServers = nbDataServers;
            this.readQuorum = readQuorum;
            this.writeQuorum = writeQuorum;
            this.dataServers = dataServers;
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

        public List<string> DataServers
        {
            get { return this.dataServers; }
            set { this.dataServers = value; }
        }

        public void addDataServer(string dataServerName) {
            this.dataServers.Add(dataServerName);
        }

        public void removeDataServer(string dataServerName)
        {
            this.dataServers.Remove(dataServerName);
        }

        public bool hasDataServer(string dataServerName)
        {
            return this.dataServers.Contains(dataServerName);
        }

    }
}
