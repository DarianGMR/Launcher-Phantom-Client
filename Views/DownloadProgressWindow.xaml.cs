using System;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using LauncherPhantom.Managers;

namespace LauncherPhantom.Views
{
    public partial class DownloadProgressWindow : Window
    {
        private bool _isDownloading = false;
        private DateTime _startTime;
        private long _lastBytesDownloaded = 0;
        private DateTime _lastSpeedCheckTime;
        private CancellationTokenSource? _cancellationTokenSource;
        private string _downloadedFilePath = "";
        private System.Windows.Threading.DispatcherTimer? _updateTimer;
        private object _lockObject = new object();
        private bool _cancelRequested = false;

        public DownloadProgressWindow()
        {
            try
            {
                InitializeComponent();
                
                // Timer para actualizaciones periódicas (500ms)
                _updateTimer = new System.Windows.Threading.DispatcherTimer();
                _updateTimer.Interval = TimeSpan.FromMilliseconds(500);
                _updateTimer.Tick += UpdateTimer_Tick;
                
                // Loaded asincrónico para iniciar descarga rápidamente
                Loaded += async (s, e) => 
                {
                    await Task.Delay(50); // Pequeño delay para que la ventana se renderice
                    await StartDownloadAsync();
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DownloadProgressWindow] Error en constructor: {ex.Message}");
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Timer para actualizaciones de UI periódicas
            // El actual se maneja en el callback de progreso
        }

        private async Task StartDownloadAsync()
        {
            try
            {
                Debug.WriteLine("[DOWNLOAD] Iniciando descarga...");
                
                _isDownloading = true;
                _cancelRequested = false;
                _startTime = DateTime.Now;
                _lastSpeedCheckTime = DateTime.Now;
                _cancellationTokenSource = new CancellationTokenSource();
                _downloadedFilePath = "";
                CancelDownloadButton.IsEnabled = true;
                CancelDownloadButton.Content = "Cancelar Descarga";

                // Obtener información de versión
                StatusText.Text = "Obteniendo información de la actualización...";
                var (hasUpdate, versionInfo) = await UpdateManager.Instance.CheckForUpdatesAsync();
                if (!hasUpdate || versionInfo == null)
                {
                    ShowError("No se pudo obtener información de la actualización");
                    return;
                }

                StatusText.Text = "Conectando con el servidor...";
                await Task.Delay(100); // Pequeño delay para actualizar UI

                StatusText.Text = "Descargando actualización...";

                // Descargar actualización con soporte para cancelación
                string filePath = await UpdateManager.Instance.DownloadUpdateAsync(
                    versionInfo,
                    _cancellationTokenSource.Token,
                    (percentage, downloadedBytes, totalBytes) =>
                    {
                        // Este callback se ejecuta en el thread de la descarga
                        Dispatcher.Invoke(() => UpdateProgress(percentage, downloadedBytes, totalBytes), 
                            System.Windows.Threading.DispatcherPriority.Normal);
                    }
                );

                _downloadedFilePath = filePath;
                
                Debug.WriteLine($"[DOWNLOAD] Descarga completada: {filePath}");
                
                _isDownloading = false;
                ProgressBar.Value = 100;
                PercentageText.Text = "100%";
                StatusText.Text = "¡Descarga completada! Preparando actualización...";
                CancelDownloadButton.IsEnabled = false;
                CancelDownloadButton.Content = "Cerrando...";

                // Esperar 2 segundos y cerrar
                await Task.Delay(2000);
                
                DialogResult = true;
                Close();
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[DOWNLOAD] Descarga cancelada por el usuario");
                _isDownloading = false;
                _cancelRequested = false;
                
                // Eliminar el archivo descargado parcialmente
                if (!string.IsNullOrEmpty(_downloadedFilePath) && File.Exists(_downloadedFilePath))
                {
                    try
                    {
                        File.Delete(_downloadedFilePath);
                        Debug.WriteLine($"[DOWNLOAD] Archivo eliminado: {_downloadedFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DOWNLOAD] Error eliminando archivo: {ex.Message}");
                    }
                }
                
                ShowError("Descarga cancelada por el usuario");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DOWNLOAD] Error en StartDownloadAsync: {ex.Message}");
                Debug.WriteLine($"[DOWNLOAD] StackTrace: {ex.StackTrace}");
                
                // Eliminar archivo si hubo error
                if (!string.IsNullOrEmpty(_downloadedFilePath) && File.Exists(_downloadedFilePath))
                {
                    try
                    {
                        File.Delete(_downloadedFilePath);
                        Debug.WriteLine($"[DOWNLOAD] Archivo eliminado por error: {_downloadedFilePath}");
                    }
                    catch
                    {
                        // Ignorar error al eliminar
                    }
                }
                
                ShowError($"Error descargando: {ex.Message}");
            }
        }

