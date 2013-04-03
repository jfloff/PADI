using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SharedLibrary
{
    [Serializable]
    public class FileDoesNotExistException : ApplicationException
    {
        private string fileName;
        private string message;

        public FileDoesNotExistException() { }

        public FileDoesNotExistException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public FileDoesNotExistException(string fileName)
            : base("File " + fileName + " does not exist.")
        {
            this.fileName = fileName;
            this.message = "File " + fileName + " does not exist.";
        }

        public string FileName
        {
            get { return this.fileName; }
        }
    }
}
