using System.Windows;

namespace LauncherPhantom.Views
{
    public partial class DownloadProgressWindow : Window
    {
        public DownloadProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int percentage, string fileName, long downloadedBytes, long totalBytes)
        {
            ProgressBar.Value = percentage;
            
            var mb = (downloadedBytes / 1024.0) / 1024.0;
            var totalMb = (totalBytes / 1024.0) / 1024.0;
            
            ProgressText.Text = $"{percentage}% - {mb:F2}MB / {totalMb:F2}MB";
            StatusText.Text = fileName;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent closing during download
            if (ProgressBar.Value < 100)
            {
                e.Cancel = true;
            }
        }
    }
}