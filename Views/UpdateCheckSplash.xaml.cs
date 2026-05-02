using System;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Managers;
using LauncherPhantom.Models;

namespace LauncherPhantom.Views
{
    public partial class UpdateCheckSplash : Window
    {
        public bool IsUpdateApplied { get; private set; } = false;
        public bool IsCancelled { get; private set; } = false;

        public UpdateCheckSplash()
        {
            try
            {
                InitializeComponent();
                Loaded += async (s, e) => await CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en constructor: {ex.Message}");
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                Debug.WriteLine("[UpdateCheckSplash] Iniciando verificación de actualizaciones...");
                
                CheckingGrid.Visibility = Visibility.Visible;
                UpdateGrid.Visibility = Visibility.Collapsed;
                NoUpdateGrid.Visibility = Visibility.Collapsed;

                var (hasUpdate, versionInfo) = await UpdateManager.Instance.CheckForUpdatesAsync();

                if (!hasUpdate || versionInfo == null)
                {
                    Debug.WriteLine("[UpdateCheckSplash] No hay actualización disponible");
                    ShowNoUpdateFound();
                    return;
                }

                Debug.WriteLine("[UpdateCheckSplash] Actualización disponible");
                ShowUpdateAvailable(versionInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en CheckForUpdatesAsync: {ex.Message}");
                ShowNoUpdateFound();
            }
        }

        private void ShowNoUpdateFound()
        {
            try
            {
                CheckingGrid.Visibility = Visibility.Collapsed;
                NoUpdateGrid.Visibility = Visibility.Visible;
                UpdateGrid.Visibility = Visibility.Collapsed;

                // Cerrar automáticamente después de 3 segundos
                Task.Delay(3000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        IsUpdateApplied = false;
                        Close();
                    });
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en ShowNoUpdateFound: {ex.Message}");
            }
        }

        private void ShowUpdateAvailable(VersionInfo versionInfo)
        {
            try
            {
                CheckingGrid.Visibility = Visibility.Collapsed;
                NoUpdateGrid.Visibility = Visibility.Collapsed;
                UpdateGrid.Visibility = Visibility.Visible;

                UpdateVersionText.Text = $"v{Constants.AppVersion} → v{versionInfo.Version}";
                UpdateChangesText.Text = versionInfo.Changes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en ShowUpdateAvailable: {ex.Message}");
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                UpdateButton.IsEnabled = false;
                CancelButton.IsEnabled = false;

                var (hasUpdate, versionInfo) = await UpdateManager.Instance.CheckForUpdatesAsync();
                if (!hasUpdate || versionInfo == null)
                {
                    MessageBox.Show("Error obteniendo información de actualización", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var downloadWindow = new DownloadProgressWindow();
                downloadWindow.Owner = this;
                downloadWindow.ShowDialog();

                if (downloadWindow.DialogResult == true)
                {
                    IsUpdateApplied = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en UpdateButton_Click: {ex.Message}");
                MessageBox.Show($"Error iniciando actualización: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                MessageBox.Show(
                    "Es necesario actualizar el launcher para conectarse al servidor.",
                    "Actualización Requerida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                
                IsCancelled = true;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateCheckSplash] Error en CancelButton_Click: {ex.Message}");
            }
        }
    }
}