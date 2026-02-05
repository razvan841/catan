using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class UnfriendCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "unfriend" };

    public UnfriendCommand(ClientSender sender, MainWindow ui)
    {
        _sender = sender;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length != 1)
        {
            _ui.AppendChatLine("Usage: /unfriend <username>", "error");
            return Task.CompletedTask;
        }
        _ui.AppendChatLine($"Unfriended user: {args[0]}", "system");
        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.UnfriendRequest,
            Payload = new UnfriendRequestDto { TargetUsername = args[0] }
        });
    }
}
