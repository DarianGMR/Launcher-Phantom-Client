using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Views;

namespace LauncherPhantom
{
    public partial class MainWindow : Window
    {
        private RotateTransform? _spinnerRotate;

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

        private async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("[MainWindow] Iniciando InitializeAsync...");
                
                // Show splash screen
                Debug.WriteLine("[MainWindow] Mostrando Splash Screen...");
                SplashScreen.Visibility = Visibility.Visible;
                MainFrame.Visibility = Visibility.Collapsed;
                
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
            // Rotate the spinner
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
                MainFrame.Navigate(page);
                Debug.WriteLine($"[MainWindow] Navegación exitosa a {page.GetType().Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Error al navegar: {ex.Message}");
                MessageBox.Show($"Error en navegación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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