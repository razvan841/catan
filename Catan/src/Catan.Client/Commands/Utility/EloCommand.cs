using System.Threading.Tasks;
using Catan.Client.State;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Client.Networking;
namespace Catan.Client.Commands;

public class EloCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly ClientSession _session;

    public string[] Aliases => new[] { "elo" };

    public EloCommand(ClientSender sender, ClientSession session)
    {
        _sender = sender;
        _session = session;
    }

    public Task Execute(string[] args)
    {
        var usernames = args.Length == 0
            ? new[] { _session.Username! }
            : args;

        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.EloRequest,
            Payload = new EloRequestDto { Usernames = usernames }
        });
    }
}
