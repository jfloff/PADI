using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class ProcessFreezedException : ApplicationException
    {
        private string id;

        public ProcessFreezedException() { }

        public ProcessFreezedException(string id)
            : base("Process " + id + " is freezed.")
        {
            this.id = id;
        }

        public ProcessFreezedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Id
        {
            get { return this.id; }
        }
    }
}

