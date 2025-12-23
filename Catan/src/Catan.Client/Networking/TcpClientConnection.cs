using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Networking.Serialization;

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
        await _stream.WriteAsync(data.AsMemory());
    }

    public async Task<ServerMessage> ReceiveAsync()
    {
        var lengthBuffer = new byte[4];
        await ReadExactAsync(lengthBuffer);

        int length = BitConverter.ToInt32(lengthBuffer, 0);

        var payload = new byte[length];
        await ReadExactAsync(payload);

        return JsonMessageSerializer.Deserialize<ServerMessage>(payload);
    }

    private async Task ReadExactAsync(byte[] buffer)
    {
        int offset = 0;

        while (offset < buffer.Length)
        {
            int read = await _stream.ReadAsync(
                buffer.AsMemory(offset, buffer.Length - offset)
            );

            if (read == 0)
                throw new IOException("Connection closed.");

            offset += read;
        }
    }

    public void Dispose()
    {
        _stream.Dispose();
        _client.Dispose();
    }
}
