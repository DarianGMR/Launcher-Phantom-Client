using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace LauncherPhantom.Managers
{
    public class SoundManager
    {
        private static SoundManager? _instance;
        private static readonly object _lock = new();

        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SoundManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private SoundManager() { }

        public void PlaySound(string soundName)
        {
            try
            {
                var soundPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                    "Resources", $"{soundName}.wav");

                Debug.WriteLine($"[SoundManager] Buscando sonido en: {soundPath}");

                if (File.Exists(soundPath))
                {
                    using (var soundPlayer = new System.Media.SoundPlayer(soundPath))
                    {
                        soundPlayer.PlaySync();
                        Debug.WriteLine($"[SoundManager] Sonido reproducido: {soundName}");
                    }
                }
                else
                {
                    Debug.WriteLine($"[SoundManager] Archivo de sonido no encontrado: {soundPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SoundManager] Error reproduciendo sonido: {ex.Message}");
            }
        }

        public void PlaySoundAsync(string soundName)
        {
            try
            {
                var task = new System.Threading.Tasks.Task(() => PlaySound(soundName));
                task.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SoundManager] Error en PlaySoundAsync: {ex.Message}");
            }
        }
    }
}