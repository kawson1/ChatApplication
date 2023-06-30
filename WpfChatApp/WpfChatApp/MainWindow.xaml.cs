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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfChatApp.Scripts;
using WpfChatApp.Windows;

namespace WpfChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string keysDest = String.Empty;

        public MainWindow()
        {
            // InitializeComponent();
        }

        private async void Start_Connection(object sender, RoutedEventArgs e)
        {
            if(UsernameInputBox.Text == "")
            {
                MessageBox.Show("Bad username", "Bad username", MessageBoxButton.OKCancel);
                return;
            }
            
            if(PasswordInputBox.Password.Length <= 0)
            {
                MessageBox.Show("Bad password", "Bad password", MessageBoxButton.OKCancel);
                return;
            }

            if(keysDest == String.Empty)
            {
                MessageBox.Show("Choose keys directory", "Keys directory", MessageBoxButton.OKCancel);
                return;
            }

            try
            {
                var connHandler = new ConnectionHandler(IPAddress.Parse(IpAddressBox.Text), Int32.Parse(PortAddressBox.Text), PasswordInputBox.Password);
                connHandler.publicKeyDestination = keysDest + "/public_key.xml";
                connHandler.privateKeyDestination = keysDest + "/privateKey/private_key.xml";

                Started startedWindow = new Started(connHandler, UsernameInputBox.Text);
                startedWindow.Show();
                // connHandler.StartHosting(IPAddress.Parse(IpAddressBox.Text), Int32.Parse(PortAddressBox.Text));
                this.Hide();
                startedWindow.Closed += (s, e) => { this.Show(); };
                startedWindow.MainFunction();
            }
            catch
            {
                MessageBox.Show("Bad IP address or Port", "Address error", MessageBoxButton.OKCancel);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = @"c:\";
            if (dlg.ShowDialog() == true)
            {
                keysDest = dlg.ResultPath;
                return;
            }
        }
    }
}
