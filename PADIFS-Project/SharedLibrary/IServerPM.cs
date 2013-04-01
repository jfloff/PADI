using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface IServerPM
    {
        void Fail();
        void Recover();
    }
}
