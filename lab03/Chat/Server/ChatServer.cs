using ChatLib;
using System;
using System.Collections;
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
        public IChatClient Proxy;
    }

    class ChatServer : MarshalByRefObject, IChatServer
    {
        const int PORT = 8086;
        ArrayList clientList = new ArrayList();

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

        public void connect(String nick, String url)
        {
            Client c = new Client();
            IChatClient client = (IChatClient)Activator.GetObject(
                typeof(IChatClient),
                url
            );

            c.Nick = nick;
            c.Url = url;
            c.Proxy = client;

            clientList.Add(c);

            Console.WriteLine("Connected client with url = " + url + " ; with nick = " + nick);
        }

        public void send(String nick, String msg)
        {
            Console.WriteLine("Sending message = " + msg + " ; from nick = " + nick);
            // alternativa é lançar uma thread
            foreach (Client c in clientList)
            {  
                if (c.Nick != nick)
                {
                    c.Proxy.broadcast(nick, msg);
                }
            }
        }
    }
}
