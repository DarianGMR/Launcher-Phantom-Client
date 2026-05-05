using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using LauncherPhantom.Models;

namespace LauncherPhantom.Managers
{
    public class UpdateManager
    {
        private static UpdateManager? _instance;
        private static readonly object _lock = new();

        public static UpdateManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UpdateManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private HttpClient _httpClient;

        private UpdateManager()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10
            };
            
            _httpClient = new HttpClient(handler, true)
            {
                Timeout = TimeSpan.FromSeconds(300) // Timeout más largo para descargas grandes
            };
            
            // Headers por defecto
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LauncherPhantom/1.0");
        }

        public async Task<(bool HasUpdate, VersionInfo? VersionInfo)> CheckForUpdatesAsync()
        {
            try
            {
                Debug.WriteLine("[UPDATE] Verificando actualizaciones...");
                
                var versionInfo = await ServerManager.Instance.GetVersionAsync();
                if (versionInfo == null)
                {
                    Debug.WriteLine("[UPDATE] VersionInfo es nulo");
                    return (false, null);
                }

                var currentVersion = new Version(Constants.AppVersion);
                var newVersion = new Version(versionInfo.Version);

                bool hasUpdate = newVersion > currentVersion;
                Debug.WriteLine($"[UPDATE] Versión actual: {currentVersion}, Nueva: {newVersion}, Tiene update: {hasUpdate}");
                
                return (hasUpdate, versionInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UPDATE] Error en CheckForUpdatesAsync: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<string> DownloadUpdateAsync(VersionInfo versionInfo, CancellationToken cancellationToken, Action<int, long, long>? progressCallback)
        {
            string? filePath = null;
            HttpResponseMessage? response = null;
            
            try
            {
                Debug.WriteLine("[UPDATE] Iniciando descarga de actualización...");
                
                // Crear carpeta de actualización
                var updateFolder = GetUpdateFolder();
                if (!Directory.Exists(updateFolder))
                {
                    Directory.CreateDirectory(updateFolder);
                    Debug.WriteLine($"[UPDATE] Carpeta de actualización creada: {updateFolder}");
                }

                var fileName = Path.GetFileName(new Uri(versionInfo.DownloadUrl).AbsolutePath);
                if (string.IsNullOrEmpty(fileName))
                    fileName = "LauncherPhantom.exe";

                filePath = Path.Combine(updateFolder, fileName);

                // Limpiar archivo anterior si existe
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch { /* Ignorar */ }
                }

                Debug.WriteLine($"[UPDATE] Descargando desde: {versionInfo.DownloadUrl}");
                Debug.WriteLine($"[UPDATE] Guardando en: {filePath}");

                // Descargar archivo con soporte para cancelación - INSTANTÁNEO
                response = await _httpClient.GetAsync(versionInfo.DownloadUrl, 
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                
                response.EnsureSuccessStatusCode();
                
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1 && progressCallback != null;

                Debug.WriteLine($"[UPDATE] Tamaño total: {totalBytes} bytes");

                using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 131072, true))
                {
                    var buffer = new byte[131072]; // Buffer grande para mejor rendimiento (128KB)
                    var isMoreToRead = true;
                    long totalRead = 0;
                    var lastProgressUpdate = DateTime.Now;
                    var lastProgressBytes = 0L;
                    var progressUpdateInterval = 250; // Actualizar cada 250ms para precisión en tiempo real

                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int read = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read, cancellationToken);
                            totalRead += read;

                            // Reportar progreso en intervalo preciso para UI en tiempo real
                            var now = DateTime.Now;
                            if (canReportProgress && (now - lastProgressUpdate).TotalMilliseconds >= progressUpdateInterval)
                            {
                                var percentage = (int)((totalRead * 100) / totalBytes);
                                progressCallback?.Invoke(percentage, totalRead, totalBytes);
                                lastProgressUpdate = now;
                                lastProgressBytes = totalRead;

                                Debug.WriteLine($"[UPDATE] Progreso: {percentage}% ({totalRead}/{totalBytes} bytes)");
                            }
                        }
                    } while (isMoreToRead);

                    // Reporte final - asegurar 100%
                    if (canReportProgress)
                    {
                        progressCallback?.Invoke(100, totalRead, totalBytes);
                    }
                }

                // Verificar integridad del archivo
                if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                {
                    throw new Exception("El archivo descargado está vacío o no existe");
                }

                Debug.WriteLine($"[UPDATE] Descarga completada exitosamente: {filePath}");
                return filePath;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[UPDATE] Descarga cancelada por el usuario");
                
                // Eliminar el archivo parcialmente descargado
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        Debug.WriteLine($"[UPDATE] Archivo parcial eliminado: {filePath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[UPDATE] Error eliminando archivo parcial: {ex.Message}");
                    }
                }
                
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UPDATE] Error en DownloadUpdateAsync: {ex.Message}");
                Debug.WriteLine($"[UPDATE] StackTrace: {ex.StackTrace}");
                
                // Eliminar archivo si hubo error
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        Debug.WriteLine($"[UPDATE] Archivo eliminado por error: {filePath}");
                    }
                    catch (Exception deleteEx)
                    {
                        Debug.WriteLine($"[UPDATE] Error eliminando archivo: {deleteEx.Message}");
                    }
                }
                
                throw new Exception($"Error descargando actualización: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public void ApplyUpdate(string installerPath)
        {
            try
            {
                Debug.WriteLine($"[UPDATE] Aplicando actualización: {installerPath}");
                
                if (!File.Exists(installerPath))
                {
                    throw new FileNotFoundException($"Archivo de actualización no encontrado: {installerPath}");
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(processInfo);
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UPDATE] Error en ApplyUpdate: {ex.Message}");
                throw new Exception($"Error aplicando actualización: {ex.Message}");
            }
        }

        public static string GetUpdateFolder()
        {
            var appFolder = Path.GetDirectoryName(System.Windows.Application.Current.StartupUri?.ToString() ?? 
                System.Reflection.Assembly.GetExecutingAssembly().Location) ?? 
                Environment.CurrentDirectory;
            
            return Path.Combine(appFolder, "Update");
        }

        public void CleanupOldUpdates()
        {
            try
            {
                var updateFolder = GetUpdateFolder();
                if (Directory.Exists(updateFolder))
                {
                    var files = Directory.GetFiles(updateFolder);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            Debug.WriteLine($"[UPDATE] Archivo limpiado: {file}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[UPDATE] Error eliminando archivo: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UPDATE] Error en CleanupOldUpdates: {ex.Message}");
            }
        }
    }
}