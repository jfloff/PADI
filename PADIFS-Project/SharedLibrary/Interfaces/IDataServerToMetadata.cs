using SharedLibrary.Entities;

namespace SharedLibrary.Interfaces
{
    public interface IDataServerToMetadata
    {
        FileData MigrationRead(string localFilename);
        void MigrationWrite(string localFilename, FileData newFile, Weight weight);
        void MigrationDelete(string localFilename);
    }
}
