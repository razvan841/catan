namespace Catan.Shared.Networking.Dtos.Server
{
    public class PlayerInfoResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Username { get; set; }
        public int Elo { get; set; }
        public string[] Friends { get; set; } = Array.Empty<string>();
    }
}