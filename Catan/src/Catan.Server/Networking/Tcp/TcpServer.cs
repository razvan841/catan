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
            Console.WriteLine("Client connected.");

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
                // Read length prefix
                var lengthBuffer = new byte[4];
                int read = await stream.ReadAsync(lengthBuffer, 0, 4);

                if (read == 0)
                    break;

                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                // Read payload
                var payloadBuffer = new byte[messageLength];
                int totalRead = 0;

                while (totalRead < messageLength)
                {
                    totalRead += await stream.ReadAsync(
                        payloadBuffer,
                        totalRead,
                        messageLength - totalRead
                    );
                }

                await MessageHandler.HandleAsync(payloadBuffer, stream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
}
