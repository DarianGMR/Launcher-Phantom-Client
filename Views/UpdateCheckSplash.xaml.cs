using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Managers;
using LauncherPhantom.Models;

namespace LauncherPhantom.Views
{
    public partial class UpdateCheckSplash : Window
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

        public void ShowWithAnimation()
        {
            try
            {
                this.Opacity = 0;
                var fadeInAnim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.3));
                this.BeginAnimation(Window.OpacityProperty, fadeInAnim);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en ShowWithAnimation: {ex.Message}");
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                Debug.WriteLine("[UpdateCheckSplash] Iniciando verificación de actualizaciones...");
                
                CheckingGrid.Visibility = Visibility.Visible;
                UpdateGrid.Visibility = Visibility.Collapsed;
                NoUpdateGrid.Visibility = Visibility.Collapsed;
                StatusMessagesStack.Visibility = Visibility.Collapsed;

                // Esperar 1 segundo antes de empezar
                await Task.Delay(1000);

                // Mostrar stack de mensajes
                CheckingGrid.Visibility = Visibility.Collapsed;
                StatusMessagesStack.Visibility = Visibility.Visible;

                // Animar primer mensaje
                await AnimateConnectingMessageAsync();
                
                // Esperar 2 segundos para leer
                await Task.Delay(2000);

                // Animar segundo mensaje
                await AnimateCheckingMessageAsync();

                // Esperar 2 segundos para leer
                await Task.Delay(2000);

                // Verificar actualizaciones
                var (hasUpdate, versionInfo) = await UpdateManager.Instance.CheckForUpdatesAsync();

                if (!hasUpdate || versionInfo == null)
                {
                    Debug.WriteLine("[UpdateCheckSplash] No hay actualización disponible");
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

        private async Task AnimateConnectingMessageAsync()
        {
            try
            {
                // Fade in del primer mensaje
                var fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.3));
                ConnectingText.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                ConnectingSpinner.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en AnimateConnectingMessageAsync: {ex.Message}");
            }
        }

        private async Task AnimateCheckingMessageAsync()
        {
            try
            {
                // Fade in del segundo mensaje
                var fadeIn = new DoubleAnimation(0.3, 1.0, TimeSpan.FromSeconds(0.3));
                CheckingText.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                CheckingSpinner.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en AnimateCheckingMessageAsync: {ex.Message}");
            }
        }

        private async Task ShowNoUpdateFoundAsync()
        {
            try
            {
                // Mostrar resultado
                StatusResult.Text = "Ya tienes la última actualización!\nConexión exitosa!";
                StatusResult.Visibility = Visibility.Visible;
                
                var fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5));
                StatusResult.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                await Task.Delay(800);

                // Fade out de mensajes
                var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.3));
                StatusMessagesStack.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                await Task.Delay(300);

                // Mostrar grid sin actualización
                StatusMessagesStack.Visibility = Visibility.Collapsed;
                NoUpdateGrid.Visibility = Visibility.Visible;

                // Fade in del grid
                NoUpdateGrid.Opacity = 0;
                var gridFadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5));
                NoUpdateGrid.BeginAnimation(UIElement.OpacityProperty, gridFadeIn);

                // Cerrar automáticamente después de 2 segundos
                await Task.Delay(2000);

                IsUpdateApplied = false;
                Close();
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
                // Mostrar resultado
                StatusResult.Text = "¡Actualización disponible encontrada!";
                StatusResult.Visibility = Visibility.Visible;
                
                var fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5));
                StatusResult.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                await Task.Delay(800);

                // Fade out de mensajes
                var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.3));
                StatusMessagesStack.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                await Task.Delay(300);

                // Mostrar grid de actualización
                UpdateVersionText.Text = $"v{Constants.AppVersion} → v{versionInfo.Version}";
                UpdateChangesText.Text = versionInfo.Changes;

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
                CheckingGrid.Visibility = Visibility.Collapsed;
                StatusMessagesStack.Visibility = Visibility.Visible;

                StatusResult.Text = "Error: No se pudo conectar con el servidor";
                StatusResult.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3333"));
                StatusResult.Visibility = Visibility.Visible;

                Task.Delay(3000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        IsCancelled = true;
                        Close();
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
                downloadWindow.Owner = this;
                downloadWindow.ShowDialog();

                if (downloadWindow.DialogResult == true)
                {
                    IsUpdateApplied = true;
                    Close();
                }
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
                MessageBox.Show(
                    "Es necesario actualizar el launcher para conectarse al servidor.",
                    "Actualización Requerida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                
                IsCancelled = true;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en CancelButton_Click: {ex.Message}");
            }
        }
    }
}