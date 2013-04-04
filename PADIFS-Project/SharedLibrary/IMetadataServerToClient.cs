using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface IMetadataServerToClient
    {
        FileMetadata Open(string fileName);
        void Close(string fileName);
        FileMetadata Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum);
        void Delete(string fileName);
        bool RegisterClient(string clientName);
    }
}
