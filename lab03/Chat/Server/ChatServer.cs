using ChatLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client
    {
        public String Url;
        public String Nick;
    }

    class ChatServer : MarshalByRefObject, IChatServer
    {
        const int PORT = 8086;

        static TcpChannel channel = new TcpChannel(PORT);

        static void Main(string[] args)
        {
            ChannelServices.RegisterChannel(channel, false); 
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ChatServer), 
                "ChatServer", 
                WellKnownObjectMode.Singleton
            );
            // exits on CTRL-C
            while (true) ; 
        }

        public String connect(String nick, String url)
        {
            return url;
        }

        public void send(String nick, String msg)
        {
            Console.WriteLine("SENDING!");
        }
    }
}
