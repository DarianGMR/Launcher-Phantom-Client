using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace LauncherPhantom.Managers
{
    public class ImageSourceManager : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? imagePath = null;
            try
            {
                if (value is string imagePathValue && !string.IsNullOrEmpty(imagePathValue))
                {
                    imagePath = imagePathValue;
                    
                    // Convertir ruta relativa a pack:// URI (recursos incrustados)
                    var packUri = $"pack://application:,,,/{imagePath}";                    
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(packUri, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    
                    return bitmap;
                }
                return GetDefaultImage();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImageManager] Error cargando {imagePath}: {ex.Message}");
                return GetDefaultImage();
            }
        }

        private static BitmapImage GetDefaultImage()
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Resources/Images/Catalogo/default.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImageManager] Error en GetDefaultImage: {ex.Message}");
                return new BitmapImage();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}