using System;
using System.Linq;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class FileData
    {
        private FileVersion version;
        private byte[] contents;

        public FileData()
        {
            this.version = new FileVersion();
            this.contents = Helper.StringToBytes(string.Empty);
        }

        public FileData(FileVersion version, byte[] contents)
        {
            this.version = version;
            this.contents = contents;
        }

        public byte[] Contents
        {
            get { return this.contents; }
            set { this.contents = value; }
        }

        public FileVersion Version
        {
            get { return this.version; }
        }

        public override string ToString()
        {
            return version + ":\"" + Helper.BytesToString(this.contents) + "\"";
        }

        public static FileData Latest(FileData f1, FileData f2)
        {
            return (FileVersion.MostRecent(f1.version, f2.version) >= 0) ? f1 : f2;
        }

        public void IncrementVersion(string clientId)
        {
            this.version.Increment(clientId);
        }
    }
}
