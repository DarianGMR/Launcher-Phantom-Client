namespace LauncherPhantom.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}