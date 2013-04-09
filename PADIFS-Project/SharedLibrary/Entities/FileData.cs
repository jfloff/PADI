

//public override bool Equals(object obj)
//{
//    return obj is FileVersion && this == (FileVersion)obj;
//}

//public override int GetHashCode()
//{
//    return this.clientId.GetHashCode() ^ this.clock.GetHashCode();
//}

//public static bool operator ==(FileVersion v1, FileVersion v2)
//{
//    return (v1.clientId.Equals(v2.clientId) && (v1.clock == v2.clock));
//}

//public static bool operator !=(FileVersion v1, FileVersion v2)
//{
//    return !v1.Equals(v2);
//}

//public static bool operator >(FileVersion v1, FileVersion v2)
//{
//    return v1.clock > v2.clock;
//}

//public static bool operator <(FileVersion v1, FileVersion v2)
//{
//    return v1.clock < v2.clock;
//}

//public static FileVersion operator ++(FileVersion v)
//{
//    return new FileVersion(v.clientId, ++v.clock);
//}


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
                this.ClientId = "";
                this.Clock = 0;
            }
        }

        Version version;
        byte[] contents;

        public FileData()
        {
            this.version = new Version();
            this.contents = Helper.StringToBytes("TESTE");
        }

        public byte[] Contents
        {
            get { return this.contents; }
            set { this.contents = value; }
        }

        public void addSalt(byte[] salt)
        {
            this.contents = this.contents.Concat(salt).ToArray();
        }

        public override string ToString()
        {
            // missing dataServersLocalFiles
            return "Version = (" + this.version.ClientId + "," + this.version.Clock + ") : "
                + "Contents = " + this.contents;
        }

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
