using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    public class FileData
    {
        int version;
        byte[] contents;

        public FileData(int version, byte[] contents)
        {
            this.version = version;
            this.contents = contents;
        }

        public int Version
        {
            get { return this.version; }
            set { this.version = value; }
        }

        public byte[] Contents
        {
            get { return this.contents; }
            set { this.contents = value; }
        }

        public void incrementVersion()
        {
            this.version++;
        }

        public void addContents(byte[] newContents)
        {
            this.contents = this.contents.Concat(newContents).ToArray();
        }
    }
}
