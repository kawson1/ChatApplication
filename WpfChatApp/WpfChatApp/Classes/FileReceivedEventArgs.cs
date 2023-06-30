using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChatApp.Classes
{
    public class FileReceivedEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public string DirectoryPath { get; set; }
        public bool DoReceive { get; set; }

        public long FileSize { get; set; }
    }
}
