using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCP_server
{
    public partial class Form1 : Form
    {
        //判斷UI狀態
        int UI_status = 1;
        //列舉判斷代碼, 方便辨識
        enum E { waiting = 1, connected = 2, display = 3, disconnected = 4}
        List<string> _Ls = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void getData()
        {
            TcpListener server = null;
            try
            {
                //設置TCPlistener port 13000
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("192.168.50.190");
                //建立新的Tcplistener
                server = new TcpListener(localAddr, port);
                //server開始監聽client的請求
                server.Start();
                //建立Buffer來讀取資料
                Byte[] bytes = new Byte[256];

                string data = null;
                //監聽迴圈
                while (true)
                {
                    //設定背景作業能報告更新
                    backgroundWorker1.WorkerReportsProgress = true;

                    //傳入一個變數給ReportProgress來改變UI
                    //無法直接在執行緒修改UI
                    UI_status = Convert.ToInt32(E.waiting);   
                    backgroundWorker1.ReportProgress(UI_status);

                    //接收client的請求
                    TcpClient client = server.AcceptTcpClient();

                    UI_status = Convert.ToInt32(E.connected);
                    backgroundWorker1.ReportProgress(UI_status);

                    data = null;
                    //取得client傳輸的資料
                    NetworkStream stream = client.GetStream();

                    //開始接收client的資料
                    int i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.Unicode.GetString(bytes, 0, i);
                        UI_status = Convert.ToInt32(E.display);
                        if (data != "disconnect")
                        {
                            //將接收的資料存入 list
                            _Ls.Add(data);
                        }
                        backgroundWorker1.ReportProgress(UI_status, data);
                    }
                    UI_status = Convert.ToInt32(E.disconnected);
                    backgroundWorker1.ReportProgress(UI_status);
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                MessageBox.Show("SocketException:{0}" + e);
            }
            //釋放區塊
            finally
            {
                //sever停止監聽
                server.Stop();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //開始執行背景作業
            backgroundWorker1.RunWorkerAsync();
        }

        private void bgWorker_RWC(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("OK!");
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            getData();
        }

        private void bgWorker_PC(object sender, ProgressChangedEventArgs e)
        {
            //此部分為看UI_status來改變UI
            if (e.ProgressPercentage == (int)E.waiting)
            {
                label1.Text = "Waiting for connecting";
            }
            else if (e.ProgressPercentage == (int)E.connected)
            {
                if (e.ProgressPercentage != (int)E.disconnected)
                {
                    label2.Text = "Connected";
                }
            }
            else if (e.ProgressPercentage == (int)E.display &&
                e.UserState.ToString() != "disconnected")
            {
                richTextBox1.Text = "";
                foreach(string data in _Ls)
                {
                    richTextBox1.Text += data;
                }
            }
            if (e.ProgressPercentage == (int)E.disconnected)
            {
                label2.Text = "Disconnected!";
            }
        }
    }
}
