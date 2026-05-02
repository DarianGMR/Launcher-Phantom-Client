namespace LauncherPhantom.Models
{
    public class VersionInfo
    {
        public string Version { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string Changes { get; set; } = "";
        public bool Required { get; set; } = false;
        public string? ReleaseDate { get; set; }
        public string? Status { get; set; }
    }
}