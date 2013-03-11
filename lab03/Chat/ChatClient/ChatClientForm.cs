using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatLib;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace ChatClient
{
    public delegate void deluc(String nick, String msg);

    public partial class ChatClientForm : Form
    {
        TcpChannel channel;
        IChatServer server;
        String nick;
        int port;

        public ChatClientForm()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (msgTextBox.Text != "")
            {
                server.send(nick, msgTextBox.Text);
            } 
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (nickTextBox.Text != "" && portTextBox.Text != "")
            {
                nick = nickTextBox.Text;
                port = Convert.ToInt32(portTextBox.Text);

                // Iniciar canal
                channel = new TcpChannel(port);
                ChannelServices.RegisterChannel(channel, false);

                // Registro do Servidor
                server = (IChatServer)Activator.GetObject(
                    typeof(IChatServer),
                    "tcp://localhost:8086/ChatServer" //lacking null verification
                );

                // Registro do cliente
                RemoteChatClient rmc = new RemoteChatClient(this);
                String clientServiceName = "ChatClient";
                RemotingServices.Marshal(
                    rmc,
                    clientServiceName,
                    typeof(RemoteChatClient)
                );

                if (server != null)
                {
                    // opcional 
                    server.connect(nick, "tcp://localhost:" + port + "/" + clientServiceName);
                }
            }
        }

        public void updateChat(String nick, String msg)
        {
            chatWindowRichTextBox.Text += nick + ": " + msg;
        }
    }

    public class RemoteChatClient : MarshalByRefObject, IChatClient
    {
        private ChatClientForm form;

        public RemoteChatClient(ChatClientForm form)
        {
            this.form = form;
        }

        public void broadcast(String nick, String msg)
        {
            this.form.Invoke(new deluc(form.updateChat), new object[] { nick, msg });
        }
    }
}
