using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IClientToPM : IProcessToPM
    {
        void Create(string fileName, int nbDataServers, int readQuorum, int writeQuorum);
        void Open(string fileName);
        void Close(string fileName);
        void Delete(string fileName);
        void Read();
        void Write();
    }
}
