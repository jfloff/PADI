using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface IClient
    {
        void create();
        void open();
        void read();
        void write();
        void close();
        void delete();
    }
}
