using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class FileUnavailableException : ApplicationException
    {
        private string fileName;

        public FileUnavailableException() { }

        public FileUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public FileUnavailableException(string fileName)
            : base("File " + fileName + " does not exist.")
        {
            this.fileName = fileName;
        }

        public string FileName
        {
            get { return this.fileName; }
        }
    }
}
