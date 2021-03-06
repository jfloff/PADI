namespace PuppetMaster
{
    partial class PuppetMasterMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.failButton = new System.Windows.Forms.Button();
            this.recoverButton = new System.Windows.Forms.Button();
            this.unfreezeButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.freezeButton = new System.Windows.Forms.Button();
            this.processLabel = new System.Windows.Forms.Label();
            this.filenameBox = new System.Windows.Forms.TextBox();
            this.filenameLabel = new System.Windows.Forms.Label();
            this.NbDataServersBox = new System.Windows.Forms.TextBox();
            this.readQuorumBox = new System.Windows.Forms.TextBox();
            this.writeQuorumBox = new System.Windows.Forms.TextBox();
            this.NbDataServersLabel = new System.Windows.Forms.Label();
            this.readQuorumLabel = new System.Windows.Forms.Label();
            this.writeQuorumLabel = new System.Windows.Forms.Label();
            this.deleteButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.portBox = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.fileRegister1Label = new System.Windows.Forms.Label();
            this.semanticsLabel = new System.Windows.Forms.Label();
            this.byteRegisterLabel = new System.Windows.Forms.Label();
            this.contentsLabel = new System.Windows.Forms.Label();
            this.contentsBox = new System.Windows.Forms.TextBox();
            this.fileRegister2Label = new System.Windows.Forms.Label();
            this.saltLabel = new System.Windows.Forms.Label();
            this.saltBox = new System.Windows.Forms.TextBox();
            this.readButton = new System.Windows.Forms.Button();
            this.writeButton = new System.Windows.Forms.Button();
            this.copyButton = new System.Windows.Forms.Button();
            this.loadScriptButton = new System.Windows.Forms.Button();
            this.dumpButton = new System.Windows.Forms.Button();
            this.componentSelectionBox = new System.Windows.Forms.ComboBox();
            this.semanticsSelectionBox = new System.Windows.Forms.ComboBox();
            this.byteRegisterNumber = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.fileRegister1Number = new System.Windows.Forms.NumericUpDown();
            this.fileRegister2Number = new System.Windows.Forms.NumericUpDown();
            this.openScriptDialog = new System.Windows.Forms.OpenFileDialog();
            this.nextStepScriptButton = new System.Windows.Forms.Button();
            this.executeAllScriptButton = new System.Windows.Forms.Button();
            this.scriptBox = new System.Windows.Forms.RichTextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.processIdBox = new System.Windows.Forms.MaskedTextBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.exeScriptButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.byteRegisterNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileRegister1Number)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileRegister2Number)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // failButton
            // 
            this.failButton.Location = new System.Drawing.Point(11, 138);
            this.failButton.Name = "failButton";
            this.failButton.Size = new System.Drawing.Size(75, 23);
            this.failButton.TabIndex = 26;
            this.failButton.Text = "Fail";
            this.failButton.UseVisualStyleBackColor = true;
            this.failButton.Visible = false;
            this.failButton.Click += new System.EventHandler(this.FailButtonClick);
            // 
            // recoverButton
            // 
            this.recoverButton.Location = new System.Drawing.Point(98, 138);
            this.recoverButton.Name = "recoverButton";
            this.recoverButton.Size = new System.Drawing.Size(75, 23);
            this.recoverButton.TabIndex = 28;
            this.recoverButton.Text = "Recover";
            this.recoverButton.UseVisualStyleBackColor = true;
            this.recoverButton.Visible = false;
            this.recoverButton.Click += new System.EventHandler(this.RecoverButtonClick);
            // 
            // unfreezeButton
            // 
            this.unfreezeButton.Location = new System.Drawing.Point(98, 166);
            this.unfreezeButton.Name = "unfreezeButton";
            this.unfreezeButton.Size = new System.Drawing.Size(75, 23);
            this.unfreezeButton.TabIndex = 29;
            this.unfreezeButton.Text = "Unfreeze";
            this.unfreezeButton.UseVisualStyleBackColor = true;
            this.unfreezeButton.Visible = false;
            this.unfreezeButton.Click += new System.EventHandler(this.UnfreezeButtonClick);
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(11, 221);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(75, 23);
            this.createButton.TabIndex = 9;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Visible = false;
            this.createButton.Click += new System.EventHandler(this.CreateButtonClick);
            // 
            // freezeButton
            // 
            this.freezeButton.Location = new System.Drawing.Point(11, 166);
            this.freezeButton.Name = "freezeButton";
            this.freezeButton.Size = new System.Drawing.Size(75, 23);
            this.freezeButton.TabIndex = 27;
            this.freezeButton.Text = "Freeze";
            this.freezeButton.UseVisualStyleBackColor = true;
            this.freezeButton.Visible = false;
            this.freezeButton.Click += new System.EventHandler(this.FreezeButtonClick);
            // 
            // processLabel
            // 
            this.processLabel.AutoSize = true;
            this.processLabel.Location = new System.Drawing.Point(8, 37);
            this.processLabel.Name = "processLabel";
            this.processLabel.Size = new System.Drawing.Size(45, 13);
            this.processLabel.TabIndex = 10;
            this.processLabel.Text = "Process";
            // 
            // filenameBox
            // 
            this.filenameBox.Location = new System.Drawing.Point(11, 156);
            this.filenameBox.Name = "filenameBox";
            this.filenameBox.Size = new System.Drawing.Size(75, 20);
            this.filenameBox.TabIndex = 5;
            this.filenameBox.Visible = false;
            // 
            // filenameLabel
            // 
            this.filenameLabel.AutoSize = true;
            this.filenameLabel.Location = new System.Drawing.Point(8, 140);
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(54, 13);
            this.filenameLabel.TabIndex = 12;
            this.filenameLabel.Text = "File Name";
            this.filenameLabel.Visible = false;
            // 
            // NbDataServersBox
            // 
            this.NbDataServersBox.Location = new System.Drawing.Point(98, 156);
            this.NbDataServersBox.Name = "NbDataServersBox";
            this.NbDataServersBox.Size = new System.Drawing.Size(75, 20);
            this.NbDataServersBox.TabIndex = 6;
            this.NbDataServersBox.Visible = false;
            // 
            // readQuorumBox
            // 
            this.readQuorumBox.Location = new System.Drawing.Point(11, 195);
            this.readQuorumBox.Name = "readQuorumBox";
            this.readQuorumBox.Size = new System.Drawing.Size(75, 20);
            this.readQuorumBox.TabIndex = 7;
            this.readQuorumBox.Visible = false;
            // 
            // writeQuorumBox
            // 
            this.writeQuorumBox.Location = new System.Drawing.Point(98, 195);
            this.writeQuorumBox.Name = "writeQuorumBox";
            this.writeQuorumBox.Size = new System.Drawing.Size(75, 20);
            this.writeQuorumBox.TabIndex = 8;
            this.writeQuorumBox.Visible = false;
            // 
            // NbDataServersLabel
            // 
            this.NbDataServersLabel.AutoSize = true;
            this.NbDataServersLabel.Location = new System.Drawing.Point(95, 140);
            this.NbDataServersLabel.Name = "NbDataServersLabel";
            this.NbDataServersLabel.Size = new System.Drawing.Size(83, 13);
            this.NbDataServersLabel.TabIndex = 16;
            this.NbDataServersLabel.Text = "NbData Servers";
            this.NbDataServersLabel.Visible = false;
            // 
            // readQuorumLabel
            // 
            this.readQuorumLabel.AutoSize = true;
            this.readQuorumLabel.Location = new System.Drawing.Point(8, 179);
            this.readQuorumLabel.Name = "readQuorumLabel";
            this.readQuorumLabel.Size = new System.Drawing.Size(73, 13);
            this.readQuorumLabel.TabIndex = 17;
            this.readQuorumLabel.Text = "Read Quorum";
            this.readQuorumLabel.Visible = false;
            // 
            // writeQuorumLabel
            // 
            this.writeQuorumLabel.AutoSize = true;
            this.writeQuorumLabel.Location = new System.Drawing.Point(98, 179);
            this.writeQuorumLabel.Name = "writeQuorumLabel";
            this.writeQuorumLabel.Size = new System.Drawing.Size(69, 13);
            this.writeQuorumLabel.TabIndex = 18;
            this.writeQuorumLabel.Text = "WriteQuorum";
            this.writeQuorumLabel.Visible = false;
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(11, 249);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 10;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Visible = false;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButtonClick);
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(98, 221);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 23);
            this.openButton.TabIndex = 11;
            this.openButton.Text = "Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Visible = false;
            this.openButton.Click += new System.EventHandler(this.OpenButtonClick);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(98, 250);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 12;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Visible = false;
            this.closeButton.Click += new System.EventHandler(this.CloseButtonClick);
            // 
            // portBox
            // 
            this.portBox.Location = new System.Drawing.Point(98, 53);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(75, 20);
            this.portBox.TabIndex = 1;
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(98, 37);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(26, 13);
            this.portLabel.TabIndex = 24;
            this.portLabel.Text = "Port";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(11, 79);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(162, 23);
            this.startButton.TabIndex = 3;
            this.startButton.Text = "Start!";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.StartButtonClick);
            // 
            // fileRegister1Label
            // 
            this.fileRegister1Label.Location = new System.Drawing.Point(8, 283);
            this.fileRegister1Label.Name = "fileRegister1Label";
            this.fileRegister1Label.Size = new System.Drawing.Size(75, 17);
            this.fileRegister1Label.TabIndex = 27;
            this.fileRegister1Label.Text = "File Register 1";
            this.fileRegister1Label.Visible = false;
            // 
            // semanticsLabel
            // 
            this.semanticsLabel.AutoSize = true;
            this.semanticsLabel.Location = new System.Drawing.Point(98, 283);
            this.semanticsLabel.Name = "semanticsLabel";
            this.semanticsLabel.Size = new System.Drawing.Size(56, 13);
            this.semanticsLabel.TabIndex = 28;
            this.semanticsLabel.Text = "Semantics";
            this.semanticsLabel.Visible = false;
            // 
            // stringRegisterLabel
            // 
            this.byteRegisterLabel.Location = new System.Drawing.Point(95, 321);
            this.byteRegisterLabel.Name = "stringRegisterLabel";
            this.byteRegisterLabel.Size = new System.Drawing.Size(78, 15);
            this.byteRegisterLabel.TabIndex = 34;
            this.byteRegisterLabel.Text = "String Register";
            this.byteRegisterLabel.Visible = false;
            // 
            // contentsLabel
            // 
            this.contentsLabel.Location = new System.Drawing.Point(8, 361);
            this.contentsLabel.Name = "contentsLabel";
            this.contentsLabel.Size = new System.Drawing.Size(57, 14);
            this.contentsLabel.TabIndex = 35;
            this.contentsLabel.Text = "Contents";
            this.contentsLabel.Visible = false;
            // 
            // contentsBox
            // 
            this.contentsBox.Location = new System.Drawing.Point(11, 378);
            this.contentsBox.Name = "contentsBox";
            this.contentsBox.Size = new System.Drawing.Size(162, 20);
            this.contentsBox.TabIndex = 18;
            this.contentsBox.Visible = false;
            // 
            // fileRegister2Label
            // 
            this.fileRegister2Label.Location = new System.Drawing.Point(8, 321);
            this.fileRegister2Label.Name = "fileRegister2Label";
            this.fileRegister2Label.Size = new System.Drawing.Size(75, 15);
            this.fileRegister2Label.TabIndex = 37;
            this.fileRegister2Label.Text = "File Register 2";
            this.fileRegister2Label.Visible = false;
            // 
            // saltLabel
            // 
            this.saltLabel.AutoSize = true;
            this.saltLabel.Location = new System.Drawing.Point(8, 430);
            this.saltLabel.Name = "saltLabel";
            this.saltLabel.Size = new System.Drawing.Size(25, 13);
            this.saltLabel.TabIndex = 39;
            this.saltLabel.Text = "Salt";
            this.saltLabel.Visible = false;
            // 
            // saltBox
            // 
            this.saltBox.Location = new System.Drawing.Point(11, 446);
            this.saltBox.Name = "saltBox";
            this.saltBox.Size = new System.Drawing.Size(75, 20);
            this.saltBox.TabIndex = 17;
            this.saltBox.Visible = false;
            // 
            // readButton
            // 
            this.readButton.Location = new System.Drawing.Point(11, 404);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(75, 23);
            this.readButton.TabIndex = 19;
            this.readButton.Text = "Read";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Visible = false;
            this.readButton.Click += new System.EventHandler(this.ReadButtonClick);
            // 
            // writeButton
            // 
            this.writeButton.Location = new System.Drawing.Point(98, 404);
            this.writeButton.Name = "writeButton";
            this.writeButton.Size = new System.Drawing.Size(75, 23);
            this.writeButton.TabIndex = 20;
            this.writeButton.Text = "Write";
            this.writeButton.UseVisualStyleBackColor = true;
            this.writeButton.Visible = false;
            this.writeButton.Click += new System.EventHandler(this.WriteButtonClick);
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(98, 443);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(75, 23);
            this.copyButton.TabIndex = 22;
            this.copyButton.Text = "Copy";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Visible = false;
            this.copyButton.Click += new System.EventHandler(this.CopyButtonClick);
            // 
            // loadScriptButton
            // 
            this.loadScriptButton.Location = new System.Drawing.Point(11, 528);
            this.loadScriptButton.Name = "loadScriptButton";
            this.loadScriptButton.Size = new System.Drawing.Size(162, 23);
            this.loadScriptButton.TabIndex = 24;
            this.loadScriptButton.Text = "Load Script";
            this.loadScriptButton.UseVisualStyleBackColor = true;
            this.loadScriptButton.Click += new System.EventHandler(this.LoadScriptButtonClick);
            // 
            // dumpButton
            // 
            this.dumpButton.Location = new System.Drawing.Point(11, 108);
            this.dumpButton.Name = "dumpButton";
            this.dumpButton.Size = new System.Drawing.Size(162, 23);
            this.dumpButton.TabIndex = 25;
            this.dumpButton.Text = "Dump Process";
            this.dumpButton.UseVisualStyleBackColor = true;
            this.dumpButton.Click += new System.EventHandler(this.DumpButtonClick);
            // 
            // componentSelectionBox
            // 
            this.componentSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.componentSelectionBox.FormattingEnabled = true;
            this.componentSelectionBox.Items.AddRange(new object[] {
            "Metadata",
            "Data Server",
            "Client"});
            this.componentSelectionBox.Location = new System.Drawing.Point(21, 12);
            this.componentSelectionBox.Name = "componentSelectionBox";
            this.componentSelectionBox.Size = new System.Drawing.Size(137, 21);
            this.componentSelectionBox.TabIndex = 48;
            this.componentSelectionBox.SelectedIndexChanged += new System.EventHandler(this.ComponentSelectionBoxSelectedIndexChanged);
            // 
            // semanticsSelectionBox
            // 
            this.semanticsSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.semanticsSelectionBox.FormattingEnabled = true;
            this.semanticsSelectionBox.Items.AddRange(new object[] {
            "default",
            "monotonic"});
            this.semanticsSelectionBox.Location = new System.Drawing.Point(98, 297);
            this.semanticsSelectionBox.Name = "semanticsSelectionBox";
            this.semanticsSelectionBox.Size = new System.Drawing.Size(75, 21);
            this.semanticsSelectionBox.TabIndex = 49;
            this.semanticsSelectionBox.Visible = false;
            // 
            // stringRegisterNumber
            // 
            this.byteRegisterNumber.Location = new System.Drawing.Point(98, 339);
            this.byteRegisterNumber.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.byteRegisterNumber.Name = "stringRegisterNumber";
            this.byteRegisterNumber.Size = new System.Drawing.Size(75, 20);
            this.byteRegisterNumber.TabIndex = 50;
            this.byteRegisterNumber.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(11, 501);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(162, 21);
            this.groupBox1.TabIndex = 51;
            this.groupBox1.TabStop = false;
            // 
            // fileRegister1Number
            // 
            this.fileRegister1Number.Location = new System.Drawing.Point(11, 297);
            this.fileRegister1Number.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.fileRegister1Number.Name = "fileRegister1Number";
            this.fileRegister1Number.Size = new System.Drawing.Size(75, 20);
            this.fileRegister1Number.TabIndex = 52;
            this.fileRegister1Number.Visible = false;
            // 
            // fileRegister2Number
            // 
            this.fileRegister2Number.Location = new System.Drawing.Point(11, 339);
            this.fileRegister2Number.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.fileRegister2Number.Name = "fileRegister2Number";
            this.fileRegister2Number.Size = new System.Drawing.Size(75, 20);
            this.fileRegister2Number.TabIndex = 53;
            this.fileRegister2Number.Visible = false;
            // 
            // openScriptDialog
            // 
            this.openScriptDialog.DefaultExt = "padis";
            this.openScriptDialog.FileName = "main.padis";
            this.openScriptDialog.Filter = "PADI-FS Scripts (*.padis)|*.padis|All files|*.*";
            // 
            // nextStepScriptButton
            // 
            this.nextStepScriptButton.Location = new System.Drawing.Point(11, 669);
            this.nextStepScriptButton.Name = "nextStepScriptButton";
            this.nextStepScriptButton.Size = new System.Drawing.Size(75, 23);
            this.nextStepScriptButton.TabIndex = 58;
            this.nextStepScriptButton.Text = "Next Step";
            this.nextStepScriptButton.UseVisualStyleBackColor = true;
            this.nextStepScriptButton.Click += new System.EventHandler(this.NextStepScriptButtonClick);
            // 
            // executeAllScriptButton
            // 
            this.executeAllScriptButton.Location = new System.Drawing.Point(98, 668);
            this.executeAllScriptButton.Name = "executeAllScriptButton";
            this.executeAllScriptButton.Size = new System.Drawing.Size(75, 23);
            this.executeAllScriptButton.TabIndex = 59;
            this.executeAllScriptButton.Text = "Execute All";
            this.executeAllScriptButton.UseVisualStyleBackColor = true;
            this.executeAllScriptButton.Click += new System.EventHandler(this.ExecuteAllScriptButtonClick);
            // 
            // scriptBox
            // 
            this.scriptBox.Location = new System.Drawing.Point(11, 557);
            this.scriptBox.Name = "scriptBox";
            this.scriptBox.ReadOnly = true;
            this.scriptBox.Size = new System.Drawing.Size(162, 106);
            this.scriptBox.TabIndex = 60;
            this.scriptBox.Text = "";
            this.scriptBox.WordWrap = false;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusStripLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 730);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(184, 22);
            this.statusStrip.TabIndex = 61;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusStripLabel
            // 
            this.statusStripLabel.Name = "statusStripLabel";
            this.statusStripLabel.Size = new System.Drawing.Size(60, 17);
            this.statusStripLabel.Text = "Welcome!";
            // 
            // processBox
            // 
            this.processIdBox.Location = new System.Drawing.Point(11, 52);
            this.processIdBox.Mask = "c-0";
            this.processIdBox.Name = "processBox";
            this.processIdBox.Size = new System.Drawing.Size(75, 20);
            this.processIdBox.TabIndex = 62;
            this.processIdBox.MaskInputRejected += new System.Windows.Forms.MaskInputRejectedEventHandler(this.ProcessBoxMaskInputRejected);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(11, 698);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(162, 23);
            this.resetButton.TabIndex = 63;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.ResetButtonClick);
            // 
            // exeScriptButton
            // 
            this.exeScriptButton.Location = new System.Drawing.Point(11, 472);
            this.exeScriptButton.Name = "exeScriptButton";
            this.exeScriptButton.Size = new System.Drawing.Size(162, 23);
            this.exeScriptButton.TabIndex = 64;
            this.exeScriptButton.Text = "Load ExeScript";
            this.exeScriptButton.UseVisualStyleBackColor = true;
            this.exeScriptButton.Click += new System.EventHandler(this.ExeScriptButtonClick);
            // 
            // PuppetMasterMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(184, 752);
            this.Controls.Add(this.exeScriptButton);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.processIdBox);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.scriptBox);
            this.Controls.Add(this.executeAllScriptButton);
            this.Controls.Add(this.nextStepScriptButton);
            this.Controls.Add(this.fileRegister2Number);
            this.Controls.Add(this.fileRegister1Number);
            this.Controls.Add(this.byteRegisterNumber);
            this.Controls.Add(this.semanticsSelectionBox);
            this.Controls.Add(this.componentSelectionBox);
            this.Controls.Add(this.dumpButton);
            this.Controls.Add(this.loadScriptButton);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.writeButton);
            this.Controls.Add(this.readButton);
            this.Controls.Add(this.saltBox);
            this.Controls.Add(this.saltLabel);
            this.Controls.Add(this.fileRegister2Label);
            this.Controls.Add(this.contentsBox);
            this.Controls.Add(this.contentsLabel);
            this.Controls.Add(this.byteRegisterLabel);
            this.Controls.Add(this.semanticsLabel);
            this.Controls.Add(this.fileRegister1Label);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.writeQuorumLabel);
            this.Controls.Add(this.readQuorumLabel);
            this.Controls.Add(this.NbDataServersLabel);
            this.Controls.Add(this.writeQuorumBox);
            this.Controls.Add(this.readQuorumBox);
            this.Controls.Add(this.NbDataServersBox);
            this.Controls.Add(this.filenameLabel);
            this.Controls.Add(this.filenameBox);
            this.Controls.Add(this.processLabel);
            this.Controls.Add(this.freezeButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.unfreezeButton);
            this.Controls.Add(this.recoverButton);
            this.Controls.Add(this.failButton);
            this.Controls.Add(this.groupBox1);
            this.Name = "PuppetMasterMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "git";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PuppetMasterFormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.byteRegisterNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileRegister1Number)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileRegister2Number)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button failButton;
        private System.Windows.Forms.Button recoverButton;
        private System.Windows.Forms.Button unfreezeButton;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button freezeButton;
        private System.Windows.Forms.Label processLabel;
        private System.Windows.Forms.TextBox filenameBox;
        private System.Windows.Forms.Label filenameLabel;
        private System.Windows.Forms.TextBox NbDataServersBox;
        private System.Windows.Forms.TextBox readQuorumBox;
        private System.Windows.Forms.TextBox writeQuorumBox;
        private System.Windows.Forms.Label NbDataServersLabel;
        private System.Windows.Forms.Label readQuorumLabel;
        private System.Windows.Forms.Label writeQuorumLabel;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label fileRegister1Label;
        private System.Windows.Forms.Label semanticsLabel;
        private System.Windows.Forms.Label byteRegisterLabel;
        private System.Windows.Forms.Label contentsLabel;
        private System.Windows.Forms.TextBox contentsBox;
        private System.Windows.Forms.Label fileRegister2Label;
        private System.Windows.Forms.Label saltLabel;
        private System.Windows.Forms.TextBox saltBox;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.Button writeButton;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button loadScriptButton;
        private System.Windows.Forms.Button dumpButton;
        private System.Windows.Forms.ComboBox componentSelectionBox;
        private System.Windows.Forms.ComboBox semanticsSelectionBox;
        private System.Windows.Forms.NumericUpDown byteRegisterNumber;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown fileRegister1Number;
        private System.Windows.Forms.NumericUpDown fileRegister2Number;
        private System.Windows.Forms.OpenFileDialog openScriptDialog;
        private System.Windows.Forms.Button nextStepScriptButton;
        private System.Windows.Forms.Button executeAllScriptButton;
        private System.Windows.Forms.RichTextBox scriptBox;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLabel;
        private System.Windows.Forms.MaskedTextBox processIdBox;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button exeScriptButton;

    }
}﻿
