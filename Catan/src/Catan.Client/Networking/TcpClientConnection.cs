using System.Net.Sockets;
using Catan.Shared.Networking.Serialization;
using Catan.Shared.Networking.Messages;
using System.IO;

namespace Catan.Client.Networking;

public class TcpClientConnection : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;

    public TcpClientConnection(string host, int port)
    {
        _client = new TcpClient();
        _client.Connect(host, port);
        _stream = _client.GetStream();
    }

    public async Task SendAsync(ClientMessage message)
    {
        var data = JsonMessageSerializer.Serialize(message);
        await _stream.WriteAsync(data);
    }

    public async Task<ServerMessage> ReceiveAsync()
    {
        var lengthBuffer = new byte[4];
        int read = await _stream.ReadAsync(lengthBuffer, 0, 4);

        if (read != 4)
            throw new IOException("Connection closed.");

        int length = BitConverter.ToInt32(lengthBuffer, 0);

        var payload = new byte[length];
        int totalRead = 0;

        while (totalRead < length)
        {
            totalRead += await _stream.ReadAsync(
                payload,
                totalRead,
                length - totalRead
            );
        }

        return JsonMessageSerializer.Deserialize<ServerMessage>(payload);
    }

    public void Dispose()
    {
        _stream.Close();
        _client.Close();
    }
}
