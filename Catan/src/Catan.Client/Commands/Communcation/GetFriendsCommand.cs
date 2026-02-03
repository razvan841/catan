using System.Threading.Tasks;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;
using Catan.Client.Networking;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class GetFriendsCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "friendlist" };

    public GetFriendsCommand(ClientSender sender, MainWindow ui)
    {
        _sender = sender;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length > 0)
        {
            _ui.AppendChatLine("Usage: /friendlist", "error");
            return Task.CompletedTask;
        }

        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.FriendListRequest
        });
    }
}
