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
            IServerPM server = (IServerPM)this.processes[this.processBox.Text];
            server.recover();
        }

        private void fail_click(object sender, EventArgs e)
        {
            IServerPM server = (IServerPM)this.processes[this.processBox.Text];
            server.fail();
        }

        private void unfreeze_click(object sender, EventArgs e)
        {
            IDataServerPM server = (IDataServerPM)this.processes[this.processBox.Text];
            server.unfreeze();
        }

        private void freeze_click(object sender, EventArgs e)
        {
            IDataServerPM server = (IDataServerPM)this.processes[this.processBox.Text];
            server.freeze();
        }

        private void create_click(object sender, EventArgs e)
        {
            IClient client = (IClient)this.processes[this.processBox.Text];
            client.create();
        }

        private void delete_click(object sender, EventArgs e)
        {
            IClient client = (IClient)this.processes[this.processBox.Text];
            client.delete();
        }

        private void open_click(object sender, EventArgs e)
        {
            IClient client = (IClient)this.processes[this.processBox.Text];
            client.open();
        }

        private void close_click(object sender, EventArgs e)
        {
            IClient client = (IClient)this.processes[this.processBox.Text];
            client.close();
        }

        private void startClient_click(object sender, EventArgs e)
        {
            String clientID = this.processBox.Text;
            String port = this.portBox.Text;

            if (!this.processes.Contains(clientID))
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
            String metadataID = this.processBox.Text;
            String port = this.portBox.Text;

            if (!this.processes.Contains(metadataID))
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
            String dataID = this.processBox.Text;
            String port = this.portBox.Text;

            if (!this.processes.Contains(dataID))
            {
                Process.Start("DataServer.exe", dataID + " " + port);
                IDataServerPM metadata = (IDataServerPM)Activator.GetObject(typeof(IDataServerPM), "tcp://localhost:" + port + "/" + dataID);
                this.processes.Add(dataID, metadata);
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
