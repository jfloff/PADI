using SharedLibrary;
using SharedLibrary.Entities;
using System;
using System.Collections.Generic;

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
        private Dictionary<int, string> filenames = new Dictionary<int, string>();
        // filename / index
        private Dictionary<string, RegisterInfo> infos = new Dictionary<string, RegisterInfo>();
        private volatile int index = 0;
        private volatile int count = 0;

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

                index++;
                count++;
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
                infos.Remove(filename);
                filenames.Remove(i);
                count--;
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
