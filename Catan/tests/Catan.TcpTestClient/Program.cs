using System.Net.Sockets;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;

const string SERVER_HOST = "127.0.0.1";
const int SERVER_PORT = 5000;

Console.WriteLine("Connecting to TCP server...");

using var client = new TcpClient();
await client.ConnectAsync(SERVER_HOST, SERVER_PORT);

Console.WriteLine("Connected.");

using var stream = client.GetStream();

var healthRequest = new ClientMessage
{
    Type = MessageType.HealthRequest,
    Payload = null
};

Console.WriteLine("Sending HealthRequest...");

var requestBytes = JsonMessageSerializer.Serialize(healthRequest);
await stream.WriteAsync(requestBytes);

Console.WriteLine("Waiting for response...");

// Read length prefix
var lengthBuffer = new byte[4];
int read = await stream.ReadAsync(lengthBuffer);

if (read != 4)
{
    Console.WriteLine("Failed to read response length.");
    return;
}

int responseLength = BitConverter.ToInt32(lengthBuffer, 0);

// Read payload
var payloadBuffer = new byte[responseLength];
int totalRead = 0;

while (totalRead < responseLength)
{
    totalRead += await stream.ReadAsync(
        payloadBuffer,
        totalRead,
        responseLength - totalRead
    );
}

var response = JsonMessageSerializer.Deserialize<ServerMessage>(payloadBuffer);

Console.WriteLine("Received response:");
Console.WriteLine($"Type: {response.Type}");
Console.WriteLine($"Payload: {response.Payload}");

Console.WriteLine("Test complete.");
