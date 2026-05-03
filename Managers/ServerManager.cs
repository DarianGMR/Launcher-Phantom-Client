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
        private const int DEFAULT_PORT = 5000;

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
                Timeout = TimeSpan.FromSeconds(3)
            };
            Debug.WriteLine("[SERVER] Inicializado");
        }

        public void SetServerUrl(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                ip = "localhost";
            }

            ip = ip.Replace("http://", "").Replace("https://", "").Split(':')[0];
            
            _serverUrl = $"http://{ip}:{DEFAULT_PORT}";

            ConfigManager.Instance.SetSetting("server_url", ip);
            Debug.WriteLine($"[SERVER] URL actualizada: {_serverUrl}");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var savedIp = ConfigManager.Instance.GetSetting("server_url");
                if (string.IsNullOrEmpty(savedIp))
                {
                    savedIp = "localhost";
                }
                
                _serverUrl = $"http://{savedIp}:{DEFAULT_PORT}";
                
                var healthUrl = $"{_serverUrl}/api/launcher/health";                
                var response = await _httpClient.GetAsync(healthUrl);
                bool isSuccess = response.IsSuccessStatusCode;
                
                if (isSuccess)
                {
                    Debug.WriteLine($"[SERVER] ✓ Conexión OK");
                }
                else
                {
                    Debug.WriteLine($"[SERVER] ✗ Conexión fallida ({response.StatusCode})");
                }
                
                return isSuccess;
            }
            catch (HttpRequestException)
            {
                Debug.WriteLine($"[SERVER] ✗ Error HTTP");
                return false;
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"[SERVER] ✗ Timeout (3s)");
                return false;
            }
            catch (Exception)
            {
                Debug.WriteLine($"[SERVER] ✗ Error general");
                return false;
            }
        }

        public async Task<VersionInfo?> GetVersionAsync()
        {
            try
            {
                Debug.WriteLine("[SERVER] Obteniendo versión desde update.json...");
                
                var response = await _httpClient.GetAsync($"{_serverUrl}/api/launcher/version");
                var content = await response.Content.ReadAsStringAsync();
                                
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
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

                        var changesToken = jObject["changes"];
                        if (changesToken != null)
                        {
                            if (changesToken.Type == JTokenType.Array)
                            {
                                var changesList = changesToken.ToObject<string[]>();
                                versionInfo.Changes = string.Join("\n", changesList ?? Array.Empty<string>());
                            }
                            else if (changesToken.Type == JTokenType.String)
                            {
                                versionInfo.Changes = changesToken.ToString();
                            }
                        }

                        Debug.WriteLine($"[SERVER] Versión obtenida: {versionInfo.Version}");
                        return versionInfo;
                    }
                    catch (JsonException)
                    {
                        Debug.WriteLine($"[SERVER] Error parseando JSON");
                        return null;
                    }
                }
                
                Debug.WriteLine($"[SERVER] Error obteniendo versión: {response.StatusCode}");
                return null;
            }
            catch (Exception)
            {
                Debug.WriteLine($"[SERVER] Error en GetVersionAsync");
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
            catch (Exception)
            {
                Debug.WriteLine($"[SERVER] Error descargando archivo");
                throw;
            }
        }

        public void StartConnectionMonitoring(Func<Task<bool>> connectionCheck, int intervalSeconds = 5)
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
                        await connectionCheck();
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine($"[SERVER] Error verificando conexión");
                    }
                };
                _connectionCheckTimer.AutoReset = true;
                _connectionCheckTimer.Start();
                
                Debug.WriteLine($"[SERVER] Monitoreo iniciado");
            }
            catch (Exception)
            {
                Debug.WriteLine($"[SERVER] Error iniciando monitoreo");
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
                    Debug.WriteLine("[SERVER] Monitoreo detenido");
                }
            }
            catch (Exception)
            {
                Debug.WriteLine($"[SERVER] Error deteniendo monitoreo");
            }
        }
    }
}