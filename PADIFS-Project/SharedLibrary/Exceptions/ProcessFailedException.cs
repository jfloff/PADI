using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class ProcessFailedException : ApplicationException
    {
        public ProcessFailedException() { }

        public ProcessFailedException(string id)
            : base("Process " + id + " is down.")
        { }

        public ProcessFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}

