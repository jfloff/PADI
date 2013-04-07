using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class FileMetadata
    {
        private string filename;
        private int nbDataServers;
        private int readQuorum;
        private int writeQuorum;
        // dataServer id / local filename
        private Dictionary<string,string> dataServersLocalFiles;

        public FileMetadata(string filename, int nbDataServers, int readQuorum, int writeQuorum, Dictionary<string, string> dataServersLocalFiles)
        {
            this.filename = filename;
            this.nbDataServers = nbDataServers;
            this.readQuorum = readQuorum;
            this.writeQuorum = writeQuorum;
            this.dataServersLocalFiles = dataServersLocalFiles;
        }

        public string FileName
        {
            get { return this.filename;  }
            set { this.filename = value; }
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

        public string ToString()
        {
            string dataServers = "[";
            foreach (var entry in dataServersLocalFiles)
            {
                string dataServerId = entry.Key;
                string localFilename = entry.Value;
                dataServers += " (" + dataServerId + "," + localFilename + ")";
            }
            dataServers += " ]";

            // missing dataServersLocalFiles
            return "Filename = " + this.filename + " : "
                + "NbDataServer = " + this.nbDataServers + " : "
                + "Read Quorum = " + this.readQuorum + " : "
                + "Write Quorum = " + this.writeQuorum + " : "
                + "Data Servers = " + dataServers;
        }
    }
}
