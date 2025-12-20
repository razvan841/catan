using System.Text.Json;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Dtos;
using Catan.Client.Networking;

Console.WriteLine("=== Catan Client (Console) ===");

Console.Write("Enter username: ");
string username = Console.ReadLine()!;

try
{
    using var connection = new TcpClientConnection(
        host: "127.0.0.1",
        port: 5000
    );

    var registerResponse = await MakeRequestAsync(
        connection,
        "register",
        username
    );

    var registerResult = ((JsonElement)registerResponse.Payload!).Deserialize<RegisterResponseDto>()!;

    Console.WriteLine(registerResult.Message);

    if (!registerResult.Success)
        return;

    // MAIN LOOP
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("h = help | q = join queue | p = pulse | d = disconnect");
        Console.Write("> ");

        string action = Console.ReadLine()!;

        if (action == "d")
            break;
        
        if (action == "h")
        {
            Console.WriteLine();
            continue;
        }

        var response = await MakeRequestAsync(
            connection,
            action,
            username
        );

        if (response == null)
            continue;

        Console.WriteLine(
            JsonSerializer.Serialize(
                response.Payload,
                new JsonSerializerOptions { WriteIndented = true }
            )
        );
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("Disconnected.");


static async Task<ServerMessage?> MakeRequestAsync(
    TcpClientConnection connection,
    string requestType,
    string username)
{
    ClientMessage message;

    switch (requestType)
    {
        case "register":
            message = new ClientMessage
            {
                Type = MessageType.RegisterRequest,
                Payload = new RegisterRequestDto
                {
                    Username = username
                }
            };
            break;

        case "q":
            message = new ClientMessage
            {
                Type = MessageType.QueueRequest,
                Payload = new QueueRequestDto
                {
                    Username = username
                }
            };
            break;

        case "p":
            message = new ClientMessage
            {
                Type = MessageType.HealthRequest,
                Payload = null
            };
            break;

        default:
            Console.WriteLine("Unknown request.");
            return null;
    }

    await connection.SendAsync(message);
    return await connection.ReceiveAsync();
}
