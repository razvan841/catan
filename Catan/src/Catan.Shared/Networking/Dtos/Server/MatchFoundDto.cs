namespace Catan.Shared.Networking.Dtos.Server;

public class MatchFoundDto
{
    public Guid MatchId { get; set; }
    public string[] Players { get; set; } = [];
    public string Game { get; set; } = string.Empty;
}
