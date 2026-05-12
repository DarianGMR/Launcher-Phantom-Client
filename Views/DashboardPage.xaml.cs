using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private Page? _currentPage;
        private Button? _lastPressedButton;
        private const string ActiveButtonColor = "#1A2050";
        private const string HoverTextColor = "#00D9FF";
        private const string DefaultTextColor = "#A0A0A0";

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
                Debug.WriteLine("[DashboardPage] Inicializando Dashboard...");
                
                _isMonitoring = true;
                _isTransitioning = false;
                
                StartConnectionMonitoring();
                await VerifyConnectionAsync();
                
                NavigateToPage(new BibliotecaPage(), "BIBLIOTECA");
                SetButtonPressed(BibliotecaButton);
                
                Debug.WriteLine("[DashboardPage] Dashboard completamente inicializado");
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
                    Debug.WriteLine("[DashboardPage] ✗ CONEXIÓN PERDIDA");
                    await HandleConnectionLoss();
                    return;
                }

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
                return;

            try
            {
                _isTransitioning = true;
                _isMonitoring = false;
                StopConnectionMonitoring();
                
                ConfigManager.Instance.SetSetting("connection_error", "Se ha perdido la conexión con el servidor.");
                
                await Dispatcher.InvokeAsync(() =>
                {
                    if (Window.GetWindow(this) is MainWindow mainWindow)
                    {
                        mainWindow.NavigateTo(new LoginPage());
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error en HandleConnectionLoss: {ex.Message}");
            }
        }

        private void StartConnectionMonitoring()
        {
            try
            {
                StopConnectionMonitoring();
                
                Debug.WriteLine("[DashboardPage] Iniciando monitoreo de conexión");
                _connectionTimer = new System.Timers.Timer(5000);
                _connectionTimer.Elapsed += async (s, e) =>
                {
                    if (!_isMonitoring || _isTransitioning)
                        return;

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
                        Debug.WriteLine($"[DashboardPage] Error en timer: {ex.Message}");
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

        private void NavigateToPage(Page page, string title)
        {
            try
            {
                _currentPage = page;
                PageTitle.Text = title;
                
                ContentFrame.Opacity = 1;
                var fadeOutAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(200));
                
                fadeOutAnim.Completed += (s, e) =>
                {
                    try
                    {
                        ContentFrame.Navigate(page);
                        var fadeInAnim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(300));
                        ContentFrame.BeginAnimation(UIElement.OpacityProperty, fadeInAnim);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DashboardPage] Error en animación: {ex.Message}");
                    }
                };
                
                ContentFrame.BeginAnimation(UIElement.OpacityProperty, fadeOutAnim);
                Debug.WriteLine($"[DashboardPage] Navegado a: {title}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error navegando: {ex.Message}");
                MessageBox.Show($"Error al navegar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn && btn != _lastPressedButton)
            {
                // Mostrar fondo azul en hover
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ActiveButtonColor));
                btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HoverTextColor));
            }
        }

        private void NavButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn && btn != _lastPressedButton)
            {
                // Volver a estado normal si no está presionado
                btn.Background = new SolidColorBrush(Colors.Transparent);
                btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultTextColor));
            }
        }

        private void SetButtonPressed(Button button)
        {
            try
            {
                // Resetear botón anterior
                if (_lastPressedButton != null && _lastPressedButton != button)
                {
                    _lastPressedButton.Background = new SolidColorBrush(Colors.Transparent);
                    _lastPressedButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultTextColor));
                }

                // Establecer nuevo botón presionado con animación
                _lastPressedButton = button;
                
                // Animación: blanco a azul
                var bgAnim = new ColorAnimation(
                    Colors.White, 
                    (Color)ColorConverter.ConvertFromString(ActiveButtonColor), 
                    TimeSpan.FromMilliseconds(300));
                
                var brush = new SolidColorBrush(Colors.White);
                button.Background = brush;
                button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HoverTextColor));
                brush.BeginAnimation(SolidColorBrush.ColorProperty, bgAnim);

                Debug.WriteLine($"[DashboardPage] Botón presionado: {button.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error en SetButtonPressed: {ex.Message}");
            }
        }

        private void BibliotecaButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonPressed(BibliotecaButton);
            NavigateToPage(new BibliotecaPage(), "BIBLIOTECA");
        }

        private void CatalogoButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonPressed(CatalogoButton);
            NavigateToPage(new CatalogoPage(), "CATÁLOGO DE JUEGOS");
        }

        private void NoticiasButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonPressed(NoticiasButton);
            NavigateToPage(new NoticiasPage(), "NOTICIAS");
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonPressed(CreditsButton);
            NavigateToPage(new CreditsPage(), "CRÉDITOS");
        }

        private void WebButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "http://example.com",
                    UseShellExecute = true
                });
                Debug.WriteLine("[DashboardPage] Abriendo navegador a http://example.com");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardPage] Error abriendo web: {ex.Message}");
                MessageBox.Show("No se pudo abrir el navegador", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonPressed(ProfileButton);
            NavigateToPage(new ProfilePage(), "PERFIL");
        }
    }
}
