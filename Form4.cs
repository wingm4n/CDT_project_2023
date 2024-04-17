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

        static void UpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            UpdateAppSettings("Connection_IP",textBox1.Text);
            
            UdpClient client = new UdpClient(textBox1.Text, 1200);



            string ch = ConfigurationManager.AppSettings.Get("My_IP");
            byte[] data = Encoding.Unicode.GetBytes(ch);
            client.Send(data, data.Length);
            client.Close();

            f1 = new Form1();
            this.Hide();
            f1.Show();
        }
    }


}
