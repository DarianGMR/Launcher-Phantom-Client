using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherPhantom.Views;

namespace LauncherPhantom
{
    public partial class MainWindow : Window
    {
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
            var bars = new[] { LoadBar1, LoadBar2, LoadBar3, LoadBar4, LoadBar5 };
            
            for (int i = 0; i < bars.Length; i++)
            {
                var bar = bars[i];
                var animation = new DoubleAnimation(0, 100, TimeSpan.FromSeconds(1.5))
                {
                    BeginTime = TimeSpan.FromMilliseconds(i * 150),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };
                bar.BeginAnimation(ProgressBar.ValueProperty, animation);
            }
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
    }
}
