using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using LauncherPhantom.Models;

namespace LauncherPhantom.Managers
{
    public class AuthManager
    {
        private static AuthManager? _instance;
        private static readonly object _lock = new();
        
        private string? _jwtToken;
        private HttpClient _httpClient;

        public static AuthManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AuthManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private AuthManager()
        {
            _httpClient = new HttpClient();
            Debug.WriteLine("[AuthManager] Inicializado");
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var serverUrl = ConfigManager.Instance.GetSetting("server_url") ?? "http://localhost:5000";
                var endpoint = $"{serverUrl}/api/auth/login";

                Debug.WriteLine($"[AuthManager] Enviando login a: {endpoint}");

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"[AuthManager] Respuesta status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);
                    if (authResponse?.Success == true && !string.IsNullOrEmpty(authResponse.Token))
                    {
                        _jwtToken = authResponse.Token;
                        
                        // Save JWT encrypted
                        ConfigManager.Instance.SetSetting("jwt_token", 
                            EncryptionManager.Instance.Encrypt(authResponse.Token));
                        
                        Debug.WriteLine("[AuthManager] Token guardado");
                        return authResponse;
                    }
                }

                return JsonConvert.DeserializeObject<AuthResponse>(responseContent) 
                    ?? new AuthResponse { Success = false, Error = "Error desconocido" };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthManager] Error en LoginAsync: {ex.Message}");
                return new AuthResponse { Success = false, Error = $"Error de conexión: {ex.Message}" };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var serverUrl = ConfigManager.Instance.GetSetting("server_url") ?? "http://localhost:5000";
                var endpoint = $"{serverUrl}/api/auth/register";

                Debug.WriteLine($"[AuthManager] Enviando registro a: {endpoint}");

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"[AuthManager] Respuesta status: {response.StatusCode}");

                return JsonConvert.DeserializeObject<AuthResponse>(responseContent) 
                    ?? new AuthResponse { Success = false, Error = "Error desconocido" };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthManager] Error en RegisterAsync: {ex.Message}");
                return new AuthResponse { Success = false, Error = $"Error de conexión: {ex.Message}" };
            }
        }

        public string? GetToken()
        {
            if (string.IsNullOrEmpty(_jwtToken))
            {
                var encryptedToken = ConfigManager.Instance.GetSetting("jwt_token");
                if (!string.IsNullOrEmpty(encryptedToken))
                {
                    _jwtToken = EncryptionManager.Instance.Decrypt(encryptedToken);
                }
            }
            return _jwtToken;
        }

        public void Logout()
        {
            _jwtToken = null;
            ConfigManager.Instance.DeleteSetting("jwt_token");
            Debug.WriteLine("[AuthManager] Sesión cerrada");
        }
    }
}