using System;
using System.IO;
using System.Reflection;

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
                var soundPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                    "Resources", $"{soundName}.wav");

                if (File.Exists(soundPath))
                {
                    using (var soundPlayer = new System.Media.SoundPlayer(soundPath))
                    {
                        soundPlayer.PlaySync();
                    }
                }
            }
            catch
            {
                // Silent fail if sound doesn't play
            }
        }

        public void PlaySoundAsync(string soundName)
        {
            try
            {
                var task = new System.Threading.Tasks.Task(() => PlaySound(soundName));
                task.Start();
            }
            catch
            {
                // Silent fail
            }
        }
    }
}