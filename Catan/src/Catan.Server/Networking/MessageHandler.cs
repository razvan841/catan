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

            case MessageType.QueueRequest:
                await HandleQueueAsync(clientMessage, session);
                break;

            case MessageType.MatchResponse:
                HandleMatchResponse(clientMessage, session);
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
            Payload = new { status = "OK", serverTime = DateTime.UtcNow }
        };
        var data = JsonMessageSerializer.Serialize(response);
        await session.Stream.WriteAsync(data);
    }

    private static async Task HandleRegisterAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<RegisterRequestDto>()!;

        if (SessionManager.UsernameExists(dto.Username))
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

        session.Register(dto.Username);
        SessionManager.Add(session);

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

    private static async Task HandleQueueAsync(ClientMessage message, ClientSession session)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<QueueRequestDto>()!;
        bool success = MatchmakingService.TryEnqueue(session, out string reason);

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
        MatchmakingService.HandleMatchResponse(session, dto);
    }

    private static async Task SendResponseAsync(ClientSession session, ServerMessage msg)
    {
        var data = JsonMessageSerializer.Serialize(msg);
        await session.Stream.WriteAsync(data);
    }
}
