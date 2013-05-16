using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata
    {
        MasterVote Uprising(MasterVote vote, bool force = false);
        string Master();
        MetadataDiff UpdateMetadata(string metadataId, int sequence);
        void SelectOnMetadata(string filename, string dataServerId, string localFilename, int sequence);
        void CreateOnMetadata(string clientId, string filename, int nbDataServers, int readQuorum, int writeQuorum, int sequence);
        void DeleteOnMetadata(string filename, int sequence);
        void DataServerOnMetadata(string id, string location, int sequence);
        void MigrateFileOnMetadata(string filename, string oldDataServerId, string newDataServerId, string oldLocalFilename, string newLocalFilename, int sequence);
        void OpenOnMetadata(string clientId, string filename, int sequence);
        void CloseOnMetadata(string clientId, string filename, int sequence);
    }
}
