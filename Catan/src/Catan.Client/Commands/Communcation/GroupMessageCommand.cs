using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Catan.Client.UI;

namespace Catan.Client.Commands;

public class GroupMessageCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "group", "g" };

    public GroupMessageCommand(ClientSender sender, MainWindow ui)
    {
        _sender = sender;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length > 1)
        {
            _ui.AppendChatLine("Usage: /group <message>", "error");
            return Task.CompletedTask;
        }
        var message = string.Join(' ', args);
        _ui.AppendChatLine($"[You → Friends] {message}", "system");

        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.GroupMessageRequest,
            Payload = new GroupMessageRequestDto { Message = message }
        });
    }
}
