using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;
using System.Net.Sockets;

namespace Catan.Server.Networking;

public static class MessageHandler
{
    public static async Task HandleAsync(byte[] payload, NetworkStream stream)
    {
        var clientMessage = JsonMessageSerializer.Deserialize<ClientMessage>(payload);

        switch (clientMessage.Type)
        {
            case MessageType.HealthRequest:
                await HandleHealthAsync(stream);
                break;

            default:
                Console.WriteLine("Unknown message type.");
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
}
