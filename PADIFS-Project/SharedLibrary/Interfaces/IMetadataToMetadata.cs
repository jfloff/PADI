using SharedLibrary.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IMetadataToMetadata
    {
        void Ping();
        void Update(MetadataState metadataSnapshot);
        void CreateOrUpdate(FileMetadata fileMetadata);
        void Delete(FileMetadata fileMetadata);
    }
}
