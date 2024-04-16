using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Double
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        Form1 f1;

        private void Form4_Load(object sender, EventArgs e)
        {
           

        }

        private void button1_Click(object sender, EventArgs e)
        {
            UdpClient client = new UdpClient(textBox1.Text, 1200);
            string ch = "192.168.1.161";
            byte[] data = Encoding.Unicode.GetBytes(ch);
            client.Send(data, data.Length);
            client.Close();

            f1 = new Form1();
            this.Hide();
            f1.Show();
        }
    }


}
