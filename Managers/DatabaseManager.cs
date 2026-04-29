using System;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;

namespace LauncherPhantom.Managers
{
    public class DatabaseManager : IDisposable
    {
        private static DatabaseManager? _instance;
        private static readonly object _lock = new();

        private string _connectionString = "";
        private SQLiteConnection? _connection;

        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private DatabaseManager() { }

        public void Initialize()
        {
            try
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Phantom");

                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                    Debug.WriteLine($"[DatabaseManager] Carpeta creada: {appDataPath}");
                }

                var dbPath = Path.Combine(appDataPath, "launcher.db");
                _connectionString = $"Data Source={dbPath};Version=3;";

                Debug.WriteLine($"[DatabaseManager] Conectando a: {dbPath}");
                
                _connection = new SQLiteConnection(_connectionString);
                _connection.Open();

                Debug.WriteLine("[DatabaseManager] Conexión abierta correctamente");

                CreateTables();
                
                Debug.WriteLine("[DatabaseManager] Base de datos inicializada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DatabaseManager] Error en Initialize: {ex.Message}");
                Debug.WriteLine($"[DatabaseManager] StackTrace: {ex.StackTrace}");
            }
        }

        private void CreateTables()
        {
            try
            {
                using (var cmd = new SQLiteCommand(_connection))
                {
                    // Create cache table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Cache (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Key TEXT UNIQUE NOT NULL,
                            Value TEXT NOT NULL,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";
                    cmd.ExecuteNonQuery();
                    Debug.WriteLine("[DatabaseManager] Tabla 'Cache' creada/verificada");

                    // Create logs table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Logs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Level TEXT NOT NULL,
                            Message TEXT NOT NULL,
                            Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";
                    cmd.ExecuteNonQuery();
                    Debug.WriteLine("[DatabaseManager] Tabla 'Logs' creada/verificada");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DatabaseManager] Error creando tablas: {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                _connection?.Close();
                _connection?.Dispose();
                Debug.WriteLine("[DatabaseManager] Conexión cerrada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DatabaseManager] Error en Dispose: {ex.Message}");
            }
        }
    }
}