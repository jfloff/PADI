using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class ProcessDownException : ApplicationException
    {
        private string id;

        public ProcessDownException() { }

        public ProcessDownException(string id)
            : base("Process " + id + " is down.")
        {
            this.id = id;
        }

        public ProcessDownException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Id
        {
            get { return this.id; }
        }
    }
}

