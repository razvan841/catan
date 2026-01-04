using System.Net.Sockets;
using System.Text.Json;
using Catan.Server.Sessions;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;
using Catan.Shared.Networking.Dtos;

namespace Catan.Server.Networking;

public static class MessageHandler
{
    public static async Task HandleAsync(byte[] payload, TcpClient client)
    {
        var session = SessionManager.GetByClient(client);
        if (session == null) return;

        var clientMessage = JsonMessageSerializer.Deserialize<ClientMessage>(payload);

        switch (clientMessage.Type)
        {
            case MessageType.HealthRequest:
                await HandleHealthAsync(session);
                break;

            case MessageType.RegisterRequest:
                await HandleRegisterAsync(clientMessage, session);
                break;
            
            case MessageType.LoginRequest:
                await HandleLoginAsync(clientMessage, session);
                break;

            case MessageType.QueueRequest:
                await HandleQueueAsync(clientMessage, session);
                break;

            case MessageType.MatchResponse:
                HandleMatchResponse(clientMessage, session);
                break;
            
            case MessageType.EloRequest:
                await HandleEloAsync(clientMessage, session);
                break;

            case MessageType.LeaderboardRequest:
                await HandleLeaderboardAsync(clientMessage, session);
                break;

            case MessageType.PlayerInfoRequest:
                await HandlePlayerInfoAsync(clientMessage, session);
                break;

            case MessageType.WhisperRequest:
                await HandleWhisperRequestAsync(clientMessage, session);
                break;

            case MessageType.ChatMessage:
                await HandleChatMessageAsync(clientMessage, session);
                break;

            default:
                Console.WriteLine($"Unknown message type: {clientMessage.Type}");
                break;
        }
    }

    private static async Task HandleHealthAsync(ClientSession session)
    {
        var response = new ServerMessage
        {
            Type = MessageType.HealthResponse,
            Payload = new HealthResponseDto { Success = true, ServerTime = DateTime.UtcNow }
        };
        var data = JsonMessageSerializer.Serialize(response);
        await session.Stream.WriteAsync(data);
    }

    private static async Task HandleRegisterAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<RegisterRequestDto>()!;

        if (Db.UsernameExists(dto.Username))
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.RegisterResponse,
                Payload = new RegisterResponseDto
                {
                    Success = false,
                    Message = "Username already in use"
                }
            });
            return;
        }

        Console.WriteLine("Adding user to DB!");
        var userId = Db.AddUser(dto.Username, dto.Password);
        session.Register(userId, dto.Username);

        await SendResponseAsync(session, new ServerMessage
        {
            Type = MessageType.RegisterResponse,
            Payload = new RegisterResponseDto
            {
                Success = true,
                Message = "Registered successfully"
            }
        });
    }

    private static async Task HandleLoginAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<LoginRequestDto>()!;

        if (!Db.ValidateUser(dto.Username, dto.Password))
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.LoginResponse,
                Payload = new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                }
            });
            return;
        }

        var userId = Db.GetUserId(dto.Username)!;
        session.Register(userId, dto.Username);        
        await SendResponseAsync(session, new ServerMessage
        {
            Type = MessageType.LoginResponse,
            Payload = new LoginResponseDto
            {
                Success = true,
                Message = "Login successful"
            }
        });
    }

    private static async Task HandleQueueAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<QueueRequestDto>()!;
        bool success = Matchmaking.MatchmakingService.TryEnqueue(session, out string reason);

        await SendResponseAsync(session, new ServerMessage
        {
            Type = MessageType.QueueResponse,
            Payload = new QueueResponseDto
            {
                Success = success,
                Message = reason
            }
        });
    }

    private static void HandleMatchResponse(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<MatchResponseDto>()!;
        Matchmaking.MatchmakingService.HandleMatchResponse(session, dto);
    }

    private static async Task HandleEloAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<EloRequestDto>()!;
        var result = new List<EloResponseEntryDto>();

        foreach (var username in dto.Usernames)
        {
            var elo = Db.GetEloByUsername(username);
            if (elo.HasValue)
                result.Add(new EloResponseEntryDto { Username = username, Elo = elo.Value });
        }

        await SendResponseAsync(session, new ServerMessage
        {
            Type = MessageType.EloResponse,
            Payload = new EloResponseDto { Entries = result.ToArray() }
        });
    }

    private static async Task HandleLeaderboardAsync(ClientMessage message, ClientSession session)
    {
        var topPlayers = Db.GetLeaderboard(10);
        await SendResponseAsync(session, new ServerMessage
        {
            Type = MessageType.LeaderboardResponse,
            Payload = new LeaderboardResponseDto
            {
                Entries = topPlayers.Select(p => new LeaderboardEntryDto
                {
                    Username = p.Username,
                    Elo = p.Elo
                }).ToArray()
            }
        });
        
    }

    public static async Task HandlePlayerInfoAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<PlayerInfoRequestDto>()!;
        var username = dto.Username;

        var info = Db.GetUserInfoByUsername(username);

        var response = new ServerMessage
        {
            Type = MessageType.PlayerInfoResponse,
            Payload = info
        };

        await SendResponseAsync(session, response);
    }

    public static async Task HandleWhisperRequestAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<WhisperRequestDto>()!;

        if (session.Username == null)
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.WhisperResponse,
                Payload = new WhisperResponseDto
                {
                    Success = false,
                    Message = "You must be logged in to whisper."
                }
            });
            return;
        }

        var targetUserId = Db.GetUserId(dto.Username);
        if (targetUserId == null)
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.WhisperResponse,
                Payload = new WhisperResponseDto
                {
                    Success = false,
                    Message = $"User '{dto.Username}' does not exist."
                }
            });
            return;
        }

        var targetSession = SessionManager.GetById(targetUserId);
        if (targetSession == null)
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.WhisperResponse,
                Payload = new WhisperResponseDto
                {
                    Success = false,
                    Message = $"User '{dto.Username}' is not online."
                }
            });
            return;
        }

        await SendResponseAsync(targetSession, new ServerMessage
        {
            Type = MessageType.WhisperIncoming,
            Payload = new WhisperIncomingDto
            {
                FromUsername = session.Username,
                Message = dto.Message
            }
        });

        await SendResponseAsync(session, new ServerMessage
        {
            Type = MessageType.WhisperResponse,
            Payload = new WhisperResponseDto
            {
                Success = true,
                Message = $"Whisper sent to {dto.Username}"
            }
        });
    }

    public static async Task HandleChatMessageAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!)
            .Deserialize<ChatMessageDto>()!;

        if (session.Username == null)
            return;

        var serverMessage = new ServerMessage
        {
            Type = MessageType.ChatMessageIncoming,
            Payload = new ChatMessageIncomingDto
            {
                FromUsername = session.Username,
                Message = dto.Message
            }
        };

        var data = JsonMessageSerializer.Serialize(serverMessage);

        foreach (var s in SessionManager.GetAll())
        {
            if (s == session) continue;
            await s.Stream.WriteAsync(data);
        }
    }

    private static async Task SendResponseAsync(ClientSession session, ServerMessage msg)
    {
        var data = JsonMessageSerializer.Serialize(msg);
        await session.Stream.WriteAsync(data);
    }
}
