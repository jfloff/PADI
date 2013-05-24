
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace SharedLibrary
{
    public static class Helper
    {
        public const int MAX_FILE_REGISTERS = 10;
        public const string URL_TEMPLATE = "tcp://localhost:{0}/{1}";

        public enum Semantics
        {
            DEFAULT,
            MONOTONIC
        };

        public const int WINDOW_HEIGHT = 5;
        public const int WINDOW_WIDTH = 80;

        public const int PING_INTERVAL = 10000;
        public const int DATASERVER_HEARTBEAT_INTERVAL = 3000;
        public const int HEARTBEAT_EXPIRE = 3;
        public const int LOAD_BALANCING_INTERVAL = 4500;
        public const double LOAD_BALANCING_THRESHOLD = 0.50;

        public static string GetUrl(string id, int port)
        {
            return string.Format(URL_TEMPLATE, port, id);
        }

        // http://stackoverflow.com/a/10380166/1700053
        public static byte[] StringToBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        // http://stackoverflow.com/a/10380166/1700053
        public static string BytesToString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        // http://stackoverflow.com/a/415839/1700053
        public static byte[] AppendBytes(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        // http://stackoverflow.com/q/10392268/1700053
        public static string SerializeObject<T>(T objectToSerialize)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream memStr = new MemoryStream();

            try
            {
                bf.Serialize(memStr, objectToSerialize);
                memStr.Position = 0;

                return Convert.ToBase64String(memStr.ToArray());
            }
            finally
            {
                memStr.Close();
            }
        }

        // http://stackoverflow.com/q/10392268/1700053
        public static T DeserializeObject<T>(string str)
        {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] b = Convert.FromBase64String(str);
            MemoryStream ms = new MemoryStream(b);

            try
            {
                return (T)bf.Deserialize(ms);
            }
            finally
            {
                ms.Close();
            }
        }
    }
}