using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LauncherPhantom.Managers
{
    public class ConfigManager
    {
        private static ConfigManager? _instance;
        private static readonly object _lock = new();

        private Dictionary<string, string> _config = new();
        private string _configPath = "";

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
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Phantom");

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            _configPath = Path.Combine(appDataPath, "config.json");
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
                }
                else
                {
                    _config = new Dictionary<string, string>();
                    SaveConfig();
                }
            }
            catch
            {
                _config = new Dictionary<string, string>();
            }
        }

        public string? GetSetting(string key)
        {
            if (_config.TryGetValue(key, out var value))
                return value;
            return null;
        }

        public void SetSetting(string key, string value)
        {
            _config[key] = value;
            SaveConfig();
        }

        public void DeleteSetting(string key)
        {
            _config.Remove(key);
            SaveConfig();
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch
            {
                // Silent fail
            }
        }
    }
}