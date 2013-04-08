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
        // data Server id / local filename
        private Dictionary<string, string> localFilenames;
        // data Server id / locations
        private Dictionary<string, string> locations;


        public FileMetadata(string filename, int nbDataServers, int readQuorum, int writeQuorum, 
            Dictionary<string, string> localFilenames, Dictionary<string, string> locations)
        {
            this.filename = filename;
            this.nbDataServers = nbDataServers;
            this.readQuorum = readQuorum;
            this.writeQuorum = writeQuorum;
            this.localFilenames = localFilenames;
            this.locations = locations;
        }

        public string Filename
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

        public Dictionary<string, string> LocalFilenames
        {
            get { return this.localFilenames; }
            set { this.localFilenames = value; }
        }

        public Dictionary<string, string> Locations
        {
            get { return this.locations; }
            set { this.locations = value; }
        }

        public override string ToString()
        {
            string dataServers = "[";
            foreach (var entry in localFilenames)
            {
                string id = entry.Key;
                string localFilename = entry.Value;
                dataServers += " (" + id + "," + localFilename + ")";
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
