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

namespace PuppetMaster
{
    public partial class PuppetMasterForm : Form
    {
        private bool flagDataServer = false;
        private bool flagMetadata = false;

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
            this.processBox.Mask = "m-0";
            this.startButton.Enabled = !flagDataServer;
        }

        private void ToggleDataServerElements(bool toggle)
        {
            this.failButton.Visible = toggle;
            this.recoverButton.Visible = toggle;
            this.freezeButton.Visible = toggle;
            this.unfreezeButton.Visible = toggle;
            this.processBox.Mask = "d-0";
            this.startButton.Enabled = flagMetadata;
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
            this.processBox.Mask = "c-0";
            this.startButton.Enabled = flagMetadata;
        }

        private void RecoverButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processBox.Text;

                PuppetMaster.RecoverProcess(id);
                SetStatus("Recovered process " + id);
            }
        }

        private void FailButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processBox.Text;

                PuppetMaster.FailProcess(id);
                SetStatus("Failed process " + id);
            }
        }

        private void UnfreezeButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processBox.Text;

                PuppetMaster.UnfreezeProcess(id);
                SetStatus("Freezed process " + id);
            }
        }

        private void FreezeButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processBox.Text;

                PuppetMaster.FreezeProcess(id);
                SetStatus("Freezed process " + id);
            }
        }

        private void CreateButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckFilename() && CheckNbData() && CheckReadQ() && CheckWriteQ())
            {
                string id = this.processBox.Text;
                string filename = this.filenameBox.Text;
                int nbData = Convert.ToInt32(this.NbDataServersBox.Text);
                int readq = Convert.ToInt32(this.readQuorumBox.Text);
                int writeq = Convert.ToInt32(this.writeQuorumBox.Text);

                PuppetMaster.CreateFile(id, filename, nbData, readq, writeq);
                SetStatus("Created file " + filename);
            }
        }

        private void OpenButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckFilename())
            {
                string id = this.processBox.Text;
                string filename = this.filenameBox.Text;

                PuppetMaster.OpenFile(id, filename);
                SetStatus("Opened file " + filename);
            }
        }

        private void DeleteButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckFilename())
            {
                string id = this.processBox.Text;
                string filename = this.filenameBox.Text;

                PuppetMaster.DeleteFile(id, filename);
                SetStatus("Deleted file " + filename);
            }
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckFilename())
            {
                string id = this.processBox.Text;
                string filename = this.filenameBox.Text;

                PuppetMaster.CloseFile(id, filename);
                SetStatus("Closed file " + filename);
            }
        }

        private void StartButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckPort())
            {
                string id = this.processBox.Text;
                int port = Convert.ToInt32(this.portBox.Text);

                switch (componentSelectionBox.SelectedIndex)
                {
                    case METADATA:
                        {
                            PuppetMaster.StartMetadata(id, port);
                            flagMetadata = true;
                            SetStatus("Created Metadata with id " + id + " at port " + port);
                            break;
                        }
                    case DATASERVER:
                        {
                            PuppetMaster.StartDataServer(id, port);
                            flagDataServer = true;
                            SetStatus("Created Data Server with id " + id + " at port " + port);
                            break;
                        }
                    case CLIENT:
                        {
                            PuppetMaster.StartClient(id, port);
                            SetStatus("Created Client with id " + id + " at port " + port);
                            break;
                        }
                }

                this.processBox.Clear();
                this.portBox.Clear();
            }
        }

        /**
         * Field checkers
         */
        private bool CheckNbData()
        {
            try
            {
                Convert.ToInt32(this.NbDataServersBox.Text);
                return true;
            }
            catch (Exception)
            {
                SetStatus("[ERROR] NbDataServers is invalid");
                return false;
            }
        }

        private bool CheckReadQ()
        {
            try
            {
                Convert.ToInt32(this.readQuorumBox.Text);
                return true;
            }
            catch (Exception)
            {
                SetStatus("[ERROR] Read Quorum is invalid");
                return false;
            }
        }

        private bool CheckWriteQ()
        {
            try
            {
                Convert.ToInt32(this.writeQuorumBox.Text);
                return true;
            }
            catch (Exception)
            {
                SetStatus("[ERROR] Write Quorum is invalid");
                return false;
            }
        }

        private bool CheckFilename()
        {
            if (string.IsNullOrEmpty(this.filenameBox.Text))
            {
                SetStatus("[ERROR] Filename is empty");
                return false;
            }

            return true;
        }

        private bool CheckPort()
        {
            int port;

            try
            {
                port = Convert.ToInt32(this.portBox.Text);
            }
            catch (Exception)
            {
                SetStatus("[ERROR] Port is invalid");
                return false;
            }

            if (PuppetMaster.portUsed(port))
            {
                SetStatus("[ERROR] Port already in used");
                return false;
            }

            return true;
        }

        private bool CheckId()
        {
            if (string.IsNullOrEmpty(this.processBox.Text))
            {
                SetStatus("[ERROR] Process Id is Empty");
                return false;
            }

            if (PuppetMaster.idUsed(this.processBox.Text))
            {
                SetStatus("[ERROR] Id already in used");
                return false;
            }

            return true;
        }

        private void LoadScriptButtonClick(object sender, EventArgs e)
        {
            DialogResult result = this.openScriptDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string filename = openScriptDialog.FileName;
                try
                {
                    this.scriptBox.Text = File.ReadAllText(filename);
                    this.currentScriptLine = -1;
                    SetStatus("Loaded script " + filename);
                }
                catch (IOException)
                {
                    SetStatus("[ERROR] Failed to load script");
                }
            }
        }

        private void ExecuteAllScriptButtonClick(object sender, EventArgs e)
        {
            int lines = this.scriptBox.Lines.Length;
            this.currentScriptLine = -1;
            for (int i = 0; i < lines; i++)
            {
                this.NextStepScriptButtonClick(null, null);
            }
        }

        private void NextStepScriptButtonClick(object sender, EventArgs e)
        {
            if (this.scriptBox.Lines.Length < 0 || this.currentScriptLine > this.scriptBox.Lines.Length - 1)
            {
                ++currentScriptLine;
                return;
            }

            int lineNumber = ++currentScriptLine;

            int start = this.scriptBox.GetFirstCharIndexFromLine(lineNumber);
            int length = this.scriptBox.Lines[lineNumber].Length;
            this.scriptBox.Select(start, length);
            this.scriptBox.SelectionBackColor = Color.Yellow;

            // get previous line
            int previousLine = lineNumber - 1;
            if (previousLine >= 0)
            {
                start = this.scriptBox.GetFirstCharIndexFromLine(previousLine);
                length = this.scriptBox.Lines[previousLine].Length;
                this.scriptBox.Select(start, length);
                this.scriptBox.SelectionBackColor = Color.Green;
            }

            string lineToRun = this.scriptBox.SelectedText;

            if (string.IsNullOrEmpty(lineToRun)) return;

            // PARSER
            string[] steps = lineToRun.Split(new string[] { "\n", " ", "," }, StringSplitOptions.None);

            if (string.IsNullOrEmpty(steps[0]) || steps[0].First() == '#') return;

            switch (steps[0])
            {
                // RUN PROCESS ID PORT
                case "RUN":
                    {
                        if (steps.Length != 4)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }
                        string id = steps[2];
                        int port = Convert.ToInt32(steps[3]);
                        switch (steps[1])
                        {
                            case "METADATA":
                                {
                                    PuppetMaster.StartMetadata(id, port);
                                    SetStatus("Created Metadata with id " + id + " at port " + port);
                                    return;
                                }
                            case "DATASERVER":
                                {
                                    PuppetMaster.StartDataServer(id, port);
                                    SetStatus("Created Data Server with id " + id + " at port " + port);
                                    return;
                                }
                            case "CLIENT":
                                {
                                    PuppetMaster.StartClient(id, port);
                                    SetStatus("Created Data Server with id " + id + " at port " + port);
                                    return;
                                }
                            default: break;
                        }

                        break;
                    }

                // CREATE ID FILENAME NBDATASERVERS READQ WRITEQ
                case "CREATE":
                    {
                        if (steps.Length != 6)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }

                        string id = steps[1];
                        string filename = steps[2];
                        int nbDataServers = Convert.ToInt32(steps[3]);
                        int readq = Convert.ToInt32(steps[4]);
                        int writeq = Convert.ToInt32(steps[5]);

                        PuppetMaster.CreateFile(id, filename, nbDataServers, readq, writeq);
                        break;
                    }

                // DELTE ID FILENAME
                case "DELETE":
                    {
                        if (steps.Length != 3)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }

                        string id = steps[1];
                        string filename = steps[2];
                        PuppetMaster.DeleteFile(id, filename);
                        break;
                    }

                // OPEN ID FILENAME
                case "OPEN":
                    {
                        if (steps.Length != 3)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }

                        string id = steps[1];
                        string filename = steps[2];
                        PuppetMaster.OpenFile(id, filename);
                        break;
                    }

                // OPEN ID FILENAME
                case "CLOSE":
                    {
                        if (steps.Length != 3)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }

                        string id = steps[1];
                        string filename = steps[2];
                        PuppetMaster.CloseFile(id, filename);
                        break;
                    }

                // FAIL ID
                case "FAIL":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }

                        string id = steps[1];
                        PuppetMaster.FailProcess(id);
                        break;
                    }

                // RECOVER ID
                case "RECOVER":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }

                        string id = steps[1];
                        PuppetMaster.RecoverProcess(id);
                        break;
                    }

                // FREEZE ID
                case "FREEZE":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }

                        string id = steps[1];
                        PuppetMaster.FreezeProcess(id);
                        break;
                    }

                // UNFREEZE ID
                case "UNFREEZE":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + previousLine);
                            return;
                        }

                        string id = steps[1];
                        PuppetMaster.UnfreezeProcess(id);
                        break;
                    }

                default:
                    {
                        SetStatus("[ERROR] Invalid Script at line " + previousLine);
                        return;
                    }
            }
        }

        private void ProcessBoxMaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            SetStatus("Invalid Process ID name");
        }

        private void SetStatus(string msg)
        {
            this.statusStripLabel.Text = msg;
        }
    }
}
