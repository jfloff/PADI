using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class ProcessFailedException : ApplicationException
    {
        private string id;

        public ProcessFailedException() { }

        public ProcessFailedException(string id)
            : base("Process " + id + " is down.")
        {
            this.id = id;
        }

        public ProcessFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Id
        {
            get { return this.id; }
        }
    }
}

