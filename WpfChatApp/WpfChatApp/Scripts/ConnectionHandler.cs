using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfChatApp.Classes;
using WpfChatApp.Interfaces;

namespace WpfChatApp.Scripts
{
    public class ConnectionHandler
    {
        public string publicKeyDestination;
        public string privateKeyDestination;

        IPAddress ipAddress { get; set; }
        Int32 port { get; set; }
        readonly string password;

        Socket hostingSocket;
        Socket connectionSocket;
        EncryptedCommunication ec = null;

        public event EventHandler<string> MessageRecived;
        public event EventHandler<FileReceivedEventArgs> FileReceived;
        public event EventHandler<DownloadEventArgs> StartedDownloading;
        public event EventHandler Disconnected;
        public event EventHandler Connected;

        bool isConnected = false;

        public ConnectionHandler(IPAddress ipAddress, Int32 port, string password)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.password = password;
            this.hostingSocket = InitializeSocket(ipAddress, port);
        }

        public void StartHosting(IPAddress _ipAddress, Int32 _port)
        {
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
        public Task StartConnectionScanAsync()
        {
            return Task.Run(() =>
            {
                ForwardMessage("Scanning...");
                hostingSocket.Listen();
                connectionSocket = hostingSocket.Accept();
                Application.Current.Dispatcher.BeginInvoke(Connected, null, null);
                isConnected = true;
                ForwardMessage("Someone connected.");
                StartReciving();
                //return 1;
            });
        }

        // TODO: If connected, we have to block possibility for connection to our socket
        public async void ConnectTo(IPAddress _ipAddress, Int32 _port)
        {
            if (_ipAddress.Equals(ipAddress) && _port.Equals(port))
            {
                throw new Exception("Adres IP i nr portu socketu do którego próbujesz się podłączyć prawdopodobnie należy do ciebie.");
            }
            // IF SERVER AVAILABLE, PUT ipAddress INSTEAD localhost
            IPHostEntry host = Dns.GetHostEntry("localhost");

            Random r = new Random();
            connectionSocket = InitializeSocket(host.AddressList[0], r.Next(10000));
            connectionSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            connectionSocket.Connect(host.AddressList[0], _port);
            StartReciving();
            var x = Application.Current.Dispatcher.BeginInvoke(Connected, null, null);
        }

        public bool DisconnectClient()
        {
            try
            {
                connectionSocket.Shutdown(SocketShutdown.Send);
                // connectionSocket.Shutdown(SocketShutdown.Send);
                //connectionSocket.Disconnect(false);
                // connectionSocket.Close();
                ec = null;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void ForwardMessage(string message)
        {
            Application.Current.Dispatcher.BeginInvoke(MessageRecived, null, message);
        }



        public void Read_Callback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;


            int read = connectionSocket.EndReceive(ar);

            if (read > 0)
            {
                if (ec == null)
                {
                    byte[] receiverPublicKey = new ArraySegment<byte>(so.buffer, 0, read).ToArray();
                    ec = new EncryptedCommunication(receiverPublicKey);
                }
                else
                {
                    byte[] receivedData = new ArraySegment<byte>(so.buffer, 0, read).ToArray();
                    EncryptedMessage encryptedMessage = Serializer.FromByteArray<EncryptedMessage>(receivedData);
                    string message = ec.DecryptMessage(encryptedMessage, password, privateKeyDestination);
                    // string message = Encoding.ASCII.GetString(so.buffer, 0, read).ToString();
                    if (message.StartsWith("PLIK"))
                    {
                        if (FileReceived != null)
                        {
                            /*int start = message.IndexOf('<');
                            int end = message.IndexOf('>', start);*/

                            var args = new FileReceivedEventArgs();
                            string fileName = Regex.Match(message, "[0-9a-z-_A-Z.]+\\.[a-zA-Z]+", RegexOptions.IgnoreCase).Value;
                            string fileSize = Regex.Match(message, "Rozmiar: [0-9]*", RegexOptions.IgnoreCase).Value.Remove(0, "Rozmiar: ".Length);

                            // SKIP HEADERS
                            // message = String.Join("\n", message.Split('\n').Skip(3));

                            byte[] decryptedPrivKey = ec.DecryptPrivateKey(password, privateKeyDestination);
                            byte[] aesKey = Encryption.rsaDecryptSHA1(encryptedMessage.Key, decryptedPrivKey);
                            byte[] fileReceiveBuffer = new byte[StateObject.BUFFER_SIZE];
                            byte[] decryptedFileReceiveBuffer;
                            byte[] encryptedFileReceiveBuffor;
                            long bytesLeftToRead = long.Parse(fileSize);
                            int indexOf0Byte;

                            // fileReceiveBuffer = Encoding.ASCII.GetBytes(message);
                            //read = message.Length;


                            args.FileName = fileName;
                            args.FileSize = bytesLeftToRead;

                            FileReceived(this, args);
                            if (args.DoReceive)
                            {
                                var downloadArgs = new DownloadEventArgs();
                                StartedDownloading(this, downloadArgs);

                                using (BinaryWriter bw = new BinaryWriter(File.Open(Path.Combine(args.DirectoryPath, args.FileName), FileMode.Append)))
                                {
                                    while (bytesLeftToRead > 0)
                                    {
                                        double wynik = (double)bytesLeftToRead / args.FileSize;
                                        int progression = 100 - (int)(wynik * 100);
                                        downloadArgs.SetProgression(progression);
                                        read = connectionSocket.Receive(fileReceiveBuffer);

                                        indexOf0Byte = FindSequence.GetIndexOfSequenceStart(fileReceiveBuffer, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                                        encryptedFileReceiveBuffor = new ArraySegment<byte>(fileReceiveBuffer, 0, indexOf0Byte).ToArray();
                                        if (encryptedFileReceiveBuffor.Length != 512 && encryptedFileReceiveBuffor.Length < 512)
                                            Array.Resize(ref encryptedFileReceiveBuffor, 512);
                                        else if (encryptedFileReceiveBuffor.Length < 528)
                                            Array.Resize(ref encryptedFileReceiveBuffor, 528);
                                        decryptedFileReceiveBuffer = Encryption.aesDecrypt(encryptedFileReceiveBuffor, aesKey);

                                        long bytesToCopy = Math.Min(decryptedFileReceiveBuffer.Length, bytesLeftToRead);
                                        bw.Write(decryptedFileReceiveBuffer, 0, (int)bytesToCopy);
                                        bytesLeftToRead -= bytesToCopy;
                                        /*                                        catch
                                                                                {
                                                                                    int x = 50;
                                                                                }*/
                                    }
                                    downloadArgs.FinishWork();
                                    ForwardMessage("Pobrano");
                                }
                            }

                        }
                    }
                    else
                    {
                        ForwardMessage(message);
                    }
                }
                connectionSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(Read_Callback), so);
            }
            else
            {
                connectionSocket.Shutdown(SocketShutdown.Both);
                connectionSocket.Close();
                // connectionSocket.Disconnect(true);
                ForwardMessage("Someone disconnected...");
                ec = null;
                Application.Current.Dispatcher.BeginInvoke(Disconnected, null, null);
                StartConnectionScanAsync();
            }
        }

        public void StartReciving()
        {
            EncryptedCommunication.GenerateRSAKeyPair(publicKeyDestination, privateKeyDestination, password);
            StateObject so = new StateObject();
            connectionSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(Read_Callback), so);
            byte[] publicKey = EncryptedCommunication.GetRSAPublicKey(publicKeyDestination);
            connectionSocket.Send(publicKey, 0, publicKey.Length, SocketFlags.None);
        }



        public Task StartRecivingAsync()
        {
            return Task.Run(() =>
            {
                Thread.CurrentThread.Name = "Receiving Thread";
                int byteCount = 0;
                byte[] bytes = new byte[256];
                connectionSocket.Blocking = false;
                while (connectionSocket.Connected)
                {
                    /*                if (connectionSocket.Connected)
                                    {

                                    }*/
                    byteCount = connectionSocket.Receive(bytes, 0, bytes.Length, SocketFlags.None);
                    if (byteCount > 0)
                        ForwardMessage(Encoding.ASCII.GetString(bytes, 0, byteCount));

                    else if (byteCount == 0) // OTHER SIDE DISCONNECTED
                    {
                        ForwardMessage("Someone disconnected...");
                        break;
                    }
                }
            });
        }

        public void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            EncryptedMessage encryptedMessage = ec.EncryptMessage(data);
            byte[] serializedEncryptedMessage = Serializer.ToByteArray(encryptedMessage);

            // Begin sending the data to the remote device.  
            connectionSocket.Send(serializedEncryptedMessage, 0, serializedEncryptedMessage.Length, SocketFlags.None);
        }

