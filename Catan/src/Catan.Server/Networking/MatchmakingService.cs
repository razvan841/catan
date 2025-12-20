using Catan.Server.Sessions;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Dtos;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;

namespace Catan.Server.Networking;

public static class MatchmakingService
{
    private static readonly List<ClientSession> _queue = new();
    private static readonly object _lock = new();
    private static readonly Dictionary<ClientSession, bool> _matchResponses = new();

    public static bool TryEnqueue(ClientSession session, out string reason)
    {
        lock (_lock)
        {
            if (_queue.Contains(session))
            {
                reason = "Already in queue.";
                return false;
            }

            session.State = SessionState.InQueue;
            _queue.Add(session);
            reason = "Added to queue.";

            if (_queue.Count == 4)
                StartMatch();

            return true;
        }
    }

    private static void StartMatch()
    {
        _matchResponses.Clear();

        var players = _queue.Select(s => s.Username).ToList();

        foreach (var session in _queue)
        {
            Send(session, new ServerMessage
            {
                Type = MessageType.MatchFoundNotification,
                Payload = new MatchFoundDto
                {
                    Players = players
                }
            });
        }
    }

    public static void HandleMatchResponse(ClientSession session, MatchResponseDto response)
    {
        lock (_lock)
        {
            _matchResponses[session] = response.Accept;

            if (_matchResponses.Count < 4)
                return;

            var deniedSessions = _matchResponses.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();

            if (deniedSessions.Any())
            {
                foreach (var sessionInQueue in _queue)
                {
                    Send(sessionInQueue, new ServerMessage
                    {
                        Type = MessageType.MatchCanceledNotification,
                        Payload = new { message = "Match canceled." }
                    });
                }

                foreach (var denied in deniedSessions)
                {
                    _queue.Remove(denied);
                    denied.State = SessionState.Registered;
                }
            }
            else
            {
                foreach (var sessionInQueue in _queue)
                {
                    Send(sessionInQueue, new ServerMessage
                    {
                        Type = MessageType.QueueResponse,
                        Payload = new { message = "Game starting!" }
                    });

                    sessionInQueue.State = SessionState.InMatch;
                    sessionInQueue.GameId = Guid.NewGuid();
                }

                _queue.Clear();
            }

            _matchResponses.Clear();
        }
    }

    private static void Send(ClientSession session, ServerMessage msg)
    {
        var data = JsonMessageSerializer.Serialize(msg);
        session.Stream.WriteAsync(data);
    }
}
