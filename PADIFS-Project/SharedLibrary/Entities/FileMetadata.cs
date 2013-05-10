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

        public string LocalFilename(string id)
        {
            return this.localFilenames[id];
        }

        public void AddDataServer(string id, string location, string localFilename)
        {
            this.locations[id] = location;
            this.localFilenames[id] = localFilename;
            this.currentNbDataServers++;
        }

        public string RemoveDataServer(string id)
        {
            string localFilename = this.localFilenames[id];
            this.localFilenames.Remove(id);
            this.locations.Remove(id);
            this.currentNbDataServers--;
            return localFilename;
        }

        public bool InDataServer(string dataServerId)
        {
            return this.localFilenames.ContainsKey(dataServerId);
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
            return this.filename + ":" + this.nbDataServers + ":" + this.readQuorum + ":" + this.writeQuorum + ":" + dataServers;
        }

        // for comparasion on hash keys, etc
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            FileMetadata fileMetadata = obj as FileMetadata;
            if ((System.Object)fileMetadata == null)
            {
                return false;
            }

            return (this.filename == fileMetadata.filename)
                && (this.nbDataServers == fileMetadata.nbDataServers)
                && (this.currentNbDataServers == fileMetadata.currentNbDataServers)
                && (this.readQuorum == fileMetadata.readQuorum)
                && (this.writeQuorum == fileMetadata.writeQuorum)
                && (this.localFilenames == fileMetadata.localFilenames);
        }

        public bool Equals(FileMetadata fileMetadata)
        {
            // If parameter is null return false:
            if ((object)fileMetadata == null)
            {
                return false;
            }

            return (this.filename == fileMetadata.filename)
                && (this.nbDataServers == fileMetadata.nbDataServers)
                && (this.currentNbDataServers == fileMetadata.currentNbDataServers)
                && (this.readQuorum == fileMetadata.readQuorum)
                && (this.writeQuorum == fileMetadata.writeQuorum)
                && (this.localFilenames == fileMetadata.localFilenames);
        }

        public override int GetHashCode()
        {
            return this.filename.GetHashCode() 
                ^ this.nbDataServers.GetHashCode() 
                ^ this.currentNbDataServers.GetHashCode()
                ^ this.readQuorum.GetHashCode() 
                ^ this.writeQuorum.GetHashCode() 
                ^ this.localFilenames.GetHashCode();
        }

        // for deep copies of dictionaries (snapshots)
        public FileMetadata Clone()
        {
            FileMetadata copy = new FileMetadata(this.filename, this.nbDataServers, this.readQuorum, this.writeQuorum);
            copy.currentNbDataServers = this.currentNbDataServers;
            copy.localFilenames = new Dictionary<string, string>(this.localFilenames);
            copy.locations = new Dictionary<string, string>(this.locations);

            return copy;
        }
    }
}
