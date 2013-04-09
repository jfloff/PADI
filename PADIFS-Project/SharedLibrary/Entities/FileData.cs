

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
            return Equals(obj as FileData);
        }

        public bool Equals(FileData fileData)
        {
            return this == fileData;
        }

        public static bool operator ==(FileData f1, FileData f2)
        {
            if (f1 == null && f2 == null) return true;
            if (f1.version.ClientId.Equals(f2.version.ClientId) && (f1.version.Clock == f2.version.Clock)) return true;

            return false;
        }

        public static bool operator !=(FileData f1, FileData f2)
        {
            return !(f1 == f2);
        }
    }
}
