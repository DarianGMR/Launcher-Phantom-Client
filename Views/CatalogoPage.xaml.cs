using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LauncherPhantom.Managers;

namespace LauncherPhantom.Views
{
    public class GameCatalogItem
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public long Size { get; set; }
    }

    public partial class CatalogoPage : Page
    {
        private ObservableCollection<GameCatalogItem>? _games;

        public CatalogoPage()
        {
            try
            {
                InitializeComponent();
                _games = new ObservableCollection<GameCatalogItem>();
                GamesCatalog.ItemsSource = _games;
                Loaded += CatalogoPage_Loaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CatalogoPage] Error en constructor: {ex.Message}");
            }
        }

        private async void CatalogoPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[CatalogoPage] Cargando catálogo...");
                await LoadGamesCatalogAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CatalogoPage] Error en Loaded: {ex.Message}");
            }
        }

        private async Task LoadGamesCatalogAsync()
        {
            try
            {
                await Task.Delay(1000);
                
                _games?.Clear();
                
                // Usar rutas pack:// para recursos incrustados
                _games?.Add(new GameCatalogItem
                {
                    Name = "Battlefield 3",
                    Description = "Juego FPS multijugador",
                    ImageUrl = "Resources/Images/Catalogo/battlefield3.png",
                    DownloadUrl = "/api/games/bf3/download",
                    Size = 15000000000
                });

                _games?.Add(new GameCatalogItem
                {
                    Name = "Minecraft",
                    Description = "Sandbox creativo",
                    ImageUrl = "Resources/Images/Catalogo/minecraft.png",
                    DownloadUrl = "/api/games/minecraft/download",
                    Size = 500000000
                });

                _games?.Add(new GameCatalogItem
                {
                    Name = "Rust",
                    Description = "Survival multijugador",
                    ImageUrl = "Resources/Images/Catalogo/rust.png",
                    DownloadUrl = "/api/games/rust/download",
                    Size = 20000000000
                });

                _games?.Add(new GameCatalogItem
                {
                    Name = "Hurtworld",
                    Description = "MMO Survival",
                    ImageUrl = "Resources/Images/Catalogo/hurtworld.png",
                    DownloadUrl = "/api/games/hurtworld/download",
                    Size = 8000000000
                });

                EmptyState.Visibility = Visibility.Collapsed;
                Debug.WriteLine("[CatalogoPage] Catálogo cargado con éxito");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CatalogoPage] Error cargando catálogo: {ex.Message}");
                EmptyState.Visibility = Visibility.Visible;
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is GameCatalogItem game)
                {
                    MessageBox.Show($"Funcion no disponible", "Descarga", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}