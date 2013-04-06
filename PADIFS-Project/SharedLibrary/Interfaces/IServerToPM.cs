using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IServerToPM : IProcessToPM
    {
        void Fail();
        void Recover();
    }
}
