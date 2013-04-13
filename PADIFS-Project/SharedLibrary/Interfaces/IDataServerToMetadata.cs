using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IDataServerToMetadata
    {
        void Create(string filename);
        void Delete(string filename);
    }
}
