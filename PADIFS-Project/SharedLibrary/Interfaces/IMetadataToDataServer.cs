using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToDataServer
    {
        GarbageCollected Heartbeat(string id, Heartbeat heartbeat);
        void DataServer(string id, string location);
    }
}
