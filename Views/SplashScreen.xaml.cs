using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using LauncherPhantom.Managers;

namespace LauncherPhantom.Views
{
    public partial class SplashScreenWindow : Window
    {
        public SplashScreenWindow()
        {
            InitializeComponent();
            
            Loaded += async (s, e) =>
            {
                await ShowSplashAsync();
            };
        }

        private async Task ShowSplashAsync()
        {
            // Fade in logo
            var logoFadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            LogoImage.BeginAnimation(OpacityProperty, logoFadeIn);

            // Fade in title
            var titleFadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            FindName("StatusText"); // Get TextBlock for animation
            await Task.Delay(250);

            // Simulate loading processes
            UpdateStatus("Cargando configuración...");
            await Task.Delay(1000);
            
            UpdateStatus("Verificando conexión...");
            await Task.Delay(1500);

            UpdateStatus("Inicializando base de datos...");
            await Task.Delay(1000);

            // Wait total 5 seconds max
            await Task.Delay(1000);

            // Fade out
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
            BeginAnimation(OpacityProperty, fadeOut);
            
            await Task.Delay(500);
            Close();
        }

        private void UpdateStatus(string text)
        {
            StatusText.Text = text;
        }
    }
}