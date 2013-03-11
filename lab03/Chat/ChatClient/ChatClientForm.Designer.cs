namespace ChatClient
{
    partial class ChatClientForm
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
            this.portLabel = new System.Windows.Forms.Label();
            this.nickLabel = new System.Windows.Forms.Label();
            this.connectButton = new System.Windows.Forms.Button();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.nickTextBox = new System.Windows.Forms.TextBox();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.chatWindowRichTextBox = new System.Windows.Forms.RichTextBox();
            this.msgTextBox = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(193, 12);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 1;
            this.portLabel.Text = "Port:";
            // 
            // nickLabel
            // 
            this.nickLabel.AutoSize = true;
            this.nickLabel.Location = new System.Drawing.Point(12, 12);
            this.nickLabel.Name = "nickLabel";
            this.nickLabel.Size = new System.Drawing.Size(32, 13);
            this.nickLabel.TabIndex = 2;
            this.nickLabel.Text = "Nick:";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(99, 35);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(108, 23);
            this.connectButton.TabIndex = 3;
            this.connectButton.Text = "Connect!";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(228, 9);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(53, 20);
            this.portTextBox.TabIndex = 5;
            // 
            // nickTextBox
            // 
            this.nickTextBox.Location = new System.Drawing.Point(50, 9);
            this.nickTextBox.Name = "nickTextBox";
            this.nickTextBox.Size = new System.Drawing.Size(136, 20);
            this.nickTextBox.TabIndex = 6;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(297, 306);
            this.shapeContainer1.TabIndex = 7;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShape1
            // 
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 10;
            this.lineShape1.X2 = 286;
            this.lineShape1.Y1 = 70;
            this.lineShape1.Y2 = 70;
            // 
            // chatWindowRichTextBox
            // 
            this.chatWindowRichTextBox.Location = new System.Drawing.Point(10, 80);
            this.chatWindowRichTextBox.Name = "chatWindowRichTextBox";
            this.chatWindowRichTextBox.Size = new System.Drawing.Size(277, 183);
            this.chatWindowRichTextBox.TabIndex = 8;
            this.chatWindowRichTextBox.Text = "";
            // 
            // msgTextBox
            // 
            this.msgTextBox.Location = new System.Drawing.Point(10, 273);
            this.msgTextBox.Name = "msgTextBox";
            this.msgTextBox.Size = new System.Drawing.Size(213, 20);
            this.msgTextBox.TabIndex = 9;
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(229, 273);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(58, 30);
            this.sendButton.TabIndex = 10;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // ChatClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 306);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.msgTextBox);
            this.Controls.Add(this.chatWindowRichTextBox);
            this.Controls.Add(this.nickTextBox);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.nickLabel);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.shapeContainer1);
            this.Name = "ChatClientForm";
            this.Text = "Chat";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Label nickLabel;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.TextBox nickTextBox;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private System.Windows.Forms.RichTextBox chatWindowRichTextBox;
        private System.Windows.Forms.TextBox msgTextBox;
        private System.Windows.Forms.Button sendButton;
    }
}

