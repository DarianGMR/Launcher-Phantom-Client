using System;
using System.Data.SQLite;
using System.IO;

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
                    Directory.CreateDirectory(appDataPath);

                var dbPath = Path.Combine(appDataPath, "launcher.db");
                _connectionString = $"Data Source={dbPath};Version=3;";

                _connection = new SQLiteConnection(_connectionString);
                _connection.Open();

                CreateTables();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
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

                    // Create logs table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Logs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Level TEXT NOT NULL,
                            Message TEXT NOT NULL,
                            Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating tables: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}