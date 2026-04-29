using System.Windows;
using System.Windows.Controls;
using LauncherPhantom.Views;
using LauncherPhantom.Managers;

namespace LauncherPhantom
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += async (s, e) =>
            {
                await InitializeAsync();
            };
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            try
            {
                // Show loading
                ShowLoading(true);

                // Initialize services
                await ServerManager.Instance.TestConnectionAsync();

                // Navigate to login
                MainFrame.Navigate(new LoginPage());
                
                ShowLoading(false);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al inicializar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowLoading(false);
            }
        }

        public void ShowLoading(bool show)
        {
            LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        public void NavigateTo(Page page)
        {
            MainFrame.Navigate(page);
        }
    }
}