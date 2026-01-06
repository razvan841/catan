using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;

namespace Catan.Client.Commands;

public class PlayerInfoCommand : ICommandHandler
{
    private readonly ClientSender _sender;

    public string[] Aliases => new[] { "playerinfo" };

    public PlayerInfoCommand(ClientSender sender)
    {
        _sender = sender;
    }

    public Task Execute(string[] args)
    {
        if (args.Length != 1)
            return Task.CompletedTask;

        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.PlayerInfoRequest,
            Payload = new PlayerInfoRequestDto { Username = args[0] }
        });
    }
}
