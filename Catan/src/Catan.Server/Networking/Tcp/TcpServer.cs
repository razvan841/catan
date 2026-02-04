using System.Net;
using System.Net.Sockets;
using Catan.Server.Sessions;
using Catan.Server.Matchmaking;

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
            var session = new ClientSession(client);

            Console.WriteLine($"New client {session.Id} connected.");

            _ = HandleClientAsync(session);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
        Console.WriteLine("TCP Server stopped.");
    }

    private async Task HandleClientAsync(ClientSession session)
    {
        var client = session.Client;
        var stream = session.Stream;

        try
        {
            while (client.Connected)
            {
                var payload = await ReadMessageAsync(stream);
                if (payload == null) break;

                await MessageHandler.HandleAsync(payload, client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with client {session.Id ?? session.Username ?? "Unknown"}: {ex.Message}");
        }
        finally
        {
            session.State = SessionState.Disconnected;
            SessionManager.Remove(session);
            MatchmakingService.ForceDequeue(session);
            client.Close();
            Console.WriteLine($"User '{session.Id ?? session.Username ?? "Unknown"}' disconnected.");
        }
    }

    private async Task<byte[]?> ReadMessageAsync(NetworkStream stream)
    {
        var lengthBuffer = new byte[4];
        int read = await stream.ReadAsync(lengthBuffer, 0, 4);

        if (read == 0)
            return null;

        if (read < 4)
            throw new IOException("Incomplete message length received.");

        int length = BitConverter.ToInt32(lengthBuffer, 0);
        var payload = new byte[length];

        int totalRead = 0;
        while (totalRead < length)
        {
            int bytesRead = await stream.ReadAsync(payload, totalRead, length - totalRead);
            if (bytesRead == 0)
                throw new IOException("Connection closed during message read.");

            totalRead += bytesRead;
        }

        return payload;
    }
}
