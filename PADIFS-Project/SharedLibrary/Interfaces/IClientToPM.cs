using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IClientToPM : IProcessToPM
    {
        void Create(string filename, int nbDataServers, int readQuorum, int writeQuorum);
        void Delete(string filename);
        void Open(string filename);
        void Close(string filename);
        void Read();
        void Write();
    }
}
