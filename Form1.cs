using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using AForge.Video.DirectShow;
using NAudio.Wave;
using System.Text;
using AForge.Video;

namespace Double
{
    public partial class Form1 : Form
    {

        private bool Audio_connection;
        //сокет отправитель
        Socket Audio_Sender;
        //поток для нашей речи
        WaveIn Audio_Record;
        //поток для речи собеседника
        WaveOut Audio_Play;
        //буфферный поток для передачи через сеть
        BufferedWaveProvider Audio_bufferStream;
        //сокет для приема (протокол UDP)
        Socket Audio_Receiver;
        private static IPEndPoint consumerEndPoint;
        private static UdpClient UdpClient = new UdpClient();


        private IPAddress Connection_Ip = IPAddress.Parse(ConfigurationManager.AppSettings.Get("Connection_Ip"));
        private static int My_Audio_Port = int.Parse(ConfigurationManager.AppSettings.Get("My_Audio_Port"));
        public static int My_Video_Port = int.Parse(ConfigurationManager.AppSettings.Get("My_Video_Port"));
        private static int Conn_Video_Port = int.Parse(ConfigurationManager.AppSettings.Get("Conn_Video_Port"));
        private static int Conn_Audio_Port = int.Parse(ConfigurationManager.AppSettings.Get("Conn_Audio_Port"));

        private bool isVideoAlive = true;
        //private static int dataOffset = 1;

        Thread THREAD_Sound_Listen;
        Thread THREAD_Sound_Send;
        Thread THREAD_Video_Send;



        public Form1()
        {
            InitializeComponent();

            pictureBox5.Controls.Add(pictureBox6);
            pictureBox6.Location = new Point(485, 58); //mute
            pictureBox6.BackColor = Color.Transparent;

            pictureBox5.Controls.Add(pictureBox7);
            pictureBox7.Location = new Point(685, 58); //stop Video
            pictureBox7.BackColor = Color.Transparent;

            pictureBox5.Controls.Add(pictureBox10);
            pictureBox10.Location = new Point(885, 33); //end call
            pictureBox10.BackColor = Color.Transparent;

            pictureBox5.Controls.Add(pictureBox8);
            pictureBox8.Location = new Point(1135, 58); //volume
            pictureBox8.BackColor = Color.Transparent;

            pictureBox5.Controls.Add(pictureBox9);
            pictureBox9.Location = new Point(1335, 58); //settings
            pictureBox9.BackColor = Color.Transparent;

            pictureBox5.Controls.Add(pictureBox13);
            pictureBox13.Location = new Point(585, 40); //red dots
            pictureBox13.BackColor = Color.Transparent;

            pictureBox5.Controls.Add(pictureBox14);
            pictureBox14.Location = new Point(785, 40);
            pictureBox14.BackColor = Color.Transparent;


            isMuted = false;
            isStoppedVideo = false;
            pictureBox13.Hide(); pictureBox14.Hide() ; label1.Hide();
           // if (ConfigurationManager.AppSettings.Get("My_settings")[0] == '1') { isMuted = true; pictureBox13.Show(); } else { isMuted = false; pictureBox13.Hide(); };
           // if (ConfigurationManager.AppSettings.Get("My_settings")[1] == '1') { isStoppedVideo = true; pictureBox14.Show(); label1.Show(); } else { isStoppedVideo = false; pictureBox14.Hide(); label1.Hide(); };
            if (ConfigurationManager.AppSettings.Get("My_settings")[2] == '1') { label2.Text = ConfigurationManager.AppSettings.Get("My_Name"); } else { label2.Hide();  };
            if (ConfigurationManager.AppSettings.Get("My_settings")[3] == '1') { label3.Text = ConfigurationManager.AppSettings.Get("My_IP"); } else { label3.Hide(); };

            if ((ConfigurationManager.AppSettings.Get("My_settings")[2] == '0') && (ConfigurationManager.AppSettings.Get("My_settings")[3] == '0')) { pictureBox15.Hide();  };


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

        private void Microphone_Init()
        {
            //создаем поток для записи нашей речи
            Audio_Record = new WaveIn();
            //определяем его формат - частота дискретизации 8000 Гц, ширина сэмпла - 16 бит, 1 канал - моно
            Audio_Record.WaveFormat = new WaveFormat(8000, 16, 1);
            //добавляем код обработки нашего голоса, поступающего на микрофон
            Audio_Record.DataAvailable += Voice_Input;
        }
            //создаем поток для прослушивания входящего звука
            
        
        private async void Form1_Load(object sender, EventArgs e)
        {

            trackBar1.Hide();

            Microphone_Init();

            Audio_Play = new WaveOut();
            //создаем поток для буферного потока и определяем у него такой же формат как и потока с микрофона
            Audio_bufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));

