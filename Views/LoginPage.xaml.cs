using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Managers;
using LauncherPhantom.Models;

namespace LauncherPhantom.Views
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            Loaded += LoginPage_Loaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
                // Verificar si hay error de conexión desde el dashboard
                var connectionError = ConfigManager.Instance.GetSetting("connection_error");
                if (!string.IsNullOrEmpty(connectionError))
                {
                    Debug.WriteLine($"[LoginPage] Conexion perdida: {connectionError}");
                    ShowError(connectionError);
                    ConfigManager.Instance.DeleteSetting("connection_error");
                }
                
                var savedUsername = ConfigManager.Instance.GetSetting("saved_username");
                var savedPassword = ConfigManager.Instance.GetSetting("saved_password");
                var savedServerIp = ConfigManager.Instance.GetSetting("server_url");

                if (!string.IsNullOrEmpty(savedUsername))
                {
                    UsernameTextBox.Text = EncryptionManager.Instance.Decrypt(savedUsername);
                    RememberMeCheckBox.IsChecked = true;
                    
                    if (!string.IsNullOrEmpty(savedPassword))
                    {
                        PasswordBox.Password = EncryptionManager.Instance.Decrypt(savedPassword);
                    }
                    
                    if (!string.IsNullOrEmpty(savedServerIp))
                    {
                        var ip = savedServerIp.Replace("http://", "").Replace("https://", "");
                        ServerIpTextBox.Text = ip;
                    }
                    
                    Debug.WriteLine("[LoginPage] Credenciales pre-llenadas");
                }
                else
                {
                    UpdatePlaceholders();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoginPage] Error en LoadedEvent: {ex.Message}");
            }
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;
            UpdatePlaceholders();
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;
            UpdatePlaceholders();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void ServerIpTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;
            UpdatePlaceholders();
        }

        private void ServerIpTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void ServerIpTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void UpdatePlaceholders()
        {
            if (UsernameTextBox.Template.FindName("PlaceholderText", UsernameTextBox) is TextBlock usernamePlaceholder)
            {
                usernamePlaceholder.Visibility = string.IsNullOrEmpty(UsernameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (PasswordBox.Template.FindName("PlaceholderText", PasswordBox) is TextBlock passwordPlaceholder)
            {
                passwordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (ServerIpTextBox.Template.FindName("PlaceholderText", ServerIpTextBox) is TextBlock serverIpPlaceholder)
            {
                serverIpPlaceholder.Visibility = string.IsNullOrEmpty(ServerIpTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[LoginPage] LoginButton_Click");
                ErrorMessageText.Visibility = Visibility.Collapsed;
                
                var validationResult = ValidateInputs();
                if (!validationResult.IsValid)
                {
                    ShowError(validationResult.Message);
                    return;
                }

                LoginButton.IsEnabled = false;
                LoginButton.Content = "Conectando...";

                ServerManager.Instance.SetServerUrl(ServerIpTextBox.Text);

                var isConnected = await ServerManager.Instance.TestConnectionAsync();
                if (!isConnected)
                {
                    ShowError("No se puede conectar con el servidor.\nVerifica la dirección IP.");
                    LoginButton.IsEnabled = true;
                    LoginButton.Content = "Iniciar Sesión";
                    return;
                }

                var loginRequest = new LoginRequest
                {
                    Username = UsernameTextBox.Text,
                    Password = PasswordBox.Password
                };

                var response = await AuthManager.Instance.LoginAsync(loginRequest);

                if (!response.Success)
                {
                    ShowError(response.Error ?? "Error desconocido en el login");
                    LoginButton.IsEnabled = true;
                    LoginButton.Content = "Iniciar Sesión";
                    return;
                }

                if (RememberMeCheckBox.IsChecked == true)
                {
                    ConfigManager.Instance.SetSetting("saved_username", 
                        EncryptionManager.Instance.Encrypt(UsernameTextBox.Text));
                    ConfigManager.Instance.SetSetting("saved_password", 
                        EncryptionManager.Instance.Encrypt(PasswordBox.Password));
                }
                else
                {
                    ConfigManager.Instance.DeleteSetting("saved_username");
                    ConfigManager.Instance.DeleteSetting("saved_password");
                }

                ConfigManager.Instance.SetSetting("current_username", UsernameTextBox.Text);

                Debug.WriteLine("[LoginPage] Login exitoso!");

                await ShowUpdateCheckSplashAsync();

                LoginButton.IsEnabled = true;
                LoginButton.Content = "Iniciar Sesión";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoginPage] Error en LoginButton_Click: {ex.Message}");
                ShowError($"Error: {ex.Message}");
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Iniciar Sesión";
            }
        }

        private async Task ShowUpdateCheckSplashAsync()
        {
            try
            {
                
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow == null)
                {
                    throw new Exception("MainWindow no encontrada");
                }

                var updateSplash = new UpdateCheckSplash();
                updateSplash.Owner = mainWindow;
                var result = updateSplash.ShowDialog();

                if (updateSplash.IsCancelled)
                {
                    ShowError("Es necesario actualizar para continuar.");
                    await Task.Delay(2000);
                    return;
                }

                if (updateSplash.IsUpdateApplied)
                {
                    Debug.WriteLine("[LoginPage] Actualización aplicada, cerrando aplicación");
                    Application.Current.Shutdown();
                    return;
                }

                mainWindow.NavigateTo(new DashboardPage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoginPage] Error en ShowUpdateCheckSplashAsync: {ex.Message}");
                ShowError($"Error verificando actualizaciones: {ex.Message}");
            }
        }

        private (bool IsValid, string Message) ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                return (false, "El nombre de usuario es requerido");

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                return (false, "La contraseña es requerida");

            if (string.IsNullOrWhiteSpace(ServerIpTextBox.Text))
                return (false, "La dirección del servidor es requerida");

            return (true, "");
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateTo(new RegisterPage());
            }
        }
    }
}