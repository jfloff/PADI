﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IProcessToPM
    {
        void Dump(); 
        void ReceiveMetadataServersLocations(List<string> metadataServerList);
    }
}
