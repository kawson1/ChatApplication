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
        public ConnectionHandler connHandler;

        public MainWindow()
        {
            // InitializeComponent();
        }

        private async void Start_Connection(object sender, RoutedEventArgs e)
        {

            try
            {
                connHandler = new ConnectionHandler();
                connHandler.StartHosting(IPAddress.Parse(IpAddressBox.Text), Int32.Parse(PortAddressBox.Text));
                Started startedWindow = new Started(connHandler);
                startedWindow.Show();
                this.Hide();
                startedWindow.Closed += (s, e) => { this.Show(); };
                startedWindow.MainFunction();
            }
            catch
            {
                MessageBox.Show("Bad IP address or Port", "Address error", MessageBoxButton.OKCancel);
            }
        }
    }
}
