using System;
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
    private static readonly List<ClientSession> _queue_Catan = new();
    private static readonly List<ClientSession> _queue_Chess = new();
    private static readonly Dictionary<Guid, PendingMatch> _pendingMatches_Catan = new();
    private static readonly Dictionary<Guid, PendingMatch> _pendingMatches_Chess = new();
    private static readonly object _lock = new();
    public const int CatanPlayers = 4;
    public const int ChessPlayers = 2;

    // ============================
    // Queue
    // ============================

    public static bool TryEnqueue(ClientSession session, string game, out string reason)
    {
        lock (_lock)
        {
            if (_queue_Catan.Contains(session) || _queue_Chess.Contains(session))
                {
                    reason = "Already in a queue!";
                    return false;
                }
            reason = "Added to queue";
            if (game == "Catan")
            {
                _queue_Catan.Add(session);
                // TODO LATER: Proper matchmaking choices (not just first 4)
                if (_queue_Catan.Count >= CatanPlayers)
                    _ = CreatePendingMatchAsync(game);
            } else
            {
                _queue_Chess.Add(session);

                if (_queue_Chess.Count >= ChessPlayers)
                    _ = CreatePendingMatchAsync(game);
            }
        }

        return true;
    }

    public static bool TryDequeue(ClientSession session, string game, out string reason)
    {
        lock (_lock)
        {
            if (game == "Catan")
            {
                if (!_queue_Catan.Contains(session))
                {
                    reason = "Already not in the queue!";
                    return false;
                }

                _queue_Catan.Remove(session);
                reason = "Removed from queue";
            } else
            {
                if (!_queue_Chess.Contains(session))
                {
                    reason = "Already not in the queue!";
                    return false;
                }

                _queue_Chess.Remove(session);
                reason = "Removed from queue";
            }
        }

        return true;
    }

    public static void ForceDequeue(ClientSession session)
    {
        lock (_lock)
        {
            if (!_queue_Catan.Contains(session))
                _queue_Catan.Remove(session);

            if (_queue_Chess.Contains(session))
            _queue_Chess.Remove(session);
        }
    }

    // ============================
    // Match creation
    // ============================

    private static async Task CreatePendingMatchAsync(string game)
    {
        PendingMatch match;
        lock (_lock)
        {
            if (game == "Catan")
            {
                if (_queue_Catan.Count < CatanPlayers)
                    return;

                var players = _queue_Catan.Take(CatanPlayers).ToList();
                _queue_Catan.RemoveRange(0, CatanPlayers);

                match = new PendingMatch(players, game);
                _pendingMatches_Catan[match.MatchId] = match;
            } else
            {
                if (_queue_Chess.Count < ChessPlayers)
                    return;

                var players = _queue_Chess.Take(ChessPlayers).ToList();
                _queue_Chess.RemoveRange(0, ChessPlayers);

                match = new PendingMatch(players, game);
                _pendingMatches_Chess[match.MatchId] = match;
            }
        }

        var msg = new ServerMessage
        {
            Type = MessageType.MatchFound,
            Payload = new MatchFoundDto
            {
                MatchId = match.MatchId,
                Players = match.Players.Select(p => p.Username!).ToArray(),
                Game = game
            }
        };

        var data = JsonMessageSerializer.Serialize(msg);
        Console.WriteLine(data);
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
            if(dto.Game == "Catan")
            {
                if (!_pendingMatches_Catan.TryGetValue(dto.MatchId, out match))
                    return;
            } else
            {
                if (!_pendingMatches_Chess.TryGetValue(dto.MatchId, out match))
                    return;
            }
            
        }

        if (!dto.Accepted)
        {
            await CancelMatchAsync(match, dto.Game, session, $"Match canceled (player {session.Username} declined)! Returning to queue...");
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
            if(dto.Game == "Catan")
            {
                _pendingMatches_Catan.Remove(match.MatchId);
            } else
            {
                _pendingMatches_Chess.Remove(match.MatchId);
            }
            
        }

        var game = new GameSession(match.Players);
        await game.StartAsync(dto.Game);
    }

    // ============================
    // Cancel match
    // ============================

    private static async Task CancelMatchAsync(PendingMatch match, string game, ClientSession declinedBy, string reason)
    {
        lock (_lock)
        {
            if (game == "Catan")
                _pendingMatches_Catan.Remove(match.MatchId);
            else
                _pendingMatches_Chess.Remove(match.MatchId);

            foreach (var player in match.Players)
            {
                if (player == declinedBy)
                    continue;

                if (game == "Catan")
                    _queue_Catan.Add(player);
                else
                    _queue_Chess.Add(player);
            }
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
