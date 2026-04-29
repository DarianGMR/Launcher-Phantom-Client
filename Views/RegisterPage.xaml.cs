using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Managers;
using LauncherPhantom.Models;

namespace LauncherPhantom.Views
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordStrength(PasswordBox.Password);
        }

        private void UpdatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                PasswordStrengthBar.Value = 0;
                PasswordStrengthText.Text = "";
                return;
            }

            int strength = 0;
            if (password.Length >= 8) strength++;
            if (Regex.IsMatch(password, "[a-z]")) strength++;
            if (Regex.IsMatch(password, "[A-Z]")) strength++;
            if (Regex.IsMatch(password, "[0-9]")) strength++;
            if (Regex.IsMatch(password, "[^a-zA-Z0-9]")) strength++;

            PasswordStrengthBar.Value = (strength / 5.0) * 100;

            switch (strength)
            {
                case 1:
                case 2:
                    PasswordStrengthText.Text = "Débil";
                    PasswordStrengthBar.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 51, 51));
                    break;
                case 3:
                    PasswordStrengthText.Text = "Regular";
                    PasswordStrengthBar.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 184, 0));
                    break;
                case 4:
                case 5:
                    PasswordStrengthText.Text = "Fuerte";
                    PasswordStrengthBar.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                    break;
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[RegisterPage] RegisterButton_Click");
            ErrorMessageText.Visibility = Visibility.Collapsed;

            var validationResult = ValidateInputs();
            if (!validationResult.IsValid)
            {
                Debug.WriteLine($"[RegisterPage] Validación falló: {validationResult.Message}");
                ShowError(validationResult.Message);
                return;
            }

            RegisterButton.IsEnabled = false;
            RegisterButton.Content = "Registrando...";

            try
            {
                Debug.WriteLine("[RegisterPage] Probando conexión con servidor...");
                ServerManager.Instance.SetServerUrl($"http://{ServerIpTextBox.Text}");
                
                bool canConnect = await ServerManager.Instance.TestConnectionAsync();
                if (!canConnect)
                {
                    ShowError("No se puede conectar con el servidor. Verifica la dirección IP.");
                    RegisterButton.IsEnabled = true;
                    RegisterButton.Content = "Registrarse";
                    return;
                }

                Debug.WriteLine("[RegisterPage] Enviando request de registro...");
                
                var request = new RegisterRequest
                {
                    Username = UsernameTextBox.Text,
                    Email = EmailTextBox.Text,
                    Password = PasswordBox.Password
                };

                var response = await AuthManager.Instance.RegisterAsync(request);

                if (response.Success)
                {
                    Debug.WriteLine("[RegisterPage] Registro exitoso");
                    SoundManager.Instance.PlaySound("success");
                    MessageBox.Show("¡Cuenta creada exitosamente!\nRedirigiendo a Login...", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    await Task.Delay(2000);
                    
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.NavigateTo(new LoginPage());
                }
                else
                {
                    Debug.WriteLine($"[RegisterPage] Registro fallo: {response.Error}");
                    SoundManager.Instance.PlaySound("error");
                    ShowError(response.Error ?? "Error desconocido");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegisterPage] ERROR: {ex.Message}");
                SoundManager.Instance.PlaySound("error");
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "Registrarse";
            }
        }

        private void TermsLink_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[RegisterPage] Abriendo Términos y Condiciones...");
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://example.com/terms",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegisterPage] Error abriendo navegador: {ex.Message}");
                MessageBox.Show("No se pudo abrir el navegador. Visita https://example.com/terms", "Información");
            }
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[RegisterPage] Navegando a LoginPage");
            try
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.NavigateTo(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegisterPage] Error al navegar: {ex.Message}");
            }
        }

        private (bool IsValid, string Message) ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                return (false, "El usuario no puede estar vacío");

            if (UsernameTextBox.Text.Length < 3 || UsernameTextBox.Text.Length > 20)
                return (false, "El usuario debe tener entre 3 y 20 caracteres");

            if (!Regex.IsMatch(UsernameTextBox.Text, @"^[a-zA-Z]"))
                return (false, "El usuario no puede empezar con número");

            if (!Regex.IsMatch(UsernameTextBox.Text, @"^[a-zA-Z0-9_-]+$"))
                return (false, "El usuario solo puede contener letras, números, guiones y guiones bajos");

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                return (false, "El email no puede estar vacío");

            if (!Regex.IsMatch(EmailTextBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return (false, "Email inválido");

            if (string.IsNullOrWhiteSpace(ServerIpTextBox.Text))
                return (false, "La dirección IP del servidor no puede estar vacía");

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                return (false, "La contraseña no puede estar vacía");

            if (PasswordBox.Password.Length < 8)
                return (false, "La contraseña debe tener al menos 8 caracteres");

            if (!Regex.IsMatch(PasswordBox.Password, "[a-z]"))
                return (false, "La contraseña debe contener minúsculas");

            if (!Regex.IsMatch(PasswordBox.Password, "[A-Z]"))
                return (false, "La contraseña debe contener mayúsculas");

            if (!Regex.IsMatch(PasswordBox.Password, "[0-9]"))
                return (false, "La contraseña debe contener números");

            if (!Regex.IsMatch(PasswordBox.Password, "[^a-zA-Z0-9]"))
                return (false, "La contraseña debe contener símbolos especiales");

            if (!TermsCheckBox.IsChecked.GetValueOrDefault())
                return (false, "Debes aceptar los Términos y Condiciones");

            return (true, "");
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }
    }
}