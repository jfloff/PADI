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
        void Read(int fileRegisterIndex, Helper.Semantics semantics, int byteRegisterIndex);
        void Write(int fileRegisterIndex, int byteRegisterIndex);
        void Write(int fileRegisterIndex, byte[] contents);
        void Copy(int fileRegisterIndex1, Helper.Semantics semantics, int fileRegisterIndex2, byte[] salt);
    }
}
