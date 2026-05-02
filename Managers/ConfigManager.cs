using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace LauncherPhantom.Managers
{
    public class ConfigManager
    {
        private static ConfigManager? _instance;
        private static readonly object _lock = new();

        private Dictionary<string, string> _config = new();
        private string _configPath = "";
        private string _appDataPath = "";

        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private ConfigManager()
        {
            try
            {
                _appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Phantom");

                if (!Directory.Exists(_appDataPath))
                {
                    Directory.CreateDirectory(_appDataPath);
                }

                _configPath = Path.Combine(_appDataPath, "config.json");
                Debug.WriteLine($"[ConfigManager] Config path: {_configPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Error en constructor: {ex.Message}");
            }
        }

        public void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) 
                        ?? new Dictionary<string, string>();
                    
                    Debug.WriteLine($"[ConfigManager] Config cargada desde {_configPath}");
                }
                else
                {
                    Debug.WriteLine("[ConfigManager] Config no existe, creando default...");
                    _config = new Dictionary<string, string>
                    {
                        { "server_url", "http://localhost:5000" },
                        { "language", "es" },
                        { "theme", "dark" }
                    };
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Error cargando config: {ex.Message}");
                _config = new Dictionary<string, string>();
            }
        }

        public string? GetSetting(string key)
        {
            try
            {
                if (_config.TryGetValue(key, out var value))
                {
                    return value;
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Error en GetSetting: {ex.Message}");
                return null;
            }
        }

        public void SetSetting(string key, string value)
        {
            try
            {
                _config[key] = value;
                SaveConfig();
                Debug.WriteLine($"[ConfigManager] Set: {key}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Error en SetSetting: {ex.Message}");
            }
        }

        public void DeleteSetting(string key)
        {
            try
            {
                _config.Remove(key);
                SaveConfig();
                Debug.WriteLine($"[ConfigManager] Deleted: {key}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Error en DeleteSetting: {ex.Message}");
            }
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
                Debug.WriteLine($"[ConfigManager] Config guardada en {_configPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Error guardando config: {ex.Message}");
            }
        }
    }
}