using System.Net.Sockets;
using System.Text.Json;
using Catan.Server.Sessions;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Dtos.Server;

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
                await HandleMatchResponse(clientMessage, session);
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

            case MessageType.FriendRequest:
                await HandleFriendRequestAsync(clientMessage, session);
                break;

            case MessageType.FriendRequestAnswer:
                await HandleFriendRequestAnswerAsync(clientMessage, session);
                break;

            case MessageType.GroupMessageRequest:
                await HandleGroupMessageAsync(clientMessage, session);
                break;

            case MessageType.NewGameRequest:
                await HandleNewGameAsync(clientMessage, session);
                break;

            case MessageType.BlockRequest:
                await HandleBlockRequestAsync(clientMessage, session);
                break;

            case MessageType.UnblockRequest:
                await HandleUnblockRequestAsync(clientMessage, session);
                break;

            case MessageType.FriendListRequest:
                await HandleFriendListRequestAsync(clientMessage, session);
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

    private static async Task HandleMatchResponse(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<MatchResponseDto>()!;
        await Matchmaking.MatchmakingService.HandleMatchResponseAsync(session, dto);
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
        if (Db.BlockExists(session.Id, targetUserId) || Db.BlockExists(targetUserId, session.Id))
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = false,
                    Message = $"Cannot send friend request to {dto.Username} because you or they have blocked each other."
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
        var dto = ((JsonElement)message.Payload!).Deserialize<ChatMessageDto>()!;

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

    public static async Task HandleFriendRequestAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<FriendRequestDto>()!;
        if (session.Username == null || session.Id == null)
            return;

        if (dto.Username == session.Username)
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = false,
                    Message = "You cannot add yourself as a friend."
                }
            });
            return;
        }

        var targetUserId = Db.GetUserId(dto.Username);
        if (targetUserId == null)
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = false,
                    Message = $"User '{dto.Username}' does not exist."
                }
            });
            return;
        }
        if (Db.BlockExists(session.Id, targetUserId) || Db.BlockExists(targetUserId, session.Id))
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = false,
                    Message = $"Cannot send friend request to {dto.Username} because you or they have blocked each other."
                }
            });
            return;
        }

        if (Db.FriendshipExists(session.Id, targetUserId))
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = false,
                    Message = $"You are already friends with {dto.Username}."
                }
            });
            return;
        }

        if (!Db.AddFriendRequest(session.Id, targetUserId))
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = false,
                    Message = $"Friend request to {dto.Username} already pending."
                }
            });
            return;
        }

        await SendResponseAsync(session, new ServerMessage
        {
            Type = MessageType.FriendResponse,
            Payload = new FriendResponseDto
            {
                Success = true,
                Message = $"Friend request sent to {dto.Username}."
            }
        });

        var targetSession = SessionManager.GetById(targetUserId);
        if (targetSession != null)
        {
            await SendResponseAsync(targetSession, new ServerMessage
            {
                Type = MessageType.FriendRequestIncoming,
                Payload = new FriendRequestIncomingDto
                {
                    FromUsername = session.Username
                }
            });
        }
    }

    public static async Task HandleFriendRequestAnswerAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<FriendRequestAnswerDto>()!;
        if (session.Username == null || session.Id == null)
            return;

        if (!string.Equals(dto.ToUsername, session.Username, StringComparison.OrdinalIgnoreCase))
            return;

        var fromUserId = Db.GetUserId(dto.FromUsername);
        if (fromUserId == null)
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = false,
                    Message = $"User '{dto.FromUsername}' does not exist."
                }
            });
            return;
        }

        if (!Db.FriendRequestExists(fromUserId, session.Id))
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = false,
                    Message = $"No pending friend request from {dto.FromUsername}."
                }
            });
            return;
        }

        Db.RemoveFriendRequest(fromUserId, session.Id);

        if (dto.Answer)
        {
            Db.AddFriendship(fromUserId, session.Id);

            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendResponse,
                Payload = new FriendResponseDto
                {
                    Success = true,
                    Message = $"You are now friends with {dto.FromUsername}."
                }
            });

            var requesterSession = SessionManager.GetById(fromUserId);
            if (requesterSession != null)
            {
                await SendResponseAsync(requesterSession, new ServerMessage
                {
                    Type = MessageType.FriendAccepted,
                    Payload = new FriendAcceptedDto
                    {
                        Username = session.Username
                    }
                });
            }
        }
        else
        {
            await SendResponseAsync(session, new ServerMessage
            {
                Type = MessageType.FriendRejected,
                Payload = new FriendResponseDto
                {
                    Success = true,
                    Message = $"You rejected the friend request from {dto.FromUsername}."
                }
            });
        }
    }

    public static async Task HandleGroupMessageAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<GroupMessageRequestDto>()!;

        if (session.Username == null)
            return;
    }
    // TODO:
    public static async Task HandleNewGameAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<NewGameRequestDto>()!;

        if (session.Username == null)
            return;
    }
    public static async Task HandleBlockRequestAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<BlockRequestDto>()!;
        if (session.Username == null) return;

        string? blockerId = Db.GetUserId(session.Username);
        string? blockedId = Db.GetUserId(dto.TargetUsername);

        if (blockerId == null || blockedId == null)
            return;

        if (blockerId == blockedId)
        {
            var response1 = new ServerMessage
            {
                Type = MessageType.BlockResponse,
                Payload = new
                {
                    Success = false,
                    Message = "Can't block yourself!"
                }
            };

            var data1 = JsonMessageSerializer.Serialize(response1);
            await session.Stream.WriteAsync(data1);
            return;
        }

        bool success = Db.AddBlock(blockerId, blockedId);

        Db.RemoveFriendship(blockerId, blockedId);

        var response = new ServerMessage
        {
            Type = MessageType.BlockResponse,
            Payload = new
            {
                Success = success,
                Message = success ? "User blocked successfully." : "User was already blocked."
            }
        };

        var data = JsonMessageSerializer.Serialize(response);
        await session.Stream.WriteAsync(data);
    }

    public static async Task HandleUnblockRequestAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<UnblockRequestDto>()!;
        if (session.Username == null)
            return;

        string? unblockerId = Db.GetUserId(session.Username);
        string? blockedId = Db.GetUserId(dto.TargetUsername);

        if (unblockerId == null || blockedId == null)
            return;

        bool success = Db.RemoveBlock(unblockerId, blockedId);

        var response = new ServerMessage
        {
            Type = MessageType.UnblockResponse,
            Payload = new
            {
                Success = success,
                TargetUsername = dto.TargetUsername,
                Message = success ? "User unblocked successfully." : "User was not blocked."
            }
        };

        var data = JsonMessageSerializer.Serialize(response);
        await session.Stream.WriteAsync(data);
    }

    public static async Task HandleFriendListRequestAsync(ClientMessage message, ClientSession session)
    {
        if (session.Username == null) return;

        string? userId = Db.GetUserId(session.Username);
        if (userId == null) return;

        var friendIds = Db.GetFriends(userId);
        var entries = new List<FriendListResponseEntryDto>();

        foreach (var fid in friendIds)
        {
            var username = Db.GetUsernameById(fid);
            if (username == null) continue;

            if (Db.BlockExists(userId, fid) || Db.BlockExists(fid, userId))
                continue;
            var friendSession = SessionManager.GetById(fid);
            entries.Add(new FriendListResponseEntryDto
            {
                Username = username,
                Online = friendSession != null 
            });
        }

        var response = new ServerMessage
        {
            Type = MessageType.FriendListResponse,
            Payload = new FriendListResponseDto
            {
                Success = true,
                Message = "Friend list retrieved successfully.",
                Entries = entries.ToArray()
            }
        };

        var data = JsonMessageSerializer.Serialize(response);
        await session.Stream.WriteAsync(data);
    }


    private static async Task SendResponseAsync(ClientSession session, ServerMessage msg)
    {
        var data = JsonMessageSerializer.Serialize(msg);
        await session.Stream.WriteAsync(data);
    }
}
