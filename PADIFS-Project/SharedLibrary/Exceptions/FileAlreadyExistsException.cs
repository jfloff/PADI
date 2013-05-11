using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class FileAlreadyExistsException : ApplicationException
    {
        public FileAlreadyExistsException() { }

        public FileAlreadyExistsException(string filename)
            : base("File " + filename + " already exists.")
        {
        }

        public FileAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
