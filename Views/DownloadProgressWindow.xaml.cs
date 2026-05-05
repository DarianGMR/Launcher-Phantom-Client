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
        private object _lockObject = new object();
        private bool _cancelRequested = false;

        public DownloadProgressWindow()
        {
            try
            {
                InitializeComponent();
                
                // Loaded asincrónico para iniciar descarga instantáneamente
                Loaded += async (s, e) => 
                {
                    // Sin delays, inicio instantáneo
                    await StartDownloadAsync();
                };
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
                    // Actualizar barra de progreso con animación suave
                    ProgressBar.Value = Math.Min(percentage, 100);
                    PercentageText.Text = $"{percentage}%";

                    // Convertir a MB con precisión
                    double downloadedMB = downloadedBytes / (1024.0 * 1024.0);
                    double totalMB = totalBytes / (1024.0 * 1024.0);
                    BytesText.Text = $"Descargado: {downloadedMB:F2} MB / {totalMB:F2} MB";

                    // Calcular velocidad y tiempo restante en tiempo real
                    var now = DateTime.Now;
                    var elapsedSinceLastCheck = (now - _lastSpeedCheckTime).TotalMilliseconds;
                    
                    if (elapsedSinceLastCheck >= 250) // Actualizar cada 250ms para precisión en tiempo real
                    {
                        var bytesDifference = downloadedBytes - _lastBytesDownloaded;
                        var elapsedSeconds = elapsedSinceLastCheck / 1000.0;
                        
                        if (elapsedSeconds > 0)
                        {
                            // Velocidad en bytes/segundo
                            double speedBytesPerSecond = bytesDifference / elapsedSeconds;
                            
                            // Convertir velocidad a KB/s o MB/s según sea necesario
                            string speedText;
                            if (speedBytesPerSecond < 1024 * 1024) // Menos de 1 MB/s
                            {
                                // Mostrar en KB/s
                                double speedKBps = speedBytesPerSecond / 1024.0;
                                speedText = $"Velocidad: {speedKBps:F0} KB/s";
                            }
                            else
                            {
                                // Mostrar en MB/s
                                double speedMBps = speedBytesPerSecond / (1024.0 * 1024.0);
                                speedText = $"Velocidad: {speedMBps:F2} MB/s";
                            }
                            SpeedText.Text = speedText;

                            // Tiempo restante con precisión
                            if (speedBytesPerSecond > 0)
                            {
                                var remainingBytes = totalBytes - downloadedBytes;
                                var remainingSeconds = remainingBytes / speedBytesPerSecond;
                                
                                if (remainingSeconds >= 0)
                                {
                                    var timeSpan = TimeSpan.FromSeconds(remainingSeconds);
                                    TimeRemainingText.Text = $"Tiempo restante: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                                }
                                else
                                {
                                    TimeRemainingText.Text = "Tiempo restante: 00:00";
                                }
                            }

                            _lastBytesDownloaded = downloadedBytes;
                            _lastSpeedCheckTime = now;
                        }
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