using System;
using System.Windows;
using System.Diagnostics;
using LauncherPhantom.Managers;

namespace LauncherPhantom
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Initialize managers
                Debug.WriteLine("[APP] Inicializando ConfigManager...");
                ConfigManager.Instance.LoadConfig();
                
                Debug.WriteLine("[APP] Inicializando DatabaseManager...");
                DatabaseManager.Instance.Initialize();
                
                Debug.WriteLine("[APP] Managers inicializados correctamente");
                
                // Create main window
                MainWindow = new MainWindow();
                MainWindow.Show();
                
                Debug.WriteLine("[APP] Aplicación iniciada correctamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APP] ERROR: {ex.Message}");
                Debug.WriteLine($"[APP] StackTrace: {ex.StackTrace}");
                
                MessageBox.Show(
                    $"Error fatal al iniciar la aplicación:\n\n{ex.Message}\n\nDetalles:\n{ex.StackTrace}",
                    "Error de Inicio",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                
                this.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                Debug.WriteLine("[APP] Cerrando aplicación...");
                DatabaseManager.Instance.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[APP] Error al cerrar: {ex.Message}");
            }
            
            base.OnExit(e);
        }
    }
}