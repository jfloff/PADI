﻿using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{

    [Serializable]
    public class FileAlreadyExistsException : ApplicationException
    {
        private string filename;

        public FileAlreadyExistsException() { }

        public FileAlreadyExistsException(string filename)
            : base("File " + filename + " already exists.")
        {
            this.filename = filename;
        }

        public FileAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Filename
        {
            get { return this.filename; }
        }
    }
}
