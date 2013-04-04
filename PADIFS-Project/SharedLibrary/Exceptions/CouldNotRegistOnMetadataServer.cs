using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class CouldNotRegistOnMetadataServer : ApplicationException
    {
        private string processName;

        public CouldNotRegistOnMetadataServer() { }

        public CouldNotRegistOnMetadataServer(string processName)
            : base("Could not regist process " + processName + " on Metadata Server.")
        {
            this.processName = processName;
        }

        public CouldNotRegistOnMetadataServer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string ProcessName
        {
            get { return this.processName; }
        }
    }
}
