using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;
using SomeProject.Library.Server;
using System.Threading;
using System.Collections.Generic;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {
        int numcl;
        int id;
        Client client = new Client();
        List<int> cl = new List<int>();
        public ClientMainWindow()
        {
            InitializeComponent();
            
        }
        /// <summary>
        /// Обработка события отправки сообщения
        /// </summary>
        /// <param name="b">A double precision number.</param>
        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            
            OperationResult oper;
            oper = client.SendMessageToServer(textBox.Text);
            if (oper.Result == Result.OK)
            {
                //textBox.Text = Server.Ret();
                textBox.Text = "Message was sent succefully!";
            }
            else
            {
                textBox.Text = "Cannot send the message to the server.";
            }
            timer.Interval = 2000;
            timer.Start();

           // textBox.Text ="New"+ oper.Message;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            textBox.Text = "";
            timer.Stop();
        }

        public TextBox getTextBox()
        {
            return textBox;
        }
        /// <summary>
        /// Обработка события нажатия на кнопку отправки файла
        /// </summary>
        private void SndFileBtn_Click(object sender, EventArgs e)
        {
            //Client client = new Client();
            OpenFileDialog dlg = new OpenFileDialog();
            //string fn=null;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Client.FileNm = dlg.FileName;

            }
            Check.c = 2;
            //Result res = client.SendFileToServer(Client.FileNm).Result;
            OperationResult oper;
            oper = client.SendFileToServer(Client.FileNm);
            if (oper.Result == Result.OK)
            {
                //textBox.Text = oper.Message;
                textBox.Text = "File was sent succefully!";
            }
            else
            {
               textBox.Text = "Cannot send the file to the server.";
            }
            timer.Interval = 2000;
            timer.Start();
        }
        /// <summary>
        /// Обработка события присоединения нового клиента
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            client.ConnectToServer("new");
            button1.Enabled = false;
            button2.Enabled = true;
            sendMsgBtn.Enabled = true;
            SndFileBtn.Enabled = true;
        }

        private void ClientMainWindow_Load(object sender, EventArgs e)
        {

            button2.Enabled = false;
            sendMsgBtn.Enabled = false;
            SndFileBtn.Enabled = false;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.ConnectToServer("dis");
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //client.ConnectToServer("log");
        }
    }
}
