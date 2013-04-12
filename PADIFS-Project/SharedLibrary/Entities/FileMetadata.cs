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
        private Dictionary<string, string> localFilenames = new Dictionary<string, string>();
        // data Server id / locations
        private Dictionary<string, string> locations = new Dictionary<string, string>();
        private int currentNbDataServers = 0;


        public FileMetadata(string filename, int nbDataServers, int readQuorum, int writeQuorum)
        {
            this.filename = filename;
            this.nbDataServers = nbDataServers;
            this.readQuorum = readQuorum;
            this.writeQuorum = writeQuorum;
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

        public int CurrentNbDataServers
        {
            get { return this.currentNbDataServers; }
        }

        public Dictionary<string, string> LocalFilenames
        {
            get { return this.localFilenames; }
        }

        public Dictionary<string, string> Locations
        {
            get { return this.locations; }
        }

        public void AddDataServer(string id, string location, string localFilename)
        {
            this.Locations[id] = location;
            this.LocalFilenames[id] = localFilename;
            this.currentNbDataServers++;
        }

        public override string ToString()
        {
            string dataServers = "[";
            foreach (var entry in localFilenames)
            {
                string id = entry.Key;
                string localFilename = entry.Value;
                dataServers += " <" + id + ":" + localFilename + ">";
            }
            dataServers += " ]";

            // missing dataServersLocalFiles
            return "(" + this.filename + ":" + this.nbDataServers + ":" + this.readQuorum + ":" + this.writeQuorum + ":" + dataServers;
        }
    }
}
