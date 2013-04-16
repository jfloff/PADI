using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToDataServer : IMetadataToProcess
    {
        void Heartbeat(string id, Heartbeat heartbeat);
        void DataServer(string id, string location);
    }
}