            Audio_bufferStream.DiscardOnBufferOverflow = true;
            //привязываем поток входящего звука к буферному потоку
            Audio_Play.Init(Audio_bufferStream);
            //сокет для отправки звука
            Audio_Sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Audio_Receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Audio_connection = true;

            //создаем поток для прослушивания
            THREAD_Sound_Listen = new Thread(new ThreadStart(Listening));
            //запускаем его
            THREAD_Sound_Listen.Start();

            THREAD_Sound_Send = new Thread(new ThreadStart(Audio_Record.StartRecording));
            THREAD_Sound_Send.Start();

            THREAD_Video_Send = new Thread(new ThreadStart(SenderMain));
            THREAD_Video_Send.Start();

            //

            Thread.Sleep(1000);

            EventArgs a = new EventArgs();
            if (ConfigurationManager.AppSettings.Get("My_settings")[0] == '1') { pictureBox7_Click(this, a); }

            a = new EventArgs();
            if (ConfigurationManager.AppSettings.Get("My_settings")[1] == '1') { pictureBox6_Click(this, a); };




            var Video_Recieve = new UdpClient(Conn_Video_Port);

            byte packageCount = 0;

            while (true)
            {
                var data = await Video_Recieve.ReceiveAsync();
                byte[] picdata = data.Buffer;
                
                packageCount = picdata[picdata.Length - 1];
                Array.Resize(ref picdata, picdata.Length - 1);

                

                if (packageCount == 1)
                {
                    using (var ms = new MemoryStream(picdata))
                    {

                        //           var key = "cR??7[?|";

                        //        cryptor.Decrypt(ms, key);
                        
               
                         pictureBox1.Image = new Bitmap(ms); 
                        

                    }
                }

                if (packageCount == 2)
                {
                    using (var ms = new MemoryStream(picdata))
                    {
                
                        pictureBox2.Image = new Bitmap(ms); 


                    }
                }

                if (packageCount == 3)
                {
                    using (var ms = new MemoryStream(picdata))
                    {

               
                         pictureBox3.Image = new Bitmap(ms);


                    }
                }

                if (packageCount == 4)
                {
                    using (var ms = new MemoryStream(picdata))
                    {

                        //if (isStoppedVideo) { pictureBox4.BackColor = Color.DarkGray; }
                       pictureBox4.Image = new Bitmap(ms); 
                      
                       

                    }
                }

                if (packageCount == 5)
                {
                    using (var ms = new MemoryStream(picdata))
                    {

                        
                        pictureBox5.Image = new Bitmap(ms);

                    }
                }


            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Audio_connection = false;
            Audio_Receiver.Close();
            Audio_Receiver.Dispose();

            Audio_Sender.Close();
            Audio_Sender.Dispose();
            if (Audio_Play != null)
            {
                Audio_Play.Stop();
                Audio_Play.Dispose();
                Audio_Play = null;
            }
            if (Audio_Record != null)
            {
                Audio_Record.Dispose();
                Audio_Record = null;
            }
            Audio_bufferStream = null;
        }

