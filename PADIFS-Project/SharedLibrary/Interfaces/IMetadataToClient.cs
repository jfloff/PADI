using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToClient
    {
        FileMetadata Open(string filename);
        void Close(string filename);
        FileMetadata Create(string filename, int nbDataServers, int readQuorum, int writeQuorum);
        void Delete(string filename);
    }
}
