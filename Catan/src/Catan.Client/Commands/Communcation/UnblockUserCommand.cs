using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class UnblockUserCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "unblock" };

    public UnblockUserCommand(ClientSender sender, MainWindow ui)
    {
        _sender = sender;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length != 1)
        {
            _ui.AppendChatLine("Usage: /unblock <username>", "error");
            return Task.CompletedTask;
        }
        _ui.AppendChatLine($"Unblocked user: {args[0]}", "system");
        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.UnblockRequest,
            Payload = new BlockRequestDto { TargetUsername = args[0] }
        });
    }
}
