
using System;
namespace SharedLibrary.Entities
{
    public class Version
    {
        private string clientId;
        private int clock;

        public Version()
        {
            this.clientId = null;
            this.clock = 0;
        }

        public Version(string clientId, int clock)
        {
            this.clientId = clientId;
            this.clock = clock;
        }

        public string ClientId
        {
            get { return this.clientId;  }
            set { this.clientId = value; }
        }

        public int Clock
        {
            get { return this.clock; }
            set { this.clock = value; }
        }

       
        public override bool Equals(object obj)
        {
            return obj is Version && this == (Version)obj;
        }

        public override int GetHashCode()
        {
            return this.clientId.GetHashCode() ^ this.clock.GetHashCode();
        }

        public static bool operator ==(Version v1, Version v2)
        {
            return (v1.clientId.Equals(v2.clientId) && (v1.clock == v2.clock));
        }

        public static bool operator !=(Version v1, Version v2)
        {
            return !v1.Equals(v2);
        }

        public static bool operator >(Version v1, Version v2)
        {
            return v1.clock > v2.clock;
        }

        public static bool operator <(Version v1, Version v2)
        {
            return v1.clock < v2.clock;
        }

        public static Version operator ++(Version v)
        {
            return new Version(v.clientId, ++v.clock);
        }

        
        public override string ToString()
        {
            return "(" + this.clientId + "," + this.clock + ")";
        }
    }
}
