using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using LauncherPhantom.Views;

namespace LauncherPhantom
{
    public partial class MainWindow : Window
    {
        private RotateTransform? _spinnerRotate;
        private const int LoginWidth = 640;
        private const int LoginHeight = 480;
        private const int DashboardWidth = 840;
        private const int DashboardHeight = 580;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                Loaded += async (s, e) =>
                {
                    await InitializeAsync();
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error en constructor: {ex.Message}");
                MessageBox.Show($"Error inicializando ventana: {ex.Message}");
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Bloquear teclas que interfieren con la navegación
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                // Permitir solo si el foco está en un TextBox o PasswordBox
                var focusedElement = Keyboard.FocusedElement;
                
                if (focusedElement is TextBox textBox)
                {
                    return;
                }
                
                if (focusedElement is PasswordBox passwordBox)
                {
                    return;
                }

                e.Handled = true;
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("[MainWindow] Iniciando InitializeAsync...");
                
                // Show splash screen
                Debug.WriteLine("[MainWindow] Mostrando Splash Screen...");
                SplashScreen.Visibility = Visibility.Visible;
                MainFrame.Visibility = Visibility.Collapsed;
                SplashScreen.Opacity = 1.0;
                
                // Animate loading progress
                for (int i = 0; i <= 100; i += 5)
                {
                    SplashProgressBar.Value = i;
                    
                    switch (i / 25)
                    {
                        case 0:
                            SplashStatus.Text = "Inicializando...";
                            break;
                        case 1:
                            SplashStatus.Text = "Cargando recursos...";
                            break;
                        case 2:
                            SplashStatus.Text = "Configurando base de datos...";
                            break;
                        case 3:
                            SplashStatus.Text = "Finalizando...";
                            break;
                    }
                    
                    await Task.Delay(250);
                }
                
                SplashProgressBar.Value = 100;
                SplashStatus.Text = "¡Listo!";
                
                // Fade out splash
                var fadeOutAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.5));
                SplashScreen.BeginAnimation(UIElement.OpacityProperty, fadeOutAnim);
                
                await Task.Delay(500);
                
                Debug.WriteLine("[MainWindow] Splash Screen completado");
                
                // Hide splash and show main frame
                SplashScreen.Visibility = Visibility.Collapsed;
                MainFrame.Visibility = Visibility.Visible;
                
                // Fade in main frame
                MainFrame.Opacity = 0;
                var fadeInAnim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.5));
                MainFrame.BeginAnimation(UIElement.OpacityProperty, fadeInAnim);

                // Navigate to login
                Debug.WriteLine("[MainWindow] Navegando a LoginPage...");
                NavigateTo(new LoginPage());
                
                Debug.WriteLine("[MainWindow] InitializeAsync completado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] ERROR: {ex.Message}");
                Debug.WriteLine($"[MainWindow] StackTrace: {ex.StackTrace}");
                
                SplashScreen.Visibility = Visibility.Collapsed;
                MainFrame.Visibility = Visibility.Visible;
                
                MessageBox.Show(
                    $"Error al inicializar:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        public void ShowLoading(bool show)
        {
            try
            {
                if (show)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                    AnimateLoading();
                }
                else
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error en ShowLoading: {ex.Message}");
            }
        }

        private void AnimateLoading()
        {
            _spinnerRotate = new RotateTransform();
            LoadingSpinner.RenderTransform = _spinnerRotate;
            LoadingSpinner.RenderTransformOrigin = new Point(0.5, 0.5);

            var rotateAnim = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(2))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            _spinnerRotate.BeginAnimation(RotateTransform.AngleProperty, rotateAnim);
        }

        public void NavigateTo(Page page)
        {
            try
            {
                // Cambiar tamaño de la ventana según la página
                if (page is DashboardPage)
                {
                    ResizeWindow(DashboardWidth, DashboardHeight);
                }
                else
                {
                    ResizeWindow(LoginWidth, LoginHeight);
                }
                
                // Aplicar fade-out a la página actual
                MainFrame.Opacity = 1;
                var fadeOutAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.3));
                MainFrame.BeginAnimation(UIElement.OpacityProperty, fadeOutAnim);
                
                // Navegar y fade-in
                Task.Delay(300).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MainFrame.Navigate(page);
                        MainFrame.Opacity = 0;
                        var fadeInAnim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.3));
                        MainFrame.BeginAnimation(UIElement.OpacityProperty, fadeInAnim);
                    });
                });
                
                Debug.WriteLine($"[MainWindow] Navegación exitosa a {page.GetType().Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error al navegar: {ex.Message}");
                MessageBox.Show($"Error en navegación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResizeWindow(int width, int height)
        {
            try
            {                
                // Obtener la posición actual del centro de la pantalla
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                            
                // Calcular la nueva posición para centrar la ventana
                var newLeft = (screenWidth - width) / 2;
                var newTop = (screenHeight - height) / 2;
                                
                // Cambiar tamaño inmediatamente
                this.Width = width;
                this.Height = height;
                
                // Centrar la ventana inmediatamente
                this.Left = newLeft;
                this.Top = newTop;
                            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error redimensionando ventana: {ex.Message}");
            }
        }

        // Title Bar drag functionality
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                return;
            
            DragMove();
        }

        // Minimize Button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Close Button
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}