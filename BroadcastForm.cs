using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UDP_Broadcast_v1
{
    public partial class BroadcastForm : Form
    {
        UdpClient udpReceiver;
        IPEndPoint remoteIp = null;
        string localAddr;
        public BroadcastForm(UdpClient receiver)
        {
            InitializeComponent();
            udpReceiver = receiver;
            localAddr = LocalIPAddress();

            timerListen2.Enabled = true;
        }
        

        private void timerListen2_Tick(object sender, EventArgs e)
        {
            byte[] data = new byte[10000000];
            byte[] tempData;


            tempData = udpReceiver.Receive(ref remoteIp);
                if (tempData[0] < 8)
                {
                    int num = tempData[0];
                    byte[] temp = GetByteArrayWithoutFirst(tempData);

                    SetDataToBeginning(temp, ref data);

                    int stopedInd = temp.Length;

                    for (int i = 0; i < num - 1; i++)
                    {
                        tempData = udpReceiver.Receive(ref remoteIp);

                        for (int j = 0; j < tempData.Length; j++)
                        {
                            data[stopedInd] = tempData[j];
                            stopedInd++;
                        }
                    }

                    var imageMemoryStream = new MemoryStream(data);
                    try
                    {
                        pictureBox1.Image = resizeImage(Image.FromStream(imageMemoryStream), pictureBox1.Size);
                    }
                    catch (Exception ex)
                    {
                        pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    }

                }
        }

        byte[] GetByteArrayWithoutFirst(byte[] arr)
        {
            byte[] tempArr = new byte[arr.Length - 1];
            for (int i = 0; i < arr.Length - 1; i++)
            {
                tempArr[i] = arr[i + 1];
            }

            return tempArr;
        }

        void SetDataToBeginning(byte[] arr, ref byte[] allData)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                allData[i] = arr[i];
            }
        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private static string LocalIPAddress()
        {
            string localIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        private void BroadcastForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            udpReceiver.Close();
            timerListen2.Enabled = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