        private void Voice_Input(object sender, WaveInEventArgs e)
        {
            try
            {
                //Подключаемся к удаленному адресу
                IPEndPoint remote_point = new IPEndPoint(Connection_Ip, Conn_Audio_Port);
                //посылаем байты, полученные с микрофона на удаленный адрес
                Audio_Sender.SendTo(e.Buffer, remote_point);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //Прослушивание входящих подключений
        private void Listening()
        {
            //Прослушиваем по адресу
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, My_Audio_Port);
            Audio_Receiver.Bind(localIP);
            //начинаем воспроизводить входящий звук
            Audio_Play.Play();
            Audio_Play.Volume = 1; //100%
            //адрес, с которого пришли данные
            EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
            //бесконечный цикл
            while (Audio_connection == true)
            {
                try
                {
                    //промежуточный буфер
                    byte[] data = new byte[65535];
                    //получено данных
                    int received = Audio_Receiver.ReceiveFrom(data, ref remoteIp);
                    //добавляем данные в буфер, откуда Audio_Play будет воспроизводить звук
                    
                    Audio_bufferStream.AddSamples(data, 0, received);
                    
                    Thread.Sleep(10);
                }
                catch (SocketException ex)
                { }
            }
        }


        static VideoCaptureDevice videoSource;
        static FilterInfoCollection videoDevices;

        static void SenderMain()
        {
            var consumerIp = ConfigurationManager.AppSettings.Get("Connection_Ip");
            var consumerPort = int.Parse(ConfigurationManager.AppSettings.Get("Conn_Video_Port"));
            consumerEndPoint = new IPEndPoint(IPAddress.Parse(consumerIp), My_Video_Port);

            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();

        }

        private static void VideoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            var bmp = new Bitmap(eventArgs.Frame, 1920, 1080);
            try
            {



                using (var ms = new MemoryStream())
                {

                    RectangleF cloneRect = new RectangleF(0, 0, 1920, 216);
                    System.Drawing.Imaging.PixelFormat format =
                        bmp.PixelFormat;
                    Bitmap cloneBitmap = bmp.Clone(cloneRect, format);

                    cloneBitmap.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();

                    Array.Resize(ref bytes, bytes.Length + 1);
                    bytes[bytes.Length - 1] = 1;

                    UdpClient.Send(bytes, bytes.Length, consumerEndPoint);

                }

                using (var ms = new MemoryStream())
                {

                    RectangleF cloneRect = new RectangleF(0, 217, 1920, 216);
                    System.Drawing.Imaging.PixelFormat format =
                        bmp.PixelFormat;
                    Bitmap cloneBitmap = bmp.Clone(cloneRect, format);

                    cloneBitmap.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();

                    Array.Resize(ref bytes, bytes.Length + 1);
                    bytes[bytes.Length - 1] = 2;

                    UdpClient.Send(bytes, bytes.Length, consumerEndPoint);

                }

                using (var ms = new MemoryStream())
                {

                    RectangleF cloneRect = new RectangleF(0, 433, 1920, 216);
                    System.Drawing.Imaging.PixelFormat format =
                        bmp.PixelFormat;
                    Bitmap cloneBitmap = bmp.Clone(cloneRect, format);

                    cloneBitmap.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();

                    Array.Resize(ref bytes, bytes.Length + 1);
                    bytes[bytes.Length - 1] = 3;

                    UdpClient.Send(bytes, bytes.Length, consumerEndPoint);

                }

                using (var ms = new MemoryStream())
                {

                    RectangleF cloneRect = new RectangleF(0, 649, 1920, 216);
                    System.Drawing.Imaging.PixelFormat format =
                        bmp.PixelFormat;
                    Bitmap cloneBitmap = bmp.Clone(cloneRect, format);

                    cloneBitmap.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();

                    Array.Resize(ref bytes, bytes.Length + 1);
                    bytes[bytes.Length - 1] = 4;

                    UdpClient.Send(bytes, bytes.Length, consumerEndPoint);

                }

                using (var ms = new MemoryStream())
                {

                    RectangleF cloneRect = new RectangleF(0, 865, 1920, 215);
                    System.Drawing.Imaging.PixelFormat format =
                        bmp.PixelFormat;
                    Bitmap cloneBitmap = bmp.Clone(cloneRect, format);

                    cloneBitmap.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();

                    Array.Resize(ref bytes, bytes.Length + 1);
                    bytes[bytes.Length - 1] = 5;

                    UdpClient.Send(bytes, bytes.Length, consumerEndPoint);
                    
                }

                Thread.Sleep(0);

            }
            catch (Exception e)
            {
            }
        }


        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            MessageBox.Show(string.Join("\n", host.AddressList.Where(i => i.AddressFamily == AddressFamily.InterNetwork)));
        }

        bool isMuted; //get from appsets
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            if (isMuted)
            {
                Microphone_Init(); Audio_Record.StartRecording(); isMuted = false; pictureBox13.Hide();
            }
            else
            {
                Audio_Record.StopRecording(); Audio_Record.Dispose(); isMuted = true; pictureBox13.Show();
            }
        }

        bool isStoppedVideo; //appsettings
        private void pictureBox7_Click(object sender, EventArgs e)
        {
            if (isStoppedVideo)
            {
                videoSource.Start(); isStoppedVideo = false; pictureBox14.Hide(); label1.Hide(); 
            }
            else
            {
                videoSource.Stop(); isStoppedVideo = true; pictureBox14.Show(); label1.Show(); 
            }
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            Audio_connection = false;    //THREAD_Sound_Listen.Abort();
            Audio_Record.StopRecording(); Audio_Record.Dispose();//THREAD_Sound_Send.Abort();
            videoSource.Stop(); //THREAD_Video_Send.Abort();

            THREAD_Sound_Listen.Abort();
            THREAD_Sound_Send.Abort();
            THREAD_Video_Send.Abort();

            this.Close();

        }

        bool istrackBarShown = false;
        private void pictureBox8_Click(object sender, EventArgs e)
        {
            if (istrackBarShown)
            {
                trackBar1.Hide(); istrackBarShown = false;
            }
            else
            {
                trackBar1.Show(); istrackBarShown = true;
            }
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Audio_Play.Volume = (float)trackBar1.Value / 10;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
