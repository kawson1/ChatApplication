using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChatApp.Classes
{
    public class DownloadEventArgs : EventArgs
    {
        public Action<int> SetProgression;
        public Action FinishWork;
    }
}
