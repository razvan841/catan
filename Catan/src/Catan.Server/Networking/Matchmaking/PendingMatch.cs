using Catan.Server.Sessions;

namespace Catan.Server.Matchmaking;

public class PendingMatch
{
    public Guid MatchId { get; } = Guid.NewGuid();
    public List<ClientSession> Players { get; }
    public string Game { get; }
    public HashSet<ClientSession> AwaitingResponses { get; }

    public PendingMatch(List<ClientSession> players, string game)
    {
        Players = players;
        Game = game;
        AwaitingResponses = players.ToHashSet();
    }
}
