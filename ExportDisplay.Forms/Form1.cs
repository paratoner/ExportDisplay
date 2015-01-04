using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExportDisplay.Winsocket;

namespace ExportDisplay.Forms
{
    public partial class Form1 : Form
    {
        readonly AsynchronousServer _serverlistener = new AsynchronousServer();
        readonly AsynchronousClient _client = new AsynchronousClient();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _serverlistener.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _client.StartClient();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_client.IsConnected) _client.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label1.Text = _client.ipAddress.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _serverlistener.Stop();
        }
    }
}
