using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Client.State;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;

namespace Catan.Client.Commands;

public class QueueCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly ClientSession _session;

    public string[] Aliases => new[] { "queue" };

    public QueueCommand(ClientSender sender, ClientSession session)
    {
        _sender = sender;
        _session = session;
    }

    public Task Execute(string[] args)
    {
        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.QueueRequest,
            Payload = new QueueRequestDto { Username = _session.Username! }
        });
    }
}