        private void UpdateProgress(int percentage, long downloadedBytes, long totalBytes)
        {
            try
            {
                lock (_lockObject)
                {
                    // Actualizar barra de progreso
                    ProgressBar.Value = Math.Min(percentage, 100);
                    PercentageText.Text = $"{percentage}%";

                    // Convertir a MB con precisión
                    double downloadedMB = downloadedBytes / (1024.0 * 1024.0);
                    double totalMB = totalBytes / (1024.0 * 1024.0);
                    BytesText.Text = $"{downloadedMB:F2} MB / {totalMB:F2} MB";

                    // Calcular velocidad y tiempo restante cada 500ms
                    var now = DateTime.Now;
                    var elapsedSinceLastCheck = (now - _lastSpeedCheckTime).TotalMilliseconds;
                    
                    if (elapsedSinceLastCheck >= 500) // Actualizar cada 500ms
                    {
                        var bytesDifference = downloadedBytes - _lastBytesDownloaded;
                        var elapsedSeconds = elapsedSinceLastCheck / 1000.0;
                        
                        if (elapsedSeconds > 0)
                        {
                            // Velocidad en MB/s
                            var speedMBps = (bytesDifference / elapsedSeconds) / (1024.0 * 1024.0);
                            SpeedText.Text = $"Velocidad: {speedMBps:F2} MB/s";

                            // Tiempo restante
                            if (speedMBps > 0)
                            {
                                var remainingBytes = totalBytes - downloadedBytes;
                                var remainingSeconds = remainingBytes / (speedMBps * 1024.0 * 1024.0);
                                
                                if (remainingSeconds > 0)
                                {
                                    var timeSpan = TimeSpan.FromSeconds(remainingSeconds);
                                    TimeRemainingText.Text = $"Tiempo restante: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                                }
                                else
                                {
                                    TimeRemainingText.Text = "Tiempo restante: --:--";
                                }
                            }

                            _lastBytesDownloaded = downloadedBytes;
                            _lastSpeedCheckTime = now;
                        }
                    }

                    // Información adicional
                    if (totalBytes > 0)
                    {
                        ProgressInfoText.Text = $"Progreso: {(double)downloadedBytes / totalBytes * 100:F1}% | Tamaño total: {totalMB:F2} MB";
                    }

                    Debug.WriteLine($"[DOWNLOAD] Progreso: {percentage}% ({downloadedMB:F2}MB / {totalMB:F2}MB)");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DOWNLOAD] Error en UpdateProgress: {ex.Message}");
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
                CancelDownloadButton.Focusable = true;
                CancelDownloadButton.Focus();

                Debug.WriteLine($"[DOWNLOAD] Error mostrado: {message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DOWNLOAD] Error mostrando error: {ex.Message}");
            }
        }

        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Evitar múltiples clicks
                if (_cancelRequested)
                {
                    Debug.WriteLine("[DOWNLOAD] Cancelación ya solicitada, ignorando click");
                    return;
                }

                _cancelRequested = true;
                CancelDownloadButton.IsEnabled = false;

                if (_isDownloading)
                {
                    Debug.WriteLine("[DOWNLOAD] Cancelación solicitada por usuario");
                    StatusText.Text = "Cancelando descarga...";
                    CancelDownloadButton.Content = "Cancelando...";
                    
                    // Cancelar descarga de forma segura
                    _cancellationTokenSource?.Cancel();
                }
                else
                {
                    Debug.WriteLine("[DOWNLOAD] Cerrando ventana");
                    DialogResult = false;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DOWNLOAD] Error en CancelDownloadButton_Click: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Permitir cerrar solo si no se está descargando
                if (_isDownloading && _cancelRequested == false)
                {
                    e.Cancel = true;
                    Debug.WriteLine("[DOWNLOAD] Cierre de ventana bloqueado - Descarga en progreso");
                }
                
                _updateTimer?.Stop();
                _cancellationTokenSource?.Dispose();
                
                Debug.WriteLine("[DOWNLOAD] Ventana cerrada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DOWNLOAD] Error en Window_Closing: {ex.Message}");
            }
        }
    }
}