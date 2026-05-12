using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using LauncherPhantom.Managers;

namespace LauncherPhantom.Views
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            try
            {
                InitializeComponent();
                Loaded += ProfilePage_Loaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePage] Error en constructor: {ex.Message}");
            }
        }

        private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var username = ConfigManager.Instance.GetSetting("current_username");
                UsernameText.Text = string.IsNullOrEmpty(username) ? "Usuario" : username;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePage] Error en Loaded: {ex.Message}");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[ProfilePage] Logout iniciado");
                
                ConfigManager.Instance.DeleteSetting("jwt_token");
                ConfigManager.Instance.DeleteSetting("current_username");
                ConfigManager.Instance.DeleteSetting("connection_error");
                
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateTo(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePage] Error en logout: {ex.Message}");
                MessageBox.Show($"Error al cerrar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}