using Server.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        /// <summary>
        /// Gửi tin cho tất cả client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            foreach (Socket item in clientList)
            {
                Send(item);
            }
            AddMessage("Server: " + txbMessage.Text);
            txbMessage.Clear();
        }

        /*Cần:
         *  socket
         *  IP
        */

        IPEndPoint IP;
        Socket server;
        Socket client;
        List<Socket> clientList;
        public static string path = @"C:\Users\Duongw\Desktop";
        /// <summary>
        /// Kết nối tới sever
        /// </summary>
        void Connect()
        {
            clientList = new List<Socket>();
            //IP: địa chỉ của sever
            IP = new IPEndPoint(IPAddress.Any, 8844);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            server.Bind(IP);
            server.Listen(100);


            Thread Listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Socket client = server.Accept();
                        clientList.Add(client);
                        //lsbClientIP.Items.Add(client);

                        Thread recieve = new Thread(Receive);
                        recieve.IsBackground = true;
                        recieve.Start(client);

                        //new Thread(delegate ()
                        //{
                        //    ReceiveFile(client);
                        //}).Start();

                    }
                }
                catch
                {
                    // khi có 1 client đóng lại thì khởi tạo lại
                    IP = new IPEndPoint(IPAddress.Any, 8844);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            Listen.IsBackground = true;
            Listen.Start();
        }

        /// <summary>
        /// Đóng kết nối hiện thời
        /// </summary>
        void Close()
        {
            server.Close();
        }

        /// <summary>
        /// Truyền tin
        /// </summary>
        void Send(Socket client)
        {
            if (client != null && txbMessage.Text != string.Empty)
                client.Send(Serialize("Sever: " + txbMessage.Text));
        }

        /// <summary>
        /// Nhận tin
        /// </summary>
        void Receive(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    int receive =  client.Receive(data);

                    string message = (string)Deserialize(data);
                    foreach (Socket item in clientList)
                    {
                        if (item != null && item != client)
                            item.Send(Serialize(message));
                       
                    }
                    AddMessage(message);
                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();
            }
        }

        /// <summary>
        /// Add Message vào khung chat
        /// </summary>
        /// <param name="s"></param>
        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
        }

        /// <summary>
        /// Phân mảnh
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);

            return stream.ToArray();
        }

        /// <summary>
        /// Gom mảnh lại
        /// </summary>
        /// <returns></returns>
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }

        /// <summary>
        /// Đóng kết nối khi đóng form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }


        private void ReceiveFile(Socket socket)
        {
            try
            {
                byte[] clientData = new byte[1024 * 5000];
                int receiveByteLen = socket.Receive(clientData);
                int fNameLen = BitConverter.ToInt32(clientData, 0);
                string fName = Encoding.ASCII.GetString(clientData, 4, fNameLen);
                BinaryWriter write = new BinaryWriter(File.Open(path + @"\" + fName, FileMode.Create));
                write.Write(clientData, 4 + fNameLen, receiveByteLen - 4 - fNameLen);
                write.Close();
                socket.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                socket.Close();
            }
        }


        private void btnUpload_Click(object sender, EventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void btnReceive_Click(object sender, EventArgs e)
        {
            new Thread(delegate () {
                ReceiveFile(client);
            }).Start();
        }
    }
}
