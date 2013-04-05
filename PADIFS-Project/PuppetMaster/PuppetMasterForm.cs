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
using SharedLibrary.Interfaces;
using Client;
using System.Diagnostics;
using System.Collections;
using System.IO;

// @TODO
// shortcuts
// enbable/disable so quando metadata created
// tab order
// masked text box for process
// VERDE o que ja foi executado
// LINHA ACTUAL A AMARELO NAO E EXECUTADA

namespace PuppetMaster
{
    public partial class PuppetMasterForm : Form
    {

        private int currentScriptLine = -1;

        private const int METADATA = 0;
        private const int DATASERVER = 1;
        private const int CLIENT = 2;

        public PuppetMasterForm()
        {
            InitializeComponent();
            // Element details
            this.componentSelectionBox.SelectedIndex = METADATA;
            this.semanticsSelectionBox.SelectedIndex = 0;
            // Connection details
            TcpChannel channel = new TcpChannel(8080);
            ChannelServices.RegisterChannel(channel, true);
        }

        private void ComponentSelectionBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox componentSelectionBox = (ComboBox)sender;

            switch (componentSelectionBox.SelectedIndex)
            {
                default:
                case METADATA:
                    {
                        ToggleDataServerElements(false);
                        ToggleClientElements(false);
                        ToggleMetadataElements(true);
                        break;
                    }
                case DATASERVER:
                    {
                        ToggleMetadataElements(false);
                        ToggleClientElements(false);
                        ToggleDataServerElements(true);
                        break;
                    }
                case CLIENT:
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
            this.executeClientScriptButton.Visible = toggle;
        }

