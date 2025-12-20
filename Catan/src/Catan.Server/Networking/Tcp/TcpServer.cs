using System.Net;
using System.Net.Sockets;

namespace Catan.Server.Networking.Tcp;

public class TcpServer
{
    private readonly TcpListener _listener;
    private bool _isRunning;

    public TcpServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        _listener.Start();

        Console.WriteLine("TCP Server started.");

        while (_isRunning)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using var stream = client.GetStream();

        try
        {
            while (client.Connected)
            {
                var lengthBuffer = new byte[4];
                int read = await stream.ReadAsync(lengthBuffer, 0, 4);
                if (read == 0) break;

                int length = BitConverter.ToInt32(lengthBuffer, 0);
                var payload = new byte[length];

                int totalRead = 0;
                while (totalRead < length)
                {
                    totalRead += await stream.ReadAsync(
                        payload,
                        totalRead,
                        length - totalRead);
                }

                await MessageHandler.HandleAsync(payload, stream, client);
            }
        }
        finally
        {
            var username = ClientRegistry.GetUsername(client);
            if (username != null)
            {
                ClientRegistry.Deregister(username);
                Console.WriteLine($"User '{username}' deregistered.");
            }

            client.Close();
        }
    }
}
