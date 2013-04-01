using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface IMetadataServer
    {
        void Open(string fileName);
        void Close(string fileName);
        void Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum);
        void Delete(string fileName);
        void RegisterClient();
        void RegisterDataServer();
    }
}
