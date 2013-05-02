using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class DataServerFiles : IEnumerable<string>
    {
        private ICollection<string> files;

        public DataServerFiles(ICollection<string> files)
        {
            this.files = files;
        }

        public bool Contains(string filename)
        {
            return this.files.Contains(filename);
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (string filename in this.files)
            {
                yield return filename;
            }
        }

        // Explicit interface implementation for nongeneric interface
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
