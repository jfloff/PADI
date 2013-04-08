using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IDataServerToClient
    {
        FileData Read(string localFilename);
        void Write(string localFilename, FileData newFileData);
    }
}
