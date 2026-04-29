using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Views;

namespace LauncherPhantom
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                Loaded += async (s, e) =>
                {
                    await InitializeAsync();
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error en constructor: {ex.Message}");
                MessageBox.Show($"Error inicializando ventana: {ex.Message}");
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("[MainWindow] Iniciando InitializeAsync...");
                
                // Show splash screen with 5 second minimum
                Debug.WriteLine("[MainWindow] Mostrando Splash Screen...");
                SplashScreen.Visibility = Visibility.Visible;
                MainFrame.Visibility = Visibility.Collapsed;
                
                // Simulate loading with progress bar animation
                for (int i = 0; i <= 100; i += 10)
                {
                    SplashProgressBar.Value = i;
                    await Task.Delay(500);
                }
                SplashProgressBar.Value = 100;
                
                // Wait for 5 seconds total
                await Task.Delay(1000);
                
                Debug.WriteLine("[MainWindow] Splash Screen completado");
                
                // Hide splash and show main frame
                SplashScreen.Visibility = Visibility.Collapsed;
                MainFrame.Visibility = Visibility.Visible;

                // Navigate to login
                Debug.WriteLine("[MainWindow] Navegando a LoginPage...");
                NavigateTo(new LoginPage());
                
                Debug.WriteLine("[MainWindow] InitializeAsync completado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] ERROR: {ex.Message}");
                Debug.WriteLine($"[MainWindow] StackTrace: {ex.StackTrace}");
                
                SplashScreen.Visibility = Visibility.Collapsed;
                MainFrame.Visibility = Visibility.Visible;
                
                MessageBox.Show(
                    $"Error al inicializar:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        public void ShowLoading(bool show)
        {
            try
            {
                LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error en ShowLoading: {ex.Message}");
            }
        }

        public void NavigateTo(Page page)
        {
            try
            {
                MainFrame.Navigate(page);
                Debug.WriteLine($"[MainWindow] Navegación exitosa a {page.GetType().Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error al navegar: {ex.Message}");
                Debug.WriteLine($"[MainWindow] StackTrace: {ex.StackTrace}");
                MessageBox.Show($"Error en navegación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}