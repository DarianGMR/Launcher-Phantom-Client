using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            // Load saved credentials if "Remember Me" was checked
            var savedUsername = ConfigManager.Instance.GetSetting("saved_username");
            var savedPassword = ConfigManager.Instance.GetSetting("saved_password");

            if (!string.IsNullOrEmpty(savedUsername))
            {
                UsernameTextBox.Text = EncryptionManager.Instance.Decrypt(savedUsername);
                RememberMeCheckBox.IsChecked = true;
                
                if (!string.IsNullOrEmpty(savedPassword))
                {
                    PasswordBox.Password = EncryptionManager.Instance.Decrypt(savedPassword);
                }
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;
            
            // Validation
            var validationResult = ValidateInputs();
            if (!validationResult.IsValid)
            {
                ShowError(validationResult.Message);
                return;
            }

            // Show loading
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Iniciando sesión...";

            try
            {
                var request = new LoginRequest
                {
                    Username = UsernameTextBox.Text,
                    Password = PasswordBox.Password
                };

                var response = await AuthManager.Instance.LoginAsync(request);

                if (response.Success)
                {
                    // Save credentials if remember me is checked
                    if (RememberMeCheckBox.IsChecked.GetValueOrDefault())
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

                    SoundManager.Instance.PlaySound("success");
                    MessageBox.Show("¡Sesión iniciada exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Close launcher (Phase 2 will open dashboard)
                    Application.Current.Shutdown();
                }
                else
                {
                    SoundManager.Instance.PlaySound("error");
                    ShowError(response.Error ?? "Error desconocido");
                }
            }
            catch (Exception ex)
            {
                SoundManager.Instance.PlaySound("error");
                ShowError($"Error de conexión: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Iniciar Sesión";
            }
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.NavigateTo(new RegisterPage());
        }

        private (bool IsValid, string Message) ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                return (false, "El usuario no puede estar vacío");

            if (UsernameTextBox.Text.Length < 3)
                return (false, "El usuario debe tener al menos 3 caracteres");

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                return (false, "La contraseña no puede estar vacía");

            if (PasswordBox.Password.Length < 6)
                return (false, "La contraseña debe tener al menos 6 caracteres");

            if (string.IsNullOrWhiteSpace(ServerIpTextBox.Text))
                return (false, "La dirección IP del servidor no puede estar vacía");

            return (true, "");
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }
    }
}