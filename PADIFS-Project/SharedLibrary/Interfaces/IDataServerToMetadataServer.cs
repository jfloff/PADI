using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IDataServerToMetadataServer
    {
        void CreateFile(string fileName);
        void DeleteFile(string fileName);
    }
}