        private void recover_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!string.IsNullOrEmpty(serverID) && this.processes.Contains(serverID))
            {
                IServerToPM server = (IServerToPM)this.processes[serverID];
                server.Recover();
            }
            else
            {
                setStatus("[ERROR] Process ID empty or not created");
            }
        }

        private void fail_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!string.IsNullOrEmpty(serverID) && this.processes.Contains(serverID))
            {
                IServerToPM server = (IServerToPM)this.processes[serverID];
                server.Fail();
            }
            else
            {
                setStatus("[ERROR] Process ID empty or not created");
            }
        }

        private void unfreeze_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!string.IsNullOrEmpty(serverID) && this.processes.Contains(serverID))
            {
                // Verificar se é um metadata server or data server (apenas data servers podem fazer unfreeze)
                IDataServerToPM server = (IDataServerToPM)this.processes[serverID];
                server.Unfreeze();
            }
            else
            {
                setStatus("[ERROR] Process ID empty or not created");
            }
        }

        private void freeze_click(object sender, EventArgs e)
        {
            string serverID = this.processBox.Text;
            if (!string.IsNullOrEmpty(serverID) && this.processes.Contains(serverID))
            {
                // Verificar se é um metadata server or data server (apenas data servers podem fazer freeze)
                IDataServerToPM server = (IDataServerToPM)this.processes[serverID];
                server.Freeze();
            }
            else
            {
                setStatus("[ERROR] Process ID empty or not created");
            }
        }

        private void Create_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            string nbData = this.NbDataServersBox.Text;
            string readq = this.readQuorumBox.Text;
            string writeq = this.writeQuorumBox.Text;

            if (!string.IsNullOrEmpty(fileName)
                && !string.IsNullOrEmpty(nbData)
                && !string.IsNullOrEmpty(readq)
                && !string.IsNullOrEmpty(writeq)
                && !string.IsNullOrEmpty(clientID)
                && this.processes.Contains(clientID))
            {
                IClientToPM client = (IClientToPM)this.processes[clientID];
                client.Create(fileName, Convert.ToInt32(nbData), Convert.ToInt32(readq), Convert.ToInt32(writeq));
            }
            else
            {
                setStatus("[ERROR] Process ID or File fields empty, or client not created");
            }
        }

        private void open_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!string.IsNullOrEmpty(clientID) && !string.IsNullOrEmpty(fileName) && this.processes.Contains(clientID))
            {
                IClientToPM client = (IClientToPM)this.processes[clientID];
                client.Open(fileName);
            }
            else
            {
                setStatus("[ERROR] Process ID or Filename empty, or client not created");
            }
        }

        private void delete_click(object sender, EventArgs e)
        {
            string clientID = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!string.IsNullOrEmpty(clientID) && !string.IsNullOrEmpty(fileName) && this.processes.Contains(clientID))
            {
                IClientToPM client = (IClientToPM)this.processes[clientID];
                client.Delete(fileName);
            }
            else
            {
                setStatus("[ERROR] Process ID or Filename empty, or client not created");
            }
        }

        private void close_click(object sender, EventArgs e)
        {
            string id = this.processBox.Text;
            string fileName = this.filenameBox.Text;
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(fileName) && PuppetMaster.idUsed(id))
            {
                
            }
            else
            {
                setStatus("[ERROR] Process ID or Filename empty, or client not created");
            }
        }

        private void StartButtonClick(object sender, EventArgs e)
        {
            string id = this.processBox.Text;
            string port = this.portBox.Text;

            // falta verificar se a porta já está a ser usada
            if (!string.IsNullOrEmpty(id)
                && !string.IsNullOrEmpty(port)
                && !PuppetMaster.portUsed(port)
                && !PuppetMaster.idUsed(id))
            {
                switch (componentSelectionBox.SelectedIndex)
                {
                    case METADATA:
                        {
                            PuppetMaster.StartMetadata(id, port);
                            setStatus("Created Metadata with id " + id + " at port " + port);
                            break;
                        }
                    case DATASERVER:
                        {
                            PuppetMaster.StartDataServer(id, port);
                            setStatus("Created Data Server with id " + id + " at port " + port);
                            break;
                        }
                    case CLIENT:
                        {
                            PuppetMaster.StartClient(id, port);
                            setStatus("Created Client with id " + id + " at port " + port);
                            break;
                        }
                }
                this.processBox.Clear();
                this.portBox.Clear();
            }
            else
            {
                setStatus("[ERROR] Empty Process ID or Port, Port already in use, or process already created");
            }
        }

        private void LoadScriptButtonClick(object sender, EventArgs e)
        {
            DialogResult result = this.openScriptDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string filename = openScriptDialog.FileName;
                try
                {
                    this.scriptBox.Text = File.ReadAllText(filename);
                    this.currentScriptLine = -1;
                    setStatus("Loaded script " + filename);
                }
                catch (IOException)
                {
                    setStatus("[ERROR] Failed to load script");
                }
            }
        }

        private void NextStepScriptButtonClick(object sender, EventArgs e)
        {
            if (this.scriptBox.Lines.Length >= 0 && this.currentScriptLine < this.scriptBox.Lines.Length - 1)
            {
                int lineNumber = ++currentScriptLine;

                int start = this.scriptBox.GetFirstCharIndexFromLine(lineNumber);
                int length = this.scriptBox.Lines[lineNumber].Length;
                this.scriptBox.Select(start, length);
                string line = this.scriptBox.SelectedText;
                this.scriptBox.SelectionBackColor = Color.Red;

                // reset previous line
                int previousLine = lineNumber - 1;
                if (previousLine >= 0)
                {
                    start = this.scriptBox.GetFirstCharIndexFromLine(previousLine);
                    length = this.scriptBox.Lines[previousLine].Length;
                    this.scriptBox.Select(start, length);
                    this.scriptBox.SelectionBackColor = SystemColors.Control;
                }

                // FALTA TRATAR LINHAS VAZIAS
                if (string.IsNullOrEmpty(line)) { }

                // PARSER
                string[] steps = line.Split(new string[] { "\n", " ", "," }, StringSplitOptions.None);

                if (!string.IsNullOrEmpty(steps[0]))
                {
                    if (steps[0].First() != '#')
                    {
                        switch (steps[0])
                        {
                            // RUN PROCESS ID PORT
                            case "RUN":
                                {
                                    if (steps.Length == 4)
                                    {
                                        string id = steps[2];
                                        string port = steps[3];
                                        switch (steps[1])
                                        {
                                            case "METADATA":
                                                {
                                                    PuppetMaster.StartMetadata(id, port);
                                                    setStatus("Created Metadata with id " + id + " at port " + port);
                                                    return;
                                                }
                                            case "DATASERVER":
                                                {
                                                    PuppetMaster.StartDataServer(id, port);
                                                    setStatus("Created Data Server with id " + id + " at port " + port);
                                                    return;
                                                }
                                            case "CLIENT":
                                                {
                                                    PuppetMaster.StartClient(id, port);
                                                    setStatus("Created Data Server with id " + id + " at port " + port);
                                                    return;
                                                }
                                            default: break;
                                        }
                                    }
                                    break;
                                }
                            // CREATE ID FILENAME NBDATASERVERS READQ WRITEQ
                            case "CREATE":
                                {
                                    if (steps.Length == 4)
                                    {
                                        string id = steps[1];
                                    }
                                    break;
                                }
                            default: break;
                        }

                        setStatus("[ERROR] Invalid Script at line " + lineNumber);
                        return;
                    }
                }
            }
            else
            {
                ++currentScriptLine;
            }
        }

        private void setStatus(string msg)
        {
            this.statusStripLabel.Text = msg;
        }
    }
}
