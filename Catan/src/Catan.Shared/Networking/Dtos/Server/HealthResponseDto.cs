namespace Catan.Shared.Networking.Dtos.Server
{
    public class HealthResponseDto
    {
        public bool Success { get; set; }
        public DateTime ServerTime  { get; set; }
    }
}