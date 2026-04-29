using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        public async Task<(bool HasUpdate, VersionInfo? VersionInfo)> CheckForUpdatesAsync()
        {
            try
            {
                var versionInfo = await ServerManager.Instance.GetVersionAsync();
                if (versionInfo == null)
                    return (false, null);

                var currentVersion = new Version(Constants.AppVersion);
                var newVersion = new Version(versionInfo.Version);

                return (newVersion > currentVersion, versionInfo);
            }
            catch
            {
                return (false, null);
            }
        }

        public async Task<string> DownloadUpdateAsync(VersionInfo versionInfo, Action<int> progressCallback)
        {
            try
            {
                var progress = new Progress<long>();
                var progressReporter = progress as IProgress<long>;

                progress.ProgressChanged += (s, bytes) =>
                {
                    var totalSize = 100 * 1024 * 1024; // Assume 100MB
                    var percentage = (int)((bytes / (double)totalSize) * 100);
                    progressCallback?.Invoke(Math.Min(percentage, 100));
                };

                return await ServerManager.Instance.DownloadFileAsync(versionInfo.DownloadUrl, progress);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error descargando actualización: {ex.Message}");
            }
        }

        public void ApplyUpdate(string installerPath)
        {
            try
            {
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
                throw new Exception($"Error aplicando actualización: {ex.Message}");
            }
        }
    }
}