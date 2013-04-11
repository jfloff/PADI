using System;
using System.Linq;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class FileData : IEquatable<FileData>
    {
        [Serializable]
        class Version
        {
            public string ClientId { get; set; }
            public int Clock { get; set; }

            public Version()
            {
                this.ClientId = string.Empty;
                this.Clock = 0;
            }
        }

        Version version;
        byte[] contents;

        public FileData()
        {
            this.version = new Version();
            this.contents = Helper.StringToBytes(string.Empty);
        }

        public byte[] Contents
        {
            get { return this.contents; }
            set { this.contents = value; }
        }

        public void AddSalt(byte[] salt)
        {
            this.contents = this.contents.Concat(salt).ToArray();
        }

        public void IncrementVersion(string clientId)
        {
            this.version.ClientId = clientId;
            this.version.Clock++;
        }

        // Returns:
        // >0  - if f1 is more recent than file f2
        // 0  - if they are the same
        // <0 - if f2 is more recent than f1
        public static int MostRecent(FileData f1, FileData f2)
        {
            int clockDiff = f1.version.Clock - f2.version.Clock;
            if (clockDiff == 0) return string.Compare(f2.version.ClientId, f1.version.ClientId);
            return clockDiff;
        }


        // Returns most recent version amongts a variable number of file datas
        // Clock is king. In case of draw, lowest clientId wins.
        public static FileData LatestVersion(params FileData[] fileDatas)
        {
            if (fileDatas.Length == 0) return null;

            FileData latest = fileDatas[0];
            for (int i = 1; i < fileDatas.Length; i++)
            {
                if (MostRecent(fileDatas[i], latest) > 0) latest = fileDatas[i];
            }
            return latest;
        }

        public override string ToString()
        {
            // missing dataServersLocalFiles
            return "Version = (" + this.version.ClientId + "," + this.version.Clock + ") : "
                + "Contents = " + this.contents;
        }

        // Operation overrides needed for Dictionaries

        public override int GetHashCode()
        {
            return this.version.ClientId.GetHashCode() ^ this.version.Clock.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            FileData fileData = obj as FileData;
            if ((System.Object) fileData == null)
            {
                return false;
            }

            return (this.version.ClientId.Equals(fileData.version.ClientId) && (this.version.Clock == fileData.version.Clock));
        }

        public bool Equals(FileData fileData)
        {
            // If parameter is null return false:
            if ((object)fileData == null)
            {
                return false;
            }

            return (this.version.ClientId.Equals(fileData.version.ClientId) && (this.version.Clock == fileData.version.Clock));
        }
    }
}
