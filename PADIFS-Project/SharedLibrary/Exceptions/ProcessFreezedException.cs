using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class ProcessFreezedException : ApplicationException
    {
        public ProcessFreezedException() { }

        public ProcessFreezedException(string id)
            : base("Process " + id + " is freezed.")
        { }

        public ProcessFreezedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}

