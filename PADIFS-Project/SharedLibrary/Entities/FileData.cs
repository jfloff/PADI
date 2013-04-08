using System.Linq;

namespace SharedLibrary.Entities
{
    public class FileData
    {
        Version version;
        byte[] contents;

        public FileData()
        {
            this.version = new Version();
        }

        public FileData(byte[] contents)
        {

            this.contents = contents;
        }

        public byte[] Contents
        {
            get { return this.contents; }
            set { this.contents = value; }
        }

        public Version Version
        {
            get { return this.version; }
            set { this.version = value; }
        }

        public void addSalt(byte[] salt)
        {
            this.contents = this.contents.Concat(salt).ToArray();
        }

        public override string ToString()
        {
            // missing dataServersLocalFiles
            return "Version = " + this.version + " : "
                + "Contents = " + this.contents;
        }
    }
}
