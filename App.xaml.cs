using System.Windows;
using LauncherPhantom.Managers;
using LauncherPhantom.Models;

namespace LauncherPhantom
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize managers
            ConfigManager.Instance.LoadConfig();
            DatabaseManager.Instance.Initialize();
            
            // Show splash screen
            var splashScreen = new SplashScreen("Resources/splash.png");
            splashScreen.Show(true);

            // Create main window
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DatabaseManager.Instance.Dispose();
            base.OnExit(e);
        }
    }
}