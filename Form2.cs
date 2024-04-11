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

namespace Double
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }


        async Task DelayAsync()
        {
            await Task.Delay(10);
        }
        private async void Form2_Load(object sender, EventArgs e){

            DateTime dt = DateTime.Now;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            label5.Text = "Ваш IP:    " + host.AddressList[host.AddressList.Length-1].ToString();

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

            }

            }

        Form1 f1;
        private void button1_Click(object sender, EventArgs e)
        {
         f1 = new Form1();
            f1.Show();
        }

    }
}
