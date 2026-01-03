using System.Net.Sockets;
using Catan.Server.Game;

namespace Catan.Server.Sessions;

public class ClientSession
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public string? Username { get; private set; }
    public SessionState State { get; set; } = SessionState.Connected;
    public GameSession? GameSession { get; set; }

    public ClientSession(TcpClient client)
    {
        Client = client;
        Stream = client.GetStream();
        SessionManager.Add(this);
    }

    public void Register(string dbUserId, string username)
    {
        SessionManager.Remove(this);

        Id = dbUserId;
        Username = username;
        State = SessionState.Registered;

        SessionManager.Add(this);
    }
}
