using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Managers;
using LauncherPhantom.Models;

namespace LauncherPhantom.Views
{
    public partial class DashboardPage : Page
    {
        private System.Timers.Timer? _connectionTimer;
        private bool _isMonitoring = true;
        private bool _isTransitioning = false;

        public DashboardPage()
        {
            try
            {
                InitializeComponent();
                Loaded += DashboardPage_Loaded;
                Unloaded += DashboardPage_Unloaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error en constructor: {ex.Message}");
            }
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {                
                // Obtener usuario
                var username = ConfigManager.Instance.GetSetting("current_username");
                if (!string.IsNullOrEmpty(username))
                {
                    WelcomeText.Text = $"Bienvenido, {username}";
                }
                
                // Actualizar versión
                VersionText.Text = Constants.AppVersion;
                
                // Iniciar monitoreo de conexión INMEDIATAMENTE
                _isMonitoring = true;
                _isTransitioning = false;
                
                StartConnectionMonitoring();
                
                // Hacer verificación inicial después de 1 segundo
                await Task.Delay(1000);
                await VerifyConnectionAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error en Loaded: {ex.Message}");
            }
        }

        private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _isMonitoring = false;
            StopConnectionMonitoring();
        }

        private async Task VerifyConnectionAsync()
        {
            if (!_isMonitoring || _isTransitioning)
                return;

            try
            {                
                var isConnected = await ServerManager.Instance.TestConnectionAsync();
                                
                if (!isConnected)
                {
                    Debug.WriteLine("[DashboardPage] CONEXIÓN PERDIDA");
                    await HandleConnectionLoss();
                    return;
                }

                UpdateConnectionStatus(true);
                Debug.WriteLine("[DashboardPage] Conexión verificada correctamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error verificando conexión: {ex.Message}");
                await HandleConnectionLoss();
            }
        }

        private async Task HandleConnectionLoss()
        {
            if (_isTransitioning)
            {
                return;
            }

            try
            {
                _isTransitioning = true;
                _isMonitoring = false;
                StopConnectionMonitoring();
                                
                // Guardar el estado de error para mostrarlo en LoginPage
                ConfigManager.Instance.SetSetting("connection_error", "CONEXIÓN PERDIDA\n\nLa conexión con el servidor se ha interrumpido.");
                
                // Usar Dispatcher para navegar (asegura que se ejecute en el thread principal)
                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        ReturnToLogin();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DashboardPage] Error en Dispatcher: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error en HandleConnectionLoss: {ex.Message}");
                Debug.WriteLine($"[DashboardPage] StackTrace: {ex.StackTrace}");
            }
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            try
            {
                if (isConnected)
                {
                    ServerStatusText.Text = "Conectado";
                    ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FF00"));
                    ConnectionStatus.Text = "Conectado al servidor";
                    ConnectionStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FF00"));
                }
                else
                {
                    ServerStatusText.Text = "Desconectado";
                    ServerStatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3333"));
                    ConnectionStatus.Text = "Conexión perdida";
                    ConnectionStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3333"));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error actualizando estado: {ex.Message}");
            }
        }

        private void StartConnectionMonitoring()
        {
            try
            {
                StopConnectionMonitoring();
                                
                _connectionTimer = new System.Timers.Timer(3000); // Verificar cada 3 segundos
                _connectionTimer.Elapsed += (s, e) =>
                {
                    if (!_isMonitoring || _isTransitioning)
                    {
                        return;
                    }

                    try
                    {                        
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var isConnected = await ServerManager.Instance.TestConnectionAsync();
                                                                
                                if (!isConnected && _isMonitoring && !_isTransitioning)
                                {
                                    await HandleConnectionLoss();
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[DashboardPage] [TIMER] Error en verificación: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DashboardPage] [TIMER] Error general: {ex.Message}");
                    }
                };
                
                _connectionTimer.AutoReset = true;
                _connectionTimer.Start();
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error iniciando monitoreo: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[DashboardPage] Logout iniciado por el usuario");
                
                _isMonitoring = false;
                _isTransitioning = true;
                StopConnectionMonitoring();
                
                ConfigManager.Instance.DeleteSetting("jwt_token");
                ConfigManager.Instance.DeleteSetting("current_username");
                ConfigManager.Instance.DeleteSetting("connection_error");
                
                ReturnToLogin();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error en LogoutButton_Click: {ex.Message}");
                MessageBox.Show($"Error al cerrar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReturnToLogin()
        {
            try
            {
                
                _isMonitoring = false;
                StopConnectionMonitoring();
                
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateTo(new LoginPage());
                }
                else
                {
                    Debug.WriteLine("[DashboardPage] MainWindow no encontrada");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error regresando a login: {ex.Message}");
                Debug.WriteLine($"[DashboardPage] StackTrace: {ex.StackTrace}");
            }
        }

        private void StopConnectionMonitoring()
        {
            try
            {
                if (_connectionTimer != null)
                {
                    _connectionTimer.Stop();
                    _connectionTimer.Dispose();
                    _connectionTimer = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error deteniendo monitoreo: {ex.Message}");
            }
        }
    }
}