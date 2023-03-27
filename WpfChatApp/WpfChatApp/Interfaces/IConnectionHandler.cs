using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfChatApp.Interfaces
{
    internal interface IConnectionHandler
    {
        int Initialize();

        void ReciveConnection(object sender, SocketAsyncEventArgs e);

        // Check if someone is listening to our address:port
        void IsConnectionEmpty();
    }
}
