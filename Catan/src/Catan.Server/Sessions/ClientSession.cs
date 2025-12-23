using System.Net.Sockets;
using Catan.Server.Game;

namespace Catan.Server.Sessions;

public class ClientSession
{
    public Guid Id { get; } = Guid.NewGuid();
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public string? Username { get; private set; }
    public SessionState State { get; set; } = SessionState.Connected;
    public GameSession? GameSession { get; set; }

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
