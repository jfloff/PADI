using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToDataServer : IMetadataToProcess
    {
        void Heartbeat(string id, Heartbeat heartbeat);
        void RegisterDataServer(string id, string location);
    }
}
