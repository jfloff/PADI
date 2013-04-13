using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class FileVersion
    {
        private string clientId;
        private int clock;

        public FileVersion()
        {
            this.clientId = string.Empty;
            this.clock = 0;
        }

        public string ClientId
        {
            get { return this.clientId; }
        }

        public int Clock
        {
            get { return this.clock; }
        }

        public void Increment(string clientId)
        {
            this.clientId = clientId;
            this.clock++;
        }

        // Returns:
        // >0  - if v1 is more recent than v2
        // 0  - if they are the same
        // <0 - if v2 is more recent than v1
        public static int MostRecent(FileVersion v1, FileVersion v2)
        {
            int clockDiff = v1.clock - v2.clock;
            if (clockDiff == 0) return string.Compare(v2.clientId, v2.clientId);
            return clockDiff;
        }

        public override string ToString()
        {
            return "(" + this.clientId + ";" + this.clock + ")";
        }

        public override int GetHashCode()
        {
            return this.clientId.GetHashCode() ^ this.clock.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            FileVersion fileVersion = obj as FileVersion;
            if ((System.Object)fileVersion == null)
            {
                return false;
            }

            return ((this.clientId == fileVersion.clientId) && (this.clock == fileVersion.clock));
        }

        public bool Equals(FileVersion fileVersion)
        {
            // If parameter is null return false:
            if ((object)fileVersion == null)
            {
                return false;
            }

            return ((this.clientId == fileVersion.clientId) && (this.clock == fileVersion.clock));
        }
    }
}
