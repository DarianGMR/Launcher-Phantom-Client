using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LauncherPhantom.Models;

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
        }

        public void SetServerUrl(string url)
        {
            _serverUrl = url;
            ConfigManager.Instance.SetSetting("server_url", url);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _serverUrl = ConfigManager.Instance.GetSetting("server_url") ?? "http://localhost:5000";
                var response = await _httpClient.GetAsync($"{_serverUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<VersionInfo?> GetVersionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_serverUrl}/api/launcher/version");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<VersionInfo>(content);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DownloadFileAsync(string fileUrl, IProgress<long> progress)
        {
            try
            {
                var downloadPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                    "Phantom", "launcher-update.exe");

                using (var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0L;
                    var canReportProgress = totalBytes != 0;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = System.IO.File.Create(downloadPath))
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

                    return downloadPath;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error descargando archivo: {ex.Message}");
            }
        }
    }
}