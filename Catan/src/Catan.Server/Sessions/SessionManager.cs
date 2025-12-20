using System.Collections.Concurrent;
using System.Net.Sockets;
namespace Catan.Server.Sessions;

public static class SessionManager
{
    private static readonly ConcurrentDictionary<Guid, ClientSession> _sessions = new();

    public static void Add(ClientSession session) => _sessions[session.Id] = session;

    public static void Remove(ClientSession session) => _sessions.TryRemove(session.Id, out _);

    public static bool UsernameExists(string username)
        => _sessions.Values.Any(s => s.Username == username);

    public static ClientSession? GetByClient(TcpClient client)
        => _sessions.Values.FirstOrDefault(s => s.Client == client);

    public static IEnumerable<ClientSession> GetAll() => _sessions.Values;
}
