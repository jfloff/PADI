using SharedLibrary;
using SharedLibrary.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Client
{
    class FileRegister
    {
        private class RegisterInfo
        {
            public int index;
            public FileData fileData;
            public FileMetadata fileMetadata;
        }

        // index / filename
        private ConcurrentDictionary<int, string> filenames = new ConcurrentDictionary<int, string>();
        // filename / index
        private ConcurrentDictionary<string, RegisterInfo> infos = new ConcurrentDictionary<string, RegisterInfo>();
        private int index = 0;
        private int count = 0;

        public bool Contains(string filename)
        {
            return infos.ContainsKey(filename);
        }

        public FileData FileDataAt(int index)
        {
            return (filenames.ContainsKey(index)) ? infos[filenames[index]].fileData : null;
        }

        public FileMetadata FileMetadataAt(int index)
        {
            return (filenames.ContainsKey(index)) ? infos[filenames[index]].fileMetadata : null;
        }

        public string FilenameAt(int index)
        {
            return (filenames.ContainsKey(index)) ? filenames[index] : null;
        }

        public void SetFileDataAt(int index, FileData newFileData)
        {
            if (!filenames.ContainsKey(index))
                throw new Exception("Index does not exist");

            infos[filenames[index]].fileData = newFileData;
        }

        public void SetFileMetadataAt(string filename, FileMetadata newFileMetadata)
        {
            if (!filenames.ContainsKey(index))
                throw new Exception("Index does not exist");

            infos[filename].fileMetadata = newFileMetadata;
        }

        public void AddOrUpdate(string filename, FileMetadata fileMetadata)
        {
            if (!this.Contains(filename))
            {
                filenames[index] = filename;

                infos[filename] = new RegisterInfo();
                infos[filename].index = index;
                infos[filename].fileData = new FileData();
                infos[filename].fileMetadata = fileMetadata;

                Interlocked.Increment(ref index);
                Interlocked.Increment(ref count);
            }
            else
            {
                infos[filename].fileMetadata = fileMetadata;
            }
        }

        public void Remove(string filename)
        {
            if (infos.ContainsKey(filename))
            {
                int i = infos[filename].index;
                RegisterInfo ignoredRegisterInfo;  infos.TryRemove(filename, out ignoredRegisterInfo);
                string ignoredFilename; filenames.TryRemove(i, out ignoredFilename);
                Interlocked.Decrement(ref count);
            }
        }

        public int Count
        {
            get { return count; }
        }

        public override string ToString()
        {
            string ret = "[\n";
            for (int i = 0; i < count; i++)
            {
                if (filenames.ContainsKey(i))
                {
                    string filename = filenames[i];
                    RegisterInfo info = infos[filename];
                    ret += "  <" + i + ": (" + filename + ";" + info.fileMetadata + ";" + info.fileData + ")> \n";
                }
            }
            return ret + "]";
        }
    }
}
