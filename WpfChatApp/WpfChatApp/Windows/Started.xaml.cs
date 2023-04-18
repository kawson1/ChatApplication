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
        readonly string username;

        public Started(ConnectionHandler _connHandler, string _username)
        {
            username = _username;
            connectionHandler = _connHandler;
            InitializeComponent();
        }

        public async void MainFunction()
        {
            connectionHandler.MessageRecived += DisplayMessage;
            DisplayMessage(null, "Scanning...");
            StartConnectionScanAsync();
        }

        public async void StartConnectionScanAsync()
        {
            if (await connectionHandler.StartConnectionScanAsync() == 1)
            {
                DisplayMessage(null, "Someone connected...");
                Task.Run(() => { connectionHandler.StartReciving(); });
                ChangeTextboxState();
            }
        }

        public void DisplayMessage(object sender, string message)
        {
            ChatMessageBox.AppendText(message+"\n");
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
                DisplayMessage(sender, "Connected!");
                ChangeTextboxState();
                Task.Run(() => { connectionHandler.StartReciving(); });
            }
            catch
            {
                MessageBox.Show("Couldn't connect to specific addres:port", "Connection error", MessageBoxButton.OKCancel);
            }
        }

        private void EnterDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string message = $"{username}: {ChatMessageInputBox.Text}";
                DisplayMessage(sender, message);
                connectionHandler.Send(message);
                ChatMessageInputBox.Clear();
            }
        }

        private void ChangeTextboxState()
        {
            ChatMessageInputBox.IsEnabled = !ChatMessageInputBox.IsEnabled;
        }
    }
}
