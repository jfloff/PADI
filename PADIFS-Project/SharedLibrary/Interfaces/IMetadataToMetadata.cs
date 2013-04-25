using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata : IMetadataToProcess
    {
        MasterVote MasterVoting(MasterVote vote);
        void UpdateState(MetadataLogDiff metadataDiff);
        void CreateOrUpdateOnMetadata(FileMetadata fileMetadata, int sequence);
        void DeleteOnMetadata(FileMetadata fileMetadata, int sequence);
        void DataServerOnMetadata(string id, string location, int sequence);
    }
}
