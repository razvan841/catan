namespace Catan.Shared.Networking.Dtos.Client
{
    public class WhisperRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}