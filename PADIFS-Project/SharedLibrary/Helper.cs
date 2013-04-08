
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
        }

        public const int WINDOW_HEIGHT = 5;
        public const int WINDOW_WIDTH = 80;

        public const int PING_INTERVAL = 10000;

        public static string GetUrl(string id, int port)
        {
            return string.Format(URL_TEMPLATE, port, id);
        }
    }
}