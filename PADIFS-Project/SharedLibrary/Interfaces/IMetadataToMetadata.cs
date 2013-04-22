using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata : IMetadataToProcess
    {
        MasterVote MasterVoting(MasterVote vote);
        void UpdateState(MetadataLogDiff metadataDiff);
        void CreateOrUpdateOnMetadata(FileMetadata fileMetadata);
        void DeleteOnMetadata(FileMetadata fileMetadata);
        void DataServerOnMetadata(string id, string location);
    }
}
