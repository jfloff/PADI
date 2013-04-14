using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToClient : IMetadataToProcess
    {
        FileMetadata Open(string filename);
        void Close(string filename);
        FileMetadata Create(string filename, int nbDataServers, int readQuorum, int writeQuorum);
        void Delete(string filename);
    }
}
