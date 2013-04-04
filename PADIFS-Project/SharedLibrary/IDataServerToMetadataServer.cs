using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface IDataServerToMetadataServer
    {
        void Create(string fileName);
        void Delete(string fileName);
    }
}
