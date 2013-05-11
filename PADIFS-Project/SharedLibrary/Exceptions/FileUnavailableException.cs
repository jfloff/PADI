using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class FileUnavailableException : ApplicationException
    {
        public FileUnavailableException() { }

        public FileUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public FileUnavailableException(string fileName)
            : base("File " + fileName + " does not exist.")
        { }
    }
}
