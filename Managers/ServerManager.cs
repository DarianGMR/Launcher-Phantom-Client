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
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            Debug.WriteLine("[ServerManager] Inicializado");
        }

        public void SetServerUrl(string url)
        {
            _serverUrl = url;
            ConfigManager.Instance.SetSetting("server_url", url);
            Debug.WriteLine($"[ServerManager] URL actualizada: {url}");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _serverUrl = ConfigManager.Instance.GetSetting("server_url") ?? "http://localhost:5000";
                Debug.WriteLine($"[ServerManager] Probando conexión a: {_serverUrl}");
                
                var response = await _httpClient.GetAsync($"{_serverUrl}/health");
                bool isSuccess = response.IsSuccessStatusCode;
                
                Debug.WriteLine($"[ServerManager] Conexión resultado: {(isSuccess ? "OK" : "FALLO")}");
                return isSuccess;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServerManager] Error en TestConnectionAsync: {ex.Message}");
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