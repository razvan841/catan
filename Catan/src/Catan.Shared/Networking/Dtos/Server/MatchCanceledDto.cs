namespace Catan.Shared.Networking.Dtos.Server;

public class MatchCanceledDto
{
    public Guid MatchId { get; set; }
    public string Reason { get; set; } = default!;
}
