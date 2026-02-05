using System.Net;
using System.Net.Sockets;
using Catan.Server.Sessions;
using Catan.Server.Matchmaking;
using Catan.Shared.Security;

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
            byte[] serverPub = EcdhHelpers.ExportPublicKey(session.Ecdh);
            await SendFramedAsync(stream, serverPub);
            byte[] clientPub = await ReceiveFramedAsync(stream);
            session.SharedSecret = EcdhHelpers.DeriveSharedSecret(session.Ecdh, clientPub);

            while (client.Connected)
            {
                var payload = await ReadMessageAsync(stream, session);
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

    private async Task<byte[]?> ReadMessageAsync(NetworkStream stream, ClientSession session)
    {
        if (!session.IsSecure)
            throw new InvalidOperationException("Client tried to send data before handshake.");

        var (aesKey, hmacKey) = CryptoHelpers.DeriveKeys(session.SharedSecret!);

        var lengthBuffer = new byte[4];
        int read = await stream.ReadAsync(lengthBuffer, 0, 4);

        if (read == 0)
            return null;

        if (read < 4)
            throw new IOException("Incomplete message length received.");

        int totalLen = BitConverter.ToInt32(lengthBuffer, 0);

        byte[] all = new byte[totalLen];
        int totalRead = 0;
        while (totalRead < totalLen)
        {
            int bytesRead = await stream.ReadAsync(all, totalRead, totalLen - totalRead);
            if (bytesRead == 0)
                throw new IOException("Connection closed during message read.");

            totalRead += bytesRead;
        }
        byte[] iv = new byte[16];
        Buffer.BlockCopy(all, 0, iv, 0, 16);

        int cipherLen = totalLen - 16 - 32;
        byte[] ciphertext = new byte[cipherLen];
        byte[] hmac = new byte[32];

        Buffer.BlockCopy(all, 16, ciphertext, 0, cipherLen);
        Buffer.BlockCopy(all, 16 + cipherLen, hmac, 0, 32);

        CryptoHelpers.VerifyHmac(ciphertext, hmacKey, hmac);
        
        byte[] plaintext = CryptoHelpers.DecryptAes(ciphertext, aesKey, iv);
        
        string decoded = System.Text.Encoding.UTF8.GetString(plaintext);
        Console.WriteLine("Decrypted payload: " + decoded);
        return plaintext;
    }


    private async Task SendFramedAsync(NetworkStream stream, byte[] data)
    {
        await stream.WriteAsync(BitConverter.GetBytes(data.Length));
        await stream.WriteAsync(data);
    }
    private async Task<byte[]> ReceiveFramedAsync(NetworkStream stream)
    {
        var lenBuf = new byte[4];
        int read = await stream.ReadAsync(lenBuf, 0, 4);
        if (read == 0) throw new IOException("Connection closed.");

        int length = BitConverter.ToInt32(lenBuf, 0);
        var data = new byte[length];

        int total = 0;
        while (total < length)
            total += await stream.ReadAsync(data, total, length - total);

        return data;
    }

}
