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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Timers;

namespace Double
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        async Task DelayAsync()
        {
            await Task.Delay(1000);
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

        private void ListenPort()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 1200);
            UdpClient server = new UdpClient(ip);

            //while (true)
            //{


                byte[] data = server.Receive(ref ip);
                string ch = Encoding.Unicode.GetString(data, 0, data.Length);

                if (ch.Length > 0)
                {
                    UpdateAppSettings("Connection_IP", ch);
                    server.Close();

                    clientConnected = true;
                    
                }

            //}
        }

        Form1 f1;

        bool clientConnected = false;
        private async void Form3_Load(object sender, EventArgs e)
        {

            Thread THREAD_listen = new Thread(new ThreadStart(ListenPort));
            //запускаем его
            THREAD_listen.Start();

            label2.Text = "Ваш IP: " + ConfigurationManager.AppSettings.Get("My_IP");

            int timer = 0;
            while (true)
            {
                await DelayAsync();
                timer++;
                label1.Text = "Ожидание подключения, " + timer.ToString() + " сек.";

                if (clientConnected)
                { break; }

                Thread.Sleep(0);
            }

            THREAD_listen.Abort();

            f1 = new Form1();
            f1.Show();
            this.Hide();




        }
    }
}
