using Catan.Server.Game;
using Catan.Server.Sessions;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;
using Catan.Shared.Networking.Dtos;
using Catan.Shared.Enums;

namespace Catan.Server.Matchmaking;

public static class MatchmakingService
{
    private static readonly List<ClientSession> _queue = new();
    private static readonly Dictionary<Guid, PendingMatch> _pendingMatches = new();

    public static bool TryEnqueue(ClientSession session, out string reason)
    {
        if (_queue.Contains(session))
        {
            reason = "Already in queue";
            return false;
        }

        _queue.Add(session);
        reason = "Added to queue";

        if (_queue.Count >= 4)
            CreatePendingMatch();

        return true;
    }

    private static async void CreatePendingMatch()
    {
        var players = _queue.Take(4).ToList();
        var match = new PendingMatch(players);

        _pendingMatches[match.MatchId] = match;

        var message = new ServerMessage
        {
            Type = MessageType.MatchFound,
            Payload = new MatchFoundDto
            {
                MatchId = match.MatchId,
                Players = players.Select(p => p.Username!).ToArray()
            }
        };

        var data = JsonMessageSerializer.Serialize(message);

        foreach (var p in players)
            await p.Stream.WriteAsync(data);
    }

    public static async void HandleMatchResponse(ClientSession session, MatchResponseDto dto)
    {
        if (!_pendingMatches.TryGetValue(dto.MatchId, out var match))
            return;

        if (!dto.Accepted)
        {
            _queue.Remove(session);
            match.Players.Remove(session);

            await NotifyAsync(match.Players, "Match canceled (player declined)");

            _pendingMatches.Remove(dto.MatchId);
            return;
        }

        match.AwaitingResponses.Remove(session);

        if (match.AwaitingResponses.Count == 0)
        {
            foreach (var p in match.Players)
                _queue.Remove(p);

            var game = new GameSession(match.Players);
            await game.StartAsync();

            _pendingMatches.Remove(dto.MatchId);
        }
    }

    private static async Task NotifyAsync(IEnumerable<ClientSession> sessions, string message)
    {
        var msg = new ServerMessage
        {
            Type = MessageType.MatchCanceled,
            Payload = new
            {
                Success = false,
                Message = message
            }
        };

        var data = JsonMessageSerializer.Serialize(msg);

        foreach (var s in sessions)
            await s.Stream.WriteAsync(data);
    }
}
