using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public static class Helper
    {
        public const int MAX_FILE_REGISTERS = 10;
        public const string URL_TEMPLATE = "tcp://localhost:{0}/{1}";

        public static string GetUrlTemplate(string id, int port)
        {
            return string.Format(URL_TEMPLATE, port, id);
        }
    }
}
