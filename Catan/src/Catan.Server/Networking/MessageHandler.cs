using System.Net.Sockets;
using System.Text.Json;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;
using Catan.Shared.Networking.Dtos;

namespace Catan.Server.Networking;

public static class MessageHandler
{
    public static async Task HandleAsync(
        byte[] payload,
        NetworkStream stream,
        TcpClient client)
    {
        var clientMessage =
            JsonMessageSerializer.Deserialize<ClientMessage>(payload);

        switch (clientMessage.Type)
        {
            case MessageType.HealthRequest:
                await HandleHealthAsync(stream);
                break;

            case MessageType.RegisterRequest:
                await HandleRegisterAsync(clientMessage, stream, client);
                break;

            case MessageType.QueueRequest:
                await HandleQueueAsync(clientMessage, stream, client);
                break;

            default:
                Console.WriteLine($"Unknown message type: {clientMessage.Type}");
                break;
        }
    }

    private static async Task HandleHealthAsync(NetworkStream stream)
    {
        var response = new ServerMessage
        {
            Type = MessageType.HealthResponse,
            Payload = new
            {
                status = "OK",
                serverTime = DateTime.UtcNow
            }
        };

        var data = JsonMessageSerializer.Serialize(response);
        await stream.WriteAsync(data);
    }

    private static async Task HandleRegisterAsync(
        ClientMessage message,
        NetworkStream stream,
        TcpClient client)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<RegisterRequestDto>()!;

        bool success = ClientRegistry.TryRegister(dto.Username, client);

        var response = new ServerMessage
        {
            Type = MessageType.RegisterResponse,
            Payload = new RegisterResponseDto
            {
                Success = success,
                Message = success
                    ? "Registered successfully"
                    : "Username already in use"
            }
        };
        if(success)
        {
            Console.WriteLine($"Client connected, using username: {dto.Username}");
        }

        var bytes = JsonMessageSerializer.Serialize(response);
        await stream.WriteAsync(bytes);
    }

    private static async Task HandleQueueAsync(
        ClientMessage message,
        NetworkStream stream,
        TcpClient client)
    {
        var dto = ((JsonElement)message.Payload!).Deserialize<QueueRequestDto>()!;
        // TODO: Add logic for joining the queue
        bool success = true;
        var response = new ServerMessage
        {
            Type = MessageType.QueueResponse,
            Payload = new RegisterResponseDto
            {
                Success = success,
                Message = success
                    ? "In the queue!"
                    : "Could not join the queue"
            }
        };

        if(success)
        {
            Console.WriteLine($"User: {dto.Username} joined queue successfully!");
        }
        

        var bytes = JsonMessageSerializer.Serialize(response);
        await stream.WriteAsync(bytes);
    }
}
