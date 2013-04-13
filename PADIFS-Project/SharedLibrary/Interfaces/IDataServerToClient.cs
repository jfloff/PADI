using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IDataServerToClient
    {
        FileVersion Version(string localFilename);
        FileData Read(string localFilename);
        void Write(string localFilename, FileData newFile);
    }
}
