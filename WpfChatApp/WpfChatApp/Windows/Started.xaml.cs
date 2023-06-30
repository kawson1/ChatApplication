using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfChatApp.Classes;
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
        readonly string password;

        public Started(ConnectionHandler _connHandler, string _username)
        {
            username = _username;
            connectionHandler = _connHandler;
            InitializeComponent();
        }

        public async void MainFunction()
        {
            // EVENTS
            connectionHandler.MessageRecived += DisplayMessage;
            connectionHandler.FileReceived += FileReceived;
            connectionHandler.Disconnected += TextboxButtonSwitch;
            connectionHandler.Connected += TextboxButtonSwitch;
            connectionHandler.StartedDownloading += StartedDownloading;

            connectionHandler.StartConnectionScanAsync();
            // StartConnectionScanAsync();
        }

        public void StartedDownloading(object sender, DownloadEventArgs e) 
        {
            this.Dispatcher.Invoke(() =>
            {
                var downloadingWindow = new DownloadingProgressBar();
                downloadingWindow.Show();
                downloadingWindow.StartedDownloading(sender, e);
            });
            return;
        }

        public void DisplayMessage(object sender, string message)
        {
            ChatMessageBox.AppendText(message+"\n");
            ChatMessageBox.ScrollToEnd();
        }

        public void FileReceived(object sender, FileReceivedEventArgs args)
        {
            this.Dispatcher.Invoke(() =>
            {
                var dialogResult = MessageBox.Show($"Czy chcesz pobrać plik o nazwie {args.FileName} ?", "Pobranie pliku", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    var dlg = new FolderPicker();
                    dlg.InputPath = @"c:\";
                    if (dlg.ShowDialog() == true)
                    {
                        args.DirectoryPath = dlg.ResultPath;
                        args.DoReceive = true;
                        return;
                    }
                }
                args.DoReceive = false;
            });
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Conection errpr", MessageBoxButton.OKCancel);
                // MessageBox.Show($"Couldn't connect to specific addres:port\n\n{ex.Message}", "Connection error", MessageBoxButton.OKCancel);
            }

        }
        private void Disconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!connectionHandler.DisconnectClient())
            {
                DisplayMessage(sender, "Couldn't disconnect!");
                return;
            }
/*            ChangeConnectButtonsState();
            ChangeTextboxState();*/
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

        private void TextboxButtonSwitch(object sender = null, EventArgs args = null)
        {
            ChangeTextboxState();
            ChangeConnectButtonsState();
        }

        private void ChangeTextboxState()
        {
            ChatMessageInputBox.IsEnabled = !ChatMessageInputBox.IsEnabled;
        }
        private void ChangeConnectButtonsState()
        {
            Connect_Button.IsEnabled = !Connect_Button.IsEnabled;
            Disconnect_Button.IsEnabled = !Disconnect_Button.IsEnabled;
            Send_Button.IsEnabled= !Send_Button.IsEnabled;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            Nullable<bool> result = dlg.ShowDialog();

            if(result == true) 
            { 
                string filename = dlg.FileName;
                long filesize = new FileInfo(dlg.FileName).Length;
                connectionHandler.SendFileBytes(filename, filesize);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.IsChecked == true)
            {
                var codingMode = radioButton.Content.ToString();
                if (codingMode == "ECB")
                    connectionHandler.ChangeBlockCipherMode(true);
                else
                    connectionHandler.ChangeBlockCipherMode(false);
            }
        }
    }
}
