namespace Catan.Shared.Networking.Dtos.Server;

public class DequeueResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
}
