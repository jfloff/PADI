using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata
    {
        void Ping();
        void UpdateState(MetadataState metadataSnapshot);
        void CreateOrUpdate(FileMetadata fileMetadata);
        void Delete(FileMetadata fileMetadata);
    }
}
