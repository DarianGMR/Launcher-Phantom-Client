using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;

namespace LauncherPhantom.Views
{
    public class GameItem
    {
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string InstallPath { get; set; } = "";
    }

    public partial class BibliotecaPage : Page
    {
        private ObservableCollection<GameItem>? _localGames;

        public BibliotecaPage()
        {
            try
            {
                InitializeComponent();
                _localGames = new ObservableCollection<GameItem>();
                GamesList.ItemsSource = _localGames;
                Loaded += BibliotecaPage_Loaded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BibliotecaPage] Error en constructor: {ex.Message}");
            }
        }

        private void BibliotecaPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[BibliotecaPage] Cargando biblioteca local...");
                LoadLocalGames();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BibliotecaPage] Error en Loaded: {ex.Message}");
            }
        }

        private void LoadLocalGames()
        {
            try
            {
                var gamesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LauncherPhantom", "Juegos");
                
                if (!Directory.Exists(gamesPath))
                {
                    LibraryStatus.Text = "No hay juegos instalados";
                    return;
                }

                _localGames?.Clear();
                var gameDirectories = Directory.GetDirectories(gamesPath);

                if (gameDirectories.Length == 0)
                {
                    LibraryStatus.Text = "No hay juegos instalados";
                    return;
                }

                foreach (var gameDir in gameDirectories)
                {
                    try
                    {
                        var dirName = Path.GetFileName(gameDir);
                        _localGames?.Add(new GameItem
                        {
                            Name = dirName,
                            Version = "v1.0",
                            ImageUrl = "pack://application:,,,/Resources/Images/icon.ico",
                            InstallPath = gameDir
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[BibliotecaPage] Error cargando juego: {ex.Message}");
                    }
                }

                LibraryStatus.Text = $"Se encontraron {_localGames?.Count} juegos";
                Debug.WriteLine($"[BibliotecaPage] {_localGames?.Count} juegos cargados");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BibliotecaPage] Error en LoadLocalGames: {ex.Message}");
                LibraryStatus.Text = "Error al cargar biblioteca";
            }
        }
    }
}