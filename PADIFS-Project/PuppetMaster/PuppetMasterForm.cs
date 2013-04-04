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
        private string urlTemplate = "tcp://localhost:{0}/{1}";
        private List<string> metadataServersLocation;
        private Hashtable processes;

        public PuppetMasterForm()
        {
            InitializeComponent();
            TcpChannel channel = new TcpChannel(8080);
            ChannelServices.RegisterChannel(channel, true);
            this.processes = new Hashtable();
            this.metadataServersLocation = new List<string>();
        }

        private void recover_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!serverID.Equals(string.Empty) && this.processes.Contains(serverID))
            {
                IServerToPM server = (IServerToPM)this.processes[serverID];
                server.Recover();
            }
        }

        private void fail_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!serverID.Equals(string.Empty) && this.processes.Contains(serverID))
            {
                IServerToPM server = (IServerToPM)this.processes[serverID];
                server.Fail();
            }
        }

        private void unfreeze_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!serverID.Equals(string.Empty) && this.processes.Contains(serverID))
            {
                // Verificar se é um metadata server or data server (apenas data servers podem fazer unfreeze)
                IDataServerToPM server = (IDataServerToPM)this.processes[serverID];
                server.Unfreeze();
            }
        }

        private void freeze_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!serverID.Equals(string.Empty) && this.processes.Contains(serverID))
            {
                // Verificar se é um metadata server or data server (apenas data servers podem fazer freeze)
                IDataServerToPM server = (IDataServerToPM)this.processes[serverID];
                server.Freeze();
            }
        }

        private void Create_click(object sender, EventArgs e)
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
                IClientToPM client = (IClientToPM)this.processes[clientID];
                client.Create(fileName, Convert.ToInt32(nbData), Convert.ToInt32(readq), Convert.ToInt32(writeq));
            }
        }

        private void open_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClientToPM client = (IClientToPM)this.processes[clientID];
                client.Open(fileName);
            }
        }

        private void delete_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClientToPM client = (IClientToPM)this.processes[clientID];
                client.Delete(fileName);
            }
        }

        private void close_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClientToPM client = (IClientToPM)this.processes[clientID];
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
                IClientToPM client = (IClientToPM)Activator.GetObject(typeof(IClientToPM), string.Format(urlTemplate, port, clientID));
                this.processes.Add(clientID, client);
                client.ReceiveMetadataServersLocations(this.metadataServersLocation);
            }
            else
            {
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
                string url = string.Format(urlTemplate, port, metadataID);
                IServerToPM metadata = (IServerToPM)Activator.GetObject(typeof(IServerToPM), url);
                this.processes.Add(metadataID, metadata);
                this.metadataServersLocation.Add(url);
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
                IDataServerToPM dataServer = (IDataServerToPM)Activator.GetObject(typeof(IDataServerToPM), string.Format(urlTemplate, port, dataID));
                this.processes.Add(dataID, dataServer);
                dataServer.ReceiveMetadataServersLocations(this.metadataServersLocation);
            }
            else
            {
                // Caso em que o identificador já está a ser usado por outro processo
                // Lançar uma excepção e/ou alerta no form
            }
        }

    }
}
