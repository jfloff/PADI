using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata : IMetadataToProcess
    {
        MasterVote MasterVoting(MasterVote vote);
        MetadataDiff UpdateMetadata(string id);
        void CreateOrUpdateOnMetadata(FileMetadata fileMetadata, int sequence);
        void DeleteOnMetadata(FileMetadata fileMetadata, int sequence);
        void DataServerOnMetadata(string id, string location, int sequence);
        void LogMarkOnMetadata(string mark, int sequence);
        void HeartbeatOnMetadata(string id, Heartbeat heartbeat);
    }
}
