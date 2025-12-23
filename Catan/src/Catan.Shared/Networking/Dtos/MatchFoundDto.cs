namespace Catan.Shared.Networking.Dtos;

public class MatchFoundDto
{
    public Guid MatchId { get; set; }
    public string[] Players { get; set; } = [];
}
