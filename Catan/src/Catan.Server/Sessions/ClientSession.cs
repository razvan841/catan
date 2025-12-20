using System.Net.Sockets;

namespace Catan.Server.Sessions;

public class ClientSession
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Username { get; private set; } = null!;
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public DateTime ConnectedAt { get; } = DateTime.UtcNow;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public SessionState State { get; set; } = SessionState.Connected;
    public Guid? GameId { get; set; }

    public ClientSession(TcpClient client)
    {
        Client = client;
        Stream = client.GetStream();
    }

    public void Register(string username)
    {
        Username = username;
        State = SessionState.Registered;
    }
}
