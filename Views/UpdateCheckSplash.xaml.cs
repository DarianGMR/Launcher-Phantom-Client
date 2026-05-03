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
    public partial class UpdateCheckSplash : Page
    {
        public bool IsUpdateApplied { get; private set; } = false;
        public bool IsCancelled { get; private set; } = false;

        public UpdateCheckSplash()
        {
            try
            {
                InitializeComponent();
                Loaded += async (s, e) => await CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en constructor: {ex.Message}");
            }
        }

        private void AnimateLoadingBar()
        {
            try
            {
                // Animación suave y rápida de la barra moviéndose de izquierda a derecha
                var animation = new DoubleAnimation(0, 240, TimeSpan.FromSeconds(1.8))
                {
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = false
                };
                
                LoadingBar.BeginAnimation(Canvas.LeftProperty, animation);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en AnimateLoadingBar: {ex.Message}");
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                Debug.WriteLine("[UpdateCheckSplash] Iniciando verificación de actualizaciones...");
                
                // Mostrar barra grande de carga y mensaje inicial
                AnimateLoadingBar();
                StatusMessageText.Text = "Conectando con el servidor...";
                StatusMessagesStack.Visibility = Visibility.Visible;

                // Esperar 1 segundo antes de empezar
                await Task.Delay(500);

                // Primer mensaje: Conectando (2 segundos)
                await Task.Delay(2000);
                
                // Segundo mensaje: Verificando (2 segundos)
                await ShowStatusMessageAsync("Verificando si hay actualización disponible...");

                // Verificar actualizaciones
                var (hasUpdate, versionInfo) = await UpdateManager.Instance.CheckForUpdatesAsync();

                if (!hasUpdate || versionInfo == null)
                {
                    Debug.WriteLine("[UpdateCheckSplash] No hay actualización disponible");
                    await ShowStatusMessageAsync("Ya tienes la última actualización!");
                    await ShowNoUpdateFoundAsync();
                    return;
                }

                Debug.WriteLine("[UpdateCheckSplash] Actualización disponible");
                await ShowUpdateAvailableAsync(versionInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en CheckForUpdatesAsync: {ex.Message}");
                ShowConnectionError();
            }
        }

        private async Task ShowStatusMessageAsync(string message)
        {
            try
            {
                // Fade out del mensaje actual
                var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.3));
                StatusMessageText.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                await Task.Delay(300);

                // Cambiar texto y fade in
                StatusMessageText.Text = message;
                StatusMessageText.Opacity = 0;

                var fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.3));
                StatusMessageText.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                // Esperar 2 segundos de visualización
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en ShowStatusMessageAsync: {ex.Message}");
            }
        }

        private async Task ShowNoUpdateFoundAsync()
        {
            try
            {
                // Detener animación de la barra
                LoadingBar.BeginAnimation(Canvas.LeftProperty, null);

                // Fade out del loading bar y mensaje
                var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.3));
                LoadingBarContainer.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                StatusMessagesStack.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                await Task.Delay(300);

                // Mostrar grid sin actualización
                LoadingBarContainer.Visibility = Visibility.Collapsed;
                StatusMessagesStack.Visibility = Visibility.Collapsed;
                NoUpdateGrid.Visibility = Visibility.Visible;

                // Fade in del grid
                NoUpdateGrid.Opacity = 0;
                var gridFadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5));
                NoUpdateGrid.BeginAnimation(UIElement.OpacityProperty, gridFadeIn);

                // Cerrar automáticamente después de 2 segundos
                await Task.Delay(2000);

                // Navegar al dashboard
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateTo(new DashboardPage());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en ShowNoUpdateFoundAsync: {ex.Message}");
            }
        }

        private async Task ShowUpdateAvailableAsync(VersionInfo versionInfo)
        {
            try
            {
                // Detener animación de la barra
                LoadingBar.BeginAnimation(Canvas.LeftProperty, null);

                // Fade out del loading bar y mensaje
                var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.3));
                LoadingBarContainer.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                StatusMessagesStack.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                await Task.Delay(300);

                // Mostrar grid de actualización
                UpdateVersionText.Text = $"v{Constants.AppVersion} → v{versionInfo.Version}";
                UpdateChangesText.Text = versionInfo.Changes;

                LoadingBarContainer.Visibility = Visibility.Collapsed;
                StatusMessagesStack.Visibility = Visibility.Collapsed;
                UpdateGrid.Visibility = Visibility.Visible;

                // Fade in del grid
                UpdateGrid.Opacity = 0;
                var gridFadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5));
                UpdateGrid.BeginAnimation(UIElement.OpacityProperty, gridFadeIn);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en ShowUpdateAvailableAsync: {ex.Message}");
            }
        }

        private void ShowConnectionError()
        {
            try
            {
                StatusMessageText.Text = "Error: No se pudo conectar con el servidor";
                StatusMessageText.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF3333"));

                Task.Delay(3000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (Window.GetWindow(this) is MainWindow mainWindow)
                        {
                            mainWindow.NavigateTo(new LoginPage());
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en ShowConnectionError: {ex.Message}");
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                UpdateButton.IsEnabled = false;
                CancelButton.IsEnabled = false;

                var (hasUpdate, versionInfo) = await UpdateManager.Instance.CheckForUpdatesAsync();
                if (!hasUpdate || versionInfo == null)
                {
                    MessageBox.Show("Error obteniendo información de actualización", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var downloadWindow = new DownloadProgressWindow();
                downloadWindow.Owner = Window.GetWindow(this);
                downloadWindow.ShowDialog();

                if (downloadWindow.DialogResult == true)
                {
                    Application.Current.Shutdown();
                    return;
                }

                UpdateButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en UpdateButton_Click: {ex.Message}");
                MessageBox.Show($"Error iniciando actualización: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                // Guardar error de actualización requerida
                ConfigManager.Instance.SetSetting("update_required_error", "Actualización requerida");
                
                // Navegar a LoginPage sin mostrar MessageBox
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateTo(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en CancelButton_Click: {ex.Message}");
            }
        }
    }
}