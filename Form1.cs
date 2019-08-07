using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace UDP_Broadcast_v1
{
    public partial class Form1 : Form
    {

        static IPAddress remoteAddresss;
        static string localAddress;
        static string remoteAddress; // хост для отправки данных
        IPEndPoint remoteIp = null;
        static int remotePort; // порт для отправки данных
        static int localPort; // локальный порт для прослушивания входящих подключений


        BroadcastForm f;

        bool mode = true; // true - передавач

        UdpClient udpSender;
        IPEndPoint endPoint;

        UdpClient udpReceiver;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            localAddress = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
            myIPlabel.Text = "ВАШ IP АДРЕС:  " + localAddress;
            TextField1.Text= "230.168.1.2";

        }

        private void connectBtn2_Click(object sender, EventArgs e)
        {
            remoteAddresss = IPAddress.Parse(TextField1.Text);
            udpReceiver = new UdpClient(Convert.ToInt32(TextFieldPort.Text));
            udpReceiver.JoinMulticastGroup(remoteAddresss);

            f = new BroadcastForm(udpReceiver);
            f.ShowDialog();
            timerSend.Enabled = false;
        }

        private void BroadCastBtn2_Click(object sender, EventArgs e)
        {
            remoteAddresss = IPAddress.Parse(TextField1.Text);

            udpSender = new UdpClient();


            remoteAddress = TextField1.Text;
            remotePort = Convert.ToInt32(TextFieldPort.Text);

            endPoint = new IPEndPoint(remoteAddresss, remotePort);

            timerSend.Enabled = true;
            
        }
        
        private void timerSend_Tick(object sender, EventArgs e)
        {
            
            ScreenCapture sc = new ScreenCapture();
            Image img = sc.CaptureScreen();

            pictureBox1.Image = resizeImage(img, pictureBox1.Size);

            //--------------------ВІДПРАВКА------------------------

            Bitmap imageToSent= (Bitmap)img;

            if (materialRadioButton3.Checked)
            {
                imageToSent = (Bitmap)resizeImage(imageToSent,new Size(Convert.ToInt32(imageToSent.Width/4), Convert.ToInt32(imageToSent.Height / 4)));
            }
            else if(materialRadioButton4.Checked)
            {
                imageToSent = (Bitmap)resizeImage(imageToSent, new Size(Convert.ToInt32(imageToSent.Width / 2), Convert.ToInt32(imageToSent.Height / 2)));

            }


            var ms = new MemoryStream();

            imageToSent.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);


            byte[] allData = ms.ToArray();

            int amountOfDataArrays = (int)(ms.ToArray().Length / 65000.0) + 1;

            byte[][] separatedData = new byte[amountOfDataArrays][];



            bool oneTime = true;
            int k = 0;

            for (int i = 0; i < amountOfDataArrays; i++)  // розділяю на декілька підмасивів
            {
                if (ms.ToArray().Length % amountOfDataArrays != 0 && oneTime)
                {
                    oneTime = false;
                    separatedData[i] = new byte[ms.ToArray().Length / amountOfDataArrays + 1];
                }
                else
                {
                    separatedData[i] = new byte[ms.ToArray().Length / amountOfDataArrays];
                }

                for (int j = 0; j < separatedData[i].Length; j++)
                {

                    separatedData[i][j] = allData[k];
                    k++;
                }
            }

            byte[] temp = separatedData[0];
            separatedData[0] = new byte[temp.Length + 1];
            separatedData[0][0] = (byte)amountOfDataArrays;
            for (int i = 0; i < temp.Length; i++)
            {
                separatedData[0][i + 1] = temp[i];
            }

            //udpSender.Send(allData, allData.Length, remoteAddress, remotePort);


            for (int j = 0; j < amountOfDataArrays; j++)
            {
                udpSender.SendAsync(separatedData[j], separatedData[j].Length, endPoint);
                System.Threading.Thread.Sleep(100);
            }
            

        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        

        private void Form1_Resize(object sender, EventArgs e)
        {
           // pbSizeLabel.Text = pictureBox1.Width + " x " + pictureBox1.Height;
        }

       
        private void materialRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (materialRadioButton2.Checked)
            {
                mode = false;

                
                materialDivider1.BackColor = Color.White;
                numericUpDown1.Visible = false;
                

                groupBox1.Visible = false;
                materialLabel3.Visible = false;
                materialLabel5.Visible = false;
                connectBtn2.Enabled = true;
                BroadCastBtn2.Enabled = false;

                pictureBox1.Visible = false;
                this.Width = 350;


            }
            else
            {
                mode = true;

                
                materialDivider1.BackColor = Color.FromArgb(241, 242, 246);
                numericUpDown1.Visible =true;


                groupBox1.Visible = true;
                materialLabel3.Visible = true;
                materialLabel5.Visible = true;
                connectBtn2.Enabled = false;
                BroadCastBtn2.Enabled = true;

                pictureBox1.Visible = true;
                this.Width = 1118;


            }
        }
        
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timerSend.Interval = Convert.ToInt32(numericUpDown1.Value);
        }
        
    }
}
