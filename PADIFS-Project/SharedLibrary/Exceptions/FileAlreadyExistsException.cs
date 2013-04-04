using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Exceptions
{

    [Serializable]
    public class FileAlreadyExistsException : ApplicationException
    {
        private string fileName;
        private string message;

        public FileAlreadyExistsException() { }

        public FileAlreadyExistsException(string fileName)
            : base("File " + fileName + " already exists.")
        {
            this.fileName = fileName;
            this.message = "File " + fileName + " already exists.";

        }

        public FileAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string FileName
        {
            get { return this.fileName; }
        }
    }
}
