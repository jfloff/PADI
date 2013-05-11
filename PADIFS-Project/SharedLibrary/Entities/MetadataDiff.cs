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
        private Queue<string> operations = new Queue<string>();

        public void AddOperation(string operation)
        {
            this.operations.Enqueue(operation);
        }

        public Queue<string> Operations
        {
            get { return this.operations; }
        }
        
    }
}

