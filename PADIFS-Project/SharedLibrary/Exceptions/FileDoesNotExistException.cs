﻿using System;
using System.Runtime.Serialization;

namespace SharedLibrary.Exceptions
{
    [Serializable]
    public class FileDoesNotExistException : ApplicationException
    {
        public FileDoesNotExistException() { }

        public FileDoesNotExistException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public FileDoesNotExistException(string fileName)
            : base("File " + fileName + " does not exist.")
        { }
    }
}
