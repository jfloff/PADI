using System;
using SharedLibrary.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataServerToDataServer
    {
        Heartbeat Heartbeat();
        bool RegisterDataServer(string dataServerName, string urlLocation);
    }
}
