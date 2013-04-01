namespace PuppetMaster
{
    partial class PuppetMasterForm
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
            this.processBox = new System.Windows.Forms.TextBox();
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
            this.startClientButton = new System.Windows.Forms.Button();
            this.portBox = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.startMetadataButton = new System.Windows.Forms.Button();
            this.startDataServerButton = new System.Windows.Forms.Button();
            this.fileRegister1Label = new System.Windows.Forms.Label();
            this.semanticsLabel = new System.Windows.Forms.Label();
            this.semanticsBox = new System.Windows.Forms.TextBox();
            this.fileRegister1Box = new System.Windows.Forms.TextBox();
            this.byteArrayRegisterBox = new System.Windows.Forms.TextBox();
            this.byteArrayRegisterLabel = new System.Windows.Forms.Label();
            this.contentsLabel = new System.Windows.Forms.Label();
            this.contentsBox = new System.Windows.Forms.TextBox();
            this.fileRegister2Label = new System.Windows.Forms.Label();
            this.fileRegister2Box = new System.Windows.Forms.TextBox();
            this.saltLabel = new System.Windows.Forms.Label();
            this.saltBox = new System.Windows.Forms.TextBox();
            this.readButton = new System.Windows.Forms.Button();
            this.writeButton = new System.Windows.Forms.Button();
            this.writeContentsButton = new System.Windows.Forms.Button();
            this.copyButton = new System.Windows.Forms.Button();
            this.executeScriptButton = new System.Windows.Forms.Button();
            this.scriptNameBox = new System.Windows.Forms.TextBox();
            this.scriptNameLabel = new System.Windows.Forms.Label();
            this.dumpButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // failButton
            // 
            this.failButton.Location = new System.Drawing.Point(12, 166);
            this.failButton.Name = "failButton";
            this.failButton.Size = new System.Drawing.Size(75, 23);
            this.failButton.TabIndex = 0;
            this.failButton.Text = "Fail";
            this.failButton.UseVisualStyleBackColor = true;
            this.failButton.Click += new System.EventHandler(this.fail_click);
            // 
            // recoverButton
            // 
            this.recoverButton.Location = new System.Drawing.Point(99, 166);
            this.recoverButton.Name = "recoverButton";
            this.recoverButton.Size = new System.Drawing.Size(75, 23);
            this.recoverButton.TabIndex = 1;
            this.recoverButton.Text = "Recover";
            this.recoverButton.UseVisualStyleBackColor = true;
            this.recoverButton.Click += new System.EventHandler(this.recover_click);
            // 
            // processBox
            // 
            this.processBox.AutoCompleteCustomSource.AddRange(new string[] {
            "m-1",
            "m-2",
            "m-3",
            "d-1",
            "d-2",
            "d-3",
            "d-4",
            "d-5",
            "c-1",
            "c-2",
            "c-3",
            "c-4",
            "c-5",
            "c-6",
            "c-7",
            "c-8",
            "c-9",
            "c-10"});
            this.processBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.processBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.processBox.Location = new System.Drawing.Point(12, 25);
            this.processBox.Name = "processBox";
            this.processBox.Size = new System.Drawing.Size(81, 20);
            this.processBox.TabIndex = 2;
            // 
            // unfreezeButton
            // 
            this.unfreezeButton.Location = new System.Drawing.Point(99, 195);
            this.unfreezeButton.Name = "unfreezeButton";
            this.unfreezeButton.Size = new System.Drawing.Size(75, 23);
            this.unfreezeButton.TabIndex = 7;
            this.unfreezeButton.Text = "Unfreeze";
            this.unfreezeButton.UseVisualStyleBackColor = true;
            this.unfreezeButton.Click += new System.EventHandler(this.unfreeze_click);
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(207, 108);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(156, 23);
            this.createButton.TabIndex = 8;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.create_click);
            // 
            // freezeButton
            // 
            this.freezeButton.Location = new System.Drawing.Point(12, 195);
            this.freezeButton.Name = "freezeButton";
            this.freezeButton.Size = new System.Drawing.Size(75, 23);
            this.freezeButton.TabIndex = 9;
            this.freezeButton.Text = "Freeze";
            this.freezeButton.UseVisualStyleBackColor = true;
            this.freezeButton.Click += new System.EventHandler(this.freeze_click);
            // 
            // processLabel
            // 
            this.processLabel.AutoSize = true;
            this.processLabel.Location = new System.Drawing.Point(9, 9);
            this.processLabel.Name = "processLabel";
            this.processLabel.Size = new System.Drawing.Size(45, 13);
            this.processLabel.TabIndex = 10;
            this.processLabel.Text = "Process";
            // 
            // filenameBox
            // 
            this.filenameBox.Location = new System.Drawing.Point(205, 25);
            this.filenameBox.Name = "filenameBox";
            this.filenameBox.Size = new System.Drawing.Size(74, 20);
            this.filenameBox.TabIndex = 11;
            // 
            // filenameLabel
            // 
            this.filenameLabel.AutoSize = true;
            this.filenameLabel.Location = new System.Drawing.Point(205, 9);
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(54, 13);
            this.filenameLabel.TabIndex = 12;
            this.filenameLabel.Text = "File Name";
            // 
            // NbDataServersBox
            // 
            this.NbDataServersBox.Location = new System.Drawing.Point(286, 25);
            this.NbDataServersBox.Name = "NbDataServersBox";
            this.NbDataServersBox.Size = new System.Drawing.Size(74, 20);
            this.NbDataServersBox.TabIndex = 13;
            // 
            // readQuorumBox
            // 
            this.readQuorumBox.Location = new System.Drawing.Point(205, 74);
            this.readQuorumBox.Name = "readQuorumBox";
            this.readQuorumBox.Size = new System.Drawing.Size(74, 20);
            this.readQuorumBox.TabIndex = 14;
            // 
            // writeQuorumBox
            // 
            this.writeQuorumBox.Location = new System.Drawing.Point(286, 74);
            this.writeQuorumBox.Name = "writeQuorumBox";
            this.writeQuorumBox.Size = new System.Drawing.Size(74, 20);
            this.writeQuorumBox.TabIndex = 15;
            // 
            // NbDataServersLabel
            // 
            this.NbDataServersLabel.AutoSize = true;
            this.NbDataServersLabel.Location = new System.Drawing.Point(283, 9);
            this.NbDataServersLabel.Name = "NbDataServersLabel";
            this.NbDataServersLabel.Size = new System.Drawing.Size(83, 13);
            this.NbDataServersLabel.TabIndex = 16;
            this.NbDataServersLabel.Text = "NbData Servers";
            // 
            // readQuorumLabel
            // 
            this.readQuorumLabel.AutoSize = true;
            this.readQuorumLabel.Location = new System.Drawing.Point(202, 58);
            this.readQuorumLabel.Name = "readQuorumLabel";
            this.readQuorumLabel.Size = new System.Drawing.Size(73, 13);
            this.readQuorumLabel.TabIndex = 17;
            this.readQuorumLabel.Text = "Read Quorum";
            // 
            // writeQuorumLabel
            // 
            this.writeQuorumLabel.AutoSize = true;
            this.writeQuorumLabel.Location = new System.Drawing.Point(283, 58);
            this.writeQuorumLabel.Name = "writeQuorumLabel";
            this.writeQuorumLabel.Size = new System.Drawing.Size(69, 13);
            this.writeQuorumLabel.TabIndex = 18;
            this.writeQuorumLabel.Text = "WriteQuorum";
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(207, 137);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(156, 23);
            this.deleteButton.TabIndex = 19;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.delete_click);
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(208, 166);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(156, 23);
            this.openButton.TabIndex = 20;
            this.openButton.Text = "Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.open_click);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(207, 195);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(156, 23);
            this.closeButton.TabIndex = 21;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.close_click);
            // 
            // startClientButton
            // 
            this.startClientButton.Location = new System.Drawing.Point(13, 117);
            this.startClientButton.Name = "startClientButton";
            this.startClientButton.Size = new System.Drawing.Size(162, 27);
            this.startClientButton.TabIndex = 22;
            this.startClientButton.Text = "Start Client";
            this.startClientButton.UseVisualStyleBackColor = true;
            this.startClientButton.Click += new System.EventHandler(this.startClient_click);
            // 
            // portBox
            // 
            this.portBox.Location = new System.Drawing.Point(100, 25);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(74, 20);
            this.portBox.TabIndex = 23;
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(99, 9);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(26, 13);
            this.portLabel.TabIndex = 24;
            this.portLabel.Text = "Port";
            // 
            // startMetadataButton
            // 
            this.startMetadataButton.Location = new System.Drawing.Point(12, 51);
            this.startMetadataButton.Name = "startMetadataButton";
            this.startMetadataButton.Size = new System.Drawing.Size(162, 27);
            this.startMetadataButton.TabIndex = 25;
            this.startMetadataButton.Text = "Start Metadata";
            this.startMetadataButton.UseVisualStyleBackColor = true;
            this.startMetadataButton.Click += new System.EventHandler(this.startMetadata_click);
            // 
            // startDataServerButton
            // 
            this.startDataServerButton.Location = new System.Drawing.Point(12, 84);
            this.startDataServerButton.Name = "startDataServerButton";
            this.startDataServerButton.Size = new System.Drawing.Size(162, 27);
            this.startDataServerButton.TabIndex = 26;
            this.startDataServerButton.Text = "Start Data Server";
            this.startDataServerButton.UseVisualStyleBackColor = true;
            this.startDataServerButton.Click += new System.EventHandler(this.startDataServer_click);
            // 
            // fileRegister1Label
            // 
            this.fileRegister1Label.Location = new System.Drawing.Point(12, 243);
            this.fileRegister1Label.Name = "fileRegister1Label";
            this.fileRegister1Label.Size = new System.Drawing.Size(56, 48);
            this.fileRegister1Label.TabIndex = 27;
            this.fileRegister1Label.Text = "File Register 1";
            // 
            // semanticsLabel
            // 
            this.semanticsLabel.AutoSize = true;
            this.semanticsLabel.Location = new System.Drawing.Point(74, 260);
            this.semanticsLabel.Name = "semanticsLabel";
            this.semanticsLabel.Size = new System.Drawing.Size(56, 13);
            this.semanticsLabel.TabIndex = 28;
            this.semanticsLabel.Text = "Semantics";
            // 
            // semanticsBox
            // 
            this.semanticsBox.Location = new System.Drawing.Point(77, 276);
            this.semanticsBox.Name = "semanticsBox";
            this.semanticsBox.Size = new System.Drawing.Size(82, 20);
            this.semanticsBox.TabIndex = 30;
            // 
            // fileRegister1Box
            // 
            this.fileRegister1Box.Location = new System.Drawing.Point(15, 276);
            this.fileRegister1Box.Name = "fileRegister1Box";
            this.fileRegister1Box.Size = new System.Drawing.Size(53, 20);
            this.fileRegister1Box.TabIndex = 32;
            // 
            // byteArrayRegisterBox
            // 
            this.byteArrayRegisterBox.Location = new System.Drawing.Point(172, 276);
            this.byteArrayRegisterBox.Name = "byteArrayRegisterBox";
            this.byteArrayRegisterBox.Size = new System.Drawing.Size(54, 20);
            this.byteArrayRegisterBox.TabIndex = 33;
            // 
            // byteArrayRegisterLabel
            // 
            this.byteArrayRegisterLabel.Location = new System.Drawing.Point(169, 243);
            this.byteArrayRegisterLabel.Name = "byteArrayRegisterLabel";
            this.byteArrayRegisterLabel.Size = new System.Drawing.Size(57, 30);
            this.byteArrayRegisterLabel.TabIndex = 34;
            this.byteArrayRegisterLabel.Text = "Byte-Array Register";
            // 
            // contentsLabel
            // 
            this.contentsLabel.Location = new System.Drawing.Point(153, 309);
            this.contentsLabel.Name = "contentsLabel";
            this.contentsLabel.Size = new System.Drawing.Size(57, 14);
            this.contentsLabel.TabIndex = 35;
            this.contentsLabel.Text = "Contents";
            // 
            // contentsBox
            // 
            this.contentsBox.Location = new System.Drawing.Point(15, 326);
            this.contentsBox.Name = "contentsBox";
            this.contentsBox.Size = new System.Drawing.Size(345, 20);
            this.contentsBox.TabIndex = 36;
            // 
            // fileRegister2Label
            // 
            this.fileRegister2Label.Location = new System.Drawing.Point(244, 243);
            this.fileRegister2Label.Name = "fileRegister2Label";
            this.fileRegister2Label.Size = new System.Drawing.Size(62, 48);
            this.fileRegister2Label.TabIndex = 37;
            this.fileRegister2Label.Text = "File Register 2";
            // 
            // fileRegister2Box
            // 
            this.fileRegister2Box.Location = new System.Drawing.Point(246, 276);
            this.fileRegister2Box.Name = "fileRegister2Box";
            this.fileRegister2Box.Size = new System.Drawing.Size(47, 20);
            this.fileRegister2Box.TabIndex = 38;
            // 
            // saltLabel
            // 
            this.saltLabel.AutoSize = true;
            this.saltLabel.Location = new System.Drawing.Point(312, 260);
            this.saltLabel.Name = "saltLabel";
            this.saltLabel.Size = new System.Drawing.Size(25, 13);
            this.saltLabel.TabIndex = 39;
            this.saltLabel.Text = "Salt";
            // 
            // saltBox
            // 
            this.saltBox.Location = new System.Drawing.Point(312, 276);
            this.saltBox.Name = "saltBox";
            this.saltBox.Size = new System.Drawing.Size(51, 20);
            this.saltBox.TabIndex = 40;
            // 
            // readButton
            // 
            this.readButton.Location = new System.Drawing.Point(18, 352);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(156, 23);
            this.readButton.TabIndex = 41;
            this.readButton.Text = "Read";
            this.readButton.UseVisualStyleBackColor = true;
            // 
            // writeButton
            // 
            this.writeButton.Location = new System.Drawing.Point(18, 381);
            this.writeButton.Name = "writeButton";
            this.writeButton.Size = new System.Drawing.Size(156, 23);
            this.writeButton.TabIndex = 42;
            this.writeButton.Text = "Write";
            this.writeButton.UseVisualStyleBackColor = true;
            // 
            // writeContentsButton
            // 
            this.writeContentsButton.Location = new System.Drawing.Point(205, 352);
            this.writeContentsButton.Name = "writeContentsButton";
            this.writeContentsButton.Size = new System.Drawing.Size(156, 23);
            this.writeContentsButton.TabIndex = 43;
            this.writeContentsButton.Text = "Write Contents";
            this.writeContentsButton.UseVisualStyleBackColor = true;
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(205, 381);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(156, 23);
            this.copyButton.TabIndex = 44;
            this.copyButton.Text = "Copy";
            this.copyButton.UseVisualStyleBackColor = true;
            // 
            // executeScriptButton
            // 
            this.executeScriptButton.Location = new System.Drawing.Point(18, 468);
            this.executeScriptButton.Name = "executeScriptButton";
            this.executeScriptButton.Size = new System.Drawing.Size(156, 23);
            this.executeScriptButton.TabIndex = 45;
            this.executeScriptButton.Text = "Execute Script";
            this.executeScriptButton.UseVisualStyleBackColor = true;
            // 
            // scriptNameBox
            // 
            this.scriptNameBox.Location = new System.Drawing.Point(18, 442);
            this.scriptNameBox.Name = "scriptNameBox";
            this.scriptNameBox.Size = new System.Drawing.Size(156, 20);
            this.scriptNameBox.TabIndex = 46;
            // 
            // scriptNameLabel
            // 
            this.scriptNameLabel.AutoSize = true;
            this.scriptNameLabel.Location = new System.Drawing.Point(19, 421);
            this.scriptNameLabel.Name = "scriptNameLabel";
            this.scriptNameLabel.Size = new System.Drawing.Size(65, 13);
            this.scriptNameLabel.TabIndex = 47;
            this.scriptNameLabel.Text = "Script Name";
            // 
            // dumpButton
            // 
            this.dumpButton.Location = new System.Drawing.Point(205, 439);
            this.dumpButton.Name = "dumpButton";
            this.dumpButton.Size = new System.Drawing.Size(155, 52);
            this.dumpButton.TabIndex = 48;
            this.dumpButton.Text = "Dump";
            this.dumpButton.UseVisualStyleBackColor = true;
            // 
            // PuppetMasterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 515);
            this.Controls.Add(this.dumpButton);
            this.Controls.Add(this.scriptNameLabel);
            this.Controls.Add(this.scriptNameBox);
            this.Controls.Add(this.executeScriptButton);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.writeContentsButton);
            this.Controls.Add(this.writeButton);
            this.Controls.Add(this.readButton);
            this.Controls.Add(this.saltBox);
            this.Controls.Add(this.saltLabel);
            this.Controls.Add(this.fileRegister2Box);
            this.Controls.Add(this.fileRegister2Label);
            this.Controls.Add(this.contentsBox);
            this.Controls.Add(this.contentsLabel);
            this.Controls.Add(this.byteArrayRegisterLabel);
            this.Controls.Add(this.byteArrayRegisterBox);
            this.Controls.Add(this.fileRegister1Box);
            this.Controls.Add(this.semanticsBox);
            this.Controls.Add(this.semanticsLabel);
            this.Controls.Add(this.fileRegister1Label);
            this.Controls.Add(this.startDataServerButton);
            this.Controls.Add(this.startMetadataButton);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.startClientButton);
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
            this.Controls.Add(this.processBox);
            this.Controls.Add(this.recoverButton);
            this.Controls.Add(this.failButton);
            this.Name = "PuppetMasterForm";
            this.Text = "PuppetMaster";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button failButton;
        private System.Windows.Forms.Button recoverButton;
        private System.Windows.Forms.TextBox processBox;
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
        private System.Windows.Forms.Button startClientButton;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Button startMetadataButton;
        private System.Windows.Forms.Button startDataServerButton;
        private System.Windows.Forms.Label fileRegister1Label;
        private System.Windows.Forms.Label semanticsLabel;
        private System.Windows.Forms.TextBox semanticsBox;
        private System.Windows.Forms.TextBox fileRegister1Box;
        private System.Windows.Forms.TextBox byteArrayRegisterBox;
        private System.Windows.Forms.Label byteArrayRegisterLabel;
        private System.Windows.Forms.Label contentsLabel;
        private System.Windows.Forms.TextBox contentsBox;
        private System.Windows.Forms.Label fileRegister2Label;
        private System.Windows.Forms.TextBox fileRegister2Box;
        private System.Windows.Forms.Label saltLabel;
        private System.Windows.Forms.TextBox saltBox;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.Button writeButton;
        private System.Windows.Forms.Button writeContentsButton;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button executeScriptButton;
        private System.Windows.Forms.TextBox scriptNameBox;
        private System.Windows.Forms.Label scriptNameLabel;
        private System.Windows.Forms.Button dumpButton;

    }
}

