using System.Text.Json;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Dtos;
using Catan.Client.Networking;

Console.WriteLine("=== Catan Client (Console) ===");

Console.Write("Enter username: ");
string username = Console.ReadLine()!;

using var connection = new TcpClientConnection("127.0.0.1", 5000);

var listener = Task.Run(async () =>
{
    while (true)
    {
        var msg = await connection.ReceiveAsync();
        await HandleServerMessageAsync(connection, msg, username);
    }
});

var registerResponse = await MakeRequestAsync(connection, "register", username);
var registerResult = ((JsonElement)registerResponse!.Payload!).Deserialize<RegisterResponseDto>()!;

Console.WriteLine(registerResult.Message);
if (!registerResult.Success)
    return;

while (true)
{
    Console.WriteLine();
    Console.WriteLine("h = help | q = join queue | p = pulse | d = disconnect");
    Console.Write("> ");

    var action = Console.ReadLine()!;

    if (action == "d")
        break;

    // TODO: Implement proper help later
    if (action == "h")
        continue;

    await MakeRequestAsync(connection, action, username);
}

Console.WriteLine("Disconnected.");


// ============================
// REQUEST SENDER
// ============================
static async Task<ServerMessage?> MakeRequestAsync(TcpClientConnection connection, string requestType, string username)
{
    ClientMessage message = requestType switch
    {
        "register" => new ClientMessage
        {
            Type = MessageType.RegisterRequest,
            Payload = new RegisterRequestDto { Username = username }
        },

        "q" => new ClientMessage
        {
            Type = MessageType.QueueRequest,
            Payload = new QueueRequestDto { Username = username }
        },

        "p" => new ClientMessage
        {
            Type = MessageType.HealthRequest
        },

        _ => null!
    };

    if (message == null)
    {
        Console.WriteLine("Unknown request.");
        return null;
    }

    await connection.SendAsync(message);
    return null;
}


// ============================
// SERVER MESSAGE HANDLER
// ============================
static async Task HandleServerMessageAsync(TcpClientConnection connection, ServerMessage message, string username)
{
    switch (message.Type)
    {
        case MessageType.QueueResponse:
            Console.WriteLine(
                JsonSerializer.Serialize(message.Payload, new JsonSerializerOptions { WriteIndented = true })
            );
            break;

        case MessageType.MatchFoundNotification:
            var match =
                ((JsonElement)message.Payload!).Deserialize<MatchFoundDto>()!;

            Console.WriteLine("Match found with:");
            foreach (var p in match.Players)
                Console.WriteLine($" - {p}");

            await connection.SendAsync(new ClientMessage
            {
                Type = MessageType.MatchResponse,
                Payload = new MatchResponseDto
                {
                    Username = username,
                    Accept = true
                }
            });
            break;

        case MessageType.MatchCanceledNotification:
            Console.WriteLine("Match was canceled.");
            break;

        default:
            Console.WriteLine($"Unhandled message: {message.Type}");
            break;
    }
}
