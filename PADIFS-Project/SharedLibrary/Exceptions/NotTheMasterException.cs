using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class NotTheMasterException : ApplicationException
    {
        private string id;

        public NotTheMasterException() { }

        public NotTheMasterException(string id)
            : base("Process " + id + " is not the master.")
        {
            this.id = id;
        }

        public NotTheMasterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Id
        {
            get { return this.id; }
        }
    }
}

