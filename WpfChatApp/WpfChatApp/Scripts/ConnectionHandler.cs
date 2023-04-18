using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfChatApp.Interfaces;

namespace WpfChatApp.Scripts
{
    public class ConnectionHandler
    {
        IPAddress ipAddress { get; set; }
        Int32 port { get; set; }

        Socket hostingSocket;
        Socket connectionSocket;

        public event EventHandler<string> MessageRecived;
        
        bool isConnected = false;

        public void StartHosting(IPAddress _ipAddress, Int32 _port)
        {
            ipAddress = _ipAddress;
            port = _port;
            hostingSocket = InitializeSocket(ipAddress, port);
        }

        public Socket InitializeSocket(IPAddress _ipAddress, Int32 _port)
        {
            // IF SERVER AVAILABLE, PUT _ipAddress INSTEAD localhost
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ip = host.AddressList[0];
            Socket socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(ip, _port));
            return socket;
        }

        public void IsConnectionEmpty()
        {
            throw new NotImplementedException();
        }

        // RETURN 1 IF CONNECTED
        public Task<int> StartConnectionScanAsync()
        {
/*            return await Task.Run(() =>
            {
                hostingSocket.Listen();
                connectionSocket = hostingSocket.Accept();
                isConnected = true;
                return 1;
            });*/

            return Task.Run(() =>
            {
                hostingSocket.Listen();
                connectionSocket = hostingSocket.Accept();
                isConnected = true;
                return 1;
            });
        }

        // TODO: If connected, we have to block possibility for connection to our socket
        public int ConnectTo(IPAddress _ipAddress, Int32 _port)
        {
            // IF SERVER AVAILABLE, PUT ipAddress INSTEAD localhost
            IPHostEntry host = Dns.GetHostEntry("localhost");
            connectionSocket = InitializeSocket(host.AddressList[0], port+10000);
            try
            {
                connectionSocket.Connect(host.AddressList[0], _port);
                isConnected = true;
                return 1;
            }
            catch (Exception ex) 
            {
                return -1;
            }
        }

        public void ForwardMessage(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(MessageRecived, null, message);
        }

        public void StartReciving()
        {
            int byteCount = 0;
            byte[] bytes = new byte[256];
            while (true)
            {
                /*                if (connectionSocket.Connected)
                                {

                                }*/
                byteCount = connectionSocket.Receive(bytes, 0, bytes.Length, SocketFlags.None);
                if (byteCount > 0)
                    ForwardMessage(Encoding.UTF8.GetString(bytes, 0, byteCount));
            }
        }

        public void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            connectionSocket.Send(byteData, 0, byteData.Length, SocketFlags.None);
        }
    }
}
