using System.Threading.Tasks;
using Catan.Client.Networking;
using Catan.Shared.Networking.Messages;
using Catan.Shared.Enums;
using Catan.Client.UI;
using Catan.Client.State;
using Catan.Shared.Networking.Dtos.Server;

namespace Catan.Client.Commands;

public class RejectFriendCommand : ICommandHandler
{
    private readonly ClientSender _sender;
    private readonly MainWindow _ui;
    private readonly ClientSession _session;

    public string[] Aliases => new[] { "no" };

    public RejectFriendCommand(ClientSender sender, MainWindow ui, ClientSession session)
    {
        _sender = sender;
        _ui = ui;
        _session = session;
    }

    public Task Execute(string[] args)
    {
        if (args.Length != 1)
        {
            _ui.AppendChatLine("Usage: /no <username>");
            return Task.CompletedTask;
        }

        return _sender.SendAsync(new ClientMessage
        {
            Type = MessageType.FriendRequestAnswer,
            Payload = new FriendRequestAnswerDto
            {
                FromUsername = args[0],        // requester
                ToUsername = _session.Username!, // responder
                Answer = false
            }
        });
    }
}
