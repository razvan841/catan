namespace Catan.Shared.Networking.Dtos.Server
{
    public class GroupMessageResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = default!;
    }
}