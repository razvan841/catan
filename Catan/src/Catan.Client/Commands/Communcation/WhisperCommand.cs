using System.Linq;
using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Client.UI;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;

namespace Catan.Client.Commands;

public class WhisperCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "m", "message", "whisper" };

    public WhisperCommand(ClientSender sender, MainWindow ui)
    {
        _sender = sender;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length < 2)
        {
            _ui.AppendChatLine("Usage: /m <username> <message>");
            return Task.CompletedTask;
        }

        var target = args[0];
        var message = string.Join(' ', args.Skip(1));

        _ui.AppendChatLine($"[You → {target}] {message}");

        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.WhisperRequest,
            Payload = new WhisperRequestDto
            {
                Username = target,
                Message = message
            }
        });
    }
}
