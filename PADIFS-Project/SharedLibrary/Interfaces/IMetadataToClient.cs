using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToClient : IMetadataToProcess
    {
        FileMetadata Open(string id, string filename);
        void Close(string id, string filename);
        FileMetadata Create(string id, string filename, int nbDataServers, int readQuorum, int writeQuorum);
        void Delete(string id, string filename);
    }
}
