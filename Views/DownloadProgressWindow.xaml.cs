using System;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Managers;

namespace LauncherPhantom.Views
{
    public partial class DownloadProgressWindow : Window
    {
        private bool _isDownloading = false;
        private DateTime _startTime;
        private long _lastBytesDownloaded = 0;
        private DateTime _lastSpeedCheckTime;

        public DownloadProgressWindow()
        {
            try
            {
                InitializeComponent();
                Loaded += async (s, e) => await StartDownloadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DownloadProgressWindow] Error en constructor: {ex.Message}");
            }
        }

        private async Task StartDownloadAsync()
        {
            try
            {
                Debug.WriteLine("[DownloadProgressWindow] Iniciando descarga...");
                
                _isDownloading = true;
                _startTime = DateTime.Now;
                _lastSpeedCheckTime = DateTime.Now;
                CancelDownloadButton.IsEnabled = true;

                // Obtener información de versión
                var (hasUpdate, versionInfo) = await UpdateManager.Instance.CheckForUpdatesAsync();
                if (!hasUpdate || versionInfo == null)
                {
                    ShowError("No se pudo obtener información de la actualización");
                    return;
                }

                StatusText.Text = "Descargando archivo de actualización...";

                // Descargar actualización
                string filePath = await UpdateManager.Instance.DownloadUpdateAsync(
                    versionInfo,
                    (percentage, downloadedBytes, totalBytes) =>
                    {
                        Dispatcher.Invoke(() => UpdateProgress(percentage, downloadedBytes, totalBytes));
                    }
                );

                Debug.WriteLine($"[DownloadProgressWindow] Descarga completada: {filePath}");
                
                _isDownloading = false;
                ProgressBar.Value = 100;
                PercentageText.Text = "100%";
                StatusText.Text = "Descarga completada. Preparando para aplicar actualización...";
                CancelDownloadButton.IsEnabled = false;

                // Esperar 2 segundos y cerrar
                await Task.Delay(2000);
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DownloadProgressWindow] Error en StartDownloadAsync: {ex.Message}");
                Debug.WriteLine($"[DownloadProgressWindow] StackTrace: {ex.StackTrace}");
                ShowError($"Error descargando actualización: {ex.Message}");
            }
        }

        private void UpdateProgress(int percentage, long downloadedBytes, long totalBytes)
        {
            try
            {
                ProgressBar.Value = percentage;
                PercentageText.Text = $"{percentage}%";

                // Convertir a MB
                double downloadedMB = downloadedBytes / (1024.0 * 1024.0);
                double totalMB = totalBytes / (1024.0 * 1024.0);
                BytesText.Text = $"{downloadedMB:F2} MB / {totalMB:F2} MB";

                // Calcular velocidad
                var now = DateTime.Now;
                var elapsed = (now - _lastSpeedCheckTime).TotalSeconds;
                
                if (elapsed >= 1) // Actualizar velocidad cada segundo
                {
                    var bytesDifference = downloadedBytes - _lastBytesDownloaded;
                    var speedMBps = (bytesDifference / elapsed) / (1024.0 * 1024.0);
                    SpeedText.Text = $"Velocidad: {speedMBps:F2} MB/s";

                    // Calcular tiempo restante
                    if (speedMBps > 0)
                    {
                        var remainingBytes = totalBytes - downloadedBytes;
                        var remainingSeconds = remainingBytes / (speedMBps * 1024.0 * 1024.0);
                        var timeSpan = TimeSpan.FromSeconds(remainingSeconds);
                        TimeRemainingText.Text = $"Tiempo restante: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                    }

                    _lastBytesDownloaded = downloadedBytes;
                    _lastSpeedCheckTime = now;
                }

                Debug.WriteLine($"[DownloadProgressWindow] Progreso: {percentage}% ({downloadedMB:F2}MB / {totalMB:F2}MB)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DownloadProgressWindow] Error en UpdateProgress: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            try
            {
                _isDownloading = false;
                StatusText.Text = message;
                StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3333")
                );
                CancelDownloadButton.Content = "Cerrar";
                CancelDownloadButton.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A1F3A")
                );
                CancelDownloadButton.IsEnabled = true;

                Debug.WriteLine($"[DownloadProgressWindow] Error mostrado: {message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DownloadProgressWindow] Error mostrando error: {ex.Message}");
            }
        }

        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isDownloading)
                {
                    _isDownloading = false;
                    StatusText.Text = "Descarga cancelada";
                }
                
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DownloadProgressWindow] Error en CancelDownloadButton_Click: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_isDownloading)
                {
                    e.Cancel = true;
                    MessageBox.Show("Por favor espere a que se complete la descarga", "Descargando", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DownloadProgressWindow] Error en Window_Closing: {ex.Message}");
            }
        }
    }
}