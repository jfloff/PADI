using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface IMetadataServerToDataServer
    {
        Heartbeat Heartbeat();
        bool RegisterDataServer(string dataServerName, string urlLocation);
    }
}
