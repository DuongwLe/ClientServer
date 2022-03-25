using Client.Properties;
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


namespace Client
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            txbUserName.Text = Settings.Default.username;
            Connect();
        }
        /// <summary>
        /// Gửi tin đi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            Send();
            AddMessage(txbUserName.Text + ": " + txbMessage.Text);
        }

        /*Cần:
         *  socket
         *  IP
        */

        IPEndPoint IP;
        Socket client;
        
        /// <summary>
        /// Kết nối tới sever
        /// </summary>
        void Connect()
        {
            //IP: địa chỉ của sever
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8844);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            try
            {
                client.Connect(IP);
                client.Send(Serialize(txbUserName.Text + " Đã kết nối tới Server! "));
            }
            catch
            {
                MessageBox.Show("Không thể kết nối sever!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }

        /// <summary>
        /// Đóng kết nối hiện thời
        /// </summary>
        void Close()
        {
            client.Close();
        }

        /// <summary>
        /// Truyền tin
        /// </summary>
        void Send()
        {
            try
            {
                //Connect();
                client.Send(Serialize(txbUserName.Text + ": " + txbMessage.Text));
            }
            catch
            {
                MessageBox.Show("Không thể kết nối sever!", "Lỗi Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// Nhận tin
        /// </summary>
        void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    string message = (string)Deserialize(data);

                    AddMessage(message);
                }
            }
            catch
            {
               // Close();
            }
        }

        /// <summary>
        /// Add Message vào khung chat
        /// </summary>
        /// <param name="s"></param>
        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
            txbMessage.Clear();
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
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void txbUserName_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.username = txbUserName.Text.ToString();
            Settings.Default.Save();
        }

        private void SendFile(string fName)
        {
            try
            {
                string Path = "";
                fName = fName.Replace("\\", "/");
                while (fName.IndexOf("/") > -1)
                {
                    Path += fName.Substring(0, fName.IndexOf("/") + 1);
                    fName = fName.Substring(fName.IndexOf("/") + 1);
                }
                byte[] fNameByte = Encoding.ASCII.GetBytes(fName);           
                byte[] fNameLen = BitConverter.GetBytes(fNameByte.Length);
                byte[] fileData = File.ReadAllBytes(Path + fName);
                //byte[] fileData = File.ReadAllBytes(fName);
                byte[] clientData = new byte[4 + fNameByte.Length + fileData.Length];
                
                fNameLen.CopyTo(clientData, 0);
                fNameByte.CopyTo(clientData, 4);
                fileData.CopyTo(clientData, 4 + fNameByte.Length);
                client.Send(Serialize(txbUserName.Text + " gửi file: " + fName.ToString()));
                client.Send(clientData);          
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnUpload_Click(object sender, EventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            if(fd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SendFile(fd.FileName);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Lỗi Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }         
            }
        }

        private void btnReceive_Click(object sender, EventArgs e)
        {
        }
    }
}
