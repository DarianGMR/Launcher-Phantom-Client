using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using LauncherPhantom.Views;
using LauncherPhantom.Managers;

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

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("[MainWindow] Iniciando InitializeAsync...");
                
                // Hide loading - no connection test on startup
                ShowLoading(false);
                Debug.WriteLine("[MainWindow] Loading ocultado");

                // Navigate to login directly - NO server connection test here
                Debug.WriteLine("[MainWindow] Navegando a LoginPage...");
                MainFrame.Navigate(new LoginPage());
                
                Debug.WriteLine("[MainWindow] InitializeAsync completado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] ERROR: {ex.Message}");
                Debug.WriteLine($"[MainWindow] StackTrace: {ex.StackTrace}");
                
                ShowLoading(false);
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
                MainFrame.RemoveBackEntry(); // Prevent going back
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error al navegar: {ex.Message}");
                MessageBox.Show($"Error en navegación: {ex.Message}");
            }
        }
    }
}