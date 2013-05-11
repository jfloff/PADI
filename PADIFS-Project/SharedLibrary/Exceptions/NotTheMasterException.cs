using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class NotTheMasterException : ApplicationException, ISerializable
    {
        private string newMaster;

        public NotTheMasterException() { }

        public NotTheMasterException(string id, string newMaster)
            : base("Process is no longer the master. New master is " + newMaster)
        {
            this.newMaster = newMaster;
        }

        public NotTheMasterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.newMaster = info.GetString("newMaster");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("newMaster", this.newMaster);
        }

        public string NewMaster
        {
            get { return this.newMaster; }
        }
    }
}