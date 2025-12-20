using System.Collections.Concurrent;
using System.Net.Sockets;
namespace Catan.Server.Networking;

public static class ClientRegistry
{
    private static readonly ConcurrentDictionary<string, TcpClient> _clients = new();

    public static bool TryRegister(string username, TcpClient client)
    {
        return _clients.TryAdd(username, client);
    }

    public static void Deregister(string username)
    {
        _clients.TryRemove(username, out _);
    }

    public static string? GetUsername(TcpClient client)
    {
        return _clients.FirstOrDefault(kv => kv.Value == client).Key;
    }
}
