using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class BlockUserCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "block" };

    public BlockUserCommand(ClientSender sender, MainWindow ui)
    {
        _sender = sender;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length != 1)
        {
            _ui.AppendChatLine("Usage: /block <username>", "error");
            return Task.CompletedTask;
        }
        _ui.AppendChatLine($"Blocked user: {args[0]}", "system");
        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.BlockRequest,
            Payload = new BlockRequestDto { TargetUsername = args[0] }
        });
    }
}
