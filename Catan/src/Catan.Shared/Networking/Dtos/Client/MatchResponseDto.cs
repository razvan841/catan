namespace Catan.Shared.Networking.Dtos.Client;

public class MatchResponseDto
{
    public Guid MatchId { get; set; }
    public bool Accepted { get; set; }
    public string Game { get; set; } = string.Empty;
}
