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

namespace ChatClient
{
    public partial class ChatClientForm : Form
    {
        TcpChannel channel = new TcpChannel();
        IChatServer server;
        String nick;

        public ChatClientForm()
        {
            InitializeComponent();
            ChannelServices.RegisterChannel(channel, false);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (nickTextBox.Text != "" && urlTextBox.Text != "" && portTextBox.Text != "")
            {
                nick = nickTextBox.Text;

                // deprecated method, we added false to the end
                server = (IChatServer)Activator.GetObject(
                    typeof(IChatServer),
                    "tcp://" + urlTextBox.Text + ":" + portTextBox.Text + "/ChatServer" //lacking null verification
                );

                if (server != null)
                {
                    msgTextBox.Text = server.connect(nick, channel. );
                }
            }
        }
    }
}
