using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata
    {
        void Ping();
        void UpdateState(MetadataDiff metadataSnapshot);
        void CreateOrUpdate(FileMetadata fileMetadata);
        void Delete(FileMetadata fileMetadata);
        void RegisterDataServer(string id, string location);
    }
}
