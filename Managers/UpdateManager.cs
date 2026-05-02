using System;
using System.Diagnostics;
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
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<(bool HasUpdate, VersionInfo? VersionInfo)> CheckForUpdatesAsync()
        {
            try
            {
                Debug.WriteLine("[UpdateManager] Verificando actualizaciones...");
                
                var versionInfo = await ServerManager.Instance.GetVersionAsync();
                if (versionInfo == null)
                {
                    Debug.WriteLine("[UpdateManager] VersionInfo es nulo");
                    return (false, null);
                }

                var currentVersion = new Version(Constants.AppVersion);
                var newVersion = new Version(versionInfo.Version);

                bool hasUpdate = newVersion > currentVersion;
                Debug.WriteLine($"[UpdateManager] Versión actual: {currentVersion}, Nueva: {newVersion}, Tiene update: {hasUpdate}");
                
                return (hasUpdate, versionInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager] Error en CheckForUpdatesAsync: {ex.Message}");
                Debug.WriteLine($"[UpdateManager] StackTrace: {ex.StackTrace}");
                return (false, null);
            }
        }

        public async Task<string> DownloadUpdateAsync(VersionInfo versionInfo, Action<int, long, long>? progressCallback)
        {
            try
            {
                Debug.WriteLine("[UpdateManager] Descargando actualización...");
                
                // Crear carpeta de actualización
                var updateFolder = GetUpdateFolder();
                if (!Directory.Exists(updateFolder))
                {
                    Directory.CreateDirectory(updateFolder);
                    Debug.WriteLine($"[UpdateManager] Carpeta de actualización creada: {updateFolder}");
                }

                var fileName = Path.GetFileName(new Uri(versionInfo.DownloadUrl).AbsolutePath);
                if (string.IsNullOrEmpty(fileName))
                    fileName = "launcher-update.exe";

                var filePath = Path.Combine(updateFolder, fileName);

                // Descargar archivo
                using (var response = await _httpClient.GetAsync(versionInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1 && progressCallback != null;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        var isMoreToRead = true;
                        long totalRead = 0;

                        do
                        {
                            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (canReportProgress)
                                {
                                    var percentage = (int)((totalRead * 100) / totalBytes);
                                    progressCallback?.Invoke(percentage, totalRead, totalBytes);
                                    Debug.WriteLine($"[UpdateManager] Descarga: {percentage}% ({totalRead}/{totalBytes} bytes)");
                                }
                            }
                        } while (isMoreToRead);
                    }
                }

                Debug.WriteLine($"[UpdateManager] Descarga completada: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager] Error en DownloadUpdateAsync: {ex.Message}");
                Debug.WriteLine($"[UpdateManager] StackTrace: {ex.StackTrace}");
                throw new Exception($"Error descargando actualización: {ex.Message}");
            }
        }

        public void ApplyUpdate(string installerPath)
        {
            try
            {
                Debug.WriteLine($"[UpdateManager] Aplicando actualización: {installerPath}");
                
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
                Debug.WriteLine($"[UpdateManager] Error en ApplyUpdate: {ex.Message}");
                throw new Exception($"Error aplicando actualización: {ex.Message}");
            }
        }

        public static string GetUpdateFolder()
        {
            var appFolder = Path.GetDirectoryName(System.Windows.Application.Current.StartupUri?.ToString() ?? 
                System.Reflection.Assembly.GetExecutingAssembly().Location) ?? 
                Environment.CurrentDirectory;
            
            return Path.Combine(appFolder, "actualizacion");
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
                            Debug.WriteLine($"[UpdateManager] Archivo eliminado: {file}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[UpdateManager] Error eliminando archivo: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager] Error en CleanupOldUpdates: {ex.Message}");
            }
        }
    }
}