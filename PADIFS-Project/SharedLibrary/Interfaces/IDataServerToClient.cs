using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IDataServerToClient
    {
        byte[] Read(string localFilename);
        void Write(string localFilename, byte[] contents);
    }
}
