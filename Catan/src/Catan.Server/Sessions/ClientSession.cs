using System.Net.Sockets;
using Catan.Server.Game;
using System.Security.Cryptography;
namespace Catan.Server.Sessions;

public class ClientSession
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public TcpClient Client { get; }
    public NetworkStream Stream { get; }
    public string? Username { get; private set; }
    public SessionState State { get; set; } = SessionState.Connected;
    public GameSession? GameSession { get; set; }
    public ECDiffieHellman Ecdh { get; set; }
    public byte[]? SharedSecret { get; set; }
    public bool IsSecure => SharedSecret != null;

    public ClientSession(TcpClient client)
    {
        Client = client;
        Stream = client.GetStream();
        SessionManager.Add(this);
        Ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
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
