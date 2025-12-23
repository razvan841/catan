namespace Catan.Shared.Enums;

public enum MessageType
{
    HealthRequest,
    HealthResponse,
    RegisterRequest,
    RegisterResponse,
    QueueRequest,
    QueueResponse,
    MatchFound,
    MatchResponse,
    MatchCanceled,
    MatchStart,
    EndTurn,
    TurnChanged
}
