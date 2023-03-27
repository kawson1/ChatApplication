using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfChatApp.Scripts;

namespace WpfChatApp.Windows
{
    /// <summary>
    /// Interaction logic for Started.xaml
    /// </summary>
    public partial class Started : Window
    {
        readonly ConnectionHandler connectionHandler;
        public Started(ConnectionHandler _connHandler)
        {
            connectionHandler = _connHandler;
            InitializeComponent();
        }

        public async void MainFunction()
        {
            connectionHandler.MessageRecived += OnMessageRecived;
            //connectionHandler.x = new ConnectionHandler.MessageRecived(OnMessageRecived);
            OnMessageRecived(null, "Scanning...");

            //await Task.Run(() => StartConnectionScanAsync());
            StartConnectionScanAsync();
            OnMessageRecived(null, "W OCZEKIWANIU");
        }

        public async void StartConnectionScanAsync()
        {
            if (await connectionHandler.StartConnectionScanAsync() == 1)
            {
                connectionHandler.ForwardMessage("Someone connected...");
                Task.Run(() => { connectionHandler.StartReciving(); });
            }
        }

        public void OnMessageRecived(object sender, string message)
        {
            ChatMessageBox.AppendText(message);
            ChatMessageBox.ScrollToEnd();

        }

        private bool _autoScroll = true;
        public void ChatMessageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {
                _autoScroll = ChatMessageScrollViewer.VerticalOffset == ChatMessageScrollViewer.ScrollableHeight;
            }

            if (_autoScroll && e.ExtentHeightChange != 0)
            {
                ChatMessageScrollViewer.ScrollToVerticalOffset(ChatMessageScrollViewer.ExtentHeight);

            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectionHandler.ConnectTo(IPAddress.Parse(DestAddressIp.Text), Int32.Parse(DestAddressPort.Text));
                OnMessageRecived(sender, "Connected!");
                Task.Run(() => { connectionHandler.StartReciving(); });
            }
            catch
            {
                MessageBox.Show("Couldn't connect to specific addres:port", "Connection error", MessageBoxButton.OKCancel);
            }
        }

        private void ChatMessageBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void EnterDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string message = ChatMessageInputBox.Text + "\n";
                ChatMessageBox.AppendText(message);
                connectionHandler.Send(message);
                ChatMessageInputBox.Clear();
            }
        }
    }
}
