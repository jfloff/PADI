using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using SharedLibrary;
using Client;
using System.Diagnostics;
using System.Collections;

namespace PuppetMaster
{
    public partial class PuppetMasterForm : Form
    {
        Hashtable processes;

        public PuppetMasterForm()
        {
            InitializeComponent();
            TcpChannel channel = new TcpChannel(8080);
            ChannelServices.RegisterChannel(channel, true);
            this.processes = new Hashtable();
        }

        private void recover_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!serverID.Equals(string.Empty) && this.processes.Contains(serverID))
            {
                IServerPM server = (IServerPM)this.processes[serverID];
                server.Recover();
            }
        }

        private void fail_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!serverID.Equals(string.Empty) && this.processes.Contains(serverID))
            {
                IServerPM server = (IServerPM)this.processes[serverID];
                server.Fail();
            }
        }

        private void unfreeze_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!serverID.Equals(string.Empty) && this.processes.Contains(serverID))
            {
                // Verificar se é um metadata server or data server (apenas data servers podem fazer unfreeze)
                IDataServerPM server = (IDataServerPM)this.processes[serverID];
                server.Unfreeze();
            }
        }

        private void freeze_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!serverID.Equals(string.Empty) && this.processes.Contains(serverID))
            {
                // Verificar se é um metadata server or data server (apenas data servers podem fazer freeze)
                IDataServerPM server = (IDataServerPM)this.processes[serverID];
                server.Freeze();
            }
        }

        private void create_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            string nbData = this.NbDataServersBox.Text;
            string readq = this.readQuorumBox.Text;
            string writeq = this.writeQuorumBox.Text;

            if (!fileName.Equals(string.Empty) && !nbData.Equals(string.Empty)
                && !readq.Equals(string.Empty) && !writeq.Equals(string.Empty)
                && !clientID.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClient client = (IClient)this.processes[clientID];
                client.Create(fileName, Convert.ToInt32(nbData), Convert.ToInt32(readq), Convert.ToInt32(writeq));
            }
        }

        private void open_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClient client = (IClient)this.processes[clientID];
                client.Open(fileName);
            }
        }

        private void delete_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClient client = (IClient)this.processes[clientID];
                client.Delete(fileName);
            }
        }

        private void close_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClient client = (IClient)this.processes[clientID];
                client.Close(fileName);
            }
        }

        private void startClient_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string port = this.portBox.Text;

            if (!clientID.Equals(string.Empty) && !port.Equals(string.Empty) && !this.processes.Contains(clientID))
            {
                Process.Start("Client.exe", clientID + " " + port);
                IClient client = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + port + "/" + clientID);
                this.processes.Add(clientID, client);
                this.processBox.Clear();
                this.portBox.Clear();
            }
            else { 
                // Caso em que o identificador já está a ser usado por outro processo
                // Lançar uma excepção e/ou alerta no form
            }
        }

        private void startMetadata_click(object sender, EventArgs e)
        {
            string metadataID = this.processBox.Text;
            string port = this.portBox.Text;

            if (!metadataID.Equals(string.Empty) && !port.Equals(string.Empty) && !this.processes.Contains(metadataID))
            {
                Process.Start("MetadataServer.exe", metadataID + " " + port);
                IServerPM metadata = (IServerPM)Activator.GetObject(typeof(IServerPM), "tcp://localhost:" + port + "/" + metadataID);
                this.processes.Add(metadataID, metadata);
                this.processBox.Clear();
                this.portBox.Clear();
            }
            else
            {
                // Caso em que o identificador já está a ser usado por outro processo
                // Lançar uma excepção e/ou alerta no form
            }
        }

        private void startDataServer_click(object sender, EventArgs e)
        {
            string dataID = this.processBox.Text;
            string port = this.portBox.Text;

            if (!dataID.Equals(string.Empty) && !port.Equals(string.Empty) && !this.processes.Contains(dataID))
            {
                Process.Start("DataServer.exe", dataID + " " + port);
                IDataServerPM dataServer = (IDataServerPM)Activator.GetObject(typeof(IDataServerPM), "tcp://localhost:" + port + "/" + dataID);
                this.processes.Add(dataID, dataServer);
                this.processBox.Clear();
                this.portBox.Clear();
              
            }
            else
            {
                // Caso em que o identificador já está a ser usado por outro processo
                // Lançar uma excepção e/ou alerta no form
            }
        }

    }
}
