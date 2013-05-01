using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PuppetMasterLog : Form
    {
        public PuppetMasterLog()
        {
            InitializeComponent();
        }

        public void AddLog(string msg)
        {
            if (this.logBox.InvokeRequired)
            {
                this.logBox.Invoke(new Action<string>(AddLog), msg);
                return;
            }   
            this.logBox.Text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + '\n';
        }
    }
}
