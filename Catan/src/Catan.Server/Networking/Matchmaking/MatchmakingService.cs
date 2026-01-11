using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catan.Server.Game;
using Catan.Server.Sessions;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Dtos.Server;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;

namespace Catan.Server.Matchmaking;

public static class MatchmakingService
{
    private static readonly List<ClientSession> _queue = new();
    private static readonly Dictionary<Guid, PendingMatch> _pendingMatches = new();
    private static readonly object _lock = new();
    public const int CatanPlayers = 2;

    // ============================
    // Queue
    // ============================

    public static bool TryEnqueue(ClientSession session, out string reason)
    {
        lock (_lock)
        {
            if (_queue.Contains(session))
            {
                reason = "Already in queue";
                return false;
            }

            _queue.Add(session);
            reason = "Added to queue";

            if (_queue.Count >= CatanPlayers)
                _ = CreatePendingMatchAsync();
        }

        return true;
    }

    // ============================
    // Match creation
    // ============================

    private static async Task CreatePendingMatchAsync()
    {
        PendingMatch match;

        lock (_lock)
        {
            if (_queue.Count < CatanPlayers)
                return;

            var players = _queue.Take(CatanPlayers).ToList();
            _queue.RemoveRange(0, CatanPlayers);

            match = new PendingMatch(players);
            _pendingMatches[match.MatchId] = match;
        }

        var msg = new ServerMessage
        {
            Type = MessageType.MatchFound,
            Payload = new MatchFoundDto
            {
                MatchId = match.MatchId,
                Players = match.Players.Select(p => p.Username!).ToArray()
            }
        };

        var data = JsonMessageSerializer.Serialize(msg);

        foreach (var p in match.Players)
            await p.Stream.WriteAsync(data);
    }

    // ============================
    // Match response
    // ============================

    public static async Task HandleMatchResponseAsync(ClientSession session, MatchResponseDto dto)
    {
        PendingMatch match;

        lock (_lock)
        {
            if (!_pendingMatches.TryGetValue(dto.MatchId, out match))
                return;
        }

        if (!dto.Accepted)
        {
            await CancelMatchAsync(match, $"Match canceled (player {session.Username} declined");
            return;
        }

        bool shouldStart;

        lock (_lock)
        {
            match.AwaitingResponses.Remove(session);
            shouldStart = match.AwaitingResponses.Count == 0;
        }

        if (!shouldStart)
            return;

        lock (_lock)
        {
            _pendingMatches.Remove(match.MatchId);
        }

        var game = new GameSession(match.Players);
        await game.StartAsync();
    }

    // ============================
    // Cancel match
    // ============================

    private static async Task CancelMatchAsync(PendingMatch match, string reason)
    {
        lock (_lock)
        {
            _pendingMatches.Remove(match.MatchId);
        }

        var msg = new ServerMessage
        {
            Type = MessageType.MatchCanceled,
            Payload = new MatchCanceledDto
            {
                MatchId = match.MatchId,
                Reason = reason
            }
        };

        var data = JsonMessageSerializer.Serialize(msg);

        foreach (var p in match.Players)
            await p.Stream.WriteAsync(data);
    }
}
