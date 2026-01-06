namespace Catan.Shared.Networking.Dtos.Server
{
    public class WhisperResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = default!;
    }
}