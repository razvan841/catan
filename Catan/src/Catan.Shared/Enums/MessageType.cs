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
    TurnChanged,
    LoginRequest,
    LoginResponse,
    ChatMessage,
    ServerChat,
    EloRequest,
    EloResponse,
    LeaderboardRequest,
    LeaderboardResponse,
    PlayerInfoRequest,
    PlayerInfoResponse,
    WhisperRequest,
    WhisperResponse,
    WhisperIncoming,
    ChatMessageIncoming
}
