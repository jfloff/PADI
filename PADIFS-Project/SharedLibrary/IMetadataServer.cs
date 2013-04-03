using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface IMetadataServer
    {
        FileMetadata Open(string fileName);
        void Close(string fileName);
        FileMetadata Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum);
        void Delete(string fileName);
        // To decide
        void RegisterClient();
        void RegisterDataServer();
    }
}
