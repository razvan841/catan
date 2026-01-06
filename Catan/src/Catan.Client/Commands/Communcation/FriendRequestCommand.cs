using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class FriendRequestCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "friend" };

    public FriendRequestCommand(ClientSender sender, MainWindow ui)
    {
        _sender = sender;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length != 1)
        {
            _ui.AppendChatLine("Usage: /friend <username>");
            return Task.CompletedTask;
        }

        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.FriendRequest,
            Payload = new FriendRequestDto { Username = args[0] }
        });
    }
}
