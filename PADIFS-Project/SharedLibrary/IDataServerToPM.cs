using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface IDataServerToPM : IServerToPM
    {
        void Freeze();
        void Unfreeze();
        void ReceiveMetadataServersLocations(List<string> metadataServerList);
    }
}
