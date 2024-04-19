using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace Double
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
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
    


    async Task DelayAsync()
        {
            await Task.Delay(10);
        }
        private async void Form2_Load(object sender, EventArgs e){

            /*Начальная настройка элементов меню */
            DateTime dt = DateTime.Now;

            UpdateAppSettings("My_IP", ""); //for auto-refinding IP every time

            string cur_IP = ConfigurationManager.AppSettings.Get("My_IP");

            if (cur_IP == "")
            {

                int findIp = 0;
                var host = Dns.GetHostEntry(Dns.GetHostName());

                for (int i = 0; i <= host.AddressList.Length - 1; i++)
                {
                    if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork) { findIp = i; }
                }

                label5.Text = "Ваш IP:    " + host.AddressList[findIp].ToString();
                UpdateAppSettings("My_IP", host.AddressList[findIp].ToString());
            }
            else
            {
                label5.Text = "Ваш IP:    " + cur_IP;
            }

            textBox1.Text = ConfigurationManager.AppSettings.Get("My_Name");

            string cur_Set = ConfigurationManager.AppSettings.Get("My_Settings");

            if (cur_Set[0]=='1')
            {
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }

            if (cur_Set[1] == '1')
            {
                checkBox2.Checked = true;
            }
            else
            {
                checkBox2.Checked = false;
            }

            if (cur_Set[2] == '1')
            {
                checkBox3.Checked = true;
            }
            else
            {
                checkBox3.Checked = false;
            }

            if (cur_Set[3] == '1')
            {
                checkBox4.Checked = true;
            }
            else
            {
                checkBox4.Checked = false;
            }


            /*-----------*/

            while (true)
            {
                await DelayAsync();
                dt = DateTime.Now;
                string hh = dt.Hour.ToString(); 
                if (dt.Hour < 10)
                {
                    hh = "0" + hh;
                }
                if (dt.Hour == 0)
                {
                   hh = "0" + hh;
                }

                string mm = dt.Minute.ToString();
                if (dt.Minute < 10)
                {
                    mm = "0" + mm;
                }
                if (dt.Minute == 0)
                {
                    mm = "0" + mm;
                }

                label4.Text = hh+":"+mm;
                string dd = dt.DayOfWeek.ToString();
                string mt = "";
                int mmt = dt.Month;

                switch (dd)
                {
                    case "Monday": 
                        dd = "Понедельник";
                        break;

                    case "Tuesday":
                        dd = "Вторник";
                        break;

                    case "Wednesday":
                        dd = "Среда";
                        break;

                    case "Thursday":
                        dd = "Четверг";
                        break;

                    case "Friday":
                        dd = "Пятница";
                        break;

                    case "Saturday":
                        dd = "Суббота";
                        break;

                    case "Sunday":
                        dd = "Воскресенье";
                        break;

                }

                switch (mmt)
                {
                    case 1:
                        mt = "Января";
                        break;

                    case 2:
                        mt = "Февраля";
                        break;

                    case 3:
                        mt = "Марта";
                        break;

                    case 4:
                        mt = "Апреля";
                        break;

                    case 5:
                        mt = "Мая";
                        break;

                    case 6:
                        mt = "Июня";
                        break;

                    case 7:
                        mt = "Июля";
                        break;

                    case 8:
                        mt = "Августа";
                        break;

                    case 9:
                        mt = "Сентября";
                        break;

                    case 10:
                        mt = "Октября";
                        break;

                    case 11:
                        mt = "Ноября";
                        break;

                    case 12:
                        mt = "Декабря";
                        break;

                }

                label6.Text = dd + ", " + dt.Day.ToString() + " " + mt;

                if (form3_Created) {
                    if (f3.IsDisposed)
                    {
                        this.Close();
                    }
                }

            }

            }

        Form3 f3;
        static bool form3_Created = false;

        Form4 f4;
        static bool form4_Created = false;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateAppSettings("My_Name", textBox1.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string cur_Set = ConfigurationManager.AppSettings.Get("My_Settings");
            char[] cur_Set_char = cur_Set.ToCharArray(); 

            if (checkBox1.Checked == true)
            {
                cur_Set_char[0] = '1';
            }
            else { cur_Set_char[0] = '0'; }

            string s1 = new string(cur_Set_char);
            UpdateAppSettings("My_Settings", s1);
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            string cur_Set = ConfigurationManager.AppSettings.Get("My_Settings");
            char[] cur_Set_char = cur_Set.ToCharArray();

            if (checkBox2.Checked == true)
            {
                cur_Set_char[1] = '1';
            }
            else { cur_Set_char[1] = '0'; }

            string s1 = new string(cur_Set_char);
            UpdateAppSettings("My_Settings", s1);
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            string cur_Set = ConfigurationManager.AppSettings.Get("My_Settings");
            char[] cur_Set_char = cur_Set.ToCharArray();

            if (checkBox3.Checked == true)
            {
                cur_Set_char[2] = '1';
            }
            else { cur_Set_char[2] = '0'; }

            string s1 = new string(cur_Set_char);
            UpdateAppSettings("My_Settings", s1);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            string cur_Set = ConfigurationManager.AppSettings.Get("My_Settings");
            char[] cur_Set_char = cur_Set.ToCharArray();

            if (checkBox4.Checked == true)
            {
                cur_Set_char[3] = '1';
            }
            else { cur_Set_char[3] = '0'; }

            string s1 = new string(cur_Set_char);
            UpdateAppSettings("My_Settings", s1);
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            f3 = new Form3();
            form3_Created = true;
            f3.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            f4 = new Form4();
            form4_Created = true;
            f4.Show();
            this.Hide();
        }
    }
}
