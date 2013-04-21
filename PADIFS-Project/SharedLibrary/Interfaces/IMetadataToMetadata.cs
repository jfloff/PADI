using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata
    {
        void Ping();
        void UpdateState(MetadataLogDiff metadataDiff);
        void CreateOrUpdateOnMetadata(FileMetadata fileMetadata);
        void DeleteOnMetadata(FileMetadata fileMetadata);
        void DataServerOnMetadata(string id, string location);
    }
}
