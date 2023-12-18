using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AForge.Video.DirectShow;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace Double
{
    public partial class Form1 : Form
    {

        private bool Sound_connection;
        //сокет отправитель
        Socket Sound_Sender;
        //поток для нашей речи
        WaveIn Sound_Record;
        //поток для речи собеседника
        WaveOut Sound_Play;
        //буфферный поток для передачи через сеть
        BufferedWaveProvider Sound_bufferStream;
        //сокет для приема (протокол UDP)
        Socket Sound_Receiver;
        private static IPEndPoint consumerEndPoint;
        private static UdpClient UdpClient = new UdpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private void Microphone_Init()
        {
            //создаем поток для записи нашей речи
            Sound_Record = new WaveIn();
            //определяем его формат - частота дискретизации 8000 Гц, ширина сэмпла - 16 бит, 1 канал - моно
            Sound_Record.WaveFormat = new WaveFormat(8000, 16, 1);
            //добавляем код обработки нашего голоса, поступающего на микрофон
            Sound_Record.DataAvailable += Voice_Input;
            //создаем поток для прослушивания входящего звука
            Sound_Play = new WaveOut();
            //создаем поток для буферного потока и определяем у него такой же формат как и потока с микрофона
            Sound_bufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
            //привязываем поток входящего звука к буферному потоку
            Sound_Play.Init(Sound_bufferStream);
            //сокет для отправки звука
            Sound_Sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Sound_Receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Sound_connection = true;
        }
        private async void Form1_Load(object sender, EventArgs e)
        {

            Microphone_Init();

            //создаем поток для прослушивания
            Thread THREAD_Sound_Listen = new Thread(new ThreadStart(Listening));
            //запускаем его
            THREAD_Sound_Listen.Start();

            Thread THREAD_Sound_Send = new Thread(new ThreadStart(Sound_Record.StartRecording));
            THREAD_Sound_Send.Start();

            Thread THREAD_Video_Send = new Thread(new ThreadStart(SenderMain));
            THREAD_Video_Send.Start();

            Thread.Sleep(0);

            var port = int.Parse(ConfigurationManager.AppSettings.Get("port"));
            var client = new UdpClient(port);

            while (true)
            {
                var data = await client.ReceiveAsync();
                using (var ms = new MemoryStream(data.Buffer))
                {
                    pictureBox1.Image = new Bitmap(ms);
                }
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Sound_connection = false;
            Sound_Receiver.Close();
            Sound_Receiver.Dispose();

            Sound_Sender.Close();
            Sound_Sender.Dispose();
            if (Sound_Play != null)
            {
                Sound_Play.Stop();
                Sound_Play.Dispose();
                Sound_Play = null;
            }
            if (Sound_Record != null)
            {
                Sound_Record.Dispose();
                Sound_Record = null;
            }
            Sound_bufferStream = null;
        }

        private void Voice_Input(object sender, WaveInEventArgs e)
        {
            try
            {
                var consumerIp = ConfigurationManager.AppSettings.Get("consumerIp");
                //Подключаемся к удаленному адресу
                IPEndPoint remote_point = new IPEndPoint(IPAddress.Parse(consumerIp), 5555);
                //посылаем байты, полученные с микрофона на удаленный адрес
                Sound_Sender.SendTo(e.Buffer, remote_point);
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
            IPEndPoint localIP = new IPEndPoint(IPAddress.Any, 5555);
            Sound_Receiver.Bind(localIP);
            //начинаем воспроизводить входящий звук
            Sound_Play.Play();
            Sound_Play.Volume = 1; //100%
            //адрес, с которого пришли данные
            EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
            //бесконечный цикл
            while (Sound_connection == true)
            {
                try
                {
                    //промежуточный буфер
                    byte[] data = new byte[65535];
                    //получено данных
                    int received = Sound_Receiver.ReceiveFrom(data, ref remoteIp);
                    //добавляем данные в буфер, откуда Sound_Play будет воспроизводить звук
                    Sound_bufferStream.AddSamples(data, 0, received);
                    Thread.Sleep(10);
                }
                catch (SocketException ex)
                { }
            }
        }


        static void SenderMain()
        {
            var consumerIp = ConfigurationManager.AppSettings.Get("consumerIp");
            var consumerPort = int.Parse(ConfigurationManager.AppSettings.Get("consumerPort"));
            consumerEndPoint = new IPEndPoint(IPAddress.Parse(consumerIp), consumerPort);

            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();


        }

        private static void VideoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            var bmp = new Bitmap(eventArgs.Frame, 800, 600);
            try
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();
                    UdpClient.Send(bytes, bytes.Length, consumerEndPoint);
                    Thread.Sleep(0);
                }

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
    }
}
