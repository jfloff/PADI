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
            public string filename = string.Empty;
            public FileData fileData = new FileData();
        }

        // index / filename+file data
        private Dictionary<int, RegisterInfo> infos = new Dictionary<int, RegisterInfo>();
        // filename / index
        private Dictionary<string, int> filenames = new Dictionary<string, int>();
        private volatile int index = 0;
        private volatile int count = 0;

        public bool Contains(string filename)
        {
            return filenames.ContainsKey(filename);
        }

        public FileData FileDataAt(int index)
        {
            return (infos.ContainsKey(index)) ? infos[index].fileData : null;
        }

        public string FilenameAt(int index)
        {
            return (infos.ContainsKey(index)) ? infos[index].filename : null;
        }

        public void SetFileDataAt(int index, FileData newFileData)
        {
            if (!infos.ContainsKey(index))
                throw new Exception("Index does not exist");

            infos[index].fileData = newFileData;
        }

        public void Add(string filename)
        {
            filenames[filename] = index;
            infos[index] = new RegisterInfo();
            infos[index].filename = filename;
            index++;
            count++;
        }

        public void Remove(string filename)
        {
            if (filenames.ContainsKey(filename))
            {
                int i = filenames[filename];
                filenames.Remove(filename);
                infos.Remove(i);
                count--;
            }
        }

        public int Count
        {
            get { return count; }
        }

        public override string ToString()
        {
            string ret = "[";
            for (int i = 0; i < count; i++)
            {
                if (infos.ContainsKey(i))
                {
                    RegisterInfo info = infos[i];
                    ret += " <" + i + ": (" + info.filename + ";" + Helper.BytesToString(info.fileData.Contents) + ")> ";
                }
            }
            return ret.TrimEnd(new char[] { ',' }) + "]";
        }
    }
}
