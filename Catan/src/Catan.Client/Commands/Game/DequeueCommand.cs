using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Client.State;
using Catan.Client.UI;
using Catan.Shared.Networking.Dtos.Client;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;

namespace Catan.Client.Commands;

public class DequeueCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly ClientSession _session;
    private readonly MainWindow _ui;

    public string[] Aliases => new[] { "dequeue", "deq" };

    public DequeueCommand(ClientSender sender, ClientSession session, MainWindow ui)
    {
        _sender = sender;
        _session = session;
        _ui = ui;
    }

    public Task Execute(string[] args)
    {
        if (args.Length != 1)
        {
            _ui.AppendChatLine("Usage: /dequeue <gameType>", "error");
            return Task.CompletedTask;
        }

        if (args[0] != "Catan" && args[0] != "Chess")
        {
            _ui.AppendChatLine("Unsupported game!", "error");
            return Task.CompletedTask;
        }
        
        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.DequeueRequest,
            Payload = new DequeueRequestDto
            {
                Username = _session.Username!,
                Game = args[0]
            }
        });
    }
}
