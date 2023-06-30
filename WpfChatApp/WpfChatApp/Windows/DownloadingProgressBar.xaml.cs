using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfChatApp.Classes;

namespace WpfChatApp.Windows
{
    /// <summary>
    /// Interaction logic for DownloadingProgressBar.xaml
    /// </summary>
    public partial class DownloadingProgressBar : Window
    {
        BackgroundWorker worker;
        public DownloadingProgressBar()
        {
            InitializeComponent();
        }

        public static void Start(object sender, DownloadEventArgs e)
        {
            var downloadingWindow = new DownloadingProgressBar();
            downloadingWindow.Show();
            downloadingWindow.StartedDownloading(sender, e);
        }

        private void Window_ContentRenderer(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            // worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        public void SetCurrentProgress(int progress) => worker.ReportProgress(progress);
        public void FinishWork() => worker.CancelAsync();
        
        public void StartedDownloading(object sender, DownloadEventArgs e)
        {
            e.SetProgression = SetCurrentProgress;
            e.FinishWork = FinishWork;
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true; 
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.DoWork += worker_doWork;
            worker.RunWorkerAsync();
        }

        void worker_doWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (true)
            {
                if(worker.CancellationPending == true)
                {
                    this.Dispatcher.Invoke(() => this.Close());
                    e.Cancel = true;
                    break;
                }
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            downloadStatus.Value = e.ProgressPercentage;
        }
    }
}
