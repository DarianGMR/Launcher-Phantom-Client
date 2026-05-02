using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private System.Timers.Timer? _connectionCheckTimer;

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
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            
            _httpClient = new HttpClient(handler) 
            { 
                Timeout = TimeSpan.FromSeconds(10)
            };
            Debug.WriteLine("[ServerManager] Inicializado");
        }

        public void SetServerUrl(string url)
        {
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
                
                if (!_serverUrl.StartsWith("http://") && !_serverUrl.StartsWith("https://"))
                {
                    _serverUrl = "http://" + _serverUrl;
                }
                
                var healthUrl = $"{_serverUrl}/api/launcher/health";                
                var response = await _httpClient.GetAsync(healthUrl);
                bool isSuccess = response.IsSuccessStatusCode;
                
                return isSuccess;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ServerManager] Error HTTP: {ex.Message}");
                return false;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"[ServerManager] Timeout: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error general: {ex.Message}");
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
                    try
                    {
                        // Usar JObject para parsear primero y convertir
                        var jObject = JObject.Parse(content);
                        
                        var versionInfo = new VersionInfo
                        {
                            Version = jObject["version"]?.ToString() ?? "",
                            DownloadUrl = jObject["downloadUrl"]?.ToString() ?? "",
                            Required = jObject["required"]?.Value<bool>() ?? false,
                            ReleaseDate = jObject["releaseDate"]?.ToString(),
                            Status = jObject["status"]?.ToString(),
                            Changes = ""
                        };

                        // Convertir el array de cambios a string
                        var changesToken = jObject["changes"];
                        if (changesToken != null)
                        {
                            if (changesToken.Type == JTokenType.Array)
                            {
                                // Si es un array, convertir a string con saltos de línea
                                var changesList = changesToken.ToObject<string[]>();
                                versionInfo.Changes = string.Join("\n", changesList);
                            }
                            else if (changesToken.Type == JTokenType.String)
                            {
                                // Si ya es un string, usar tal cual
                                versionInfo.Changes = changesToken.ToString();
                            }
                        }

                        Debug.WriteLine($"[ServerManager] Versión obtenida: {versionInfo.Version}");
                        Debug.WriteLine($"[ServerManager] Cambios: {versionInfo.Changes}");
                        return versionInfo;
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine($"[ServerManager] Error parseando JSON: {ex.Message}");
                        Debug.WriteLine($"[ServerManager] Contenido problemático: {content}");
                        return null;
                    }
                }
                
                Debug.WriteLine($"[ServerManager] Error obteniendo versión: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error en GetVersionAsync: {ex.Message}");
                Debug.WriteLine($"[ServerManager] StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<string> DownloadFileAsync(string fileUrl, IProgress<long>? progress = null)
        {
            try
            {
                var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
                var downloadPath = Path.Combine(UpdateManager.GetUpdateFolder(), fileName);

                using (var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    
                    var totalSize = response.Content.Headers.ContentLength ?? -1L;
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        int read;
                        long totalRead = 0;

                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;
                            progress?.Report(totalRead);
                        }
                    }
                }

                return downloadPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error descargando archivo: {ex.Message}");
                throw;
            }
        }

        public void StartConnectionMonitoring(Func<Task<bool>> connectionCheck, int intervalSeconds = 30)
        {
            try
            {
                if (_connectionCheckTimer != null)
                {
                    _connectionCheckTimer.Stop();
                    _connectionCheckTimer.Dispose();
                }

                _connectionCheckTimer = new System.Timers.Timer(intervalSeconds * 1000);
                _connectionCheckTimer.Elapsed += async (s, e) =>
                {
                    try
                    {
                        var isConnected = await connectionCheck();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ServerManager] Error verificando conexión: {ex.Message}");
                    }
                };
                _connectionCheckTimer.AutoReset = true;
                _connectionCheckTimer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error iniciando monitoreo: {ex.Message}");
            }
        }

        public void StopConnectionMonitoring()
        {
            try
            {
                if (_connectionCheckTimer != null)
                {
                    _connectionCheckTimer.Stop();
                    _connectionCheckTimer.Dispose();
                    _connectionCheckTimer = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error deteniendo monitoreo: {ex.Message}");
            }
        }
    }
}