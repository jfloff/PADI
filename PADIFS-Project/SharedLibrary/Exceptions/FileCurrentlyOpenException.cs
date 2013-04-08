using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class FileCurrentlyOpenException : ApplicationException
    {
        private string filename;

        public FileCurrentlyOpenException() { }

        public FileCurrentlyOpenException(string filename)
            : base("File " + filename + " is currently open by other clients.")
        {
            this.filename = filename;
        }

        public FileCurrentlyOpenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public string Filename
        {
            get { return this.filename; }
        }
    }
}
