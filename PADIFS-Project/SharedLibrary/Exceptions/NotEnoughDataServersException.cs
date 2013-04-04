using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class NotEnoughDataServersException : ApplicationException
    {
        private int nbDataServersRequested;
        private int nbDataServersRunning;

        public NotEnoughDataServersException() { }

        public NotEnoughDataServersException(int nbDataServersRequested, int nbDataServersRunning)
            : base("Not enough DataServers. Requested: " + nbDataServersRequested + " Running: " + nbDataServersRunning)
        {
            this.nbDataServersRequested = nbDataServersRequested;
            this.nbDataServersRunning = nbDataServersRunning;
        }

        public NotEnoughDataServersException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public int NbDataServersRequested
        {
            get { return this.nbDataServersRequested; }
        }

        public int NbDataServersRunning
        {
            get { return this.nbDataServersRunning; }
        }
    }
}