        public void Send(byte[] byteData)
        {
            EncryptedMessage encryptedMessage = ec.EncryptData(byteData);
            byte[] serializedEncryptedMessage = Serializer.ToByteArray(encryptedMessage);

            // Begin sending the data to the remote device.  
            connectionSocket.Send(serializedEncryptedMessage, 0, serializedEncryptedMessage.Length, SocketFlags.None);
        }

        public void SendFile(string filepath, long filesize)
        {
            Match m = Regex.Match(filepath, "[0-9a-z-_A-Z]*\\.[a-zA-Z]*", RegexOptions.IgnoreCase);
            string filename = m.Value;

            byte[] Buff = Encoding.ASCII.GetBytes($"PLIK\n" +
                $"Nazwa: {filename}\n" +
                $"Rozmiar: {filesize}\n");
            connectionSocket.SendFile(filepath, Buff, null, TransmitFileOptions.UseDefaultWorkerThread);
        }

        public void SendFileBytes(string filepath, long filesize)
        {
            Match m = Regex.Match(filepath, "[0-9a-z-_A-Z.]+\\.[a-zA-Z]+", RegexOptions.IgnoreCase);
            string filename = m.Value;
            byte[] AESKey;

            EncryptedMessage encryptedMessage = ec.EncryptMessage($"PLIK\n" +
                $"Nazwa: {filename}\n" +
                $"Rozmiar: {filesize}\n", out AESKey);
            byte[] serializedEncryptedMessage = Serializer.ToByteArray(encryptedMessage);
            connectionSocket.Send(serializedEncryptedMessage, 0, serializedEncryptedMessage.Length, SocketFlags.None);


            byte[] buffer = new byte[StateObject.BUFFER_SIZE / 2];
            using (FileStream fs = File.OpenRead(filepath))
            {
                int bytesRead;
                while ((bytesRead = fs.Read(buffer, 0, StateObject.BUFFER_SIZE / 2)) > 0)
                {
                    // byte[] receiverPublicKey = new ArraySegment<byte>(buffer, 0, bytesRead).ToArray();
                    // 1040 bajtow !!!! a miesci sie StateObject.BUFFER_SIZE = 1024
                    byte[] encryptedBuffer = Encryption.aesEncrypt(buffer, AESKey);
                    Array.Resize(ref encryptedBuffer, StateObject.BUFFER_SIZE);
                    // 1024 bajtow !!!!
                    // byte[] decryptedBuffer = Encryption.aesDecrypt(encryptedBuffer, AESKey);
                    connectionSocket.Send(encryptedBuffer, 0, encryptedBuffer.Length, SocketFlags.None);
                }
            }
        }

        public void ChangeBlockCipherMode(bool ECB)
        {
            if (ec == null)
                return;
            else
                ec.ECB = ECB;
        }
    }
}
