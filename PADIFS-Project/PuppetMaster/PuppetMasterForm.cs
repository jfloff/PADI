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
        private Hashtable processes;
       
        public PuppetMasterForm()
        {
            InitializeComponent();
            // Element details
            this.componentSelectionBox.SelectedIndex = 0;
            this.semanticsSelectionBox.SelectedIndex = 0;
            // Connection details
            TcpChannel channel = new TcpChannel(8080);
            ChannelServices.RegisterChannel(channel, true);
            this.processes = new Hashtable();
        }

        private void ComponentSelectionBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox componentSelectionBox = (ComboBox)sender;

            switch (componentSelectionBox.SelectedIndex) 
            {
                default:
                case 0:
                    {
                        ToggleDataServerElements(false);
                        ToggleClientElements(false);
                        ToggleMetadataElements(true);
                        break;
                    }
                case 1:
                    {
                        ToggleMetadataElements(false);
                        ToggleClientElements(false);
                        ToggleDataServerElements(true);
                        break;
                    }
                case 2:
                    {
                        ToggleMetadataElements(false);
                        ToggleDataServerElements(false);
                        ToggleClientElements(true);
                        break;
                    }
            }
        }

        private void ToggleMetadataElements(bool toggle)
        {
            this.failButton.Visible = toggle;
            this.recoverButton.Visible = toggle;
        }

        private void ToggleDataServerElements(bool toggle)
        {
            this.failButton.Visible = toggle;
            this.recoverButton.Visible = toggle;
            this.freezeButton.Visible = toggle;
            this.unfreezeButton.Visible = toggle;
        }

        private void ToggleClientElements(bool toggle)
        {
            this.filenameBox.Visible = toggle;
            this.filenameLabel.Visible = toggle;
            this.NbDataServersBox.Visible = toggle;
            this.NbDataServersLabel.Visible = toggle;
            this.readQuorumBox.Visible = toggle;
            this.readQuorumLabel.Visible = toggle;
            this.writeQuorumBox.Visible = toggle;
            this.writeQuorumLabel.Visible = toggle;
            this.createButton.Visible = toggle;
            this.deleteButton.Visible = toggle;
            this.openButton.Visible = toggle;
            this.closeButton.Visible = toggle;
            this.fileRegister1Label.Visible = toggle;
            this.fileRegister1Number.Visible = toggle;
            this.fileRegister2Label.Visible = toggle;
            this.fileRegister2Number.Visible = toggle;
            this.semanticsLabel.Visible = toggle;
            this.semanticsSelectionBox.Visible = toggle;
            this.stringRegisterLabel.Visible = toggle;
            this.stringRegisterNumber.Visible = toggle;
            this.contentsLabel.Visible = toggle;
            this.contentsBox.Visible = toggle;
            this.readButton.Visible = toggle;
            this.writeButton.Visible = toggle;
            this.saltLabel.Visible = toggle;
            this.saltBox.Visible = toggle;
            this.copyButton.Visible = toggle;
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
                IClientPM client = (IClientPM)this.processes[clientID];
                client.Create(fileName, Convert.ToInt32(nbData), Convert.ToInt32(readq), Convert.ToInt32(writeq));
            }
        }

        private void open_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClientPM client = (IClientPM)this.processes[clientID];
                client.Open(fileName);
            }
        }

        private void delete_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClientPM client = (IClientPM)this.processes[clientID];
                client.Delete(fileName);
            }
        }

        private void close_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!clientID.Equals(string.Empty) && !fileName.Equals(string.Empty) && this.processes.Contains(clientID))
            {
                IClientPM client = (IClientPM)this.processes[clientID];
                client.Close(fileName);
            }
        }

        private void StartButtonClick(object sender, EventArgs e)
        {
            string id = this.processBox.Text;
            string port = this.portBox.Text;

            if (!String.IsNullOrEmpty(id) && !String.IsNullOrEmpty(port) && !this.processes.Contains(id))
            {
                switch (componentSelectionBox.SelectedIndex)
                {
                    case 0:
                        {
                            Process.Start("MetadataServer.exe", id + " " + port);
                            IServerPM metadata = (IServerPM)Activator.GetObject(typeof(IServerPM), "tcp://localhost:" + port + "/" + id);
                            this.processes.Add(id, metadata);
                            break;
                        }
                    case 1:
                        {
                            Process.Start("DataServer.exe", id + " " + port);
                            IDataServerPM dataServer = (IDataServerPM)Activator.GetObject(typeof(IDataServerPM), "tcp://localhost:" + port + "/" + id);
                            this.processes.Add(id, dataServer);
                            break;
                        }
                    case 3:
                        {
                            Process.Start("Client.exe", id + " " + port);
                            IClientPM client = (IClientPM)Activator.GetObject(typeof(IClientPM), "tcp://localhost:" + port + "/" + id);
                            this.processes.Add(id, client);
                            break;
                        }
                }

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
