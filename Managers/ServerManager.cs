using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using LauncherPhantom.Models;
using System.IO;

namespace LauncherPhantom.Managers
{
    public class ServerManager
    {
        private static ServerManager? _instance;
        private static readonly object _lock = new();
        
        private HttpClient _httpClient;
        private string _serverUrl = "http://localhost:5000";

        public static ServerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServerManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private ServerManager()
        {
            // Agregar timeout suficiente y configuración de handler
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; // Solo para desarrollo
            
            _httpClient = new HttpClient(handler) 
            { 
                Timeout = TimeSpan.FromSeconds(10)
            };
            Debug.WriteLine("[ServerManager] Inicializado");
        }

        public void SetServerUrl(string url)
        {
            // Normalizar URL: agregar http:// si falta y remover puerto por defecto si está
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }

            _serverUrl = url;
            ConfigManager.Instance.SetSetting("server_url", url);
            Debug.WriteLine($"[ServerManager] URL actualizada: {url}");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _serverUrl = ConfigManager.Instance.GetSetting("server_url") ?? "http://localhost:5000";
                
                // Normalizar URL
                if (!_serverUrl.StartsWith("http://") && !_serverUrl.StartsWith("https://"))
                {
                    _serverUrl = "http://" + _serverUrl;
                }

                Debug.WriteLine($"[ServerManager] Probando conexión a: {_serverUrl}");
                
                // Intentar acceder al endpoint /api/launcher/health
                var healthUrl = $"{_serverUrl}/api/launcher/health";
                Debug.WriteLine($"[ServerManager] URL de salud: {healthUrl}");
                
                var response = await _httpClient.GetAsync(healthUrl);
                bool isSuccess = response.IsSuccessStatusCode;
                
                Debug.WriteLine($"[ServerManager] Status Code: {response.StatusCode}");
                Debug.WriteLine($"[ServerManager] Conexión resultado: {(isSuccess ? "OK" : "FALLO")}");
                return isSuccess;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ServerManager] Error HTTP en TestConnectionAsync: {ex.Message}");
                Debug.WriteLine($"[ServerManager] Inner Exception: {ex.InnerException?.Message}");
                return false;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"[ServerManager] Timeout en TestConnectionAsync: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error general en TestConnectionAsync: {ex.Message}");
                Debug.WriteLine($"[ServerManager] Tipo de excepción: {ex.GetType().Name}");
                return false;
            }
        }

        public async Task<VersionInfo?> GetVersionAsync()
        {
            try
            {
                Debug.WriteLine("[ServerManager] Obteniendo versión...");
                
                var response = await _httpClient.GetAsync($"{_serverUrl}/api/launcher/version");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var versionInfo = JsonConvert.DeserializeObject<VersionInfo>(content);
                    Debug.WriteLine($"[ServerManager] Versión obtenida: {versionInfo?.Version}");
                    return versionInfo;
                }
                
                Debug.WriteLine("[ServerManager] Error obteniendo versión");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error en GetVersionAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<string> DownloadFileAsync(string fileUrl, IProgress<long> progress)
        {
            try
            {
                var downloadPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Phantom", "launcher-update.exe");

                Debug.WriteLine($"[ServerManager] Descargando desde: {fileUrl}");

                using (var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0L;
                    var canReportProgress = totalBytes != 0;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(downloadPath))
                    {
                        var totalRead = 0L;
                        var buffer = new byte[8192];
                        int read;

                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;

                            if (canReportProgress)
                            {
                                progress.Report(totalRead);
                            }
                        }
                    }

                    Debug.WriteLine($"[ServerManager] Descarga completada: {downloadPath}");
                    return downloadPath;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error en DownloadFileAsync: {ex.Message}");
                throw new Exception($"Error descargando archivo: {ex.Message}");
            }
        }
    }
}
