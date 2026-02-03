using System.Threading.Tasks;
using Catan.Shared.Networking.Messages;

namespace Catan.Client.Networking;

public class ClientSender
{
    private TcpClientConnection? _connection;

    public void Attach(TcpClientConnection connection)
    {
        _connection = connection;
    }

    public Task SendAsync(ClientMessage message)
    {
        return _connection != null
            ? _connection.SendAsync(message)
            : Task.CompletedTask;
    }
}
