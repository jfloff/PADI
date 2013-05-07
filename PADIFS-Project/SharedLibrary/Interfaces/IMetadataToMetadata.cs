using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata : IMetadataToProcess
    {
        MasterVote MasterVoting(MasterVote vote);
        MetadataDiff UpdateMetadata(string id);
        void SelectOnMetadata(string filename, string dataServerId, string localFilename, int sequence);
        void CreateOnMetadata(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum, int sequence);
        void DeleteOnMetadata(string filename, int sequence);
        void DataServerOnMetadata(string id, string location, int sequence);
        void AddMarkOnMetadata(string mark, int markSequence, int sequence);
        void HeartbeatOnMetadata(string id, Heartbeat heartbeat, int sequence);
        void OpenOnMetadata(string clientId, string filename, int sequence);
        void CloseOnMetadata(string clientId, string filename, int sequence);
    }
}
