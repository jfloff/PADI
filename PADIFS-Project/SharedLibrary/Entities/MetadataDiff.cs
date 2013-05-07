using SharedLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedLibrary.Entities
{
    [Serializable]
    public class MetadataDiff
    {
        // action queue
        private Stack<string> operations = new Stack<string>();

        public void AddOperation(string operation)
        {
            this.operations.Push(operation);
        }

        public Stack<string> Operations
        {
            get { return this.operations; }
        }
        
    }
}

