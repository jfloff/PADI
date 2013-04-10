

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

        public void AddSalt(byte[] salt)
        {
            this.contents = this.contents.Concat(salt).ToArray();
        }

        public void IncrementVersion(string clientId)
        {
            this.version.ClientId = clientId;
            this.version.Clock++;
        }

        // Returns most recent version amongts a variable number of file datas
        //Clock is king. In case of draw, lowest clientId wins.
        public static FileData LatestVersion(params FileData[] fileDatas)
        {
            if (fileDatas.Length == 0) return null;

            FileData latest = fileDatas[0];
            for (int i = 1; i < fileDatas.Length; i++)
            {
                if (fileDatas[i].version.Clock > latest.version.Clock)
                {
                    latest = fileDatas[i];
                }
                if (fileDatas[i].version.Clock == latest.version.Clock)
                {
                    if (String.Compare(latest.version.ClientId, fileDatas[i].version.ClientId) > 0)
                    {
                        latest = fileDatas[i];
                    }
                }
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
