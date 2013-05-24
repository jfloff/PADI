using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PuppetMasterMain : Form
    {
        private int currentScriptLine = -1;
        private string filepath;

        private const int METADATA = 0;
        private const int DATASERVER = 1;
        private const int CLIENT = 2;

        private PuppetMasterLog log;

        public PuppetMasterMain()
        {
            InitializeComponent();
            // Initialize Log
            this.log = new PuppetMasterLog();
            this.log.Show();
            // Element details
            this.componentSelectionBox.SelectedIndex = METADATA;
            this.semanticsSelectionBox.SelectedIndex = 0;
            // Connection details
            TcpChannel channel = new TcpChannel(8080);
            ChannelServices.RegisterChannel(channel, true);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);
        }

        static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            // PuppetMaster.KillConsoles();
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
            this.processIdBox.Mask = "m-0";
            this.startButton.Enabled = true;
        }

        private void ToggleDataServerElements(bool toggle)
        {
            this.failButton.Visible = toggle;
            this.recoverButton.Visible = toggle;
            this.freezeButton.Visible = toggle;
            this.unfreezeButton.Visible = toggle;
            this.processIdBox.Mask = "d-0";
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
            this.byteRegisterLabel.Visible = toggle;
            this.byteRegisterNumber.Visible = toggle;
            this.contentsLabel.Visible = toggle;
            this.contentsBox.Visible = toggle;
            this.readButton.Visible = toggle;
            this.writeButton.Visible = toggle;
            this.saltLabel.Visible = toggle;
            this.saltBox.Visible = toggle;
            this.copyButton.Visible = toggle;
            this.processIdBox.Mask = "c-0";
            this.exeScriptButton.Visible = toggle;
        }

        private void DumpButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processIdBox.Text;

                PuppetMaster.DumpProcess(id);
                SetStatus("DUMP " + id);
            }
        }

        private void RecoverButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processIdBox.Text;

                PuppetMaster.RecoverProcess(id);
                SetStatus("RECOVER " + id);
            }
        }

        private void FailButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processIdBox.Text;

                PuppetMaster.FailProcess(id);
                SetStatus("FAIL " + id);
            }
        }

        private void UnfreezeButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processIdBox.Text;

                PuppetMaster.UnfreezeProcess(id);
                SetStatus("UNFREEZE " + id);
            }
        }

        private void FreezeButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processIdBox.Text;

                PuppetMaster.FreezeProcess(id);
                SetStatus("FREEZE " + id);
            }
        }

        private void CreateButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckFilename() && CheckNbDataServers() && CheckReadQuorum() && CheckWriteQuorum())
            {
                string id = this.processIdBox.Text;
                string filename = this.filenameBox.Text;
                int nbData = Convert.ToInt32(this.NbDataServersBox.Text);
                int readq = Convert.ToInt32(this.readQuorumBox.Text);
                int writeq = Convert.ToInt32(this.writeQuorumBox.Text);

                PuppetMaster.CreateFile(id, filename, nbData, readq, writeq);
                SetStatus("CREATE " + id + " => " + filename + ":" + nbData + ":" + readq + ":" + writeq);
            }
        }

        private void OpenButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckFilename())
            {
                string id = this.processIdBox.Text;
                string filename = this.filenameBox.Text;

                PuppetMaster.OpenFile(id, filename);
                SetStatus("OPEN " + id + " => " + filename);
            }
        }

        private void DeleteButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckFilename())
            {
                string id = this.processIdBox.Text;
                string filename = this.filenameBox.Text;

                PuppetMaster.DeleteFile(id, filename);
                SetStatus("DELETE " + id + " => " + filename);
            }
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckFilename())
            {
                string id = this.processIdBox.Text;
                string filename = this.filenameBox.Text;

                PuppetMaster.CloseFile(id, filename);
                SetStatus("CLOSE " + id + " => " + filename);
            }
        }

        private void StartButtonClick(object sender, EventArgs e)
        {
            if (CheckId() && CheckPort())
            {
                string id = this.processIdBox.Text;
                int port = Convert.ToInt32(this.portBox.Text);

                switch (componentSelectionBox.SelectedIndex)
                {
                    case METADATA:
                        {
                            PuppetMaster.StartMetadata(id, port);
                            PuppetMaster.RecoverProcess(id);
                            break;
                        }
                    case DATASERVER:
                        {
                            PuppetMaster.StartDataServer(id, port);
                            break;
                        }
                    case CLIENT:
                        {
                            PuppetMaster.StartClient(id, port);
                            break;
                        }
                }

                SetStatus("START " + id + ":" + port);

                this.processIdBox.Clear();
                this.portBox.Clear();
            }
        }

        private void ReadButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processIdBox.Text;
                int fileRegister = (int) this.fileRegister1Number.Value;
                string semantics = this.semanticsSelectionBox.ValueMember;
                int byteRegister = (int)this.byteRegisterNumber.Value;

                PuppetMaster.ReadFile(id, fileRegister, semantics, byteRegister);
                SetStatus("READ " + id + " => " + fileRegister + ":" + semantics + ":" + byteRegister);
            }
        }

        private void WriteButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processIdBox.Text;
                int fileRegister = (int)this.fileRegister1Number.Value;

                // write from byteRegister
                if (string.IsNullOrEmpty(this.filenameBox.Text))
                {
                    int byteRegister = (int)this.byteRegisterNumber.Value;

                    PuppetMaster.WriteFile(id, fileRegister, byteRegister);
                    SetStatus("WRITE " + id + " => " + fileRegister + ":" + byteRegister);
                }
                // write contents
                else
                {
                    string contents = this.contentsBox.Text;

                    PuppetMaster.WriteFile(id, fileRegister, contents);
                    SetStatus("WRITE " + id + " => " + fileRegister + ":" + contents);
                }
            }
        }

        private void CopyButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                string id = this.processIdBox.Text;
                int fileRegister1 = (int)this.fileRegister1Number.Value;
                int fileRegister2 = (int)this.fileRegister2Number.Value;
                string semantics = this.semanticsSelectionBox.ValueMember;
                string salt = saltBox.Text;

                PuppetMaster.CopyFile(id, fileRegister1, semantics, fileRegister2, salt);
                SetStatus("COPY " + id + " => " + fileRegister1 + ":" + semantics + ":" + fileRegister2 + ":" + salt);
            }
        }

        private void ExeScriptButtonClick(object sender, EventArgs e)
        {
            if (CheckId())
            {
                DialogResult result = this.openScriptDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string id = this.processIdBox.Text;
                    string filename = openScriptDialog.FileName;

                    ExeScript(id, filename);
                }
            }
        }

        private void ExeScript(string id, string filename)
        {
            PuppetMaster.GetProcess(id);

            Thread script = new Thread(() =>
            {
                int linenumber = 0;
                foreach (string line in File.ReadLines(filepath + filename))
                {
                    ReadCommand(line, linenumber++);
                }
            });
            script.Start();
        }

        /**
         * Field checkers
         */
        private bool CheckNbDataServers()
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

        private bool CheckReadQuorum()
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

        private bool CheckWriteQuorum()
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
            try
            {
                Convert.ToInt32(this.portBox.Text);
            }
            catch (Exception)
            {
                SetStatus("[ERROR] Port is invalid");
                return false;
            }
            return true;
        }

        private bool CheckId()
        {
            if (string.IsNullOrEmpty(this.processIdBox.Text))
            {
                SetStatus("[ERROR] Process Id is Empty");
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
                filepath = GetFilepath(filename);
                this.SetStatus("CURRENT FILEPATH = " + filepath);
                try
                {
                    this.scriptBox.Text = File.ReadAllText(filename);
                    this.currentScriptLine = -1;
                    SetStatus("LOAD SCRIPT " + filename);
                }
                catch (IOException)
                {
                    SetStatus("[ERROR] Failed to load script");
                }
            }
        }

        private string GetFilepath(string filename)
        {
            int index = filename.LastIndexOf("\\");
            return filename.Substring(0, index+1);
        }

        private void ExecuteAllScriptButtonClick(object sender, EventArgs e)
        {
            int lines = this.scriptBox.Lines.Length;
            this.currentScriptLine = -1;
            for (int i = 0; i < lines; i++)
            {
                // if breakpoint found
                if (NextStep()) break;
            }
        }

        private void NextStepScriptButtonClick(object sender, EventArgs e)
        {
            NextStep();
        }

        // returns true if break point
        private bool NextStep()
        {
            if (this.scriptBox.Lines.Length < 0 || this.currentScriptLine >= (this.scriptBox.Lines.Length - 1))
            {
                ++currentScriptLine;
                return false;
            }

            int lineNumber = ++currentScriptLine;

            string text = this.scriptBox.Text;
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
                this.scriptBox.ScrollToCaret();

                string lineToRun = this.scriptBox.SelectedText;
                return ReadCommand(lineToRun, previousLine);
            }

            return false;
        }

        // returns true if break point
        private bool ReadCommand(string lineToRun, int lineNumber){
            
            if (string.IsNullOrEmpty(lineToRun)) return false;

            // PARSER
            string[] steps = lineToRun.Split(new char[] { '\n', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (string.IsNullOrEmpty(steps[0])) return false;

            if (steps[0].First() == '#')
            {
                // #! is a BREAKPOINT 'laica boss'
                if (steps[0].Length == 2 && steps[0].Substring(0,2) == "#!") 
                    return true;

                return false;
            } 

            switch (steps[0])
            {
                // CREATE ID FILENAME NBDATASERVERS READQ WRITEQ
                case "CREATE":
                    {
                        if (steps.Length != 6)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        string filename = steps[2];
                        int nbDataServers = Convert.ToInt32(steps[3]);
                        int readq = Convert.ToInt32(steps[4]);
                        int writeq = Convert.ToInt32(steps[5]);

                        PuppetMaster.CreateFile(id, filename, nbDataServers, readq, writeq);
                        SetStatus("CREATE " + id + " => " + filename + ":" + nbDataServers + ":" + readq + ":" + writeq);

                        break;
                    }

                // DELETE ID FILENAME
                case "DELETE":
                    {
                        if (steps.Length != 3)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        string filename = steps[2];
                        PuppetMaster.DeleteFile(id, filename);
                        SetStatus("DELETE " + id + " => " + filename);
                        break;
                    }

                // OPEN ID FILENAME
                case "OPEN":
                    {
                        if (steps.Length != 3)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        string filename = steps[2];
                        PuppetMaster.OpenFile(id, filename);
                        SetStatus("OPEN " + id + " => " + filename);
                        break;
                    }

                // OPEN ID FILENAME
                case "CLOSE":
                    {
                        if (steps.Length != 3)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        string filename = steps[2];
                        PuppetMaster.CloseFile(id, filename);
                        SetStatus("CLOSE " + id + " => " + filename);
                        break;
                    }

                // FAIL ID
                case "FAIL":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        PuppetMaster.FailProcess(id);
                        SetStatus("FAIL " + id);
                        break;
                    }

                // RECOVER ID
                case "RECOVER":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        PuppetMaster.RecoverProcess(id);
                        SetStatus("RECOVER " + id);
                        break;
                    }

                // FREEZE ID
                case "FREEZE":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        PuppetMaster.FreezeProcess(id);
                        SetStatus("FREEZE " + id);
                        break;
                    }

                // UNFREEZE ID
                case "UNFREEZE":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        PuppetMaster.UnfreezeProcess(id);
                        SetStatus("UNFREEZE " + id);

                        break;
                    }

                // READ ID FILE REGISTER SEMANTICS STRING-REGISTER
                case "READ":
                    {
                        if (steps.Length != 5)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        int fileRegister = Convert.ToInt32(steps[2]);
                        string semantic = steps[3];
                        int byteRegister = Convert.ToInt32(steps[4]);

                        PuppetMaster.ReadFile(id, fileRegister, semantic, byteRegister);
                        SetStatus("READ " + id + " => " + fileRegister + ":" + semantic + ":" + byteRegister);

                        break;
                    }

                // WRITE ID FILE-REGISTER BYTE-ARRAY-REGISTER
                // WRITE ID FILE-REGISTER CONTENTS
                case "WRITE":
                    {
                        string[] writeSteps = lineToRun.Split(new char[] { ' ', ',' }, 4, StringSplitOptions.RemoveEmptyEntries);
                        if (writeSteps.Length != 4)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = writeSteps[1];
                        int fileRegister = Convert.ToInt32(writeSteps[2]);
                        string contents = writeSteps[3];

                        if (contents.First() != '"'){
                            int byteArrayRegister = Convert.ToInt32(contents);
                            PuppetMaster.WriteFile(id, fileRegister, byteArrayRegister);
                            SetStatus("WRITE " + id + " => " + fileRegister + ":" + byteArrayRegister);
                            return false;
                        }

                        string actualContents = contents.Substring(1,contents.Length-2);
                        PuppetMaster.WriteFile(id, fileRegister, actualContents);
                        SetStatus("WRITE " + id + " => " + fileRegister + ":" + actualContents);
                        break;
                    }

                // COPY ID FILE-REGISTER-1 SEMANTICS FILE-REGISTER-2 SALT
                case "COPY":
                    {
                        string[] copySteps = lineToRun.Split(new char[] { ' ', ',' }, 6, StringSplitOptions.RemoveEmptyEntries);

                        if (copySteps.Length != 6)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = copySteps[1];
                        int fileRegister1 = Convert.ToInt32(copySteps[2]);
                        int fileRegister2 = Convert.ToInt32(copySteps[4]);
                        string semantics = copySteps[3];
                        string salt = copySteps[5].Substring(1, copySteps[5].Length - 2);

                        PuppetMaster.CopyFile(id, fileRegister1, semantics, fileRegister2, salt);
                        SetStatus("COPY " + id + " => " + fileRegister1 + ":" + semantics + ":" + fileRegister2 + ":" + salt);

                        break;
                    }

                // DUMP ID
                case "DUMP":
                    {
                        if (steps.Length != 2)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        PuppetMaster.DumpProcess(id);
                        SetStatus("DUMP " + id);

                        break;
                    }

                // EXESCRIPT ID FILENAME
                case "EXESCRIPT":
                    {
                        if (steps.Length != 3)
                        {
                            SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                            return false;
                        }

                        string id = steps[1];
                        string filename = steps[2];

                        SetStatus("EXESCRIPT " + id + ":" + filename);

                        ExeScript(id, filename);

                        break;
                    }

                default:
                    {
                        SetStatus("[ERROR] Invalid Script at line " + lineNumber);
                        return false;
                    }
            }

            return false;
        }

        private void ProcessBoxMaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            SetStatus("[ERROR] Invalid Process ID name");
        }

        private void SetStatus(string msg)
        {
            if (statusStrip.InvokeRequired)
            {
                this.statusStrip.Invoke(new Action<string>(SetStatus), msg);
                return;
            }

            this.statusStripLabel.Text = msg;
            this.log.AddLog(msg);
        }

        private void PuppetMasterFormClosing(object sender, FormClosingEventArgs e)
        {
            PuppetMaster.KillConsoles();
        }

        private void ResetButtonClick(object sender, EventArgs e)
        {
            this.SetStatus("RESET PUPPET MASTER");
            PuppetMaster.Reset();
            currentScriptLine = -1;
            this.scriptBox.Clear();
        }
    }
}
