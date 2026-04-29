using System.Windows;

namespace LauncherPhantom.Views
{
    public partial class UpdateDialog : Window
    {
        public UpdateDialog(string currentVersion, string newVersion, string changes)
        {
            InitializeComponent();

            VersionInfoText.Text = $"Nueva versión: {newVersion} (Actual: {currentVersion})";
            ChangesText.Text = changes;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void LaterButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}